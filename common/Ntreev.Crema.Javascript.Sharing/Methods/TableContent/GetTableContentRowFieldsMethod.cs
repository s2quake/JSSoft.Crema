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
using Ntreev.Crema.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Javascript.Methods.TableContent
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(TableContent))]
    class GetTableContentRowFieldsMethod : DomainScriptMethodBase
    {
        [ImportingConstructor]
        public GetTableContentRowFieldsMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override Delegate CreateDelegate()
        {
            return new Func<string, string, object[], IDictionary<string, object>>(this.GetTableContentRowFields);
        }

        private IDictionary<string, object> GetTableContentRowFields(string domainID, string tableName, object[] keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            var contents = this.GetDomainHost<IEnumerable<ITableContent>>(domainID);
            var content = contents.FirstOrDefault(item => item.Dispatcher.Invoke(() => item.Table.Name) == tableName);
            if (content == null)
                throw new TableNotFoundException(tableName);
            var authentication = this.Context.GetAuthentication(this);
            var task = InvokeAsync();
            task.Wait();
            return task.Result;

            async Task<IDictionary< string, object>> InvokeAsync()
            {
                var row = await content.FindAsync(authentication, keys);
                var tableInfo = content.Table.TableInfo;
                var fields = new Dictionary<string, object>(tableInfo.Columns.Length);
                foreach (var item in tableInfo.Columns)
                {
                    fields.Add(item.Name, row[item.Name]);
                }
                return fields;
            };
        }
    }
}
