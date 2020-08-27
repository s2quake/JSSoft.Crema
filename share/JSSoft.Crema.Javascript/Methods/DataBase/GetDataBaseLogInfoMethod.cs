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

using JSSoft.Crema.ServiceModel;
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
    class GetDataBaseLogInfoMethod : ScriptFuncTaskBase<string, string, IDictionary<string, object>[]>
    {
        [ImportingConstructor]
        public GetDataBaseLogInfoMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override async Task<IDictionary<string, object>[]> OnExecuteAsync(string dataBaseName, string revision = null)
        {
            var dataBase = await this.CremaHost.GetDataBaseAsync(dataBaseName);
            var authentication = this.Context.GetAuthentication(this);
            var logInfos = await dataBase.GetLogAsync(authentication, revision);
            return this.GetLogInfo(logInfos);
        }

        private IDictionary<string, object>[] GetLogInfo(LogInfo[] logInfos)
        {
            var props = new IDictionary<string, object>[logInfos.Length];
            for (var i = 0; i < logInfos.Length; i++)
            {
                props[i] = this.GetLogInfo(logInfos[i]);
            }
            return props;
        }

        private IDictionary<string, object> GetLogInfo(LogInfo columnInfo)
        {
            var props = new Dictionary<string, object>
            {
                { nameof(columnInfo.UserID), columnInfo.UserID },
                { nameof(columnInfo.Revision), columnInfo.Revision },
                { nameof(columnInfo.Comment), columnInfo.Comment },
                { nameof(columnInfo.DateTime), columnInfo.DateTime }
            };
            return props;
        }
    }
}
