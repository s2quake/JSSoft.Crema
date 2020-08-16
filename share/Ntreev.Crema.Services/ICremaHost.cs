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
using System;
using System.Security;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    public interface ICremaHost : IServiceProvider, IDisposable, IDispatcherObject
    {
#if CLIENT
        Task<Guid> OpenAsync(string address, string userID, SecureString password);

        string Address { get; }

        string UserID { get; }

        Authority Authority { get; }

#elif SERVER
        Task<Guid> OpenAsync();

        Task<Guid> LoginAsync(string userID, SecureString password);

        Task<Authentication> AuthenticateAsync(Guid authenticationToken);

        Task LogoutAsync(Authentication authentication);

        string GetPath(CremaPath pathType, params string[] paths);

#endif
        event EventHandler Opening;

        event EventHandler Opened;

        event CloseRequestedEventHandler CloseRequested;

        event EventHandler Closing;

        event ClosedEventHandler Closed;

        event EventHandler Disposed;

        //void SaveConfigs();

        Task CloseAsync(Guid token);

        Task ShutdownAsync(Authentication authentication, int milliseconds, ShutdownType shutdownType, string message);

        Task CancelShutdownAsync(Authentication authentication);

        ServiceState ServiceState { get; }
    }
}