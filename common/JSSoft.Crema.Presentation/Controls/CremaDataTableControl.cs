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
using JSSoft.Library.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JSSoft.Crema.Presentation.Controls
{
    public class CremaDataTableControl : Control
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(CremaDataTable), typeof(CremaDataTableControl),
                new PropertyMetadata(null, SourcePropertyChangedCallback));

        private static readonly DependencyPropertyKey TablesPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Tables), typeof(CremaDataTable[]), typeof(CremaDataTableControl),
                new PropertyMetadata(new CremaDataTable[] { }));
        public static readonly DependencyProperty TablesProperty = TablesPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SelectedTableProperty =
            DependencyProperty.Register(nameof(SelectedTable), typeof(CremaDataTable), typeof(CremaDataTableControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register(nameof(ReadOnly), typeof(bool), typeof(CremaDataTableControl));

        public CremaDataTable Source
        {
            get => (CremaDataTable)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        public CremaDataTable[] Tables => (CremaDataTable[])this.GetValue(TablesProperty);

        public CremaDataTable SelectedTable
        {
            get => (CremaDataTable)this.GetValue(SelectedTableProperty);
            set => this.SetValue(SelectedTableProperty, value);
        }

        public bool ReadOnly
        {
            get => (bool)this.GetValue(ReadOnlyProperty);
            set => this.SetValue(ReadOnlyProperty, value);
        }

        private static void SourcePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is CremaDataTable dataTable)
            {
                var items = EnumerableUtility.Friends(dataTable, dataTable.Childs);
                d.SetValue(TablesPropertyKey, items.ToArray());
                d.SetValue(SelectedTableProperty, items.First());
            }
            else
            {
                d.ClearValue(TablesPropertyKey);
                d.ClearValue(SelectedTableProperty);
            }
        }
    }
}
