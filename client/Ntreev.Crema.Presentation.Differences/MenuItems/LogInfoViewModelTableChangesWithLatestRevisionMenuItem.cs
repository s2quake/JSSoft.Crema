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
using Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels;
using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Diff;
using Ntreev.Crema.ServiceModel;
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
    [ParentType(typeof(LogInfoViewModel))]
    class LogInfoViewModelTableChangesWithLatestRevisionMenuItem : MenuItemBase
    {
        [Import]
        private Authenticator authenticator = null;

        public LogInfoViewModelTableChangesWithLatestRevisionMenuItem()
        {
            this.DisplayName = Resources.MenuItem_CompareWithLatestRevision;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is LogInfoViewModel viewModel && viewModel.Target is ITable)
                return true;
            return false;
        }

        protected override void OnExecute(object parameter)
        {
            var viewModel = parameter as LogInfoViewModel;
            var table = viewModel.Target as ITable;
            var dialog = new DiffDataTableViewModel(this.Initialize(viewModel, table))
            {
                DisplayName = Resources.Title_CompareWithLatestRevision,
            };
            dialog.ShowDialog();
        }

        private async Task<DiffDataTable> Initialize(LogInfoViewModel viewModel, ITable table)
        {
            var logs = await table.GetLogAsync(this.authenticator, null);
            var log = logs.First();

            var dataSet1 = await table.GetDataSetAsync(this.authenticator, viewModel.Revision);
            var dataSet2 = await table.GetDataSetAsync(this.authenticator, log.Revision);
            var header1 = $"[{viewModel.DateTime}] {viewModel.Revision}";
            var header2 = $"[{log.DateTime}] {log.Revision}";
            var dataSet = new DiffDataSet(dataSet1, dataSet2)
            {
                Header1 = header1,
                Header2 = header2,
            };
            return dataSet.Tables.First();
        }
    }
}
