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

using Ntreev.Crema.Presentation.Differences.Dialogs.ViewModels;
using Ntreev.Crema.Presentation.Differences.Properties;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Framework.Dialogs.ViewModels;
using Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels;
using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Diff;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Differences.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType("Ntreev.Crema.Presentation.Tables.BrowserItems.ViewModels.TableTreeViewItemViewModel, Ntreev.Crema.Presentation.Tables, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class CompareTableWithPrevRevisionMenuItem : MenuItemBase
    {
        [Import]
        private Authenticator authenticator = null;

        public CompareTableWithPrevRevisionMenuItem()
        {
            this.DisplayName = Resources.MenuItem_CompareWithPreviousResivision;
        }

        protected override void OnExecute(object parameter)
        {
            var tableDescriptor = parameter as ITableDescriptor;
            var table = tableDescriptor.Target;

            var dialog = new DiffDataTableViewModel(this.Initialize(table))
            {
                DisplayName = Resources.Title_CompareWithPreviousResivision,
            };
            dialog.ShowDialog();
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
