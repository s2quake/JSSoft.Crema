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

using System;
using System.Linq;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Threading;
using Ntreev.Crema.Services;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.ServiceModel;
using System.Threading.Tasks;
using Ntreev.Library.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Ntreev.ModernUI.Framework;
using Ntreev.Crema.Data;
using Ntreev.Crema.Presentation.Framework.Dialogs.ViewModels;
using Ntreev.Library.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using Ntreev.ModernUI.Framework.ViewModels;
using System.Collections;
using Ntreev.Library;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Ntreev.Crema.Presentation.Framework
{
    public class TableTemplateDescriptor : DescriptorBase, ITableTemplateDescriptor
    {
        private ITableTemplate template;
        private readonly object owner;

        private IDomain domain;
        private TableInfo tableInfo = TableInfo.Default;
        private bool isModified;

        public TableTemplateDescriptor(Authentication authentication, ITableTemplate table)
            : this(authentication, table, DescriptorTypes.All)
        {

        }

        public TableTemplateDescriptor(Authentication authentication, ITableTemplate template, DescriptorTypes descriptorTypes)
            : this(authentication, template, descriptorTypes, null)
        {

        }

        public TableTemplateDescriptor(Authentication authentication, ITableTemplate template, DescriptorTypes descriptorTypes, object owner)
            : base(authentication, template, descriptorTypes)
        {
            this.template = template;
            this.owner = owner ?? this;
            this.template.Dispatcher.VerifyAccess();
            this.domain = this.template.Domain;

            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
            {
                this.template.EditBegun += Template_EditBegun;
                this.template.EditEnded += Template_EditEnded;
                this.template.EditCanceled += Template_EditCanceled;
                this.template.Changed += Template_Changed;
                if (this.template.Target is ITable table)
                {
                    table.TableInfoChanged += Table_TableInfoChanged;
                }
            }
        }

        [DescriptorProperty]
        public string Name => this.tableInfo.Name;

        [DescriptorProperty]
        public string TableName => this.tableInfo.TableName;

        [DescriptorProperty]
        public string Path => this.tableInfo.CategoryPath + this.tableInfo.Name;

        [DescriptorProperty]
        public string DisplayName => this.tableInfo.Name;

        [DescriptorProperty]
        public bool IsModified => this.isModified;

        [DescriptorProperty]
        public IDomain TargetDomain => this.domain;

        public event EventHandler EditBegun;

        public event EventHandler EditEnded;

        public event EventHandler EditCanceled;

        protected virtual void OnEditBegun(EventArgs e)
        {
            this.EditBegun?.Invoke(this, e);
        }

        protected virtual void OnEditEnded(EventArgs e)
        {
            this.EditEnded?.Invoke(this, e);
        }

        protected virtual void OnEditCanceled(EventArgs e)
        {
            this.EditCanceled?.Invoke(this, e);
        }

        private void Template_EditBegun(object sender, EventArgs e)
        {
            this.domain = this.template.Domain;
            this.Dispatcher.InvokeAsync(async () =>
            {
                await this.RefreshAsync();
                this.OnEditBegun(e);
            });
        }

        private void Template_EditEnded(object sender, EventArgs e)
        {
            this.domain = null;
            this.Dispatcher.InvokeAsync(async () =>
            {
                await this.RefreshAsync();
                this.OnEditEnded(e);
            });
        }

        private void Template_EditCanceled(object sender, EventArgs e)
        {
            this.domain = null;
            this.Dispatcher.InvokeAsync(async () =>
            {
                await this.RefreshAsync();
                this.OnEditCanceled(e);
            });
        }

        private async void Template_Changed(object sender, EventArgs e)
        {
            this.isModified = this.template.IsModified;
            await this.RefreshAsync();
        }

        private async void Table_TableInfoChanged(object sender, EventArgs e)
        {
            if (this.template.Target is ITable table)
            {
                this.tableInfo = table.TableInfo;
                await this.RefreshAsync();
            }
        }

        #region ITableTemplateDescriptor

        ITableTemplate ITableTemplateDescriptor.Target => this.template as ITableTemplate;

        #endregion
    }
}
