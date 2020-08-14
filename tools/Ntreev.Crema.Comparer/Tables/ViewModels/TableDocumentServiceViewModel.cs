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

using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Comparer.Tables.ViewModels
{
    [Export]
    class TableDocumentServiceViewModel : DocumentServiceBase<IDocument>
    {
        public TableDocumentServiceViewModel()
        {

        }

        public async Task ViewAsync(TableTreeViewItemViewModel viewModel)
        {
            var cancellation = new CancellationTokenSource();
            var targetModel = (TableTreeViewItemViewModel)viewModel.Parent ?? viewModel;
            var document = this.Items.OfType<TableDocumentViewModel>().FirstOrDefault(item => item.Source == targetModel.Source);
            if (document == null)
            {
                document = new TableDocumentViewModel(targetModel);
                this.Items.Add(document);
            }
            document.SelectedName = $"{viewModel}";
            await this.ActivateItemAsync(document, cancellation.Token);
        }
    }
}
