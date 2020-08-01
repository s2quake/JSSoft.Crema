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

using Ntreev.ModernUI.Framework.DataGrid.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.DataGrid;

namespace Ntreev.Crema.Presentation.Assets
{
    partial class CremaDataTableItemControl : ResourceDictionary
    {
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var cell = checkBox.Tag as ModernDataCell;
            if (cell != null && cell.IsBeingEdited == true)
                cell.EndEdit();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var cell = checkBox.Tag as ModernDataCell;
            if (cell != null && cell.IsBeingEdited == true)
                cell.EndEdit();
        }

        private void CheckBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void TagSelector_ValueChanged(object sender, RoutedEventArgs e)
        {
            var tagSelector = sender as Controls.TagSelector;
            var cell = tagSelector.Tag as Cell;
            //if (cell != null)
            //    cell.EndEdit();
        }

        private void TagSelector_PopupClosed(object sender, EventArgs e)
        {
            var tagSelector = sender as Controls.TagSelector;
            var cell = tagSelector.Tag as ModernDataCell;
            if (cell != null && cell.IsBeingEdited == true)
                cell.EndEdit();
        }
    }
}
