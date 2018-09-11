﻿//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Domains;
using Ntreev.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    partial class TableContent
    {
        private TableContentDomainHost domainHost;

        public class TableContentDomainHost : IDomainHost, IEnumerable<ITableContent>
        {
            private readonly TableCollection container;
            private readonly TableContent[] contents;
            private Domain domain;

            public TableContentDomainHost(TableCollection container, Domain domain, string itemPath)
            {
                var items = StringUtility.Split(itemPath, '|');
                var tableList = new List<Table>(items.Length);
                var dataBase = container.DataBase;
                foreach (var item in items)
                {
                    if (dataBase.TableContext[item] is Table table)
                    {
                        tableList.Add(table);
                    }
                }

                this.container = container;
                this.Tables = tableList.ToArray();
                this.contents = tableList.Select(item => item.Content).ToArray();
                this.domain = domain;
                foreach (var item in this.contents)
                {
                    item.domainHost = this;
                }
            }

            public void AttachDomainEvent()
            {
                this.domain.Deleted += Domain_Deleted;
                this.domain.RowAdded += Domain_RowAdded;
                this.domain.RowChanged += Domain_RowChanged;
                this.domain.RowRemoved += Domain_RowRemoved;
                this.domain.PropertyChanged += Domain_PropertyChanged;
            }

            public void DetachDomainEvent()
            {
                this.domain.Deleted -= Domain_Deleted;
                this.domain.RowAdded -= Domain_RowAdded;
                this.domain.RowChanged -= Domain_RowChanged;
                this.domain.RowRemoved -= Domain_RowRemoved;
                this.domain.PropertyChanged -= Domain_PropertyChanged;
            }

            public void InvokeEditBegunEvent(EventArgs e)
            {
                foreach (var item in this.contents)
                {
                    item.OnEditBegun(e);
                }
            }

            public void InvokeEditEndedEvent(EventArgs e)
            {
                foreach (var item in this.contents)
                {
                    item.OnEditEnded(e);
                }
            }

            public void InvokeEditCanceledEvent(EventArgs e)
            {
                foreach (var item in this.contents)
                {
                    item.OnEditCanceled(e);
                }
            }

            public async Task BeginContentAsync(Authentication authentication)
            {
                var dataSet = this.domain.Source as CremaDataSet;
                foreach (var item in this.contents)
                {
                    item.domain = domain;
                    item.dataTable = dataSet.Tables[item.Table.Name, item.Table.Category.Path];
                    item.Table.SetTableState(TableState.IsBeingEdited);
                    item.IsModified = domain.ModifiedTables.Contains(item.dataTable.Name);
                }
                await this.domain.Dispatcher.InvokeAsync(this.AttachDomainEvent);
                this.container.InvokeTablesStateChangedEvent(authentication, this.Tables);
            }

            public async Task EndContentAsync(Authentication authentication)
            {
                var dataSet = this.domain.Source as CremaDataSet;
                var tables = this.contents.Where(item => item.IsModified).Select(item => item.Table).ToArray();
                if (this.domain.IsModified == true)
                {
                    await this.container.InvokeTableEndContentEditAsync(authentication, this.Tables, dataSet);
                }
                await this.domain.Dispatcher.InvokeAsync(() =>
                {
                    this.DetachDomainEvent();
                    this.domain.Dispose(authentication, false);
                });
                foreach (var item in this.contents)
                {
                    if (item.IsModified == true)
                        item.Table.UpdateContent(item.dataTable.TableInfo);
                    item.domain = null;
                    item.IsModified = false;
                    item.dataTable = null;
                    item.Table.SetTableState(TableState.None);
                }
                if (tables.Any() == true)
                    this.container.InvokeTablesContentChangedEvent(authentication, tables, dataSet);
                this.container.InvokeTablesStateChangedEvent(authentication, this.Tables);
            }

            public async Task CancelContentAsync(Authentication authentication)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.DetachDomainEvent();
                    this.domain.Dispose(authentication, true);
                });
                foreach (var item in this.contents)
                {
                    item.domain = null;
                    item.IsModified = false;
                    item.dataTable = null;
                    item.Table.SetTableState(TableState.None);
                }
                this.container.InvokeTablesStateChangedEvent(authentication, this.Tables);
            }

            public void EnterContent(Authentication authentication)
            {

            }

            public void LeaveContent(Authentication authentication)
            {

            }

            public Table[] Tables { get; }

            private async void Domain_Deleted(object sender, DomainDeletedEventArgs e)
            {
                if (e.IsCanceled == false)
                {
                    await this.EndContentAsync(e.Authentication);
                    await this.Dispatcher.InvokeAsync(() => this.InvokeEditEndedEvent(e));
                }
                else
                {
                    await this.CancelContentAsync(e.Authentication);
                    await this.Dispatcher.InvokeAsync(() => this.InvokeEditCanceledEvent(e));
                }
            }

            private async void Domain_RowAdded(object sender, DomainRowEventArgs e)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var query = from row in e.Rows
                                join content in this.contents on row.TableName equals content.dataTable.Name
                                select content;
                    foreach (var item in query)
                    {
                        item.IsModified = true;
                        item.OnChanged(e);
                    }
                });
            }

            private async void Domain_RowChanged(object sender, DomainRowEventArgs e)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var query = from row in e.Rows
                                join content in this.contents on row.TableName equals content.dataTable.Name
                                select content;
                    foreach (var item in query)
                    {
                        item.IsModified = true;
                        item.OnChanged(e);
                    }
                });
            }

            private async void Domain_RowRemoved(object sender, DomainRowEventArgs e)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var query = from row in e.Rows
                                join content in this.contents on row.TableName equals content.dataTable.Name
                                select content;
                    foreach (var item in query)
                    {
                        item.IsModified = true;
                        item.OnChanged(e);
                    }
                });
            }

            private void Domain_PropertyChanged(object sender, DomainPropertyEventArgs e)
            {

            }

            private CremaDispatcher Dispatcher => this.container.Dispatcher;

            #region IDomainHost

            void IDomainHost.Detach()
            {
                this.domain.Dispatcher.Invoke(this.DetachDomainEvent);
                this.domain = null;
                foreach (var item in this.contents)
                {
                    item.domain = null;
                    item.dataTable = null;
                }
            }

            void IDomainHost.Restore(Authentication authentication, Domain domain)
            {
                var dataSet = domain.Source as CremaDataSet;
                this.domain = domain;
                foreach (var item in this.contents)
                {
                    item.domainHost = this;
                    item.domain = domain;
                    item.dataTable = dataSet.Tables[item.Table.Name, item.Table.Category.Path];
                    item.Table.SetTableState(TableState.IsBeingEdited);
                    item.IsModified = domain.ModifiedTables.Contains(item.dataTable.Name);
                }
                this.domain.Dispatcher.Invoke(this.AttachDomainEvent);
                this.container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                this.InvokeEditBegunEvent(EventArgs.Empty);
            }

            void IDomainHost.ValidateDelete(Authentication authentication, bool isCanceled)
            {

            }

            #endregion

            #region IEnumerable

            IEnumerator<ITableContent> IEnumerable<ITableContent>.GetEnumerator()
            {
                foreach (var item in this.contents)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (var item in this.contents)
                {
                    yield return item;
                }
            }

            #endregion
        }
    }
}
