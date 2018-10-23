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

using Ntreev.Crema.Client.Framework.Controls;
using Ntreev.Crema.Services;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Xml;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.DataGrid.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid;

namespace Ntreev.Crema.Client.Framework.Controls
{
    public class DomainDataCell : ModernDataCell
    {
        public static readonly DependencyProperty UserIDProperty =
            DependencyProperty.Register("UserID", typeof(string), typeof(DomainDataCell));

        public static readonly DependencyProperty UserBrushProperty =
            DependencyProperty.Register(nameof(UserBrush), typeof(Brush), typeof(DomainDataCell));

        public static readonly DependencyProperty HasUserProperty =
            DependencyProperty.Register(nameof(HasUser), typeof(bool), typeof(DomainDataCell));

        public static readonly DependencyProperty IsUserEditingProperty =
            DependencyProperty.Register(nameof(IsUserEditing), typeof(bool), typeof(DomainDataCell));

        public static readonly DependencyProperty IsClientAloneProperty =
            DependencyProperty.Register(nameof(IsClientAlone), typeof(bool), typeof(DomainDataCell));

        private readonly DomainDataUserCollection users = new DomainDataUserCollection();

        private DomainDataRow parentRow;

        public DomainDataCell()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, this.Reset_Execute, this.Reset_CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, this.PasteFromClipboard_Execute, this.PasteFromClipboard_CanExecute));
            this.users.CollectionChanged += Users_CollectionChanged;
        }

        public Task ResetAsync()
        {
            var domain = this.GridControl.Domain;
            var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
            var item = this.DataContext;
            var fieldName = this.FieldName;
            return domain.SetRowAsync(authenticator, item, fieldName, DBNull.Value);
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
            var domain = gridControl.Domain;
            var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
            await domain.SetRowAsync(authenticator, rowInfos);
            parser.SelectRange();
        }

        public IEnumerable Users
        {
            get { return this.users; }
        }

        public Brush UserBrush
        {
            get { return (Brush)GetValue(UserBrushProperty); }
            private set { SetValue(UserBrushProperty, value); }
        }

        public bool HasUser
        {
            get { return (bool)GetValue(HasUserProperty); }
            private set { SetValue(HasUserProperty, value); }
        }

        public bool IsUserEditing
        {
            get { return (bool)GetValue(IsUserEditingProperty); }
            private set { SetValue(IsUserEditingProperty, value); }
        }

        public bool IsClientAlone
        {
            get { return (bool)GetValue(IsClientAloneProperty); }
            private set { SetValue(IsClientAloneProperty, value); }
        }

        public bool CanReset
        {
            get
            {
                if (this.ReadOnly == true)
                    return false;

                if (this.IsBeingEdited == true)
                    return false;

                return this.IsSingleSelection;
            }
        }

        public bool CanPaste
        {
            get
            {
                if (this.IsBeingEdited == true)
                    return false;
                if (this.ReadOnly == true)
                    return false;
                if (Clipboard.ContainsText() == false)
                    return false;

                return this.IsSingleSelection;
            }
        }

        public new DomainDataGridControl GridControl
        {
            get { return (DomainDataGridControl)base.GridControl; }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }

        protected override async void OnEditBegun()
        {
            base.OnEditBegun();
            await this.RequestBeginEditAsync();
        }

        protected override async void OnEditEnded()
        {
            base.OnEditEnded();
            var parentRow = this.ParentRow as DomainDataRow;
            if (parentRow.IsBeginEnding == false)
                parentRow.EndEdit();
            await this.RequestEditEndAsync();
        }

        protected override async void OnEditEnding(CancelRoutedEventArgs e)
        {
            var editingContent = this.EditingContent;
            var content = this.Content;
            base.OnEditEnding(e);
            if (editingContent != content)
            {
                await this.RequestSetRowAsync(editingContent);
            }
        }

        protected override async void OnEditCanceled()
        {
            base.OnEditCanceled();
            var parentRow = this.ParentRow as DomainDataRow;
            if (parentRow.IsBeginEnding == false)
                this.ParentRow.CancelEdit();
            this.Focus();
            await this.RequestEditEndAsync();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        protected override void InitializeCore(DataGridContext dataGridContext, Row parentRow, ColumnBase parentColumn)
        {
            base.InitializeCore(dataGridContext, parentRow, parentColumn);

            if (this.parentRow != null)
                this.parentRow.UserInfos.CollectionChanged -= UserInfos_CollectionChanged;

            this.parentRow = parentRow as DomainDataRow;

            if (this.parentRow != null)
                this.parentRow.UserInfos.CollectionChanged += UserInfos_CollectionChanged;
        }

        protected override void PrepareContainer(DataGridContext dataGridContext, object item)
        {
            base.PrepareContainer(dataGridContext, item);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            if (this.IsBeingEdited == true && e.NewFocus != null)
            {
                var gridContext = DataGridControl.GetDataGridContext(e.NewFocus as DependencyObject);
                if (gridContext == null)
                {
                    this.EndEdit();
                }
            }
        }

        private void UserInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.users.Clear();
            }
            else
            {
                if (e.NewItems != null)
                {
                    foreach (DomainDataUser item in e.NewItems)
                    {
                        if (item.Location.ColumnName == this.FieldName)
                        {
                            if (item.UserID == this.parentRow.UserID)
                                this.users.Insert(0, item);
                            else
                                this.users.Add(item);
                        }
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (DomainDataUser item in e.OldItems)
                    {
                        if (item.Location.ColumnName == this.FieldName)
                        {
                            this.users.Remove(item.UserID);
                        }
                    }
                }
            }
        }

        private void Users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                if (this.users.Any() == true)
                {
                    var domainUser = this.users.First();
                    this.UserBrush = domainUser.Background;
                    this.HasUser = true;
                    this.IsUserEditing = domainUser.UserID == this.parentRow.UserID ? false : domainUser.IsBeingEdited;
                    if (this.users.Count == 1 && domainUser.UserID == this.parentRow.UserID)
                        this.IsClientAlone = true;
                    else
                        this.IsClientAlone = false;
                }
                else
                {
                    this.UserBrush = null;
                    this.HasUser = false;
                    this.IsUserEditing = false;
                    this.IsClientAlone = false;
                }
            });
        }

        private void PasteFromClipboard_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanPaste;
        }

        private void PasteFromClipboard_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            this.RequestPasteFromClipboard();
        }

        private void Reset_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanReset;
        }

        private void Reset_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (AppMessageBox.ShowQuestion(Properties.Resources.Message_ConfirmToResetField) == false)
                return;

            this.RequestReset();
        }

        private async Task RequestBeginEditAsync()
        {
            try
            {
                var domain = this.GridControl.Domain;
                if (domain != null)
                {
                    var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
                    var item = this.DataContext;
                    var fieldName = this.FieldName;
                    await domain.BeginEditAsync(authenticator, item, fieldName);
                }
            }
            catch (Exception e)
            {
                AppMessageBox.ShowError(e);
            }
        }

        private async Task RequestEditEndAsync()
        {
            try
            {
                var domain = this.GridControl.Domain;
                if (domain == null)
                    return;

                var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
                if (domain != null)
                    await domain.EndUserEditAsync(authenticator);
            }
            catch (Exception e)
            {
                AppMessageBox.ShowError(e);
            }
        }

        private async void RequestReset()
        {
            try
            {
                await this.ResetAsync();
            }
            catch (Exception ex)
            {
                AppMessageBox.ShowError(ex);
            }
        }

        private void RequestPasteFromClipboard()
        {
            try
            {
                this.PasteFromClipboard();
            }
            catch (Exception ex)
            {
                AppMessageBox.ShowError(ex);
            }
        }

        private async Task RequestSetRowAsync(object value)
        {
            try
            {
                var domain = this.GridControl.Domain;
                var item = this.DataContext;
                var fieldName = this.FieldName;
                var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;

                await domain.SetRowAsync(authenticator, item, fieldName, value);
            }
            catch (Exception e)
            {
                AppMessageBox.ShowError(e);
            }
        }

        private bool IsSingleSelection
        {
            get
            {
                if (this.GridControl.SelectedContexts.Count != 1)
                    return false;

                var itemIndex = this.GridContext.Items.IndexOf(this.DataContext);
                var columnIndex = this.GridContext.VisibleColumns.IndexOf(this.ParentColumn);

                foreach (var item in this.GridContext.SelectedCellRanges)
                {
                    if (item.ColumnRange.Length != 1)
                        return false;
                    if (item.ItemRange.Length != 1)
                        return false;
                    if (item.ColumnRange.StartIndex != columnIndex)
                        return false;
                    if (item.ItemRange.StartIndex != itemIndex)
                        return false;
                }
                return true;
            }
        }
    }
}
