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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Framework
{
    public class TableContentListItemBase : DescriptorListItemBase<TableContentDescriptor>, ITableContentDescriptor
    {
        //private IDomain domain;

        public TableContentListItemBase(Authentication authentication, ITableContent content, object owner)
            : base(authentication, new TableContentDescriptor(authentication, content, DescriptorTypes.IsSubscriptable, owner), owner)
        {

        }

        //public TableContentListItemBase(Authentication authentication, ITableContentDescriptor descriptor, object owner)
        //    : base(authentication, new TableContentDescriptor(authentication, descriptor, DescriptorTypes.IsSubscriptable, owner), owner)
        //{

        //}

        public TableContentListItemBase(Authentication authentication, TableContentDescriptor descriptor, object owner)
            : base(authentication, descriptor, owner)
        {

        }

        public async Task BeginEditAsync()
        {
            await TableContentDescriptorUtility.BeginEditAsync(this.authentication, this.descriptor);
        }

        public string Name => this.descriptor.Name;

        public string TableName => this.descriptor.TableName;

        public string Path => this.descriptor.Path;

        public override string DisplayName => this.descriptor.Name;

        public bool IsModified => this.descriptor.IsModified;

        public DomainAccessType AccessType => this.descriptor.AccessType;

        public IDomain TargetDomain => this.descriptor.TargetDomain;

        #region ITableContentDescriptor

        ITableContent ITableContentDescriptor.Target => this.descriptor.Target as ITableContent;

        #endregion
    }
}
