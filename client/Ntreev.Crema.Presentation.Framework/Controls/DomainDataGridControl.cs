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

using Ntreev.Crema.Services;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Xml;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.DataGrid.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Export;

namespace Ntreev.Crema.Presentation.Framework.Controls
{
    public class DomainDataGridControl : ModernDataGridControl
    {
        public static DependencyProperty DomainProperty =
            DependencyProperty.Register(nameof(Domain), typeof(IDomain), typeof(DomainDataGridControl),
                new PropertyMetadata(null, DomainPropertyChangedCallback));

        private object currentItem;
        private ColumnBase currentColumn;
        private bool isLocationSending;

        public DomainDataGridControl()
        {
            this.ClipboardExporters[DataFormats.UnicodeText] = new DomainTextClipboardExporter();
        }

        public void DeleteSelectedItems()
        {

        }

        public async void PasteFromClipboard()
        {
            if (Clipboard.ContainsText() == false)
                return;

            var gridContext = DataGridControl.GetDataGridContext(this);
            var gridControl = gridContext.DataGridControl as DomainDataGridControl;

            var parser = new DomainTextClipboardPaster(gridContext);
            parser.Parse(ClipboardUtility.GetData());
            var rowInfos = parser.DomainRows;
            var domain = this.Domain;
            var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
            await domain.SetRowAsync(authenticator, rowInfos);
            parser.SelectRange();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.TryFindResource("BooleanEditor") is CellEditor editor)
            {
                this.DefaultCellEditors[typeof(bool)] = editor;
            }

            var scrollViewer = (ScrollViewer)this.Template.FindName("PART_ScrollViewer", this);
            if (scrollViewer != null)
            {
                scrollViewer.Loaded += ScrollViewer_Loaded;

            }
        }

        public IDomain Domain
        {
            get { return (IDomain)this.GetValue(DomainProperty); }
            set { this.SetValue(DomainProperty, value); }
        }

        public bool CanDeleteSelectedItems
        {
            get
            {
                if (this.ReadOnly == true)
                    return false;

                if (this.IsBeingEdited == true)
                    return false;

                foreach (var gridContext in this.SelectedContexts)
                {
                    foreach (var range in gridContext.SelectedCellRanges)
                    {
                        if (range.ColumnRange.Length != gridContext.VisibleColumns.Count)
                            return false;
                    }
                }
                return true;
            }
        }

        public bool CanResetSelectedFields
        {
            get
            {
                if (this.ReadOnly == true)
                    return false;

                if (this.IsBeingEdited == true)
                    return false;

                foreach (var gridContext in this.SelectedContexts)
                {
                    if (gridContext.HasRectangularSelection() == false || gridContext.GetSelectedColumns().Count() != gridContext.VisibleColumns.Count)
                        return true;
                }
                return false;
            }
        }

        public bool CanPaste
        {
            get
            {
                if (this.IsBeingEdited == true)
                    return false;
                if (this.currentColumn == null)
                    return false;
                if (this.currentColumn.ReadOnly == true)
                    return false;
                if (this.ReadOnly == true)
                    return false;
                if (Clipboard.ContainsText() == false)
                    return false;

                return true;
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is DomainDataRow;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DomainDataRow();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        protected async override void OnCopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                base.OnCopyExecuted(sender, e);
            }
            catch (Exception ex)
            {
                await AppMessageBox.ShowErrorAsync(ex);
            }
        }

        protected async override void OnCopyWithHeadersExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                base.OnCopyWithHeadersExecuted(sender, e);
            }
            catch (Exception ex)
            {
                await AppMessageBox.ShowErrorAsync(ex);
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
        }

        private void DomainDataGridControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var isChanged = false;
            if (e.PropertyName == nameof(DomainDataGridControl.GlobalCurrentColumn))
            {
                isChanged = this.currentColumn != this.GlobalCurrentColumn;
                this.currentColumn = this.GlobalCurrentColumn;
            }
            else if (e.PropertyName == nameof(DomainDataGridControl.GlobalCurrentItem))
            {
                isChanged = this.currentItem != this.GlobalCurrentItem;
                this.currentItem = this.GlobalCurrentItem;
            }
            else
            {
                return;
            }

            if (isChanged == true)
            {
                if (this.isLocationSending == false)
                {
                    this.isLocationSending = true;
                    this.Dispatcher.InvokeAsync(async () =>
                    {
                        var domain = this.Domain;
                        var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
                        if (this.currentItem != null && this.currentColumn != null)
                        {
                            var currentItem = this.currentItem;
                            var fieldName = this.currentColumn.FieldName;
                            try
                            {
                                await domain.SetLocationAsync(authenticator, currentItem, fieldName);
                            }
                            catch { }
                        }
                        else if (this.currentItem == null)
                        {
                            await domain.SetLocationAsync(authenticator);
                        }
                        this.isLocationSending = false;
                    });
                }
            }
        }

        private void DomainDataGridControl_ValueChanging(object sender, ValueChangingEventArgs e)
        {
            e.Cancel = true;
        }

        private async static void DomainPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as DomainDataGridControl;
            var oldDomain = e.OldValue as IDomain;
            var newDomain = e.NewValue as IDomain;
            if (oldDomain != null)
            {
                self.PropertyChanged -= self.DomainDataGridControl_PropertyChanged;
                self.ValueChanging -= self.DomainDataGridControl_ValueChanging;
                oldDomain.Deleted -= self.Domain_Deleted;
            }

            if (newDomain != null)
            {
                self.PropertyChanged += self.DomainDataGridControl_PropertyChanged;
                self.ValueChanging += self.DomainDataGridControl_ValueChanging;
                await newDomain.Dispatcher.InvokeAsync(() => newDomain.Deleted += self.Domain_Deleted);
            }
        }

        private async void Domain_Deleted(object sender, DomainDeletedEventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() => this.Domain = null, DispatcherPriority.Send);
        }

        private IEnumerable<object> CollectItems(DataGridContext gridContext, SelectionRange itemRange)
        {
            var startIndex = Math.Min(itemRange.StartIndex, itemRange.EndIndex);
            var endIndex = Math.Max(itemRange.StartIndex, itemRange.EndIndex);
            for (var i = startIndex; i <= endIndex; i++)
            {
                yield return gridContext.Items.GetItemAt(i);
            }
        }

        private IEnumerable<string> CollectFields(DataGridContext gridContext, object item, SelectionRange columnRange)
        {
            var startIndex = Math.Min(columnRange.StartIndex, columnRange.EndIndex);
            var endIndex = Math.Max(columnRange.StartIndex, columnRange.EndIndex);
            for (var i = startIndex; i <= endIndex; i++)
            {
                foreach (var column in gridContext.Columns)
                {
                    if (column.VisiblePosition == i)
                        yield return column.FieldName;
                }
            }
        }

        private async Task RequestRemoveRowsAsync(object[] items)
        {
            try
            {
                var domain = this.Domain;
                var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
                await domain.RemoveRowsAsync(authenticator, items);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        private async void RequestResetFields(DomainRowInfo[] rows)
        {
            try
            {
                var domain = this.Domain;
                var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
                domain.Dispatcher.Invoke(() => domain.SetRowAsync(authenticator, rows));
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer.Template.FindName("PART_ScrollContentPresenter", scrollViewer) is ScrollContentPresenter contentPresenter)
            {
                contentPresenter.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, this.DeleteRows_Execute, this.DeleteRows_CanExecute));
                contentPresenter.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, this.ResetFields_Execute, this.ResetFields_CanExecute));
                contentPresenter.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, this.PasteFromClipboard_Execute, this.PasteFromClipboard_CanExecute));
            }
        }

        private void DeleteRows_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanDeleteSelectedItems;
        }

        private async void DeleteRows_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var query = from gridContext in this.SelectedContexts
                        from range in gridContext.SelectedCellRanges
                        from item in this.CollectItems(gridContext, range.ItemRange)
                        select item;

            if (await AppMessageBox.ShowQuestion(Properties.Resources.Message_ConfirmToDeleteSelectedRows_Format, query.Count()) == false)
                return;

            await this.RequestRemoveRowsAsync(query.ToArray());
        }

        private void ResetFields_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanResetSelectedFields;
        }

        private async void ResetFields_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (await AppMessageBox.ShowQuestion(Properties.Resources.Message_ConfirmToResetSelectedFields) == false)
                return;

            var query = from gridContext in this.SelectedContexts
                        from range in gridContext.SelectedCellRanges
                        from item in this.CollectItems(gridContext, range.ItemRange)
                        from column in this.CollectFields(gridContext, item, range.ColumnRange)
                        group column by item into fields
                        select new DomainRowInfo()
                        {
                            Keys = CremaDataRowUtility.GetKeys(fields.Key),
                            TableName = CremaDataRowUtility.GetTableName(fields.Key),
                            Fields = CremaDataRowUtility.GetResetFields(fields.Key, fields.ToArray()),
                        };

            this.RequestResetFields(query.ToArray());
        }

        private void PasteFromClipboard_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanPaste;
        }

        private void PasteFromClipboard_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            this.RequestPasteFromClipboard();
        }

        private async Task RequestPasteFromClipboard()
        {
            try
            {
                this.PasteFromClipboard();
            }
            catch (Exception ex)
            {
                await AppMessageBox.ShowErrorAsync(ex);
            }
        }
    }
}