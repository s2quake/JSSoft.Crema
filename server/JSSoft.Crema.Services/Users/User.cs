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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users.Arguments;
using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Users
{
    class User : UserBase<User, UserCategory, UserCollection, UserCategoryCollection, UserContext>,
        IUser, IUserItem, IInfoProvider, IStateProvider
    {
        public User()
        {

        }

        public async Task<Guid> LoginAsync(SecureString password)
        {
            try
            {
                if (password is null)
                    throw new ArgumentNullException(nameof(password));

                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(Authentication.System, this, nameof(LoginAsync), this);
                    this.ValidateLogin(password);
                    var users = new User[] { this };
                    var authentication = new Authentication(new UserAuthenticationProvider(this), Guid.NewGuid());
                    var taskID = GuidUtility.FromName($"{authentication.Token}");
                    if (this.Authentication != null)
                    {
                        throw new CremaException("b722d687-0a8d-4999-ad54-cf38c0c25d6f");
                    }
                    this.Authentication = authentication;
                    this.IsOnline = true;
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeUsersStateChangedEvent(this.Authentication, users);
                    this.Container.InvokeUsersLoggedInEvent(this.Authentication, users);
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                    return this.Authentication.Token;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> LogoutAsync(Authentication authentication)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));

                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LogoutAsync), this);
                    this.ValidateLogout(authentication);
                    var users = new User[] { this };
                    var taskID = this.Authentication.Token;
                    this.CremaHost.Sign(authentication);
                    this.Authentication.InvokeExpiredEvent(authentication.ID, string.Empty);
                    this.Authentication = null;
                    this.IsOnline = false;
                    this.Container.InvokeUsersStateChangedEvent(authentication, users);
                    this.Container.InvokeUsersLoggedOutEvent(authentication, users, CloseInfo.Empty);
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                    return taskID;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task RenameAsync(Authentication authentication, string newName)
        {
            if (authentication is null)
                throw new ArgumentNullException(nameof(authentication));
            if (authentication.IsExpired == true)
                throw new AuthenticationExpiredException(nameof(authentication));
            if (newName is null)
                throw new ArgumentNullException(nameof(newName));
            throw new NotImplementedException();
        }

        public async Task<Guid> MoveAsync(Authentication authentication, string categoryPath)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (categoryPath is null)
                    throw new ArgumentNullException(nameof(categoryPath));

                this.ValidateExpired();
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(MoveAsync), this, categoryPath);
                    this.ValidateMove(authentication, categoryPath);
                    return new UserMoveArguments(this, categoryPath);
                });
                var taskID = Guid.NewGuid();
                await this.Container.InvokeUserMoveAsync(authentication, args);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    base.Move(authentication, categoryPath);
                    this.Container.InvokeUsersMovedEvent(authentication, args.Items, args.OldPaths, args.OldCategoryPaths);
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> DeleteAsync(Authentication authentication)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));

                this.ValidateExpired();
                var container = this.Container;
                var repository = this.Repository;
                var cremaHost = this.CremaHost;
                var context = this.Context;
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    this.ValidateDelete(authentication);
                    this.CremaHost.Sign(authentication);
                    return new UserDeleteArguments(this);
                });
                var taskID = Guid.NewGuid();
                await this.Container.InvokeUserDeleteAsync(authentication, args);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.Delete(authentication);
                    cremaHost.Sign(authentication);
                    container.InvokeUsersDeletedEvent(authentication, args.Items, args.OldPaths);
                    context.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetUserNameAsync(Authentication authentication, SecureString password, string userName)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (password is null)
                    throw new ArgumentNullException(nameof(password));
                if (userName is null)
                    throw new ArgumentNullException(nameof(userName));

                this.ValidateExpired();
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetUserNameAsync), this);
                    this.ValidateSetUserName(authentication, password, userName);
                    this.CremaHost.Sign(authentication);
                    return new UserSetNameArguments(this, userName);
                });
                var taskID = Guid.NewGuid();
                var userInfo = await this.Container.InvokeUserNameSetAsync(authentication, args);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    base.UpdateUserInfo(userInfo);
                    this.Container.InvokeUsersChangedEvent(authentication, args.Items);
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetPasswordAsync(Authentication authentication, SecureString password, SecureString newPassword)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (password is null)
                    throw new ArgumentNullException(nameof(password));
                if (newPassword is null)
                    throw new ArgumentNullException(nameof(newPassword));

                this.ValidateExpired();
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPasswordAsync), this);
                    this.ValidateSetPassword(authentication, password, newPassword);
                    this.CremaHost.Sign(authentication);
                    return new UserSetPasswordArguments(this, newPassword);
                });
                var taskID = Guid.NewGuid();
                var userInfo = await this.Container.InvokeUserPasswordSetAsync(authentication, args);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    this.Password = UserContext.StringToSecureString(userInfo.Password);
                    base.UpdateUserInfo((UserInfo)userInfo);
                    this.Container.InvokeUsersChangedEvent(authentication, args.Items);
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> ResetPasswordAsync(Authentication authentication)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));

                this.ValidateExpired();
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPasswordAsync), this);
                    this.ValidateResetPassword(authentication);
                    this.CremaHost.Sign(authentication);
                    return new UserResetPasswordArguments(this);
                });
                var taskID = Guid.NewGuid();
                var userInfo = await this.Container.InvokeUserPasswordResetAsync(authentication, args);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    this.Password = UserContext.StringToSecureString(userInfo.Password);
                    base.UpdateUserInfo((UserInfo)userInfo);
                    this.Container.InvokeUsersChangedEvent(authentication, args.Items);
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SendMessageAsync(Authentication authentication, string message)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (message is null)
                    throw new ArgumentNullException(nameof(message));

                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SendMessageAsync), this, message);
                    this.ValidateSendMessage(authentication, message);
                    var taskID = Guid.NewGuid();
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeSendMessageEvent(authentication, this, message);
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                    return taskID;
                });

            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> KickAsync(Authentication authentication, string comment)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (comment is null)
                    throw new ArgumentNullException(nameof(comment));

                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(KickAsync), this, comment);
                    this.ValidateKick(authentication, comment);
                    this.CremaHost.Sign(authentication);
                    var taskID = Guid.NewGuid();
                    var items = new User[] { this };
                    var comments = Enumerable.Repeat(comment, items.Length).ToArray();
                    this.IsOnline = false;
                    this.Authentication.InvokeExpiredEvent(authentication.ID, comment);
                    this.Authentication = null;
                    this.Container.InvokeUsersKickedEvent(authentication, items, comments);
                    this.Container.InvokeUsersStateChangedEvent(authentication, items);
                    this.Container.InvokeUsersLoggedOutEvent(authentication, items, new CloseInfo(CloseReason.Kicked, comment));
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                    return taskID;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> BanAsync(Authentication authentication, string comment)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (comment is null)
                    throw new ArgumentNullException(nameof(comment));

                this.ValidateExpired();
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BanAsync), this, comment);
                    this.ValidateBan(authentication, comment);
                    return new UserBanArguments(this, comment);
                });
                var taskID = Guid.NewGuid();
                var userInfo = await this.Container.InvokeUserBanAsync(authentication, args);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.Ban(authentication, (BanInfo)userInfo.BanInfo);
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeUsersBannedEvent(authentication, args.Items, args.Comments);
                    if (this.IsOnline == true)
                    {
                        this.Authentication.InvokeExpiredEvent(authentication.ID, comment);
                        this.Authentication = null;
                        this.IsOnline = false;
                        this.Container.InvokeUsersStateChangedEvent(authentication, args.Items);
                        this.Container.InvokeUsersLoggedOutEvent(authentication, args.Items, new CloseInfo(CloseReason.Banned, comment));
                    }
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> UnbanAsync(Authentication authentication)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));

                this.ValidateExpired();
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnbanAsync), this);
                    this.ValidateUnban(authentication);
                    return new UserUnbanArguments(this);
                });
                var taskID = Guid.NewGuid();
                await this.Container.InvokeUserUnbanAsync(authentication, args);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    base.Unban(authentication);
                    this.Container.InvokeUsersUnbannedEvent(authentication, args.Items);
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public bool VerifyPassword(SecureString password)
        {
            return UserContext.SecureStringToString(this.Password) == UserContext.SecureStringToString(password).Encrypt();
        }

        public Authentication Authentication { get; set; }

        public string ID => base.Name;

        public string UserName => base.UserInfo.Name;

        public new string Path => base.Path;

        public new Authority Authority => base.Authority;

        public new UserInfo UserInfo => base.UserInfo;

        public new UserState UserState => base.UserState;

        public new BanInfo BanInfo => base.BanInfo;

        public bool IsBanned => this.BanInfo.Path != string.Empty;

        public SecureString Password { get; set; }

        public UserSerializationInfo SerializationInfo
        {
            get
            {
                var userInfo = (UserSerializationInfo)base.UserInfo;
                userInfo.Password = UserContext.SecureStringToString(this.Password);
                userInfo.BanInfo = (BanSerializationInfo)base.BanInfo;
                return userInfo;
            }
        }

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public CremaHost CremaHost => this.Context.CremaHost;

        public UserRepositoryHost Repository => this.Context.Repository;

        public IObjectSerializer Serializer => this.Context.Serializer;

        public new event EventHandler Renamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Renamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Renamed -= value;
            }
        }

        public new event EventHandler Moved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Moved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Moved -= value;
            }
        }

        public new event EventHandler Deleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Deleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Deleted -= value;
            }
        }

        public new event EventHandler UserInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.UserInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.UserInfoChanged -= value;
            }
        }

        public new event EventHandler UserStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.UserStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.UserStateChanged -= value;
            }
        }

        public new event EventHandler UserBanInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.UserBanInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.UserBanInfoChanged -= value;
            }
        }

        protected override void OnDeleted(EventArgs e)
        {
            if (this.IsOnline == true)
            {
                throw new InvalidOperationException(Resources.Exception_UserIDIsConnecting);
            }

            base.OnDeleted(e);
        }

        private void ValidateUpdate(Authentication authentication)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
            {
                throw new PermissionDeniedException();
            }
        }

        [Obsolete]
        private void ValidateUserInfoChange(Authentication authentication, SecureString password, SecureString newPassword, string userName, Authority? authority)
        {
            if (this.ID != authentication.ID)
            {
                var isAdmin = authentication.Types.HasFlag(AuthenticationType.Administrator);

                if (base.UserInfo.ID == Authentication.AdminID)
                    throw new InvalidOperationException(Resources.Exception_AdminCanChangeAdminInfo);

                if (authority.HasValue == true && this.IsOnline == true)
                    throw new InvalidOperationException(Resources.Exception_OnlineUserAuthorityCannotChanged);

                if (newPassword != null)
                {
                    if (isAdmin == false)
                    {
                        if (this.VerifyPassword(password) == false)
                            throw new ArgumentException(Resources.Exception_IncorrectPassword, nameof(password));
                        if (this.VerifyPassword(newPassword) == true)
                            throw new ArgumentException(Resources.Exception_CannotChangeToOldPassword, nameof(newPassword));
                    }
                    else
                    {
                        if (this.VerifyPassword(newPassword) == true)
                            throw new ArgumentException(Resources.Exception_CannotChangeToOldPassword, nameof(newPassword));
                    }
                }
            }
            else
            {
                if (newPassword != null)
                {
                    if (newPassword == null || this.VerifyPassword(password) == false)
                        throw new ArgumentException(Resources.Exception_IncorrectPassword, nameof(password));
                    if (this.VerifyPassword(newPassword) == true)
                        throw new ArgumentException(Resources.Exception_CannotChangeToOldPassword, nameof(newPassword));
                }
                if (authority.HasValue == true)
                    throw new ArgumentException(Resources.Exception_CannotChangeYourAuthority);
            }
        }

        private void ValidateSetUserName(Authentication authentication, SecureString password, string userName)
        {
            if (this.ID != authentication.ID)
                throw new InvalidOperationException("cannot set user name");
            if (this.VerifyPassword(password) == false)
                throw new ArgumentException(Resources.Exception_IncorrectPassword, nameof(password));
            if (userName == string.Empty)
                throw new ArgumentException(Resources.Exception_EmptyStringIsNotAllowed);
        }

        private void ValidateSetPassword(Authentication authentication, SecureString password, SecureString newPassword)
        {
            if (base.UserInfo.ID != authentication.ID)
                throw new InvalidOperationException();
            if (this.VerifyPassword(password) == false)
                throw new ArgumentException("wrong password", nameof(password));
            if (this.VerifyPassword(newPassword) == true)
                throw new ArgumentException(Resources.Exception_CannotChangeToOldPassword, nameof(newPassword));
        }

        private void ValidateResetPassword(Authentication authentication)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();

            if (base.UserInfo.ID != authentication.ID && base.UserInfo.ID == Authentication.AdminID)
                throw new PermissionDeniedException(Resources.Exception_AdminCanChangeAdminInfo);
        }

        private void ValidateSendMessage(Authentication authentication, string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (message == string.Empty)
                throw new ArgumentException(Resources.Exception_EmptyStringCannotSend, nameof(message));
            if (this.IsOnline == false)
                throw new InvalidOperationException(Resources.Exception_CannotSendMessageToOfflineUser);
        }

        private void ValidateMove(Authentication authentication, string categoryPath)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();

            if (this.Authentication != authentication)
            {
                if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                {
                    throw new PermissionDeniedException();
                }
            }

            base.ValidateMove(authentication, categoryPath);
        }

        private void ValidateDelete(Authentication authentication)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();

            if (base.UserInfo.ID == Authentication.AdminID)
                throw new InvalidOperationException(Resources.Exception_AdminCannotDeleted);

            if (this.ID == authentication.ID)
                throw new InvalidOperationException(Resources.Exception_CannotDeleteYourself);

            if (this.IsOnline == true)
                throw new InvalidOperationException(Resources.Exception_LoggedInUserCannotDelete);

            base.ValidateDelete();
        }

        private void ValidateLogin(SecureString password)
        {
            if (this.IsBanned == true)
                throw new InvalidOperationException(Resources.Exception_BannedUserCannotLogin);
        }

        private void ValidateLogout(Authentication authentication)
        {
            if (this.IsOnline == false)
                throw new InvalidOperationException(Resources.Exception_UserIsNotLoggedIn);
            if (authentication.ID != this.ID)
                throw new PermissionDeniedException();
        }

        private void ValidateBan(Authentication authentication, string comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));
            if (comment == string.Empty)
                throw new ArgumentException(nameof(comment), Resources.Exception_EmptyStringIsNotAllowed);
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (this.IsBanned == true)
                throw new InvalidOperationException(Resources.Exception_UserIsAlreadyBanned);
            if (this.Authority == Authority.Admin)
                throw new PermissionDeniedException(Resources.Exception_AdminCannotBanned);
            if (authentication.ID == this.ID)
                throw new PermissionDeniedException(Resources.Exception_CannotBanYourself);
        }

        private void ValidateUnban(Authentication authentication)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (this.IsBanned == false)
                throw new InvalidOperationException(Resources.Exception_UserIsNotBanned);
        }

        private void ValidateKick(Authentication authentication, string comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));
            if (comment == string.Empty)
                throw new ArgumentException(nameof(comment), Resources.Exception_EmptyStringIsNotAllowed);
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (this.IsOnline == false)
                throw new InvalidOperationException(Resources.Exception_OfflineUserCannotKicked);
            if (authentication.ID == this.ID)
                throw new InvalidOperationException(Resources.Exception_CannotKickYourself);
        }

        #region IUser

        Task IUser.MoveAsync(Authentication authentication, string categoryPath)
        {
            return this.MoveAsync(authentication, categoryPath);
        }

        Task IUser.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        Task IUser.SetUserNameAsync(Authentication authentication, SecureString password, string userName)
        {
            return this.SetUserNameAsync(authentication, password, userName);
        }

        Task IUser.SetPasswordAsync(Authentication authentication, SecureString password, SecureString newPassword)
        {
            return this.SetPasswordAsync(authentication, password, newPassword);
        }

        Task IUser.ResetPasswordAsync(Authentication authentication)
        {
            return this.ResetPasswordAsync(authentication);
        }

        Task IUser.KickAsync(Authentication authentication, string comment)
        {
            return this.KickAsync(authentication, comment);
        }

        Task IUser.BanAsync(Authentication authentication, string comment)
        {
            return this.BanAsync(authentication, comment);
        }

        Task IUser.UnbanAsync(Authentication authentication)
        {
            return this.UnbanAsync(authentication);
        }

        Task IUser.SendMessageAsync(Authentication authentication, string message)
        {
            return this.SendMessageAsync(authentication, message);
        }

        string IUser.ID => this.ID;

        IUserCategory IUser.Category => this.Category;

        #endregion

        #region IUserItem

        Task IUserItem.MoveAsync(Authentication authentication, string parentPath)
        {
            return this.MoveAsync(authentication, parentPath);
        }

        Task IUserItem.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        string IUserItem.Name => this.ID;

        IUserItem IUserItem.Parent => this.Category;

        IEnumerable<IUserItem> IUserItem.Childs
        {
            get
            {
                this.Dispatcher.VerifyAccess();
                return Enumerable.Empty<IUserItem>();
            }
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.Context as IUserContext).GetService(serviceType);
        }

        #endregion

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => this.UserInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => this.UserState;

        #endregion
    }
}
