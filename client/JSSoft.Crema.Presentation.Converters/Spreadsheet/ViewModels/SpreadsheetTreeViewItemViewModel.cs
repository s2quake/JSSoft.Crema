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
using JSSoft.Crema.Spreadsheet;
using JSSoft.ModernUI.Framework.ViewModels;
using System.Linq;

namespace JSSoft.Crema.Presentation.Converters.Spreadsheet.ViewModels
{
    class SpreadsheetTreeViewItemViewModel : CheckableTreeViewItemViewModel
    {
        private string errorString;

        public SpreadsheetTreeViewItemViewModel(string path)
        {
            this.Path = path;
            this.Initialize();
        }

        public void Read(CremaDataSet dataSet)
        {
            using var reader = new SpreadsheetReader(this.Path);
            reader.Read(dataSet);
        }

        public override string DisplayName => this.Path;

        public string Path { get; private set; }

        public bool IsEnabled => string.IsNullOrEmpty(this.errorString) == true;

        public string ErrorString
        {
            get => this.errorString;
            set
            {
                this.errorString = value;
                this.NotifyOfPropertyChange(nameof(this.IsEnabled));
                this.NotifyOfPropertyChange(nameof(this.IsChecked));
                this.NotifyOfPropertyChange(nameof(this.ErrorString));
            }
        }

        public string[] GetSelectedSheetNames()
        {
            return this.Items.Cast<SheetTreeViewItemViewModel>()
                             .Where(item => item.IsChecked == true)
                             .Select(item => item.SheetName)
                             .ToArray();
        }

        private void Initialize()
        {
            foreach (var item in SpreadsheetReader.ReadSheetNames(this.Path))
            {
                var viewModel = new SheetTreeViewItemViewModel(item)
                {
                    Parent = this
                };
            }
        }
    }
}
