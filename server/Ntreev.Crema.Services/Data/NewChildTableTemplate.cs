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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using System;

namespace Ntreev.Crema.Services.Data
{
    class NewChildTableTemplate : TableTemplateBase
    {
        private Table parent;
        private Table table;

        public NewChildTableTemplate(Table parent)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.parent.Attach(this);
            this.IsNew = true;
        }

        public override void OnValidateBeginEdit(Authentication authentication, object target)
        {
            base.OnValidateBeginEdit(authentication, target);
            if (this.parent == null)
                throw new InvalidOperationException(Resources.Exception_Expired);
            if (this.Domain != null)
                throw new InvalidOperationException(Resources.Exception_ItIsAlreadyBeingEdited);
            this.parent.ValidateAccessType(authentication, AccessType.Master);
        }

        public override void OnValidateEndEdit(Authentication authentication, object target)
        {
            base.OnValidateEndEdit(authentication, target);
            this.parent.ValidateAccessType(authentication, AccessType.Master);
            this.TemplateSource.Validate();
        }

        public override void OnValidateCancelEdit(Authentication authentication, object target)
        {
            base.OnValidateCancelEdit(authentication, target);
            this.parent.ValidateAccessType(authentication, AccessType.Master);
        }

        public override ITable Table
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.table;
            }
        }

        public override DomainContext DomainContext => this.parent.GetService(typeof(DomainContext)) as DomainContext;

        public override string ItemPath => this.parent.Path;

        public override CremaHost CremaHost => this.parent.CremaHost;

        public override CremaDispatcher Dispatcher => this.parent?.Dispatcher;

        public override DataBase DataBase => this.parent.DataBase;

        public override IPermission Permission => this.parent;

        protected override void OnBeginEdit(Authentication authentication)
        {
            base.OnBeginEdit(authentication);
        }

        protected override void OnEndEdit(Authentication authentication, CremaTemplate template)
        {
            base.OnEndEdit(authentication, template);
            this.table = this.parent.AddNew(authentication, template);
            this.parent = null;
        }

        protected override void OnCancelEdit(Authentication authentication)
        {
            base.OnCancelEdit(authentication);
            this.parent = null;
        }

        protected override CremaTemplate CreateSource(Authentication authentication)
        {
            var dataSet = this.parent.ReadAll(authentication, true);
            var dataTable = dataSet.Tables[this.parent.Name, this.parent.Category.Path];
            return CremaTemplate.Create(dataTable);
        }
    }
}
