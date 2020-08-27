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

using JSSoft.Crema.Comparer.Templates.ViewModels;
using JSSoft.Crema.Data.Diff;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JSSoft.Crema.Comparer.Tables.ViewModels
{
    public class TableUnresolvedItemListBoxItemViewModel : ListBoxItemViewModel
    {
        [Import]
        private TemplateBrowserViewModel templateBrowser = null;
        [Import]
        private ShellViewModel shellViewModel = null;
        [Import]
        private TemplatesViewModel templatesViewModel = null;

        private readonly object item;

        public TableUnresolvedItemListBoxItemViewModel(object item)
        {
            this.item = item;
            this.Target = item;
            if (this.item is TemplateTreeViewItemViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        public void SelectInBrowserCommand()
        {
            if (this.item is TemplateTreeViewItemViewModel templateViewModel)
            {
                this.Dispatcher.InvokeAsync(() => this.shellViewModel.SelectedContent = this.templatesViewModel);
                this.Dispatcher.InvokeAsync(() => this.templateBrowser.SelectedItem = templateViewModel, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }

        public override string DisplayName => $"{this.item}";

        public DiffState DiffState
        {
            get
            {
                if (this.item is TemplateTreeViewItemViewModel templateViewModel)
                {
                    return templateViewModel.DiffState;
                }
                return DiffState.Unchanged;
            }
        }

        public bool IsResolved
        {
            get
            {
                if (this.item is TemplateTreeViewItemViewModel templateViewModel)
                {
                    return templateViewModel.IsResolved;
                }
                return true;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TemplateTreeViewItemViewModel viewModel && e.PropertyName == nameof(TemplateTreeViewItemViewModel.IsResolved))
            {
                this.NotifyOfPropertyChange(nameof(this.IsResolved));
            }
        }
    }
}
