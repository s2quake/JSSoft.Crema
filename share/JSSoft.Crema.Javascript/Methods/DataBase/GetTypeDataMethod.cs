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
using JSSoft.Crema.Services.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript.Methods.DataBase
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(DataBase))]
    class GetTypeDataMethod : ScriptFuncTaskBase<string, string, string, IDictionary<int, object>>
    {
        [ImportingConstructor]
        public GetTypeDataMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override async Task<IDictionary<int, object>> OnExecuteAsync(string dataBaseName, string typeName, string revision)
        {
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            var type = await dataBase.GetTypeAsync(typeName);
            var revisionValue = revision;
            var authentication = this.Context.GetAuthentication(this);
            var dataSet = await type.GetDataSetAsync(authentication, revisionValue);
            var dataType = dataSet.Types[typeName];
            return this.GetTypeMembers(dataType);
        }

        private IDictionary<int, object> GetTypeMembers(CremaDataType dataType)
        {
            var props = new Dictionary<int, object>();
            for (var i = 0; i < dataType.Members.Count; i++)
            {
                var typeMember = dataType.Members[i];
                props.Add(i, this.GetTypeMember(typeMember));
            }
            return props;
        }

        private IDictionary<string, object> GetTypeMember(CremaDataTypeMember typeMember)
        {
            var props = new Dictionary<string, object>
            {
                { nameof(typeMember.Name), typeMember.Name },
                { nameof(typeMember.Value), typeMember.Value },
                { nameof(typeMember.Comment), typeMember.Comment }
            };
            return props;
        }
    }
}
