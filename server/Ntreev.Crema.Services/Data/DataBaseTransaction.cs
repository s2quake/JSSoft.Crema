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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class DataBaseTransaction : ITransaction
    {
        private readonly Authentication authentication;
        private readonly DataBase dataBase;
        private readonly DataBaseRepositoryHost repository;
        private readonly TypeInfo[] typeInfos;
        private readonly TableInfo[] tableInfos;
        private readonly string transactionPath;
        private readonly string domainPath;
        private bool isDisposed;

        public DataBaseTransaction(Authentication authentication, DataBase dataBase, DataBaseRepositoryHost repository)
        {
            this.authentication = authentication;
            this.dataBase = dataBase;
            this.repository = repository;
            this.typeInfos = dataBase.TypeContext.Types.Select((Type item) => item.TypeInfo).ToArray();
            this.tableInfos = dataBase.TableContext.Tables.Select((Table item) => item.TableInfo).ToArray();
            this.transactionPath = dataBase.CremaHost.GetPath(CremaPath.Transactions, $"{dataBase.ID}");
            this.domainPath = dataBase.CremaHost.GetPath(CremaPath.Domains, $"{dataBase.ID}");
            this.CopyDomains(authentication);
            this.repository.BeginTransaction(authentication.ID, dataBase.Name);
            this.authentication.Expired += Authentication_Expired;
        }

        private void BackupDomains()
        {

        }

        public Task CommitAsync(Authentication authentication)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateCommit(authentication);
                    this.dataBase.VerifyAccess(authentication);
                    this.CremaHost.Sign(authentication);
                    this.repository.EndTransaction();
                    this.authentication.Expired -= Authentication_Expired;
                    this.isDisposed = true;
                    this.OnDisposed(EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task RollbackAsync(Authentication authentication)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(async () =>
                {
                    this.ValidateRollback(authentication);
                    this.dataBase.VerifyAccess(authentication);
                    this.CremaHost.Sign(authentication);
                    this.dataBase.ResettingDataBase(authentication);
                    await this.RollbackDomainsAsync(authentication);
                    await this.dataBase.ResetDataBaseAsync(authentication, this.typeInfos, this.tableInfos);
                    this.authentication.Expired -= Authentication_Expired;
                    this.isDisposed = true;
                    this.OnDisposed(EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public CremaDispatcher Dispatcher => this.dataBase.Dispatcher;

        public CremaHost CremaHost => this.dataBase.CremaHost;

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }

        private async void Authentication_Expired(object sender, EventArgs e)
        {
            this.authentication.Expired -= Authentication_Expired;
            await this.RollbackAsync(this.authentication);
        }

        private void CopyDomains(Authentication authentication)
        {
            DirectoryUtility.Delete(this.transactionPath);
            if (DirectoryUtility.Exists(this.domainPath) == true)
                DirectoryUtility.Copy(this.domainPath, this.transactionPath);
        }

        private async Task RollbackDomainsAsync(Authentication authentication)
        {
            this.repository.CancelTransaction();

            if (this.dataBase.GetService(typeof(DomainContext)) is DomainContext domainContext)
            {
                if (DirectoryUtility.Exists(this.transactionPath) == true)
                    DirectoryUtility.Copy(this.transactionPath, this.domainPath);
                await domainContext.RestoreAsync(authentication, this.dataBase);
                DirectoryUtility.Delete(this.transactionPath);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void ValidateCommit(Authentication authentication)
        {
            if (this.isDisposed == true)
                throw new InvalidOperationException(Resources.Exception_Expired);
        }

        private void ValidateRollback(Authentication authentication)
        {
            if (this.isDisposed == true)
                throw new InvalidOperationException(Resources.Exception_Expired);
        }
    }
}
