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

using JSSoft.Crema.Data;
using JSSoft.Crema.Presentation.Types.Properties;
using JSSoft.Crema.Services;
using JSSoft.Library.IO;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;

namespace JSSoft.Crema.Presentation.Types.Dialogs.ViewModels
{
    class PreviewTypeCategoryViewModel : ModalDialogBase
    {
        private readonly Authentication authentication;
        private readonly ITypeCategory category;
        private readonly string revision;

        private CremaDataSet source;
        private readonly ObservableCollection<TreeViewItemViewModel> itemsSource = new();
        private readonly PreviewDocumentViewModel documents = new();
        private object selectedItem;

        public PreviewTypeCategoryViewModel(Authentication authentication, ITypeCategory category, string revision)
        {
            this.authentication = authentication;
            this.category = category;
            this.revision = revision;
            this.Initialize();
        }

        public CremaDataSet Source
        {
            get => this.source;
            private set
            {
                var builder = new PreviewTreeViewItemViewModelBuilder(this.documents.ViewType);
                var itemPaths = value.Types.Select(item => item.CategoryPath + item.Name).ToArray();
                var items = builder.Create(itemPaths, false);

                foreach (var item in value.Types)
                {
                    items[item.CategoryPath + item.Name].Target = item;
                }

                this.itemsSource.Clear();
                this.itemsSource.Add(items[PathUtility.Separator]);
                this.source = value;
                this.NotifyOfPropertyChange(nameof(this.Source));
            }
        }

        public IEnumerable ItemsSource => this.itemsSource;

        public object SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.selectedItem = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
            }
        }

        public IDocumentService Documents => this.documents;

        private async void Initialize()
        {
            try
            {
                this.DisplayName = await this.category.Dispatcher.InvokeAsync(() => $"{this.category.Path} - {revision}");
                this.BeginProgress(Resources.Message_ReceivingInfo);
                this.Source = await this.category.GetDataSetAsync(this.authentication, this.revision);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
                await this.TryCloseAsync();
            }
            finally
            {
                this.EndProgress();
            }
        }
    }
}
