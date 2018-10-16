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
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.Data.Xml;
using Ntreev.Crema.Data;
using System.Collections.Generic;
using Ntreev.Library.Linq;
using System.Collections.Specialized;
using Ntreev.Library;

namespace Ntreev.Crema.ServiceHosts.Data
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    partial class DataBaseCollectionService : CremaServiceItemBase<IDataBaseCollectionEventCallback>, IDataBaseCollectionService
    {
        private Authentication authentication;

        public DataBaseCollectionService(ICremaHost cremaHost)
            : base(cremaHost.GetService(typeof(ILogService)) as ILogService)
        {
            this.CremaHost = cremaHost;
            this.LogService = cremaHost.GetService(typeof(ILogService)) as ILogService;
            this.DataBases = cremaHost.GetService(typeof(IDataBaseCollection)) as IDataBaseCollection;
            this.UserContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;

            this.LogService.Debug($"{nameof(DataBaseCollectionService)} Constructor");
        }

        public Task<ResultBase> DefinitionTypeAsync(LogInfo[] param1)
        {
            return Task.Run(() => new ResultBase());
        }

        public async Task<ResultBase<DataBaseCollectionMetaData>> SubscribeAsync(Guid authenticationToken)
        {
            var result = new ResultBase<DataBaseCollectionMetaData>();
            try
            {
                this.authentication = await this.UserContext.AuthenticateAsync(authenticationToken);
                await this.authentication.AddRefAsync(this);
                this.OwnerID = this.authentication.ID;
                result.Value = await this.AttachEventHandlersAsync();
                result.SignatureDate = this.authentication.SignatureDate;
                this.LogService.Debug($"[{this.OwnerID}] {nameof(DataBaseCollectionService)} {nameof(SubscribeAsync)}");
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> UnsubscribeAsync()
        {
            var result = new ResultBase();
            try
            {
                await this.DetachEventHandlersAsync();
                await this.UserContext.Dispatcher.InvokeAsync(() =>
                {
                    this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut;

                });
                await this.authentication.RemoveRefAsync(this);
                this.authentication = null;
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
                this.LogService.Debug($"[{this.OwnerID}] {nameof(DataBaseCollectionService)} {nameof(UnsubscribeAsync)}");
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> SetPublicAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.SetPublicAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<AccessInfo>> SetPrivateAsync(string dataBaseName)
        {
            var result = new ResultBase<AccessInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.SetPrivateAsync(this.authentication);
                result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.AccessInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<AccessMemberInfo>> AddAccessMemberAsync(string dataBaseName, string memberID, AccessType accessType)
        {
            var result = new ResultBase<AccessMemberInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.AddAccessMemberAsync(this.authentication, memberID, accessType);
                var accessInfo = await dataBase.Dispatcher.InvokeAsync(() => dataBase.AccessInfo);
                result.Value = accessInfo.Members.Where(item => item.UserID == memberID).First();
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<AccessMemberInfo>> SetAccessMemberAsync(string dataBaseName, string memberID, AccessType accessType)
        {
            var result = new ResultBase<AccessMemberInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.SetAccessMemberAsync(this.authentication, memberID, accessType);
                var accessInfo = await dataBase.Dispatcher.InvokeAsync(() => dataBase.AccessInfo);
                result.Value = accessInfo.Members.Where(item => item.UserID == memberID).First();
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> RemoveAccessMemberAsync(string dataBaseName, string memberID)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.RemoveAccessMemberAsync(this.authentication, memberID);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<LockInfo>> LockAsync(string dataBaseName, string comment)
        {
            var result = new ResultBase<LockInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.LockAsync(this.authentication, comment);
                result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.LockInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> UnlockAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.UnlockAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> LoadAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.LoadAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> UnloadAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.UnloadAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DataBaseInfo>> CreateAsync(string dataBaseName, string comment)
        {
            var result = new ResultBase<DataBaseInfo>();
            try
            {
                var dataBase = await this.DataBases.AddNewDataBaseAsync(this.authentication, dataBaseName, comment);
                result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.DataBaseInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> RenameAsync(string dataBaseName, string newDataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.RenameAsync(this.authentication, newDataBaseName);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> DeleteAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.DeleteAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DataBaseInfo>> CopyAsync(string dataBaseName, string newDataBaseName, string comment, bool force)
        {
            var result = new ResultBase<DataBaseInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                var newDataBase = await dataBase.CopyAsync(this.authentication, newDataBaseName, comment, force);
                result.Value = await newDataBase.Dispatcher.InvokeAsync(() => newDataBase.DataBaseInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<LogInfo[]>> GetLogAsync(string dataBaseName, string revision)
        {
            var result = new ResultBase<LogInfo[]>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.Value = await dataBase.GetLogAsync(this.authentication, revision);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DataBaseInfo>> RevertAsync(string dataBaseName, string revision)
        {
            var result = new ResultBase<DataBaseInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                await dataBase.RevertAsync(this.authentication, revision);
                result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.DataBaseInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> BeginTransactionAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                var transaction = await dataBase.BeginTransactionAsync(this.authentication);
                dataBase.ExtendedProperties[typeof(ITransaction)] = transaction;
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> EndTransactionAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                var transaction = dataBase.ExtendedProperties[typeof(ITransaction)] as ITransaction;
                await transaction.CommitAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> CancelTransactionAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                var transaction = dataBase.ExtendedProperties[typeof(ITransaction)] as ITransaction;
                await transaction.RollbackAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<bool> IsAliveAsync()
        {
            if (this.authentication == null)
                return false;
            this.LogService.Debug($"[{this.authentication}] {nameof(DataBaseCollectionService)}.{nameof(IsAliveAsync)} : {DateTime.Now}");
            await this.authentication.PingAsync();
            return true;
        }

        public ICremaHost CremaHost { get; }

        public ILogService LogService { get; }

        public IDataBaseCollection DataBases { get; }

        public IUserContext UserContext { get; }

        protected override void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            this.Callback?.OnServiceClosed(signatureDate, closeInfo);
        }

        private async void Users_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            var actionUserID = e.UserID;
            var contains = e.Items.Any(item => item.ID == this.authentication.ID);
            var closeInfo = (CloseInfo)e.MetaData;
            var signatureDate = e.SignatureDate;
            if (actionUserID != this.authentication.ID && contains == true)
            {
                await this.DetachEventHandlersAsync();
                this.authentication = null;
                this.Channel.Abort();
            }
        }

        private void DataBases_ItemsCreated(object sender, ItemsCreatedEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var dataBaseNames = e.Items.Select(item => item.Name).ToArray();
            var dataBaseInfos = e.Arguments.Select(item => (DataBaseInfo)item).ToArray();
            var comment = e.MetaData as string;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesCreated(signatureDate, dataBaseNames, dataBaseInfos, comment));
        }

        private void DataBases_ItemsRenamed(object sender, ItemsRenamedEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var oldNames = e.OldNames;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesRenamed(signatureDate, oldNames, itemNames));
        }

        private void DataBases_ItemsDeleted(object sender, ItemsDeletedEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var itemPaths = e.ItemPaths;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesDeleted(signatureDate, itemPaths));
        }

        private void DataBases_ItemsLoaded(object sender, ItemsEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesLoaded(signatureDate, itemNames));
        }

        private void DataBases_ItemsUnloaded(object sender, ItemsEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesUnloaded(signatureDate, itemNames));
        }

        private void DataBases_ItemsResetting(object sender, ItemsEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesResetting(signatureDate, itemNames));
        }

        private void DataBases_ItemsReset(object sender, ItemsEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            var metaDatas = e.MetaData as DomainMetaData[];
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesReset(signatureDate, itemNames, metaDatas));
        }

        private void DataBases_ItemsAuthenticationEntered(object sender, ItemsEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            var authenticationInfo = (AuthenticationInfo)e.MetaData;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesAuthenticationEntered(signatureDate, itemNames, authenticationInfo));
        }

        private void DataBases_ItemsAuthenticationLeft(object sender, ItemsEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            var authenticationInfo = (AuthenticationInfo)e.MetaData;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesAuthenticationLeft(signatureDate, itemNames, authenticationInfo));
        }

        private void DataBases_ItemsInfoChanged(object sender, ItemsEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var dataBaseInfos = e.Items.Select(item => item.DataBaseInfo).ToArray();

            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesInfoChanged(signatureDate, dataBaseInfos));
        }

        private void DataBases_ItemsStateChanged(object sender, ItemsEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            var dataBaseStates = e.Items.Select(item => item.DataBaseState).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesStateChanged(signatureDate, itemNames, dataBaseStates));
        }

        private void DataBases_ItemsAccessChanged(object sender, ItemsEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var values = new AccessInfo[e.Items.Length];
            for (var i = 0; i < e.Items.Length; i++)
            {
                var item = e.Items[i];
                var accessInfo = item.AccessInfo;
                if (item.AccessInfo.Path != item.Name)
                {
                    accessInfo = AccessInfo.Empty;
                    accessInfo.Path = item.Name;
                }
                values[i] = accessInfo;
            }
            var metaData = e.MetaData as object[];
            var changeType = (AccessChangeType)metaData[0];
            var memberIDs = metaData[1] as string[];
            var accessTypes = metaData[2] as AccessType[];

            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesAccessChanged(signatureDate, changeType, values, memberIDs, accessTypes));
        }

        private void DataBases_ItemsLockChanged(object sender, ItemsEventArgs<IDataBase> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var values = new LockInfo[e.Items.Length];
            for (var i = 0; i < e.Items.Length; i++)
            {
                var item = e.Items[i];
                var lockInfo = item.LockInfo;
                if (item.LockInfo.Path != item.Name)
                {
                    lockInfo = LockInfo.Empty;
                    lockInfo.Path = item.Name;
                }
                values[i] = lockInfo;
            }
            var metaData = e.MetaData as object[];
            var changeType = (LockChangeType)metaData[0];
            var comments = metaData[1] as string[];

            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDataBasesLockChanged(signatureDate, changeType, values, comments));
        }

        private async Task<DataBaseCollectionMetaData> AttachEventHandlersAsync()
        {
            await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserContext.Users.UsersLoggedOut += Users_UsersLoggedOut;
            });
            var metaData = await this.DataBases.Dispatcher.InvokeAsync(() =>
            {
                this.DataBases.ItemsCreated += DataBases_ItemsCreated;
                this.DataBases.ItemsRenamed += DataBases_ItemsRenamed;
                this.DataBases.ItemsDeleted += DataBases_ItemsDeleted;
                this.DataBases.ItemsLoaded += DataBases_ItemsLoaded;
                this.DataBases.ItemsUnloaded += DataBases_ItemsUnloaded;
                this.DataBases.ItemsResetting += DataBases_ItemsResetting;
                this.DataBases.ItemsReset += DataBases_ItemsReset;
                this.DataBases.ItemsAuthenticationEntered += DataBases_ItemsAuthenticationEntered;
                this.DataBases.ItemsAuthenticationLeft += DataBases_ItemsAuthenticationLeft;
                this.DataBases.ItemsInfoChanged += DataBases_ItemsInfoChanged;
                this.DataBases.ItemsStateChanged += DataBases_ItemsStateChanged;
                this.DataBases.ItemsAccessChanged += DataBases_ItemsAccessChanged;
                this.DataBases.ItemsLockChanged += DataBases_ItemsLockChanged;
                return this.DataBases.GetMetaData(this.authentication);
            });
            this.LogService.Debug($"[{this.OwnerID}] {nameof(DataBaseCollectionService)} {nameof(AttachEventHandlersAsync)}");
            return metaData;
        }

        private async Task DetachEventHandlersAsync()
        {
            await this.DataBases.Dispatcher.InvokeAsync(() =>
            {
                this.DataBases.ItemsCreated -= DataBases_ItemsCreated;
                this.DataBases.ItemsRenamed -= DataBases_ItemsRenamed;
                this.DataBases.ItemsDeleted -= DataBases_ItemsDeleted;
                this.DataBases.ItemsLoaded -= DataBases_ItemsLoaded;
                this.DataBases.ItemsUnloaded -= DataBases_ItemsUnloaded;
                this.DataBases.ItemsResetting -= DataBases_ItemsResetting;
                this.DataBases.ItemsReset -= DataBases_ItemsReset;
                this.DataBases.ItemsAuthenticationEntered -= DataBases_ItemsAuthenticationEntered;
                this.DataBases.ItemsAuthenticationLeft -= DataBases_ItemsAuthenticationLeft;
                this.DataBases.ItemsInfoChanged -= DataBases_ItemsInfoChanged;
                this.DataBases.ItemsStateChanged -= DataBases_ItemsStateChanged;
                this.DataBases.ItemsAccessChanged -= DataBases_ItemsAccessChanged;
                this.DataBases.ItemsLockChanged -= DataBases_ItemsLockChanged;
            });
            await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut;
            });
            this.LogService.Debug($"[{this.OwnerID}] {nameof(DataBaseCollectionService)} {nameof(DetachEventHandlersAsync)}");
        }

        private async Task<IDataBase> GetDataBaseAsync(string dataBaseName)
        {
            var dataBase = await this.CremaHost.Dispatcher.InvokeAsync(() => this.DataBases[dataBaseName]);
            if (dataBase == null)
                throw new DataBaseNotFoundException(dataBaseName);
            return dataBase;
        }

        #region ICremaServiceItem

        protected override async Task OnCloseAsync(bool disconnect)
        {
            if (this.authentication != null)
            {
                await this.DetachEventHandlersAsync();
                this.authentication = null;
            }
        }

        #endregion
    }
}
