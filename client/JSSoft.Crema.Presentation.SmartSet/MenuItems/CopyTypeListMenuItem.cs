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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.SmartSet.BrowserItems.ViewModels;
using JSSoft.Crema.Presentation.SmartSet.Properties;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.Presentation.SmartSet.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(SmartSetTreeViewItemViewModel))]
    [ParentType(typeof(BookmarkRootTreeViewItemViewModel))]
    [ParentType(typeof(BookmarkCategoryTreeViewItemViewModel))]
    class CopyTypeListMenuItem : MenuItemBase
    {
        private readonly ICremaAppHost cremaAppHost;

        [ImportingConstructor]
        public CopyTypeListMenuItem(ICremaAppHost cremaAppHost, Authenticator authenticator)
        {
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Loaded += this.InvokeCanExecuteChangedEvent;
            this.cremaAppHost.Unloaded += this.InvokeCanExecuteChangedEvent;
            this.DisplayName = Resources.MenuItem_CopyList;
            this.HideOnDisabled = true;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (this.cremaAppHost.IsLoaded == false)
                return false;

            if (parameter is BookmarkRootTreeViewItemViewModel root)
            {
                return root.Browser is TypeSmartSetBrowserViewModel;
            }
            else if (parameter is BookmarkCategoryTreeViewItemViewModel category)
            {
                return category.Browser is TypeSmartSetBrowserViewModel;
            }
            else if (parameter is SmartSetTreeViewItemViewModel smartSet)
            {
                return smartSet.SmartSet is TypeSmartSet;
            }

            return false;
        }

        protected async override void OnExecute(object parameter)
        {
            var viewModel = parameter as TreeViewItemViewModel;
            var items = viewModel.Items.OfType<ITypeDescriptor>().Select(item => item.Name);
            await AppMessageBox.ShowAsync(string.Join(Environment.NewLine, items));
        }
    }
}
