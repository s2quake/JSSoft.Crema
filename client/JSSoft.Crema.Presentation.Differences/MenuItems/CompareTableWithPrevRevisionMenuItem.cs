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
using JSSoft.Crema.Data.Diff;
using JSSoft.Crema.Presentation.Differences.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.Differences.Properties;
using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Differences.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType("JSSoft.Crema.Presentation.Tables.BrowserItems.ViewModels.TableTreeViewItemViewModel, JSSoft.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class CompareTableWithPrevRevisionMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;

        public CompareTableWithPrevRevisionMenuItem(Authenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        [ImportingConstructor]
        public CompareTableWithPrevRevisionMenuItem()
        {
            this.DisplayName = Resources.MenuItem_CompareWithPreviousResivision;
        }

        protected async override void OnExecute(object parameter)
        {
            var tableDescriptor = parameter as ITableDescriptor;
            var table = tableDescriptor.Target;

            var dialog = new DiffDataTableViewModel(this.Initialize(table))
            {
                DisplayName = Resources.Title_CompareWithPreviousResivision,
            };
            await dialog.ShowDialogAsync();
        }

        private async Task<DiffDataTable> Initialize(ITable table)
        {
            var logs = await table.GetLogAsync(this.authenticator, null);
            var hasRevision = logs.Length >= 2;
            var dataSet1 = hasRevision ? await table.GetDataSetAsync(this.authenticator, logs[1].Revision) : new CremaDataSet();
            var dataSet2 = await table.GetDataSetAsync(this.authenticator, null);
            var header1 = hasRevision ? $"[{logs[1].DateTime}] {logs[1].Revision}" : string.Empty;
            var dataSet = new DiffDataSet(dataSet1, dataSet2)
            {
                Header1 = header1,
                Header2 = Resources.Text_Current,
            };
            return dataSet.Tables.First();
        }
    }
}
