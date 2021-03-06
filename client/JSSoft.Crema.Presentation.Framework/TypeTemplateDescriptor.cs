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
using JSSoft.Crema.Services;
using System;

namespace JSSoft.Crema.Presentation.Framework
{
    public class TypeTemplateDescriptor : DescriptorBase, ITypeTemplateDescriptor
    {
        private readonly ITypeTemplate template;
        private TypeInfo typeInfo = TypeInfo.Default;

        public TypeTemplateDescriptor(Authentication authentication, ITypeTemplate type)
            : this(authentication, type, DescriptorTypes.All)
        {

        }

        public TypeTemplateDescriptor(Authentication authentication, ITypeTemplate template, DescriptorTypes descriptorTypes)
            : this(authentication, template, descriptorTypes, null)
        {

        }

        public TypeTemplateDescriptor(Authentication authentication, ITypeTemplate template, DescriptorTypes descriptorTypes, object owner)
            : base(authentication, template, descriptorTypes)
        {
            this.template = template;
            this.Owner = owner ?? this;
            this.template.Dispatcher.VerifyAccess();
            this.TargetDomain = this.template.Domain;
            this.Editor = this.template.Editor;

            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
            {
                this.template.EditBegun += Template_EditBegun;
                this.template.EditEnded += Template_EditEnded;
                this.template.EditCanceled += Template_EditCanceled;
                this.template.Changed += Template_Changed;
                this.template.Type.TypeInfoChanged += Type_TypeInfoChanged;
            }
        }

        [DescriptorProperty]
        public string Name => this.typeInfo.Name;

        [DescriptorProperty]
        public string Path => this.typeInfo.CategoryPath + this.typeInfo.Name;

        [DescriptorProperty]
        public string DisplayName => this.typeInfo.Name;

        [DescriptorProperty]
        public bool IsModified { get; private set; }

        [DescriptorProperty]
        public IDomain TargetDomain { get; private set; }

        [DescriptorProperty]
        public string Editor { get; private set; } = string.Empty;

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

        protected object Owner { get; }

        private void Template_EditBegun(object sender, EventArgs e)
        {
            this.TargetDomain = this.template.Domain;
            this.Editor = this.template.Editor;
            this.Dispatcher.InvokeAsync(async () =>
            {
                await this.RefreshAsync();
                this.OnEditBegun(e);
            });
        }

        private void Template_EditEnded(object sender, EventArgs e)
        {
            this.TargetDomain = null;
            this.Editor = string.Empty;
            this.Dispatcher.InvokeAsync(async () =>
            {
                await this.RefreshAsync();
                this.OnEditEnded(e);
            });
        }

        private void Template_EditCanceled(object sender, EventArgs e)
        {
            this.TargetDomain = null;
            this.Editor = string.Empty;
            this.Dispatcher.InvokeAsync(async () =>
            {
                await this.RefreshAsync();
                this.OnEditCanceled(e);
            });
        }

        private async void Template_Changed(object sender, EventArgs e)
        {
            this.IsModified = this.template.IsModified;
            await this.RefreshAsync();
        }

        private async void Type_TypeInfoChanged(object sender, EventArgs e)
        {
            this.typeInfo = this.template.Type.TypeInfo;
            await this.RefreshAsync();
        }

        #region ITypeTemplateDescriptor

        ITypeTemplate ITypeTemplateDescriptor.Target => this.template as ITypeTemplate;

        #endregion
    }
}
