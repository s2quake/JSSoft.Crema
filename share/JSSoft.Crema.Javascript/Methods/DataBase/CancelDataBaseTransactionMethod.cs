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
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript.Methods.DataBase
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(DataBase))]
    class CancelDataBaseTransactionMethod : ScriptActionTaskBase<string>
    {
        [ImportingConstructor]
        public CancelDataBaseTransactionMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override async Task OnExecuteAsync(string transactionID)
        {
            if (transactionID == null)
                throw new ArgumentNullException(nameof(transactionID));
            if (this.Context.Properties.ContainsKey(transactionID) == false)
                throw new ItemNotFoundException(transactionID);
            if (this.Context.Properties[transactionID] is ITransaction transaction)
            {
                var authentication = this.Context.GetAuthentication(this);
                await transaction.RollbackAsync(authentication);
                this.Context.Properties.Remove(transactionID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
