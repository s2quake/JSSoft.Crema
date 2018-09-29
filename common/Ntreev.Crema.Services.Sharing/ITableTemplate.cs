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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ntreev.Library;

namespace Ntreev.Crema.Services
{
    public interface ITableTemplate : IEnumerable<ITableColumn>, IDispatcherObject, IPermission
    {
        Task BeginEditAsync(Authentication authentication);

        Task EndEditAsync(Authentication authentication);

        Task CancelEditAsync(Authentication authentication);

        Task SetTableNameAsync(Authentication authentication, string value);

        Task SetTagsAsync(Authentication authentication, TagInfo tags);

        Task SetCommentAsync(Authentication authentication, string value);

        Task<ITableColumn> AddNewAsync(Authentication authentication);

        Task EndNewAsync(Authentication authentication, ITableColumn column);

        Task<bool> ContainsAsync(string columnName);

        IDomain Domain { get; }

        object Target { get; }

        int Count { get; }

        ITableColumn this[string columnName] { get; }

        string[] SelectableTypes { get; }

        IEnumerable<ITableColumn> PrimaryKey { get; }

        string TableName { get; }

        TagInfo Tags { get; }

        string Comment { get; }

        bool IsNew { get; }

        bool IsModified { get; }

        ServiceState ServiceState { get; }

        event EventHandler EditBegun;

        event EventHandler EditEnded;

        event EventHandler EditCanceled;

        event EventHandler Changed;
    }
}
