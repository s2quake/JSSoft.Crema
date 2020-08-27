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

using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.DataGrid.Controls;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.Views
{
    public partial class ViewTemplateView : UserControl
    {
        [Import]
        private readonly IAppConfiguration configs = null;

        public ViewTemplateView()
        {

        }

        private void PART_DataGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            var gridControl = sender as ModernDataGridControl;
            this.configs.Update(this);
        }

        private void PART_DataGridControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var gridControl = sender as ModernDataGridControl;
            this.configs.Commit(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //this.tableName.Focus();
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Focus();
        }

        private void TableName_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                this.buttons.IsEnabled = false;
            }
            else if (e.Action == ValidationErrorEventAction.Removed)
            {
                this.buttons.IsEnabled = true;
            }
        }
    }
}
