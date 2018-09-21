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
using Ntreev.Crema.Services.DataBaseCollectionService;
using Ntreev.Crema.Services.Domains;
using System;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class DataBaseTransaction : ITransaction
    {
        private Authentication authentication;
        private readonly DataBase dataBase;
        private readonly IDataBaseCollectionService service;

        public DataBaseTransaction(Authentication authentication, DataBase dataBase, IDataBaseCollectionService service)
        {
            this.authentication = authentication;
            this.dataBase = dataBase;
            this.service = service;
        }

        public async Task CommitAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.dataBase.VerifyAccess(authentication);
                    var result = await this.service.EndTransactionAsync(this.dataBase.Name);
                    this.CremaHost.Sign(authentication, result);
                    this.OnDisposed(EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task RollbackAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.dataBase.VerifyAccess(authentication);
                    var result = await this.service.CancelTransactionAsync(this.dataBase.Name);
                    this.CremaHost.Sign(authentication, result);
                    this.RollbackDomains(authentication);
                    this.OnDisposed(EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        private void RollbackDomains(Authentication authentication)
        {
            if (this.dataBase.GetService(typeof(DomainContext)) is DomainContext domainContext)
            {
                this.dataBase.SetResetting(authentication);
                var metaDatas = domainContext.Restore(this.dataBase);
                this.dataBase.SetReset(authentication, metaDatas);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            this.authentication = null;
        }

        public CremaDispatcher Dispatcher => this.dataBase.Dispatcher;

        public CremaHost CremaHost => this.dataBase.CremaHost;

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }
    }
}
