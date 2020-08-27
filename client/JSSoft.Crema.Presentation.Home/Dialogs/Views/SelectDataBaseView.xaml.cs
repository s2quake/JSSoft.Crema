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
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid;

namespace Ntreev.Crema.Presentation.Home.Dialogs.Views
{
    /// <summary>
    /// SelectDataBaseView.xaml에 대한 상호 작용 논리
    /// </summary>
    [Export]
    public partial class SelectDataBaseView : UserControl
    {
        private readonly IAppConfiguration configs;

        public SelectDataBaseView()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public SelectDataBaseView(IAppConfiguration configs)
        {
            this.configs = configs;
            InitializeComponent();
            this.Loaded += DataBaseListView_Loaded;
            this.Unloaded += DataBaseListView_Unloaded;
        }

        private void DataBaseListView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.configs.Commit(this);
        }

        private void DataBaseListView_Loaded(object sender, RoutedEventArgs e)
        {
            this.configs.Update(this);
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.OK.IsEnabled == true && e.ChangedButton == MouseButton.Left)
            {
                this.OK.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, this.OK));
            }
        }

        private void GridControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                if (this.gridControl.Tag is bool value)
                {
                    if (this.gridControl.Columns["Color"] is Column column)
                    {
                        column.Visible = value;
                    }
                }
                if (this.Tag is bool supportsDescriptor && supportsDescriptor == false)
                {
                    this.gridControl.Columns["LockInfo"].Visible = false;
                    this.gridControl.Columns["AccessInfo"].Visible = false;
                    this.gridControl.Columns["IsLoaded"].Visible = false;
                }
            }, DispatcherPriority.Background);
        }
    }
}
