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

using Ntreev.Crema.Presentation.SmartSet.BrowserItems.ViewModels;
using Ntreev.Crema.Presentation.SmartSet.Dialogs.ViewModels;
using Ntreev.Crema.Presentation.SmartSet.Properties;
using Ntreev.ModernUI.Framework;
using System.ComponentModel.Composition;

namespace Ntreev.Crema.Presentation.SmartSet.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(BookmarkTableTreeViewItemViewModel))]
    class MoveBookmarkTableMenuItem : MenuItemBase
    {
        public MoveBookmarkTableMenuItem()
        {
            this.DisplayName = Resources.MenuItem_MoveBookmark;
            this.HideOnDisabled = true;
        }

        protected override bool OnCanExecute(object parameter)
        {
            return parameter is BookmarkTableTreeViewItemViewModel;
        }

        protected async override void OnExecute(object parameter)
        {
            if (parameter is BookmarkTableTreeViewItemViewModel viewModel)
            {
                var browser = viewModel.Browser;
                var itemPaths = viewModel.Browser.GetBookmarkItemPaths();
                var dialog = new MoveBookmarkItemViewModel(viewModel.BookmarkPath, itemPaths);
                if (await dialog.ShowDialogAsync() == true)
                {
                    if (browser.GetBookmarkItem(dialog.TargetPath) is BookmarkCategoryTreeViewItemViewModel categoryViewModel)
                    {
                        viewModel.Parent = categoryViewModel;
                    }
                }
            }
        }
    }
}