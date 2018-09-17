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
using Ntreev.Library.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class TableTemplate : TableTemplateBase
    {
        private readonly Table table;

        public TableTemplate(Table table)
        {
            this.table = table;
        }

        public override object Target => this.table;

        public override DomainContext DomainContext => this.table.GetService(typeof(DomainContext)) as DomainContext;

        public override string ItemPath => this.table.Path;

        public override CremaHost CremaHost => this.table.CremaHost;

        public override DataBase DataBase => this.table.DataBase;

        public override IDispatcherObject DispatcherObject => this.table;

        public override IPermission Permission => this.table;

        protected override async Task OnBeginEditAsync(Authentication authentication)
        {
            await base.OnBeginEditAsync(authentication);
            this.table.SetTableState(TableState.IsBeingSetup);
            this.Container.InvokeTablesStateChangedEvent(authentication, new Table[] { this.table });
        }

        protected override async Task OnEndEditAsync(Authentication authentication)
        {
            await this.Container.InvokeTableEndTemplateEditAsync(authentication, this.table, this.TemplateSource);
            await base.OnEndEditAsync(authentication);
            this.table.UpdateTemplate(this.TemplateSource.TableInfo);
            this.table.UpdateTags(this.TemplateSource.Tags);
            this.table.UpdateComment(this.TemplateSource.Comment);
            this.table.SetTableState(TableState.None);

            var items = EnumerableUtility.One(this.table).ToArray();
            this.Container.InvokeTablesStateChangedEvent(authentication, items);
            this.Container.InvokeTablesTemplateChangedEvent(authentication, items, this.TemplateSource.TargetTable.DataSet);
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await base.OnCancelEditAsync(authentication);
            this.table.SetTableState(TableState.None);
            this.Container.InvokeTablesStateChangedEvent(authentication, new Table[] { this.table });
        }

        protected override void OnRestore(Domain domain)
        {
            this.table.SetTableState(TableState.IsBeingSetup);
            base.OnRestore(domain);
        }

        protected override async Task<CremaTemplate> CreateSourceAsync(Authentication authentication)
        {
            var dataSet = await this.table.ReadAllAsync(authentication, true);
            return new CremaTemplate(dataSet.Tables[this.table.Name, this.table.Category.Path]);
        }

        private TableCollection Container => this.table.Container;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateBeginEdit(Authentication authentication, object target)
        {
            base.OnValidateBeginEdit(authentication, target);

            if (target == this && this.table.TemplatedParent != null)
                throw new InvalidOperationException(Resources.Exception_InheritedTableCannotEditTemplate);

            this.table.ValidateAccessType(authentication, AccessType.Master);
            this.table.ValidateNotBeingEdited();
            this.table.ValidateHasNotBeingEditedType();

            var templates = this.table.Childs.Select(item => item.Template);
            foreach (var item in templates)
            {
                item.OnValidateBeginEdit(authentication, target);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateEndEdit(Authentication authentication, object target)
        {
            base.OnValidateEndEdit(authentication, target);

            if (target == this)
            {
                if (this.IsBeingEdited == false)
                    throw new InvalidOperationException(Resources.Exception_TableTemplateIsNotBeingEdited);
                if (this.table.TemplatedParent != null)
                    throw new InvalidOperationException(Resources.Exception_InheritedTableTemplateCannotEdit);
                this.TemplateSource.Validate();
            }

            this.table.ValidateAccessType(authentication, AccessType.Master);

            var templates = this.table.Childs.Select(item => item.Template);
            foreach (var item in templates)
            {
                item.OnValidateEndEdit(authentication, target);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateCancelEdit(Authentication authentication, object target)
        {
            base.OnValidateCancelEdit(authentication, target);

            if (target == this && this.IsBeingEdited == false)
            {
                throw new InvalidOperationException(Resources.Exception_TableTemplateIsNotBeingEdited);
            }

            this.table.ValidateAccessType(authentication, AccessType.Master);

            var templates = this.table.Childs.Select(item => item.Template);
            foreach (var item in templates)
            {
                item.OnValidateCancelEdit(authentication, target);
            }
        }
    }
}
