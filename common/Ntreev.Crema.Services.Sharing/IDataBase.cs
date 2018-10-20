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
using Ntreev.Crema.Data;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    public interface IDataBase : IAccessible, IPermission, ILockable, IServiceProvider, IDispatcherObject, IExtendedProperties
    {
        Task LoadAsync(Authentication authentication);

        Task UnloadAsync(Authentication authentication);

        Task EnterAsync(Authentication authentication);

        Task LeaveAsync(Authentication authentication);

        Task RenameAsync(Authentication authentication, string name);

        Task DeleteAsync(Authentication authentication);

        bool Contains(Authentication authentication);

        Task<LogInfo[]> GetLogAsync(Authentication authentication, string revision);

        Task RevertAsync(Authentication authentication, string revision);

        Task ImportAsync(Authentication authentication, CremaDataSet dataSet, string comment);

        Task<CremaDataSet> GetDataSetAsync(Authentication authentication, DataSetType dataSetType, string filterExpression, string revision);

        Task<ITransaction> BeginTransactionAsync(Authentication authentication);

        Task<IDataBase> CopyAsync(Authentication authentication, string newDataBaseName, string comment, bool force);

        ITypeContext TypeContext { get; }

        ITableContext TableContext { get; }

        string Name { get; }

        bool IsLoaded { get; }

        bool IsLocked { get; }

        bool IsPrivate { get; }

        Guid ID { get; }

        DataBaseInfo DataBaseInfo { get; }

        DataBaseState DataBaseState { get; }

        AuthenticationInfo[] AuthenticationInfos { get; }

        event EventHandler Renamed;

        event EventHandler Deleted;

        event EventHandler Loaded;

        event EventHandler Unloaded;

        event EventHandler Resetting;

        event EventHandler Reset;

        event EventHandler<AuthenticationEventArgs> AuthenticationEntered;

        event EventHandler<AuthenticationEventArgs> AuthenticationLeft;

        event EventHandler DataBaseInfoChanged;

        event EventHandler DataBaseStateChanged;

        event EventHandler LockChanged;

        event EventHandler AccessChanged;

        DataBaseMetaData GetMetaData(Authentication authentication);
    }
}
