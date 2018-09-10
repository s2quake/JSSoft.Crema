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
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable 0612

namespace Ntreev.Crema.Services.Data
{
    class TableCollection : TableCollectionBase<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        ITableCollection
    {
        private ItemsCreatedEventHandler<ITable> tablesCreated;
        private ItemsRenamedEventHandler<ITable> tablesRenamed;
        private ItemsMovedEventHandler<ITable> tablesMoved;
        private ItemsDeletedEventHandler<ITable> tablesDeleted;
        private ItemsEventHandler<ITable> tablesChanged;
        private ItemsEventHandler<ITable> tablesStateChanged;

        public TableCollection()
        {

        }

        public Table AddNew(Authentication authentication, string name, string categoryPath)
        {
            if (NameValidator.VerifyName(name) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidName_Format, name), nameof(name));
            return this.BaseAddNew(name, categoryPath, authentication);
        }

        public Table[] AddNew(Authentication authentication, CremaDataSet dataSet)
        {
            var query = from item in dataSet.Tables
                        where this.Contains(item.Name) == false
                        orderby item.Name
                        orderby item.TemplatedParentName != string.Empty
                        select item;
            var tables = query.ToArray();
            foreach (var item in tables)
            {
                this.ValidateAddNew(item.Name, item.CategoryPath, authentication);
            }
            var tableList = new List<Table>(tables.Length);
            this.InvokeTableCreate(authentication, tables.Select(item => item.Name).ToArray(), dataSet, null);
            foreach (var item in tables)
            {
                var table = this.AddNew(authentication, item.Name, item.CategoryPath);
                if (item.TemplatedParentName != string.Empty)
                    table.TemplatedParent = this[item.TemplatedParentName];
                table.Initialize(item.TableInfo);
                tableList.Add(table);
            }

            this.InvokeTablesCreatedEvent(authentication, tableList.ToArray(), dataSet);
            return tableList.ToArray();
        }

        public Table Inherit(Authentication authentication, Table table, string newTableName, string categoryPath, bool copyContent)
        {
            this.ValidateInherit(authentication, table, newTableName, categoryPath, copyContent);
            this.Sign(authentication);
            var dataSet = table.ReadData(authentication);
            var dataTable = dataSet.Tables[table.Name, table.Category.Path];
            var itemName = new ItemName(categoryPath, newTableName);
            var newDataTable = dataTable.Inherit(itemName, copyContent);
            newDataTable.CategoryPath = categoryPath;
            this.AddNew(authentication, dataSet);
            return this[newTableName];
        }

        public async Task<Table> CopyAsync(Authentication authentication, Table table, string newTableName, string categoryPath, bool copyContent)
        {
            try
            {
                return await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.ValidateCopy(authentication, table, newTableName, categoryPath, copyContent);
                    this.Sign(authentication);
                    var dataSet = table.ReadData(authentication);
                    var dataTable = dataSet.Tables[table.Name, table.Category.Path];
                    var itemName = new ItemName(categoryPath, newTableName);
                    var newDataTable = dataTable.Copy(itemName, copyContent);
                    newDataTable.CategoryPath = categoryPath;
                    this.AddNew(authentication, dataSet);
                    return this[newTableName];
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public object GetService(System.Type serviceType)
        {
            return this.DataBase.GetService(serviceType);
        }

        public void InvokeTableCreate(Authentication authentication, string[] tableNames, CremaDataSet dataSet, Table sourceTable)
        {
            var message = EventMessageBuilder.CreateTable(authentication, tableNames);
            try
            {
                this.Repository.CreateTable(dataSet);
                this.Repository.Commit(authentication, message);
            }
            catch
            {
                this.Repository.Revert();
                throw;
            }
        }

        public Task<SignatureDate> InvokeTableRenameAsync(Authentication authentication, Table table, string newName, CremaDataSet dataSet)
        {
            var message = EventMessageBuilder.RenameTable(authentication, table.Name, newName);
            var dataBaseSet = new DataBaseSet(table.DataBase, dataSet);
            var tablePath = table.Path;
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var signatureDate = authentication.Sign();
                    this.Repository.RenameTable(dataBaseSet, tablePath, newName);
                    this.Repository.Commit(authentication, message);
                    return signatureDate;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task<SignatureDate> InvokeTableMoveAsync(Authentication authentication, Table table, string newCategoryPath, CremaDataSet dataSet)
        {
            var message = EventMessageBuilder.MoveTable(authentication, table.Name, newCategoryPath, table.Category.Path);
            var dataBaseSet = new DataBaseSet(table.DataBase, dataSet);
            var tablePath = table.Path;
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var signatureDate = authentication.Sign();
                    this.Repository.MoveTable(dataBaseSet, tablePath, newCategoryPath);
                    this.Repository.Commit(authentication, message);
                    return signatureDate;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task<SignatureDate> InvokeTableDeleteAsync(Authentication authentication, Table table, CremaDataSet dataSet)
        {
            var message = EventMessageBuilder.DeleteTable(authentication, table.Name);
            var dataBaseSet = new DataBaseSet(table.DataBase, dataSet);
            var tablePath = table.Path;
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var signatureDate = authentication.Sign();
                    this.Repository.DeleteTable(dataBaseSet, tablePath);
                    this.Repository.Commit(authentication, message);
                    return signatureDate;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public void InvokeTableEndContentEdit(Authentication authentication, Table[] tables, CremaDataSet dataSet)
        {
            var message = EventMessageBuilder.ChangeTableContent(authentication, tables);
            try
            {
                this.Repository.ModifyTable(dataSet, this.DataBase);
                this.Repository.Commit(authentication, message);
            }
            catch
            {
                this.Repository.Revert();
                throw;
            }
        }

        public void InvokeTableEndTemplateEdit(Authentication authentication, Table table, CremaTemplate template)
        {
            var dataTable = template.TargetTable;
            var dataSet = dataTable.DataSet;
            var message = EventMessageBuilder.ChangeTableTemplate(authentication, table.Name);
            try
            {
                this.Repository.ModifyTable(dataSet, table);
                this.Repository.Commit(authentication, message);
            }
            catch
            {
                this.Repository.Revert();
                throw;
            }
        }

        public void InvokeTablesCreatedEvent(Authentication authentication, Table[] tables, CremaDataSet dataSet)
        {
            var args = tables.Select(item => (object)item.TableInfo).ToArray();
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesCreatedEvent), tables);
            var message = EventMessageBuilder.CreateTable(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesCreated(new ItemsCreatedEventArgs<ITable>(authentication, tables, args, dataSet));
            this.Context.InvokeItemsCreatedEvent(authentication, tables, args, dataSet);
        }

        public void InvokeTablesRenamedEvent(Authentication authentication, Table[] tables, string[] oldNames, string[] oldPaths, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesRenamedEvent), tables, oldNames, oldPaths);
            var message = EventMessageBuilder.RenameTable(authentication, tables, oldNames);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesRenamed(new ItemsRenamedEventArgs<ITable>(authentication, tables, oldNames, oldPaths, dataSet));
            this.Context.InvokeItemsRenamedEvent(authentication, tables, oldNames, oldPaths, dataSet);
        }

        public void InvokeTablesMovedEvent(Authentication authentication, Table[] tables, string[] oldPaths, string[] oldCategoryPaths, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesMovedEvent), tables, oldPaths, oldCategoryPaths);
            var message = EventMessageBuilder.MoveTable(authentication, tables, oldCategoryPaths);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesMoved(new ItemsMovedEventArgs<ITable>(authentication, tables, oldPaths, oldCategoryPaths, dataSet));
            this.Context.InvokeItemsMovedEvent(authentication, tables, oldPaths, oldCategoryPaths, dataSet);
        }

        public void InvokeTablesDeletedEvent(Authentication authentication, Table[] tables, string[] oldPaths)
        {
            var dataSet = CremaDataSet.Create(new SignatureDateProvider(authentication.ID));
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesDeletedEvent), oldPaths);
            var message = EventMessageBuilder.DeleteTable(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesDeleted(new ItemsDeletedEventArgs<ITable>(authentication, tables, oldPaths, dataSet));
            this.Context.InvokeItemsDeletedEvent(authentication, tables, oldPaths, dataSet);
        }

        public void InvokeTablesStateChangedEvent(Authentication authentication, Table[] tables)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeTablesStateChangedEvent), tables);
            this.OnTablesStateChanged(new ItemsEventArgs<ITable>(authentication, tables));
        }

        public void InvokeTablesTemplateChangedEvent(Authentication authentication, Table[] tables, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesTemplateChangedEvent), tables);
            var message = EventMessageBuilder.ChangeTableTemplate(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesChanged(new ItemsEventArgs<ITable>(authentication, tables, dataSet));
            this.Context.InvokeItemsChangedEvent(authentication, tables, dataSet);
        }

        public void InvokeTablesContentChangedEvent(Authentication authentication, Table[] tables, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesContentChangedEvent), tables);
            var message = EventMessageBuilder.ChangeTableContent(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesChanged(new ItemsEventArgs<ITable>(authentication, tables));
            this.Context.InvokeItemsChangedEvent(authentication, tables, dataSet);
        }

        public DataBaseRepositoryHost Repository => this.DataBase.Repository;

        public CremaHost CremaHost => this.Context.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public new int Count => base.Count;

        public event ItemsCreatedEventHandler<ITable> TablesCreated
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesCreated += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesCreated -= value;
            }
        }

        public event ItemsRenamedEventHandler<ITable> TablesRenamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesRenamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesRenamed -= value;
            }
        }

        public event ItemsMovedEventHandler<ITable> TablesMoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesMoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesMoved -= value;
            }
        }

        public event ItemsDeletedEventHandler<ITable> TablesDeleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesDeleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesDeleted -= value;
            }
        }

        public event ItemsEventHandler<ITable> TablesChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesChanged -= value;
            }
        }

        public event ItemsEventHandler<ITable> TablesStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesStateChanged -= value;
            }
        }

        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.CollectionChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.CollectionChanged -= value;
            }
        }

        protected override void ValidateAddNew(string name, string categoryPath, object validation)
        {
            base.ValidateAddNew(name, categoryPath, validation);

            if (NameValidator.VerifyName(name) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidName_Format, name), nameof(name));

            if (validation is Authentication authentication)
            {
                var category = this.GetCategory(categoryPath);
                if (category == null)
                    throw new CategoryNotFoundException(categoryPath);
                category.ValidateAccessType(authentication, AccessType.Master);
            }
        }

        protected virtual void OnTablesCreated(ItemsCreatedEventArgs<ITable> e)
        {
            this.tablesCreated?.Invoke(this, e);
        }

        protected virtual void OnTablesRenamed(ItemsRenamedEventArgs<ITable> e)
        {
            this.tablesRenamed?.Invoke(this, e);
        }

        protected virtual void OnTablesMoved(ItemsMovedEventArgs<ITable> e)
        {
            this.tablesMoved?.Invoke(this, e);
        }

        protected virtual void OnTablesDeleted(ItemsDeletedEventArgs<ITable> e)
        {
            this.tablesDeleted?.Invoke(this, e);
        }

        protected virtual void OnTablesStateChanged(ItemsEventArgs<ITable> e)
        {
            this.tablesStateChanged?.Invoke(this, e);
        }

        protected virtual void OnTablesChanged(ItemsEventArgs<ITable> e)
        {
            this.tablesChanged?.Invoke(this, e);
        }

        protected override Table NewItem(params object[] args)
        {
            return new Table();
        }

        private void Sign(Authentication authentication)
        {
            authentication.Sign();
        }

        private void ValidateInherit(Authentication authentication, Table table, string newTableName, string categoryPath, bool copyXml)
        {
            table.ValidateAccessType(authentication, AccessType.Master);

            if (this.Contains(newTableName) == true)
                throw new ArgumentException(Resources.Exception_SameTableNameExist, nameof(newTableName));
            if (table.Parent != null)
                throw new InvalidOperationException(Resources.Exception_ChildTableCannotInherit);
            if (table.TemplatedParent != null)
                throw new InvalidOperationException(Resources.Exception_InheritedTableCannotInherit);

            NameValidator.ValidateCategoryPath(categoryPath);

            if (this.Context.Categories.Contains(categoryPath) == false)
                throw new CategoryNotFoundException(categoryPath);

            var category = this.Context.Categories[categoryPath];
            category.ValidateAccessType(authentication, AccessType.Master);

            if (copyXml == true)
            {
                foreach (var item in EnumerableUtility.Friends(table, table.Childs))
                {
                    item.ValidateHasNotBeingEditedType();
                }
            }
        }

        private void ValidateCopy(Authentication authentication, Table table, string newTableName, string categoryPath, bool copyXml)
        {
            table.ValidateAccessType(authentication, AccessType.Master);

            if (this.Contains(newTableName) == true)
                throw new ArgumentException(Resources.Exception_SameTableNameExist, nameof(newTableName));
            if (table.Parent != null)
                throw new InvalidOperationException(Resources.Exception_ChildTableCannotCopy);

            NameValidator.ValidateCategoryPath(categoryPath);

            if (this.Context.Categories.Contains(categoryPath) == false)
                throw new CategoryNotFoundException(categoryPath);

            var category = this.Context.Categories[categoryPath];
            category.ValidateAccessType(authentication, AccessType.Master);

            if (copyXml == true)
            {
                foreach (var item in EnumerableUtility.Friends(table, table.Childs))
                {
                    item.ValidateHasNotBeingEditedType();
                }
            }
        }

        #region ITableCollection

        bool ITableCollection.Contains(string tableName)
        {
            return this.Contains(tableName);
        }

        ITable ITableCollection.this[string tableName] => this[tableName];

        #endregion

        #region IEnumerable

        IEnumerator<ITable> IEnumerable<ITable>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.DataBase as IDataBase).GetService(serviceType);
        }

        #endregion
    }
}
