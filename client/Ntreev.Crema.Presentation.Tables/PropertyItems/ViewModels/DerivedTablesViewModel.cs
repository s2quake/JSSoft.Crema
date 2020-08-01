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

using System;
using System.Linq;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Services;
using Ntreev.Crema.ServiceModel;
using System.Threading.Tasks;
using Ntreev.Library;
using System.Collections.Generic;
using System.Collections;
using Ntreev.ModernUI.Framework;
using System.ComponentModel;
using Ntreev.Library.Linq;
using Ntreev.Crema.Presentation.Tables.Properties;
using Ntreev.ModernUI.Framework.ViewModels;
using Ntreev.Crema.Presentation.Tables.BrowserItems.ViewModels;

namespace Ntreev.Crema.Presentation.Tables.PropertyItems.ViewModels
{
    [Export(typeof(IPropertyItem))]
    [RequiredAuthority(Authority.Guest)]
    [Dependency(typeof(TableInfoViewModel))]
    [ParentType(typeof(PropertyService))]
    class DerivedTablesViewModel : PropertyItemBase, ISelector
    {
        [Import]
        private Lazy<TableBrowserViewModel> browser = null;
        [Import]
        private Authenticator authenticator = null;
        private ITableDescriptor descriptor;
        private TableListBoxItemViewModel[] tables;
        private TableListBoxItemViewModel selectedTable;
        [Import]
        private IBuildUp buildUp = null;

        [ImportingConstructor]
        public DerivedTablesViewModel()
        {
            this.DisplayName = Resources.Title_DerivedTable;
        }

        public override bool CanSupport(object obj)
        {
            return obj is ITableDescriptor;
        }

        public override void SelectObject(object obj)
        {
            this.descriptor = obj as ITableDescriptor;

            if (this.descriptor != null)
            {
                var items = EnumerableUtility.Descendants<TreeViewItemViewModel, ITableDescriptor>(this.Browser.Items, item => item.Items)
                                             .Where(item => item.TableInfo.TemplatedParent == this.descriptor.Name)
                                             .ToArray();

                var viewModelList = new List<TableListBoxItemViewModel>();
                foreach (var item in items)
                {
                    var table = item.Target;
                    if (table.ExtendedProperties.ContainsKey(this) == true)
                    {
                        var descriptor = table.ExtendedProperties[this] as TableDescriptor;
                        viewModelList.Add(descriptor.Host as TableListBoxItemViewModel);
                    }
                    else
                    {
                        var viewModel = new TableListBoxItemViewModel(this.authenticator, item, this);
                        this.buildUp.BuildUp(viewModel);
                        viewModelList.Add(viewModel);
                    }
                }
                this.Tables = viewModelList.ToArray();
            }
            else
            {
                this.Tables = new TableListBoxItemViewModel[] { };
            }

            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.SelectedObject));
        }

        public override bool IsVisible
        {
            get
            {
                if (this.descriptor == null)
                    return false;
                return this.tables != null && this.tables.Any();
            }
        }

        public override object SelectedObject
        {
            get { return this.descriptor; }
        }

        public TableListBoxItemViewModel[] Tables
        {
            get { return this.tables; }
            set
            {
                this.tables = value;
                 this.NotifyOfPropertyChange(nameof(this.Tables));
            }
        }

        public TableListBoxItemViewModel SelectedTable
        {
            get { return this.selectedTable; }
            set
            {
                if (this.selectedTable != null)
                    this.selectedTable.IsSelected = false;
                this.selectedTable = value;
                if (this.selectedTable != null)
                    this.selectedTable.IsSelected = true;
                this.NotifyOfPropertyChange(nameof(this.SelectedTable));
            }
        }

        private TableBrowserViewModel Browser => this.browser.Value;

        #region ISelector

        object ISelector.SelectedItem
        {
            get { return this.SelectedTable; }
            set { this.SelectedTable = value as TableListBoxItemViewModel; }
        }

        #endregion
    }
}
