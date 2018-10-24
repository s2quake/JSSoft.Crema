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
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.ComponentModel;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.Data;
using System.Threading.Tasks;
using Ntreev.Crema.Services.Extensions;

namespace Ntreev.Crema.Javascript.Methods.DataBase
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(DataBase))]
    class GetTypeInfoMethod : ScriptFuncTaskBase<string, string, IDictionary<string, object>>
    {
        [ImportingConstructor]
        public GetTypeInfoMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override async Task<IDictionary<string, object>> OnExecuteAsync(string dataBaseName, string typeName)
        {
            var type = await this.CremaHost.GetTypeAsync(dataBaseName, typeName);
            return await type.Dispatcher.InvokeAsync(() =>
            {
                var typeInfo = type.TypeInfo;
                var props = new Dictionary<string, object>
                {
                    { nameof(typeInfo.ID), typeInfo.ID },
                    { nameof(typeInfo.Name), typeInfo.Name },
                    { nameof(typeInfo.Comment), typeInfo.Comment },
                    { nameof(typeInfo.Tags), $"{typeInfo.Tags}" },
                    { nameof(typeInfo.IsFlag), typeInfo.IsFlag },
                    { nameof(typeInfo.CategoryPath), typeInfo.CategoryPath },
                    { nameof(typeInfo.HashValue), typeInfo.HashValue },
                    { CremaSchema.Creator, typeInfo.CreationInfo.ID },
                    { CremaSchema.CreatedDateTime, typeInfo.CreationInfo.DateTime },
                    { CremaSchema.Modifier, typeInfo.ModificationInfo.ID },
                    { CremaSchema.ModifiedDateTime, typeInfo.ModificationInfo.DateTime },
                    { nameof(typeInfo.Members), this.GetMembersInfo(typeInfo.Members) }
                };

                return props;
            });
        }

        private object[] GetMembersInfo(TypeMemberInfo[] members)
        {
            var props = new object[members.Length];
            for (var i = 0; i < members.Length; i++)
            {
                props[i] = this.GetMemberInfo(members[i]);
            }
            return props;
        }

        private IDictionary<string, object> GetMemberInfo(TypeMemberInfo memberInfo)
        {
            var props = new Dictionary<string, object>
            {
                { nameof(memberInfo.ID), memberInfo.ID },
                { nameof(memberInfo.Name), memberInfo.Name },
                { nameof(memberInfo.Value), memberInfo.Value },
                { nameof(memberInfo.Comment), memberInfo.Comment },
                { nameof(memberInfo.Tags), $"{memberInfo.Tags}" },
                { nameof(memberInfo.DerivedTags), $"{memberInfo.DerivedTags}" },
                { nameof(memberInfo.IsEnabled), memberInfo.IsEnabled },
                { CremaSchema.Creator, memberInfo.CreationInfo.ID },
                { CremaSchema.CreatedDateTime, memberInfo.CreationInfo.DateTime },
                { CremaSchema.Modifier, memberInfo.ModificationInfo.ID },
                { CremaSchema.ModifiedDateTime, memberInfo.ModificationInfo.DateTime }
            };
            return props;
        }
    }
}
