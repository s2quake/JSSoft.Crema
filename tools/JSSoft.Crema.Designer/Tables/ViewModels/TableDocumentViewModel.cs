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
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Designer.Tables.ViewModels
{
    [Export]
    class TableDocumentViewModel : DocumentServiceBase<IDocument>
    {
        [Import]
        private Lazy<IServiceProvider> serviceProvider = null;

        public TableDocumentViewModel()
        {

        }

        public async Task OpenTableAsync(CremaDataTable dataTable)
        {
            var targetTable = dataTable.Parent ?? dataTable;
            await this.OpenTableAsync(targetTable, targetTable.Name, dataTable.Name);
        }

        private async Task OpenTableAsync(CremaDataTable targetTable, string targetName, string tableName)
        {
            var cancellation = new CancellationTokenSource();
            var document = this.Items.OfType<TableEditorViewModel>().FirstOrDefault(item => item.Table == targetTable);
            if (document == null)
            {
                var compositionService = this.ServiceProvider.GetService(typeof(ICompositionService)) as ICompositionService;
                document = new TableEditorViewModel(targetTable) { DisplayName = targetName, };
                compositionService.SatisfyImportsOnce(document);
                this.Items.Add(document);
            }
            document.SelectedTableName = tableName;
            await this.ActivateItemAsync(document, cancellation.Token);
        }

        private IServiceProvider ServiceProvider => this.serviceProvider.Value;
    }
}
