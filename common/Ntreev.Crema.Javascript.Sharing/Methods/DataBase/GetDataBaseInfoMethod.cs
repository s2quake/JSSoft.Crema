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
using System.Reflection;
using Ntreev.Crema.Data;
using System.Threading.Tasks;
using Ntreev.Crema.Services.Extensions;

namespace Ntreev.Crema.Javascript.Methods.DataBase
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(DataBase))]
    class GetDataBaseInfoMethod : ScriptFuncTaskBase<string, IDictionary<string, object>>
    {
        [ImportingConstructor]
        public GetDataBaseInfoMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override async Task<IDictionary<string, object>> OnExecuteAsync(string dataBaseName)
        {
            var dataBase = await this.CremaHost.GetDataBaseAsync(dataBaseName);
            return await dataBase.Dispatcher.InvokeAsync(() =>
            {
                var dataBaseInfo = dataBase.DataBaseInfo;
                var props = new Dictionary<string, object>
                {
                    { nameof(dataBaseInfo.ID), $"{dataBaseInfo.ID}" },
                    { nameof(dataBaseInfo.Name), dataBaseInfo.Name },
                    { nameof(dataBaseInfo.Comment), dataBaseInfo.Comment },
                    { nameof(dataBaseInfo.Revision), dataBaseInfo.Revision },
                    { nameof(dataBaseInfo.Tags), dataBaseInfo.Tags },
                    { nameof(dataBaseInfo.TypesHashValue), dataBaseInfo.TypesHashValue },
                    { nameof(dataBaseInfo.TablesHashValue), dataBaseInfo.TablesHashValue },
                    { nameof(dataBaseInfo.Paths), dataBaseInfo.Paths },
                    { CremaSchema.Creator, dataBaseInfo.CreationInfo.ID },
                    { CremaSchema.CreatedDateTime, dataBaseInfo.CreationInfo.DateTime },
                    { CremaSchema.Modifier, dataBaseInfo.ModificationInfo.ID },
                    { CremaSchema.ModifiedDateTime, dataBaseInfo.ModificationInfo.DateTime }
                };
                return props;
            });
        }
    }
}
