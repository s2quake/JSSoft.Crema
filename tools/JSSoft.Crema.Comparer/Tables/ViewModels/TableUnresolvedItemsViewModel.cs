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

using JSSoft.Crema.Comparer.Properties;
using JSSoft.Crema.Comparer.Tables.Views;
using JSSoft.Crema.Comparer.Templates.ViewModels;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Comparer.Tables.ViewModels
{
    [Export(typeof(IPropertyItem))]
    [ParentType(typeof(TablePropertyViewModel))]
    [Order(-1)]
    class TableUnresolvedItemsViewModel : PropertyItemBase
    {
        [Import]
        private Lazy<TemplateBrowserViewModel> templateBrowser = null;
        [Import]
        private ICompositionService compositionService = null;

        private TableTreeViewItemViewModel viewModel;
        private ObservableCollection<TableUnresolvedItemListBoxItemViewModel> itemList = new ObservableCollection<TableUnresolvedItemListBoxItemViewModel>();
        private TableUnresolvedItemListBoxItemViewModel selectedItem;

        public TableUnresolvedItemsViewModel()
        {
            this.DisplayName = Resources.Title_UnresolvedItems;
        }

        public override bool IsVisible => this.itemList.Any();

        public override object SelectedObject => this.viewModel;

        public override bool CanSupport(object obj)
        {
            return obj is TableTreeViewItemViewModel;
        }

        public ObservableCollection<TableUnresolvedItemListBoxItemViewModel> Items
        {
            get { return this.itemList; }
        }

        public override void SelectObject(object obj)
        {
            if (obj is TableTreeViewItemViewModel viewModel)
            {
                var query = from viewModelItem in this.GetViewModels()
                            join unresolvedItem in viewModel.Source.UnresolvedItems on viewModelItem.Target equals unresolvedItem
                            select viewModelItem;

                this.itemList.Clear();
                foreach (var item in query)
                {
                    var itemViewModel = new TableUnresolvedItemListBoxItemViewModel(item);
                    itemList.Add(itemViewModel);
                    itemViewModel.PropertyChanged += ItemViewModel_PropertyChanged;
                }

                foreach (var item in itemList)
                {
                    this.compositionService.SatisfyImportsOnce(item);
                }

                this.viewModel = viewModel;
                this.selectedItem = itemList.FirstOrDefault();
            }
            else
            {
                this.itemList.Clear();
                this.viewModel = null;
            }
            this.NotifyOfPropertyChange(nameof(this.DisplayName));
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.Items));
        }

        private void ItemViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TableUnresolvedItemListBoxItemViewModel viewModel && e.PropertyName == nameof(TemplateUnresolvedItemListBoxItemViewModel.IsResolved))
            {
                if (viewModel.IsResolved == true)
                {
                    this.itemList.Remove(viewModel);
                    this.NotifyOfPropertyChange(nameof(this.IsVisible));
                }
            }
        }

        private IEnumerable<TreeViewItemViewModel> GetViewModels()
        {
            foreach (var item in EnumerableUtility.FamilyTree(this.templateBrowser.Value.Items, i => i.Items))
            {
                yield return item;
            }
        }
    }
}
