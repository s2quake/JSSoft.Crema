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

using Ntreev.Crema.Data.Diff;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.ModernUI.Framework.DataGrid.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid;

namespace Ntreev.Crema.Presentation.Controls
{
    class DiffDataContainer : ModernDataRow
    {
        private static readonly DependencyPropertyKey DiffStatePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DiffState), typeof(DiffState), typeof(DiffDataContainer),
                new UIPropertyMetadata(DiffState.Unchanged));
        public static readonly DependencyProperty DiffStateProperty = DiffStatePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsStartSelectionPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsStartSelection), typeof(bool), typeof(DiffDataContainer),
                new UIPropertyMetadata(false));
        public static readonly DependencyProperty IsStartSelectionProperty = IsStartSelectionPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsEndSelectionPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsEndSelection), typeof(bool), typeof(DiffDataContainer),
                new UIPropertyMetadata(false));
        public static readonly DependencyProperty IsEndSelectionProperty = IsEndSelectionPropertyKey.DependencyProperty;

        private HashSet<string> diffFields;
        private bool updatingError;

        public DiffDataContainer()
        {

        }

        public bool Contains(string fieldName)
        {
            if (this.diffFields == null)
                return false;
            return this.diffFields.Contains(fieldName);
        }

        public DiffState DiffState => (DiffState)this.GetValue(DiffStateProperty);

        public bool IsStartSelection => (bool)this.GetValue(IsStartSelectionProperty);

        public bool IsEndSelection => (bool)this.GetValue(IsEndSelectionProperty);

        public DiffDataContainer DestContainer
        {
            get
            {
                var gridContext = DataGridControl.GetDataGridContext(this);
                var gridControl = gridContext.DataGridControl as DiffDataGridControl;
                var destControl = gridControl.DestControl;
                if (destControl == null)
                    return null;

                if (gridContext.ParentDataGridContext == null)
                {
                    var index = gridContext.Items.IndexOf(this.DataContext);
                    if (index < 0)
                        return null;
                    var destItem = destControl.Items.GetItemAt(index);
                    return destControl.GetContainerFromItem(destItem) as DiffDataContainer;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public object DestDataContext
        {
            get
            {
                var gridContext = DataGridControl.GetDataGridContext(this);
                var gridControl = gridContext.DataGridControl as DiffDataGridControl;
                var destControl = gridControl.DestControl;
                if (destControl == null)
                    return null;

                if (gridContext.ParentDataGridContext == null)
                {
                    var index = gridContext.Items.IndexOf(this.DataContext);
                    if (index < 0)
                        return null;
                    return destControl.Items.GetItemAt(index);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public new DiffDataGridControl GridControl => base.GridControl as DiffDataGridControl;

        protected override Cell CreateCell(ColumnBase column)
        {
            return new DiffDataCell();
        }

        protected override void PrepareContainer(DataGridContext dataGridContext, object item)
        {
            if (this.DataContext is INotifyPropertyChanged oldRow)
            {
                oldRow.PropertyChanged -= DataRowView_PropertyChanged;
            }

            if (item is ICustomTypeDescriptor descriptor)
            {
                this.diffFields = new HashSet<string>(DiffUtility.GetDiffFields(item));
            }
            else
            {
                this.diffFields = null;
            }

            if (item is INotifyPropertyChanged newRow)
            {
                newRow.PropertyChanged += DataRowView_PropertyChanged;
            }

            this.SetValue(DiffStatePropertyKey, DiffUtility.GetDiffState(item));
            this.PropertyChanged -= DiffDataContainer_PropertyChanged;
            dataGridContext.DataGridControl.SelectionChanged -= DataGridControl_SelectionChanged;
            base.PrepareContainer(dataGridContext, item);
            this.SetSelectionProperty();
            dataGridContext.DataGridControl.SelectionChanged += DataGridControl_SelectionChanged;
            this.PropertyChanged += DiffDataContainer_PropertyChanged;
        }

        private void DataGridControl_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(this.SetSelectionProperty, DispatcherPriority.Render);
        }

        private void SetSelectionProperty()
        {
            var gridContext = DataGridControl.GetDataGridContext(this);
            var gridControl = gridContext.DataGridControl;
            var isStartSelection = false;
            var isEndSelection = this.IsSelected;
            var isLastItem = (bool)this.GetValue(Xceed.Wpf.DataGrid.Views.TableView.IsLastItemProperty);

            if (this.IsSelected == true)
            {
                var index = DataGridVirtualizingPanel.GetItemIndex(this);
                if (index == 0)
                    isStartSelection = true;

                {
                    var nextItem = gridContext.GetNextItem(this.DataContext, false);
                    var query = from object item in gridControl.GlobalSelectedItems
                                where item == nextItem
                                select item;

                    isEndSelection = isLastItem || gridContext.AreDetailsExpanded(this.DataContext) || query.Any() == false;
                }
            }
            else if (this.DataContext != null)
            {
                var nextItem = gridContext.GetNextItem(this.DataContext, false);

                if (isLastItem == true && gridContext.ParentDataGridContext != null)
                {
                    var query = from object item in gridContext.ParentDataGridContext.SelectedItems
                                where item == nextItem
                                select item;
                    isEndSelection = query.Any() == true;
                }
                else
                {
                    var query = from object item in gridContext.SelectedItems
                                where item == nextItem
                                select item;
                    isEndSelection = query.Any() == true;
                }
            }

            this.SetValue(IsStartSelectionPropertyKey, isStartSelection);
            this.SetValue(IsEndSelectionPropertyKey, isEndSelection);
        }

        private void DataRowView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.DataContext == null)
                return;

            if (e.PropertyName == DiffUtility.DiffFieldsKey || e.PropertyName == string.Empty)
            {
                this.diffFields = new HashSet<string>(DiffUtility.GetDiffFields(this.DataContext));
            }
            if (e.PropertyName == DiffUtility.DiffStateKey || e.PropertyName == DiffUtility.DiffEnabledKey || e.PropertyName == string.Empty)
            {
                this.SetValue(DiffStatePropertyKey, DiffUtility.GetDiffState(this.DataContext));
            }
            if (e.PropertyName == CremaSchema.Index)
            {
                this.Dispatcher.InvokeAsync(this.SetSelectionProperty);
            }

            if (e.PropertyName == string.Empty)
            {
                if (sender is IDataErrorInfo errorInfo)
                {
                    if (errorInfo.Error == string.Empty && this.HasValidationError == true)
                    {
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            this.EndEditCore();
                        });
                    }
                }
            }
            this.RefreshError();
        }

        private void DiffDataContainer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == HeightProperty.Name)
            {
                if (this.Visibility == Visibility.Collapsed)
                    return;
                var destContainer = this.DestContainer;
                if (destContainer == null || destContainer.Visibility == Visibility.Collapsed)
                    return;
                destContainer.Height = this.Height;
            }
        }

        private void RefreshError()
        {
            if (this.updatingError == true)
                return;
            if (this.updatingError == false)
                this.updatingError = true;
            this.Dispatcher.InvokeAsync(() =>
            {
                this.RefreshDataContextError();
                this.updatingError = false;
            });
        }
    }
}
