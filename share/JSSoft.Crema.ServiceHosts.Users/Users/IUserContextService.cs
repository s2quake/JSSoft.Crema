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
using JSSoft.Communication;
using JSSoft.Crema.ServiceModel;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts.Users
{
    [ServiceContract(PerPeer = true)]
    public partial interface IUserContextService
    {
        [OperationContract]
        Task<ResultBase<UserContextMetaData>> SubscribeAsync(Guid token);

        [OperationContract]
        Task<ResultBase> UnsubscribeAsync(Guid token);

        [OperationContract]
        Task<ResultBase<UserInfo>> NewUserAsync(Guid authenticationToken, string userID, string categoryPath, byte[] password, string userName, Authority authority);

        [OperationContract]
        Task<ResultBase> NewUserCategoryAsync(Guid authenticationToken, string categoryPath);

        [OperationContract]
        Task<ResultBase> RenameUserItemAsync(Guid authenticationToken, string itemPath, string newName);

        [OperationContract]
        Task<ResultBase> MoveUserItemAsync(Guid authenticationToken, string itemPath, string parentPath);

        [OperationContract]
        Task<ResultBase> DeleteUserItemAsync(Guid authenticationToken, string itemPath);

        [OperationContract]
        Task<ResultBase<UserInfo>> SetUserNameAsync(Guid authenticationToken, string userID, byte[] password, string userName);

        [OperationContract]
        Task<ResultBase<UserInfo>> SetPasswordAsync(Guid authenticationToken, string userID, byte[] password, byte[] newPassword);

        [OperationContract]
        Task<ResultBase<UserInfo>> ResetPasswordAsync(Guid authenticationToken, string userID);

        [OperationContract]
        Task<ResultBase> KickAsync(Guid authenticationToken, string userID, string comment);

        [OperationContract]
        Task<ResultBase<BanInfo>> BanAsync(Guid authenticationToken, string userID, string comment);

        [OperationContract]
        Task<ResultBase> UnbanAsync(Guid authenticationToken, string userID);

        [OperationContract]
        Task<ResultBase> SendMessageAsync(Guid authenticationToken, string userID, string message);

        [OperationContract]
        Task<ResultBase> NotifyMessageAsync(Guid authenticationToken, string[] userIDs, string message);
    }
}
