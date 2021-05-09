﻿// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Tables;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.ObjectModel;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using Dispatcher = System.Windows.Threading.Dispatcher;

namespace JSSoft.Crema.Presentation.SmartSet
{
    [Export]
    class TableSmartSetContext : ItemContext<TableSmartSet, TableSmartSetCategory, TableSmartSetCollection, TableSmartSetCategoryCollection, TableSmartSetContext>,
        IServiceProvider, IXmlSerializable
    {
        private readonly ICremaAppHost cremaAppHost;
        private readonly ITableBrowser tableBrowser;
        private readonly IRule[] rules;
        private readonly HashSet<string> bookmarks = new();
        private bool isModified;

        [ImportingConstructor]
        public TableSmartSetContext(ICremaAppHost cremaAppHost, ITableBrowser tableBrowser, [ImportMany] IEnumerable<IRule> rules)
        {
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Loaded += CremaAppHost_Loaded;
            this.cremaAppHost.Unloaded += CremaAppHost_Unloaded;
            this.cremaAppHost.Resetting += CremaAppHost_Resetting;
            this.cremaAppHost.Reset += CremaAppHost_Reset;
            this.tableBrowser = tableBrowser;
            this.rules = rules.Where(item => item.SupportType == typeof(ITableDescriptor)).ToArray();
        }

        public object GetService(Type serviceType)
        {
            return this.cremaAppHost.GetService(serviceType);
        }

        public bool Verify(ITableDescriptor descriptor, IRuleItem ruleItem)
        {
            var rule = this.rules.FirstOrDefault(item => item.Name == ruleItem.RuleName);
            if (rule == null)
                return false;
            return rule.Verify(descriptor, ruleItem);
        }

        internal void UpdateBookmarkItems()
        {
            var items = TreeViewItemViewModelBuilder.MakeItemList(this.bookmarks.ToArray());
            this.bookmarks.Clear();
            foreach (var item in items)
            {
                this.bookmarks.Add(item);
            }
        }

        public void AddBookmarkItem(string itemPath)
        {
            this.bookmarks.Add(itemPath);
            this.OnBookmarkChanged(EventArgs.Empty);
        }

        public void RemoveBookmarkItem(string itemPath)
        {
            this.bookmarks.Add(itemPath);
            this.OnBookmarkChanged(EventArgs.Empty);
        }

        public ICremaAppHost CremaAppHost => this.cremaAppHost;

        public IRule[] Rules => this.rules;

        [ConfigurationProperty("bookmarkItems")]
        public string[] BookmarkItems
        {
            get => this.bookmarks.ToArray();
            set
            {
                this.bookmarks.Clear();
                if (value == null)
                    return;
                foreach (var item in value)
                {
                    this.bookmarks.Add(item);
                }
            }
        }

        public ITableDescriptor[] GetDescriptors()
        {
            return EnumerableUtility.Descendants<TreeViewItemViewModel, ITableDescriptor>(this.tableBrowser.Items.OfType<TreeViewItemViewModel>(), item => item.Items).ToArray();
        }

        public Dispatcher Dispatcher => Application.Current.Dispatcher;

        public event EventHandler Loaded;

        public event EventHandler BookmarkChanged;

        protected virtual void OnLoaded(EventArgs e)
        {
            this.Loaded?.Invoke(this, e);
        }

        protected virtual void OnBookmarkChanged(EventArgs e)
        {
            this.BookmarkChanged?.Invoke(this, e);
        }

        private async void CremaAppHost_Loaded(object sender, EventArgs e)
        {
            try
            {
                this.cremaAppHost.UserConfigs.Update(this);
            }
            catch
            {

            }

            if (this.cremaAppHost.GetService(typeof(IDataBase)) is IDataBase dataBase)
            {
                await dataBase.Dispatcher.InvokeAsync(() =>
                {
                    var tableContext = dataBase.TableContext;
                    tableContext.Tables.TablesChanged += Tables_TablesChanged;
                    tableContext.Tables.TablesStateChanged += Tables_TablesStateChanged;
                    tableContext.ItemsCreated += TableContext_ItemCreated;
                    tableContext.ItemsRenamed += TableContext_ItemRenamed;
                    tableContext.ItemsMoved += TableContext_ItemMoved;
                    tableContext.ItemsDeleted += TableContext_ItemDeleted;
                    tableContext.ItemsAccessChanged += TableContext_ItemsAccessChanged;
                    tableContext.ItemsLockChanged += TableContext_ItemsLockChanged;
                });
            }

            await this.Dispatcher.InvokeAsync(() => this.Refresh(), DispatcherPriority.ApplicationIdle);
        }

        private void CremaAppHost_Unloaded(object sender, EventArgs e)
        {
            try
            {
                this.cremaAppHost.UserConfigs?.Commit(this);
            }
            catch (Exception ex)
            {
                CremaLog.Error(ex);
            }
        }

        private void CremaAppHost_Resetting(object sender, EventArgs e)
        {
            try
            {
                this.cremaAppHost.UserConfigs?.Commit(this);
            }
            catch (Exception ex)
            {
                CremaLog.Error(ex);
            }
        }

        private async void CremaAppHost_Reset(object sender, EventArgs e)
        {
            if (this.cremaAppHost.GetService(typeof(IDataBase)) is IDataBase dataBase)
            {
                await dataBase.Dispatcher.InvokeAsync(() =>
                {
                    var tableContext = dataBase.TableContext;
                    tableContext.Tables.TablesChanged += Tables_TablesChanged;
                    tableContext.Tables.TablesStateChanged += Tables_TablesStateChanged;
                    tableContext.ItemsCreated += TableContext_ItemCreated;
                    tableContext.ItemsRenamed += TableContext_ItemRenamed;
                    tableContext.ItemsMoved += TableContext_ItemMoved;
                    tableContext.ItemsDeleted += TableContext_ItemDeleted;
                    tableContext.ItemsAccessChanged += TableContext_ItemsAccessChanged;
                    tableContext.ItemsLockChanged += TableContext_ItemsLockChanged;
                });
            }

            await this.Dispatcher.InvokeAsync(() => this.Refresh(), DispatcherPriority.ApplicationIdle);
        }

        private void Tables_TablesChanged(object sender, ItemsEventArgs<ITable> e)
        {
            this.Dispatcher.InvokeAsync(this.Refresh);
        }

        private void Tables_TablesStateChanged(object sender, ItemsEventArgs<ITable> e)
        {
            this.Dispatcher.InvokeAsync(this.Refresh);
        }

        private void TableContext_ItemCreated(object sender, Services.ItemsCreatedEventArgs<ITableItem> e)
        {
            this.Dispatcher.InvokeAsync(this.Refresh);
        }

        private void TableContext_ItemRenamed(object sender, Services.ItemsRenamedEventArgs<ITableItem> e)
        {
            this.Dispatcher.InvokeAsync(this.Refresh);
        }

        private void TableContext_ItemMoved(object sender, Services.ItemsMovedEventArgs<ITableItem> e)
        {
            this.Dispatcher.InvokeAsync(this.Refresh);
        }

        private void TableContext_ItemDeleted(object sender, Services.ItemsDeletedEventArgs<ITableItem> e)
        {
            this.Dispatcher.InvokeAsync(this.Refresh);
        }

        private void TableContext_ItemsAccessChanged(object sender, ItemsEventArgs<ITableItem> e)
        {
            this.Dispatcher.InvokeAsync(this.Refresh);
        }

        private void TableContext_ItemsLockChanged(object sender, ItemsEventArgs<ITableItem> e)
        {
            this.Dispatcher.InvokeAsync(this.Refresh);
        }

        private void Refresh()
        {
            if (this.isModified == true)
                return;

            this.isModified = true;

            this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in this.Items.ToArray())
                {
                    item.Refresh();
                }
                this.isModified = false;
            }, DispatcherPriority.ApplicationIdle);
        }

        #region IXmlSerializable

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            throw new NotImplementedException();
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.ReadStartElement(this.GetType().Name);

            reader.MoveToContent();
            if (reader.IsEmptyElement == false)
            {
                reader.ReadStartElement("Categories");
                while (reader.MoveToContent() == XmlNodeType.Element)
                {
                    var path = reader.ReadElementContentAsString();
                    this.Categories.Prepare(path);
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                    reader.ReadEndElement();
            }
            else
            {
                reader.Skip();
            }

            reader.MoveToContent();
            if (reader.IsEmptyElement == false)
            {
                reader.ReadStartElement("Items");
                while (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "Item")
                {
                    var path = reader.GetAttribute("Path");
                    var itemName = new ItemName(path);
                    var item = new TableSmartSet()
                    {
                        Name = itemName.Name,
                        Category = this.Categories[itemName.CategoryPath],
                    };
                    reader.ReadStartElement("Item");
                    reader.MoveToContent();
                    item.ReadXml(reader);
                    reader.MoveToContent();
                    reader.ReadEndElement();
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                    reader.ReadEndElement();
            }
            else
            {
                reader.Skip();
            }

            reader.MoveToContent();
            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(this.GetType().Name);

            writer.WriteStartElement("Categories");
            foreach (var item in this.Categories.OrderBy(item => item.Path))
            {
                if (item == this.Root)
                    continue;

                writer.WriteElementString("Category", item.Path);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Items");
            foreach (var item in this.Items)
            {
                writer.WriteStartElement("Item");
                writer.WriteAttributeString("Path", item.Path);
                item.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        #endregion
    }
}