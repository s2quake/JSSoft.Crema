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
using JSSoft.Crema.ServiceHosts.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Domains;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    class NewTableTemplate : TableTemplateBase
    {
        private Table[] tables;

        public NewTableTemplate(TableCategory category)
        {
            this.Parent = category ?? throw new ArgumentNullException(nameof(category));
            this.DispatcherObject = category;
            this.DomainContext = category.GetService(typeof(DomainContext)) as DomainContext;
            this.ItemPath = category.Path;
            this.CremaHost = category.CremaHost;
            this.DataBase = category.DataBase;
            this.Permission = category;
            this.IsNew = true;
            this.Container = category.GetService(typeof(TableCollection)) as TableCollection;
            this.Service = category.Service;
        }

        public NewTableTemplate(Table parent)
        {
            this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.DispatcherObject = parent;
            this.DomainContext = parent.GetService(typeof(DomainContext)) as DomainContext;
            this.ItemPath = parent.Path;
            this.CremaHost = parent.CremaHost;
            this.DataBase = parent.DataBase;
            this.Permission = parent;
            this.IsNew = true;
            this.Container = parent.GetService(typeof(TableCollection)) as TableCollection;
            this.Service = parent.Service;
        }

        public override AccessType GetAccessType(Authentication authentication)
        {
            return this.Permission.GetAccessType(authentication);
        }

        public override object Target => this.tables;

        public override DomainContext DomainContext { get; }

        public override string ItemPath { get; }

        public override CremaHost CremaHost { get; }

        public override DataBase DataBase { get; }

        public override IPermission Permission { get; }

        public override IDispatcherObject DispatcherObject { get; }

        public IDataBaseService Service { get; }

        protected override async Task OnBeginEditAsync(Authentication authentication)
        {
            await base.OnBeginEditAsync(authentication);
        }

        protected override async Task OnEndEditAsync(Authentication authentication)
        {
            var domain = this.Domain;
            var taskID = domain.ID;
            await base.OnEndEditAsync(authentication);
            await this.DataBase.WaitAsync(taskID);
            var tableInfos = domain.Result as TableInfo[];
            this.tables = await this.Dispatcher.InvokeAsync(() => tableInfos.Select(item => this.Container[item.Name]).ToArray());
            this.Parent = null;
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await base.OnCancelEditAsync(authentication);
            this.Parent = null;
        }

        protected override Task<ResultBase<DomainMetaData>> OnBeginDomainAsync(Authentication authentication)
        {
            return this.Service.BeginNewTableAsync(authentication.Token, this.ItemPath);
        }

        protected override async Task<ResultBase<TableInfo[]>> OnEndDomainAsync(Authentication authentication)
        {
            return await this.Service.EndTableTemplateEditAsync(authentication.Token, this.Domain.ID);
        }

        protected override async Task<ResultBase> OnCancelDomainAsync(Authentication authentication)
        {
            return await this.Service.CancelTableTemplateEditAsync(authentication.Token, this.Domain.ID);
        }

        protected object Parent { get; private set; }

        private TableCollection Container { get; }
    }
}
