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
using Ntreev.Crema.Services.Domains.Serializations;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using Ntreev.Library.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Domains
{
    abstract class Domain : DomainBase<Domain, DomainCategory, DomainCollection, DomainCategoryCollection, DomainContext>,
        IDomain, IDomainItem, IInfoProvider, IStateProvider
    {
        private const string dataKey = "Data";
        private const string usersKey = "Users";
        private object source;
        private byte[] data;
        private Func<DateTime> dateTimeProvider;
        private readonly HashSet<string> modifiedTableList = new HashSet<string>();

        private EventHandler<DomainUserEventArgs> userAdded;
        private EventHandler<DomainUserEventArgs> userChanged;
        private EventHandler<DomainUserRemovedEventArgs> userRemoved;
        private EventHandler<DomainRowEventArgs> rowAdded;
        private EventHandler<DomainRowEventArgs> rowRemoved;
        private EventHandler<DomainRowEventArgs> rowChanged;
        private EventHandler<DomainPropertyEventArgs> propertyChanged;
        private EventHandler<DomainDeletedEventArgs> deleted;

        protected Domain(SerializationInfo info, StreamingContext context)
        {
            var domainInfo = (DomainInfo)info.GetValue(typeof(DomainInfo).Name, typeof(DomainInfo));
            var userInfos = FindUsersValue();
            this.Initialize(domainInfo);
            this.Name = base.DomainInfo.DomainID.ToString();
            this.data = (byte[])info.GetValue(dataKey, typeof(byte[]));
            this.source = this.DerializeSource(this.data);
            this.Users = new DomainUserCollection(this);
            this.InitializeUsers(userInfos);

            DomainUserInfo[] FindUsersValue()
            {
                var enumerator = info.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    if (item.Name == usersKey)
                    {
                        return XmlSerializerUtility.ReadString<DomainUserInfo[]>(item.Value as string);
                    }
                }
                return null;
            }
        }

        protected Domain(DomainSerializationInfo serializationInfo, object source)
        {
            this.source = source;
            this.Initialize(serializationInfo.DomainInfo);
            this.Name = serializationInfo.DomainInfo.DomainID.ToString();
            this.data = this.SerializeSource(source);
            this.Users = new DomainUserCollection(this);
            this.InitializeUsers(serializationInfo.UserInfos);
        }

        protected Domain(string creatorID, object source, Guid dataBaseID, string itemPath, string itemType)
        {
            var signatureDate = new SignatureDate(creatorID, DateTime.UtcNow);
            var domainInfo = new DomainInfo()
            {
                DomainID = Guid.NewGuid(),
                CreationInfo = signatureDate,
                ModificationInfo = signatureDate,
                DataBaseID = dataBaseID,
                ItemPath = itemPath,
                ItemType = itemType,
                DomainType = this.GetType().Name,

            };
            this.source = source;
            this.Initialize(domainInfo);
            base.DomainState = DomainState.IsActivated;
            this.Name = base.DomainInfo.DomainID.ToString();
            this.Users = new DomainUserCollection(this);
        }

        public Task DeleteAsync(Authentication authentication, bool isCanceled)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, isCanceled);
                    this.ValidateDelete(authentication, isCanceled);
                    this.Sign(authentication, true);
                    var container = this.Container;
                    this.Dispatcher.Dispose();
                    this.Dispatcher = null;
                    this.Logger?.Dispose(true);
                    this.Logger = null;
                    this.Users.Clear();
                    this.Dispose();
                    this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled));
                    container.InvokeDomainDeletedEvent(authentication, this, isCanceled);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task BeginUserEditAsync(Authentication authentication, DomainLocationInfo location)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginUserEditAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                    this.ValidateBeginUserEdit(authentication, location);
                    this.CremaHost.Sign(authentication);
                    this.InvokeBeginUserEdit(authentication, location, out var domainUser);
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
                    this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task EndUserEditAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndUserEditAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                    this.ValidateEndUserEdit(authentication);
                    this.CremaHost.Sign(authentication);
                    this.InvokeEndUserEdit(authentication, out var domainUser);
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
                    this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<DomainRowInfo[]> NewRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NewRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                    this.ValidateNewRow(authentication, rows);
                    this.CremaHost.Sign(authentication);
                    this.Logger.NewRow(authentication, rows);
                    this.InvokeNewRow(authentication, ref rows);
                    this.data = null;
                    this.IsModified = true;
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.Logger.Complete();
                    this.OnRowAdded(new DomainRowEventArgs(authentication, this, rows));
                    this.Container.InvokeDomainRowAddedEvent(authentication, this, rows);
                    this.Container.InvokeDomainStateChangedEvent(authentication, this);
                    this.Container.InvokeDomainInfoChangedEvent(authentication, this);
                    return rows;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<DomainRowInfo[]> SetRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                    this.ValidateSetRow(authentication, rows);
                    this.CremaHost.Sign(authentication);
                    this.Logger.SetRow(authentication, rows);
                    this.InvokeSetRow(authentication, ref rows);
                    this.data = null;
                    this.IsModified = true;
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.Logger.Complete();
                    this.OnRowChanged(new DomainRowEventArgs(authentication, this, rows));
                    this.Container.InvokeDomainRowChangedEvent(authentication, this, rows);
                    this.Container.InvokeDomainStateChangedEvent(authentication, this);
                    this.Container.InvokeDomainInfoChangedEvent(authentication, this);
                    return rows;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task RemoveRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                    this.ValidateRemoveRow(authentication, rows);
                    this.CremaHost.Sign(authentication);
                    this.Logger.RemoveRow(authentication, rows);
                    this.InvokeRemoveRow(authentication, rows);
                    this.data = null;
                    this.IsModified = true;
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.Logger.Complete();
                    this.OnRowRemoved(new DomainRowEventArgs(authentication, this, rows));
                    this.Container.InvokeDomainRowRemovedEvent(authentication, this, rows);
                    this.Container.InvokeDomainStateChangedEvent(authentication, this);
                    this.Container.InvokeDomainInfoChangedEvent(authentication, this);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetPropertyAsync(Authentication authentication, string propertyName, object value)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPropertyAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, propertyName, value);
                    this.ValidateSetProperty(authentication, propertyName, value);
                    this.CremaHost.Sign(authentication);
                    this.Logger.SetProperty(authentication, propertyName, value);
                    this.InvokeSetProperty(authentication, propertyName, value);
                    this.data = null;
                    this.IsModified = true;
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.Logger.Complete();
                    this.OnPropertyChanged(new DomainPropertyEventArgs(authentication, this, propertyName, value));
                    this.Container.InvokeDomainPropertyChangedEvent(authentication, this, propertyName, value);
                    this.Container.InvokeDomainStateChangedEvent(authentication, this);
                    this.Container.InvokeDomainInfoChangedEvent(authentication, this);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetUserLocationAsync(Authentication authentication, DomainLocationInfo location)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetUserLocationAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                    this.ValidateSetLocation(authentication, location);
                    this.CremaHost.Sign(authentication);
                    this.InvokeSetUserLocation(authentication, location, out var domainUser);
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
                    this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<DomainUserInfo> KickAsync(Authentication authentication, string userID, string comment)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(KickAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, userID, comment);
                    this.ValidateKick(authentication, userID, comment);
                    this.CremaHost.Sign(authentication);
                    this.Logger.Kick(authentication, userID, comment);
                    this.InvokeKick(authentication, userID, comment, out var domainUser, out var removeInfo);
                    this.Users.Remove(userID);
                    this.Logger.Complete();
                    this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, removeInfo));
                    this.Container.InvokeDomainUserRemovedEvent(authentication, this, domainUser, removeInfo);
                    return domainUser.DomainUserInfo;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetOwnerAsync(Authentication authentication, string userID)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetOwnerAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, userID);
                    this.ValidateSetOwner(authentication, userID);
                    this.CremaHost.Sign(authentication);
                    this.Logger.SetOwner(authentication, userID);
                    this.InvokeSetOwner(authentication, userID, out var oldOwner, out var newOwner);
                    this.Users.Owner = newOwner;
                    this.Logger.Complete();
                    if (oldOwner != null)
                    {
                        this.OnUserChanged(new DomainUserEventArgs(authentication, this, oldOwner));
                        this.Container.InvokeDomainUserChangedEvent(authentication, this, oldOwner);
                    }
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, newOwner));
                    this.Container.InvokeDomainUserChangedEvent(authentication, this, newOwner);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<DomainMetaData> GetMetaDataAsync(Authentication authentication)
        {
            this.ValidateExpired();
            return this.Dispatcher.InvokeAsync(() =>
            {
                var metaData = new DomainMetaData()
                {
                    DomainID = Guid.Parse(this.Name),
                    DomainInfo = base.DomainInfo,
                    Users = this.Users.Select<DomainUser, DomainUserMetaData>(item => item.GetMetaData(authentication)).ToArray(),
                    DomainState = base.DomainState,
                    ModifiedTables = this.modifiedTableList.ToArray(),
                };

                if (this.Users.Contains(authentication.ID) == true)
                {
                    if (this.data == null)
                        this.data = this.SerializeSource(this.Source);
                    metaData.Data = this.data;
                }

                return metaData;
            });
        }

        public void Dispose(DomainContext domainContext)
        {
            this.Dispatcher.VerifyAccess();
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
            this.Logger?.Dispose(false);
            this.Logger = null;
            this.Dispose();
        }

        //public void Initialize()
        //{
        //    this.Dispatcher = new CremaDispatcher(this);
        //    this.Logger = new DomainLogger(this.Context.Serializer, this);
        //}

        public void Dispose(Authentication authentication, bool isCanceled)
        {
            base.Dispose();
            this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled));
        }

        public void Attach(Authentication authentication)
        {
            this.Dispatcher.VerifyAccess();
            this.Sign(authentication, true);
            this.InvokeAttach(authentication, out var domainUser);
            this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
            this.OnDomainStateChanged(new DomainEventArgs(authentication, this));
            this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
            this.Container.InvokeDomainStateChangedEvent(authentication, this);
        }

        public void Detach(Authentication authentication)
        {
            this.Dispatcher.VerifyAccess();
            this.Sign(authentication, true);
            this.InvokeDetach(authentication, out var domainUser);
            this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
            this.OnDomainStateChanged(new DomainEventArgs(authentication, this));
            this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
            this.Container.InvokeDomainStateChangedEvent(authentication, this);
        }

        public Task AddUserAsync(Authentication authentication, DomainAccessType accessType)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.DebugMethod(authentication, this, nameof(AddUserAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, accessType);
                this.ValidateAdd(authentication);
                this.CremaHost.Sign(authentication);
                this.Logger.Join(authentication, accessType);
                this.InvokeAddUser(authentication, accessType, out var domainUser);
                this.Users.Add(domainUser);
                this.Logger.Complete();
                this.OnUserAdded(new DomainUserEventArgs(authentication, this, domainUser));
                this.Container.InvokeDomainUserAddedEvent(authentication, this, domainUser);
            });
        }

        public Task RemoveUserAsync(Authentication authentication)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.DebugMethod(authentication, this, nameof(RemoveUserAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                this.ValidateRemove(authentication);
                this.CremaHost.Sign(authentication);
                this.Logger.Disjoin(authentication, RemoveInfo.Empty);
                this.InvokeRemoveUser(authentication, out var domainUser, out var isMaster);
                this.Users.Remove(authentication.ID);
                this.Logger.Complete();
                this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, RemoveInfo.Empty));
                this.Container.InvokeDomainUserRemovedEvent(authentication, this, domainUser, RemoveInfo.Empty);

                if (isMaster == true && this.Users.Owner != null)
                {
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, this.Users.Owner));
                    this.Container.InvokeDomainUserChangedEvent(authentication, this, this.Users.Owner);
                }
            });
        }

        public void SetDomainHost(IDomainHost host)
        {
            Authentication.System.Sign();
            this.Host = host;
            if (this.Host != null)
            {
                base.DomainState |= DomainState.IsActivated;
            }
            else
            {
                base.DomainState &= ~DomainState.IsActivated;
            }
            this.Dispatcher.Invoke(() =>
            {
                this.OnDomainStateChanged(new DomainEventArgs(Authentication.System, this));
                this.Container.InvokeDomainStateChangedEvent(Authentication.System, this);
            });
        }

        public DomainSerializationInfo GetSerializationInfo()
        {
            var query = from DomainUser item in this.Users select item.DomainUserInfo;
            var properties = new Dictionary<string, object>();
            var serializationInfo = new DomainSerializationInfo()
            {
                DomainType = this.GetType().AssemblyQualifiedName,
                SourceType = this.Source.GetType().AssemblyQualifiedName,
                DomainInfo = this.DomainInfo,
                UserInfos = query.ToArray(),
            };
            this.OnSerializaing(properties);
            foreach (var item in properties)
            {
                serializationInfo.AddProperty(item.Key, item.Value);
            }
            return serializationInfo;
        }

        public Guid ID
        {
            get => Guid.Parse(this.Name);
            set => this.Name = value.ToString();
        }

        public DomainUserCollection Users { get; }

        public IDomainHost Host { get; set; }

        public Guid DataBaseID => base.DomainInfo.DataBaseID;

        public object Source => this.source;

        public CremaDispatcher Dispatcher { get; internal set; }

        public DomainLogger Logger { get; set; }

        public string ItemPath => base.DomainInfo.ItemPath;

        public Func<DateTime> DateTimeProvider
        {
            get => this.dateTimeProvider ?? this.GetTime;
            set => this.dateTimeProvider = value;
        }

        public CremaHost CremaHost => this.Context.CremaHost;

        public string[] ModifiedTables => this.modifiedTableList.OrderBy(item => item).ToArray();

        public event EventHandler<DomainUserEventArgs> UserAdded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userAdded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userAdded -= value;
            }
        }

        public event EventHandler<DomainUserEventArgs> UserChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userChanged -= value;
            }
        }

        public event EventHandler<DomainUserRemovedEventArgs> UserRemoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userRemoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userRemoved -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> RowAdded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.rowAdded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.rowAdded -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> RowRemoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.rowRemoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.rowRemoved -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> RowChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.rowChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.rowChanged -= value;
            }
        }

        public event EventHandler<DomainPropertyEventArgs> PropertyChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.propertyChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.propertyChanged -= value;
            }
        }

        public new event EventHandler<DomainDeletedEventArgs> Deleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.deleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.deleted -= value;
                if (this.deleted != null)
                {
                    int qwer = 0;
                }
            }
        }

        public new event EventHandler DomainInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainInfoChanged -= value;
            }
        }

        public new event EventHandler DomainStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainStateChanged -= value;
            }
        }

        protected virtual DomainRowInfo[] OnNewRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            return rows;
        }

        protected virtual DomainRowInfo[] OnSetRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            return rows;
        }

        protected virtual void OnRemoveRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {

        }

        protected virtual void OnSetProperty(DomainUser domainUser, string propertyName, object value, SignatureDateProvider signatureProvider)
        {

        }

        protected virtual void OnSetLocation(DomainUser domainUser, DomainLocationInfo location)
        {

        }

        protected virtual void OnBeginUserEdit(DomainUser domainUser, DomainLocationInfo location)
        {

        }

        protected virtual void OnEndUserEdit(DomainUser domainUser)
        {

        }

        protected abstract byte[] SerializeSource(object source);

        protected abstract object DerializeSource(byte[] data);

        protected virtual void OnSerializaing(IDictionary<string, object> properties)
        {

        }

        protected virtual void OnUserAdded(DomainUserEventArgs e)
        {
            this.userAdded?.Invoke(this, e);
        }

        protected virtual void OnUserChanged(DomainUserEventArgs e)
        {
            this.userChanged?.Invoke(this, e);
        }

        protected virtual void OnUserRemoved(DomainUserRemovedEventArgs e)
        {
            this.userRemoved?.Invoke(this, e);
        }

        protected virtual void OnRowAdded(DomainRowEventArgs e)
        {
            this.rowAdded?.Invoke(this, e);
        }

        protected virtual void OnRowChanged(DomainRowEventArgs e)
        {
            this.rowChanged?.Invoke(this, e);
        }

        protected virtual void OnRowRemoved(DomainRowEventArgs e)
        {
            this.rowRemoved?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(DomainPropertyEventArgs e)
        {
            this.propertyChanged?.Invoke(this, e);
        }

        protected virtual void OnDeleted(DomainDeletedEventArgs e)
        {
            this.deleted?.Invoke(this, e);
        }

        private void ValidateDelete(Authentication authentication, bool isCanceled)
        {
            var isOwner = this.Users.OwnerUserID == authentication.ID;
            if (authentication.IsAdmin == false && isOwner == false)
                throw new PermissionDeniedException();
            if (this.Host is IDomainHost domainHost)
                domainHost.ValidateDelete(authentication, isCanceled);
        }

        private void ValidateAdd(Authentication authentication)
        {
            if (this.Users.Contains(authentication.ID) == true)
                throw new NotImplementedException();
        }

        private void ValidateRemove(Authentication authentication)
        {
            var domainUser = this.GetDomainUser(authentication);
        }

        private void ValidateNewRow(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateSetRow(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateRemoveRow(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateSetProperty(Authentication authentication, string propertyName, object value)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateSetLocation(Authentication authentication, DomainLocationInfo location)
        {
            var domainUser = this.GetDomainUser(authentication);
        }

        private void ValidateBeginUserEdit(Authentication authentication, DomainLocationInfo location)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateEndUserEdit(Authentication authentication)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateSendMessage(Authentication authentication, string message)
        {
            var domainUser = this.GetDomainUser(authentication);
        }

        private void ValidateKick(Authentication authentication, string userID, string comment)
        {
            if (userID == null)
                throw new ArgumentNullException(nameof(userID));
            if (this.Users.ContainsKey(userID) == false)
                throw new ArgumentException(string.Format(Resources.Exception_UserIsNotInDomain_Format, userID), nameof(userID));

            if (authentication.ID == userID)
                throw new ArgumentException(Resources.Exception_CannotKickYourself, nameof(userID));

            var domainUser = this.Users[userID];
            if (domainUser.IsOwner == true)
                throw new PermissionDeniedException(Resources.Exception_OwnerCannotKicked);
        }

        private void ValidateSetOwner(Authentication authentication, string userID)
        {
            if (userID == null)
                throw new ArgumentNullException(nameof(userID));
            if (this.Users.ContainsKey(userID) == false)
                throw new ArgumentException(string.Format(Resources.Exception_UserIsNotInDomain_Format, userID), nameof(userID));
        }

        private void ValidateDomainUser(Authentication authentication)
        {
            if (this.Dispatcher == null)
                throw new NotImplementedException();

            if (this.Users.Contains(authentication.ID) == false)
                throw new UserNotFoundException(authentication.ID);
        }

        private void InvokeBeginUserEdit(Authentication authentication, DomainLocationInfo location, out DomainUser domainUser)
        {
            domainUser = this.GetDomainUser(authentication);
            this.OnBeginUserEdit(domainUser, location);
            domainUser.Location = location;
            domainUser.IsBeingEdited = true;
        }

        private void InvokeEndUserEdit(Authentication authentication, out DomainUser domainUser)
        {
            domainUser = this.GetDomainUser(authentication);
            this.OnEndUserEdit(domainUser);
            domainUser.IsBeingEdited = false;
        }

        private void InvokeNewRow(Authentication authentication, ref DomainRowInfo[] rows)
        {
            var domainUser = this.GetDomainUser(authentication);
            var result = this.OnNewRow(domainUser, rows, authentication.GetSignatureDateProvider());
            foreach (var item in rows)
            {
                this.modifiedTableList.Add(item.TableName);
            }
            domainUser.IsModified = true;
            rows = result;
        }

        private void InvokeSetRow(Authentication authentication, ref DomainRowInfo[] rows)
        {
            var domainUser = this.GetDomainUser(authentication);
            var result = this.OnSetRow(domainUser, rows, authentication.GetSignatureDateProvider());
            foreach (var item in rows)
            {
                this.modifiedTableList.Add(item.TableName);
            }
            domainUser.IsModified = true;
            rows = result;
        }

        private void InvokeRemoveRow(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = this.GetDomainUser(authentication);
            this.OnRemoveRow(domainUser, rows, authentication.GetSignatureDateProvider());
            foreach (var item in rows)
            {
                this.modifiedTableList.Add(item.TableName);
            }
            domainUser.IsModified = true;
        }

        private void InvokeSetProperty(Authentication authentication, string propertyName, object value)
        {
            var domainUser = this.GetDomainUser(authentication);
            this.OnSetProperty(domainUser, propertyName, value, authentication.GetSignatureDateProvider());
            domainUser.IsModified = true;
        }

        private void InvokeSetUserLocation(Authentication authentication, DomainLocationInfo location, out DomainUser domainUser)
        {
            domainUser = this.GetDomainUser(authentication);
            this.OnSetLocation(domainUser, location);
            domainUser.Location = location;
        }

        private void InvokeKick(Authentication authentication, string userID, string comment, out DomainUser domainUser, out RemoveInfo removeInfo)
        {
            removeInfo = new RemoveInfo(RemoveReason.Kick, comment);
            domainUser = this.Users[userID];
            if (domainUser.Authentication != null)
            {
                domainUser.Authentication.Expired -= Authentication_Expired;
                domainUser.Authentication = null;
            }
        }

        private void InvokeSetOwner(Authentication authentication, string userID, out DomainUser oldOwner, out DomainUser newOwner)
        {
            oldOwner = this.Users.Owner;
            newOwner = this.Users[userID];
        }

        private void InvokeAttach(Authentication authentication, out DomainUser domainUser)
        {
            domainUser = this.Users[authentication.ID];
            domainUser.IsOnline = true;
            domainUser.Authentication = authentication;
            authentication.Expired += Authentication_Expired;
        }

        private void InvokeDetach(Authentication authentication, out DomainUser domainUser)
        {
            domainUser = this.Users[authentication.ID];
            domainUser.IsOnline = false;
            if (domainUser.Authentication != null)
            {
                domainUser.Authentication.Expired -= Authentication_Expired;
                domainUser.Authentication = null;
            }
        }

        private void InvokeAddUser(Authentication authentication, DomainAccessType accessType, out DomainUser domainUser)
        {
            domainUser = new DomainUser(this, authentication.ID, authentication.Name, accessType)
            {
                IsOnline = authentication.Types.HasFlag(AuthenticationType.User),
            };
            domainUser.Authentication = authentication;
            authentication.Expired += Authentication_Expired;
        }

        private void InvokeRemoveUser(Authentication authentication, out DomainUser domainUser, out bool isOwner)
        {
            domainUser = this.GetDomainUser(authentication);
            isOwner = domainUser.IsOwner;
            domainUser.Authentication = null;
            authentication.Expired -= Authentication_Expired;
        }

        private DomainUser GetDomainUser(Authentication authentication)
        {
            if (this.Dispatcher == null)
                throw new NotImplementedException();

            if (this.Users.Contains(authentication.ID) == false)
                throw new UserNotFoundException(authentication.ID);

            return this.Users[authentication.ID];
        }

        private DateTime GetTime()
        {
            return DateTime.UtcNow;
        }

        private void Sign(Authentication authentication, bool defaultProvider)
        {
            if (defaultProvider == false)
            {
                var signatureProvider = new InternalSignatureDateProvider(authentication, this.DateTimeProvider());
                authentication.Sign(signatureProvider.DateTime);
            }
            else
            {
                authentication.Sign();
            }
        }

        private void Authentication_Expired(object sender, EventArgs e)
        {
            var authentication = sender as Authentication;

            if (this.Dispatcher == null)
                return;

            this.Dispatcher.Invoke(() =>
            {
                var domainUser = this.GetDomainUser(authentication);
                domainUser.Authentication = null;
                domainUser.IsOnline = false;
                authentication.Sign();
                this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
            });
        }

        private void InitializeUsers(DomainUserInfo[] users)
        {
            if (users == null)
                return;

            foreach (var item in users)
            {
                this.Users.Add(new DomainUser(this, item.UserID, item.UserName, item.AccessType));
            }
        }

        #region IDomain

        IDomainUserCollection IDomain.Users => this.Users;

        DomainInfo IDomain.DomainInfo => base.DomainInfo;

        object IDomain.Host => this.Host;

        #endregion

        #region IDomainItem

        IDomainItem IDomainItem.Parent => this.Category;

        IEnumerable<IDomainItem> IDomainItem.Childs => Enumerable.Empty<IDomainItem>();

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IDataBase) && this.Category != null)
            {
                return this.Category.DataBase;
            }
            return (this.Context as IServiceProvider).GetService(serviceType);
        }

        #endregion

        #region classes

        class InternalSignatureDateProvider : SignatureDateProvider
        {
            public InternalSignatureDateProvider(Authentication authentication, DateTime dateTime)
                : base(authentication.ID)
            {
                this.DateTime = dateTime;
            }

            public DateTime DateTime { get; }

            protected override DateTime GetTime()
            {
                return this.DateTime;
            }
        }

        #endregion

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => base.DomainInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => base.DomainState;

        #endregion
    }
}
