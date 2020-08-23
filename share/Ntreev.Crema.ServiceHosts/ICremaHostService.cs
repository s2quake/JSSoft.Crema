﻿//Released under the MIT License.
//
//Copyright Async(c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files Async(the "Software"), to deal in the Software without restriction, including without limitation the 
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

using JSSoft.Communication;
using Ntreev.Crema.ServiceModel;
using System;
using System.Threading.Tasks;

namespace Ntreev.Crema.ServiceHosts
{
    [ServiceContract(PerPeer = true)]
    public interface ICremaHostService
    {
        [OperationContract]
        Task<ResultBase<Guid>> SubscribeAsync(string userID, byte[] password, string version, string platformID, string culture);

        [OperationContract]
        Task<ResultBase> UnsubscribeAsync();

        [OperationContract]
        Task<ResultBase<string>> GetVersionAsync();

        [OperationContract]
        Task<ResultBase<bool>> IsOnlineAsync(string userID, byte[] password);

        [OperationContract]
        Task<ResultBase<DataBaseInfo[]>> GetDataBaseInfosAsync();

        [OperationContract]
        Task<ResultBase<ServiceInfo>> GetServiceInfoAsync();

        [OperationContract]
        Task<ResultBase> ShutdownAsync(int milliseconds, ShutdownType shutdownType, string message);

        [OperationContract]
        Task<ResultBase> CancelShutdownAsync();

        [OperationContract]
        Task<bool> IsAliveAsync();
    }
}
