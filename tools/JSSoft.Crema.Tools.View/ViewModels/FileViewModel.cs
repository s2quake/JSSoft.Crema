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

using JSSoft.Crema.Reader;
using JSSoft.Crema.Runtime.Serialization;
using JSSoft.Crema.RuntimeService;
using JSSoft.Crema.Tools.Framework;
using JSSoft.Crema.Tools.View.Views;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JSSoft.Crema.Tools.View.ViewModels
{
    [View(typeof(ItemsView))]
    class FileViewModel : ContentBase, IDisposable
    {
        private readonly DataViewModel dataViewModel;
        private IDataSet dataSet;
        private ObservableCollection<ItemViewModel> tables = new ObservableCollection<ItemViewModel>();

        public FileViewModel(DataViewModel dataViewModel)
        {
            this.DisplayName = string.Empty;
            this.GroupName = "View";
            this.dataViewModel = dataViewModel;
        }

        public async Task OpenAsync(string filename)
        {
            try
            {
                this.DisplayName = "loading...";
                this.BeginProgress();
                this.dataSet = await Task.Run(() => CremaReader.Read(filename));
                this.tables = new ObservableCollection<ItemViewModel>();

                foreach (var item in this.dataSet.Tables.OrderBy(i => i.Name))
                {
                    this.tables.Add(new ItemViewModel(item));
                }

                this.EndProgress();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e.Message);
                this.EndProgress();
                this.Dispose();
                return;
            }

            this.DisplayName = Path.GetFileName(filename);
            this.Comment = filename;
            this.NotifyOfPropertyChange(() => this.ItemsSource);
        }

        public void Dispose()
        {
            if (this.IsProgressing == true)
                return;
            this.OnDisposed(EventArgs.Empty);
        }

        public IEnumerable ItemsSource
        {
            get { return this.tables; }
        }

        public ICommand LoadCommand
        {
            get { return this.dataViewModel.LoadCommand; }
        }

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }
    }
}
