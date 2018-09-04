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

using Ntreev.Library.ObjectModel;
using Ntreev.Crema.ServiceModel;
using System;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Threading;
using System.Security;

namespace Ntreev.Crema.Services
{
    public interface IUserCategory : IServiceProvider, IDispatcherObject, IExtendedProperties
    {
        Task RenameAsync(Authentication authentication, string newName);

        Task MoveAsync(Authentication authentication, string parentPath);

        Task DeleteAsync(Authentication authentication);

        Task<IUserCategory> AddNewCategoryAsync(Authentication authentication, string name);

        Task<IUser> AddNewUserAsync(Authentication authentication, string userID, SecureString password, string userName, Authority authority);

        string Name { get; }

        string Path { get; }

        IUserCategory Parent { get; }

        IContainer<IUserCategory> Categories { get; }

        IContainer<IUser> Users { get; }

        event EventHandler Renamed;

        event EventHandler Moved;

        event EventHandler Deleted;
    }
}
