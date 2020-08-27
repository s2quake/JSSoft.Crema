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

using JSSoft.Crema.ServiceHosts.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Properties;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;

namespace JSSoft.Crema.Services.Data
{
    class TypeContext : TypeContextBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext>,
        ITypeContext
    {
        private DataBase dataBase;

        private ItemsCreatedEventHandler<ITypeItem> itemsCreated;
        private ItemsRenamedEventHandler<ITypeItem> itemsRenamed;
        private ItemsMovedEventHandler<ITypeItem> itemsMoved;
        private ItemsDeletedEventHandler<ITypeItem> itemsDeleted;
        private ItemsEventHandler<ITypeItem> itemsChanged;
        private ItemsEventHandler<ITypeItem> itemsAccessChanged;
        private ItemsEventHandler<ITypeItem> itemsLockChanged;

        public TypeContext(DataBase dataBase, DataBaseMetaData metaData)
        {
            this.dataBase = dataBase;
            this.Initialize(metaData);
        }

        public void Dispose()
        {
            this.dataBase = null;
        }

#pragma warning disable IDE0060 // 사용하지 않는 매개 변수를 제거하세요.
        public void InvokeTypeItemLock(Authentication authentication, ITypeItem typeItem, string comment)
        {

        }

        public void InvokeTypeItemUnlock(Authentication authentication, ITypeItem typeItem)
        {

        }

        public void InvokeTypeItemSetPrivate(Authentication authentication, ITypeItem typeItem, AccessInfo accessInfo)
        {

        }

        public void InvokeTypeItemSetPublic(Authentication authentication, ITypeItem typeItem, AccessInfo accessInfo)
        {

        }

        public void InvokeTypeItemAddAccessMember(Authentication authentication, ITypeItem typeItem, AccessInfo accessInfo, string memberID, AccessType accessType)
        {

        }

        public void InvokeTypeItemSetAccessMember(Authentication authentication, ITypeItem typeItem, AccessInfo accessInfo, string memberID, AccessType accessType)
        {

        }

        public void InvokeTypeItemRemoveAccessMember(Authentication authentication, ITypeItem typeItem, AccessInfo accessInfo, string memberID)
        {

        }
#pragma warning restore IDE0060 // 사용하지 않는 매개 변수를 제거하세요.

        public void InvokeItemsSetPublicEvent(Authentication authentication, ITypeItem[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetPublicEvent), items);
            var message = EventMessageBuilder.SetPublicTypeItem(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Public);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsSetPrivateEvent(Authentication authentication, ITypeItem[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetPrivateEvent), items);
            var message = EventMessageBuilder.SetPrivateTypeItem(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Private);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsAddAccessMemberEvent(Authentication authentication, ITypeItem[] items, string[] memberIDs, AccessType[] accessTypes)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsAddAccessMemberEvent), items, memberIDs, accessTypes);
            var message = EventMessageBuilder.AddAccessMemberToTypeItem(authentication, items, memberIDs, accessTypes);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Add, memberIDs, accessTypes);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsSetAccessMemberEvent(Authentication authentication, ITypeItem[] items, string[] memberIDs, AccessType[] accessTypes)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetAccessMemberEvent), items, memberIDs, accessTypes);
            var message = EventMessageBuilder.SetAccessMemberOfTypeItem(authentication, items, memberIDs, accessTypes);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Set, memberIDs, accessTypes);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsRemoveAccessMemberEvent(Authentication authentication, ITypeItem[] items, string[] memberIDs)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsRemoveAccessMemberEvent), items, memberIDs);
            var message = EventMessageBuilder.RemoveAccessMemberFromTypeItem(authentication, items, memberIDs);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Remove, memberIDs);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsLockedEvent(Authentication authentication, ITypeItem[] items, string[] comments)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsLockedEvent), items, comments);
            var message = EventMessageBuilder.LockTypeItem(authentication, items, comments);
            var metaData = EventMetaDataBuilder.Build(items, LockChangeType.Lock, comments);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsLockChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsUnlockedEvent(Authentication authentication, ITypeItem[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsUnlockedEvent), items);
            var message = EventMessageBuilder.UnlockTypeItem(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, LockChangeType.Unlock);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsLockChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsCreatedEvent(Authentication authentication, ITypeItem[] items, object[] args)
        {
            this.OnItemsCreated(new ItemsCreatedEventArgs<ITypeItem>(authentication, items, args));
        }

        public void InvokeItemsRenamedEvent(Authentication authentication, ITypeItem[] items, string[] oldNames, string[] oldPaths)
        {
            this.OnItemsRenamed(new ItemsRenamedEventArgs<ITypeItem>(authentication, items, oldNames, oldPaths));
        }

        public void InvokeItemsMovedEvent(Authentication authentication, ITypeItem[] items, string[] oldPaths, string[] oldParentPaths)
        {
            this.OnItemsMoved(new ItemsMovedEventArgs<ITypeItem>(authentication, items, oldPaths, oldParentPaths));
        }

        public void InvokeItemsDeleteEvent(Authentication authentication, ITypeItem[] items, string[] itemPaths)
        {
            this.OnItemsDeleted(new ItemsDeletedEventArgs<ITypeItem>(authentication, items, itemPaths));
        }

        public void InvokeItemsChangedEvent(Authentication authentication, ITypeItem[] items)
        {
            this.OnItemsChanged(new ItemsEventArgs<ITypeItem>(authentication, items));
        }

        public TypeCollection Types => this.Items;

        public IDataBaseService Service => this.DataBase.Service;

        public CremaHost CremaHost => this.DataBase.CremaHost;

        public DataBase DataBase
        {
            get
            {
                if (this.dataBase == null)
                    throw new InvalidOperationException(Resources.Exception_InvalidObject);
                return this.dataBase;
            }
        }

        public CremaDispatcher Dispatcher => this.dataBase?.Dispatcher;

        public new ITypeItem this[string itemPath] => base[itemPath] as ITypeItem;

        public event ItemsCreatedEventHandler<ITypeItem> ItemsCreated
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsCreated += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsCreated -= value;
            }
        }

        public event ItemsRenamedEventHandler<ITypeItem> ItemsRenamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsRenamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsRenamed -= value;
            }
        }

        public event ItemsMovedEventHandler<ITypeItem> ItemsMoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsMoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsMoved -= value;
            }
        }

        public event ItemsDeletedEventHandler<ITypeItem> ItemsDeleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsDeleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsDeleted -= value;
            }
        }

        public event ItemsEventHandler<ITypeItem> ItemsChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsChanged -= value;
            }
        }

        public event ItemsEventHandler<ITypeItem> ItemsAccessChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsAccessChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsAccessChanged -= value;
            }
        }

        public event ItemsEventHandler<ITypeItem> ItemsLockChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsLockChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsLockChanged -= value;
            }
        }

        protected virtual void OnItemsCreated(ItemsCreatedEventArgs<ITypeItem> e)
        {
            this.itemsCreated?.Invoke(this, e);
        }

        protected virtual void OnItemsRenamed(ItemsRenamedEventArgs<ITypeItem> e)
        {
            this.itemsRenamed?.Invoke(this, e);
        }

        protected virtual void OnItemsMoved(ItemsMovedEventArgs<ITypeItem> e)
        {
            this.itemsMoved?.Invoke(this, e);
        }

        protected virtual void OnItemsDeleted(ItemsDeletedEventArgs<ITypeItem> e)
        {
            this.itemsDeleted?.Invoke(this, e);
        }

        protected virtual void OnItemsChanged(ItemsEventArgs<ITypeItem> e)
        {
            this.itemsChanged?.Invoke(this, e);
        }

        protected virtual void OnItemsAccessChanged(ItemsEventArgs<ITypeItem> e)
        {
            this.itemsAccessChanged?.Invoke(this, e);
        }

        protected virtual void OnItemsLockChanged(ItemsEventArgs<ITypeItem> e)
        {
            this.itemsLockChanged?.Invoke(this, e);
        }

        protected override IEnumerable<ITableInfoProvider> GetTables()
        {
            return this.DataBase.TableContext.Tables;
        }

        private void Sign(Authentication authentication, ResultBase result)
        {
            result.Validate(authentication);
        }

        private void Sign<T>(Authentication authentication, ResultBase<T> result)
        {
            result.Validate(authentication);
        }

        private void Initialize(DataBaseMetaData metaData)
        {
            foreach (var item in metaData.TypeCategories)
            {
                if (item.Path == this.Root.Path)
                    continue;
                this.Categories.Prepare(item.Path);
            }

            foreach (var item in metaData.Types)
            {
                var typeInfo = item.TypeInfo;
                var typeState = item.TypeState;
                var type = this.Types.AddNew(Authentication.System, typeInfo.Name, typeInfo.CategoryPath);
                type.Initialize(typeInfo);
                type.SetTypeState(typeState);
            }

            foreach (var item in metaData.TypeCategories)
            {
                var category = this.Categories[item.Path];

                if (item.AccessInfo.IsPrivate == true && item.AccessInfo.IsInherited == false)
                {
                    category.SetAccessInfo(item.AccessInfo);
                }

                if (item.LockInfo.IsLocked == true && item.LockInfo.IsInherited == false)
                {
                    category.SetLockInfo(item.LockInfo);
                }
            }

            foreach (var item in metaData.Types)
            {
                var itemName = new ItemName(item.Path);
                var type = this.Types[itemName.Name];

                if (item.AccessInfo.IsPrivate == true && item.AccessInfo.IsInherited == false)
                {
                    type.SetAccessInfo(item.AccessInfo);
                }

                if (item.LockInfo.IsLocked == true && item.LockInfo.IsInherited == false)
                {
                    type.SetLockInfo(item.LockInfo);
                }
            }
        }

        #region ITypeContext

        bool ITypeContext.Contains(string itemPath)
        {
            return this.Contains(itemPath);
        }

        ITypeCollection ITypeContext.Types => this.Types;

        ITypeCategoryCollection ITypeContext.Categories => this.Categories;

        ITypeCategory ITypeContext.Root => this.Root;

        ITypeItem ITypeContext.this[string itemPath] => this[itemPath] as ITypeItem;

        #endregion

        #region IEnumerable

        IEnumerator<ITypeItem> IEnumerable<ITypeItem>.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item as ITypeItem;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item as ITypeItem;
            }
        }

        #endregion 

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.DataBase as IDataBase).GetService(serviceType);
        }

        #endregion
    }
}
