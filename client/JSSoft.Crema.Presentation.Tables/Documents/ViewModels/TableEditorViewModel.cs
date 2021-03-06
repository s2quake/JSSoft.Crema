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
using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Tables.Properties;
using JSSoft.Crema.Services;
using JSSoft.Library.Linq;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Tables.Documents.ViewModels
{
    class TableEditorViewModel : TableDocumentBase
    {
        private readonly Authentication authentication;
        private readonly TableDescriptor descriptor;
        private readonly TableContentDescriptor contentDescriptor;
        [Import]
        private readonly IFlashService flashServie = null;

        public TableEditorViewModel(Authentication authentication, TableDescriptor descriptor)
        {
            this.authentication = authentication;
            this.authentication.Expired += (s, e) => this.Dispatcher.InvokeAsync(() => this.Tables.Clear());
            this.descriptor = descriptor;
            this.contentDescriptor = descriptor.ContentDescriptor;
            this.AttachEvent();
            this.DisplayName = descriptor.DisplayName;
            this.Target = descriptor.Target;
            foreach (var item in EnumerableUtility.FamilyTree(this.descriptor, item => item.Childs))
            {
                this.Tables.Add(new TableItemViewModel(this.authentication, item, this));
            }
            this.Initialize();
        }

        protected override async Task<bool> CloseAsync()
        {
            if (this.Tables.Any() == true)
            {
                try
                {
                    await TableContentDescriptorUtility.EndEditAsync(this.authentication, this.contentDescriptor);
                    this.DetachEvent();
                }
                catch (Exception e)
                {
                    await AppMessageBox.ShowErrorAsync(e);
                }
            }
            return true;
        }

        private async void ContentDescriptor_EditEnded(object sender, EventArgs e)
        {
            this.DetachEvent();
            this.Tables.Clear();
            if (e is DomainDeletedEventArgs ex)
            {
                this.flashServie?.Flash();
                await AppMessageBox.ShowInfoAsync("'{0}'에 의해서 편집이 종료되었습니다.", ex.UserID);
            }
            await this.TryCloseAsync();
        }

        private async void ContentDescriptor_EditCanceled(object sender, EventArgs e)
        {
            this.DetachEvent();
            this.Tables.Clear();
            if (e is DomainDeletedEventArgs ex)
            {
                this.flashServie?.Flash();
                await AppMessageBox.ShowInfoAsync("'{0}'에 의해서 편집이 취소되었습니다.", ex.UserID);
            }
            await this.TryCloseAsync();
        }

        private async void ContentDescriptor_Kicked(object sender, EventArgs e)
        {
            this.DetachEvent();
            this.Tables.Clear();
            this.flashServie?.Flash();
            if (e is DomainUserRemovedEventArgs ex)
            {
                await AppMessageBox.ShowInfoAsync(ex.RemoveInfo.Message, "추방되었습니다.");
            }
            await this.TryCloseAsync();
        }

        private async void Initialize()
        {
            try
            {
                this.BeginProgress(Resources.Message_LoadingData);
                await TableContentDescriptorUtility.BeginEditAsync(this.authentication, this.contentDescriptor);
                if (this.contentDescriptor.TargetDomain is IDomain domain && domain.Source is CremaDataSet dataSet)
                {
                    foreach (var item in this.Tables)
                    {
                        item.Source = dataSet.Tables[item.Name];
                        item.Domain = domain;
                    }
                }
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
                this.EndProgress();
                this.DetachEvent();
                this.Tables.Clear();
                await this.TryCloseAsync();
                return;
            }

            this.EndProgress();
            this.NotifyOfPropertyChange(nameof(this.Tables));
            this.NotifyOfPropertyChange(nameof(this.SelectedTable));
            this.NotifyOfPropertyChange(nameof(this.IsProgressing));
        }

        private void AttachEvent()
        {
            this.contentDescriptor.EditEnded += ContentDescriptor_EditEnded;
            this.contentDescriptor.EditCanceled += ContentDescriptor_EditCanceled;
            this.contentDescriptor.Kicked += ContentDescriptor_Kicked;
        }

        private void DetachEvent()
        {
            this.contentDescriptor.EditEnded -= ContentDescriptor_EditEnded;
            this.contentDescriptor.EditCanceled -= ContentDescriptor_EditCanceled;
            this.contentDescriptor.Kicked -= ContentDescriptor_Kicked;
        }
    }
}
