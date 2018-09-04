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
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users.Serializations;
using Ntreev.Library;
using Ntreev.Library.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Users
{
    class User : UserBase<User, UserCategory, UserCollection, UserCategoryCollection, UserContext>,
        IUser, IUserItem, IInfoProvider, IStateProvider
    {
        public User()
        {

        }

        public Task RenameAsync(Authentication authentication, string newName)
        {
            throw new NotSupportedException();
        }

        public Task MoveAsync(Authentication authentication, string categoryPath)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(Move), this, categoryPath);
                    this.ValidateMove(authentication, categoryPath);
                    this.Sign(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldCategoryPaths = items.Select(item => item.Category.Path).ToArray();
                    this.Container.InvokeUserMove(authentication, this, categoryPath);
                    base.Move(authentication, categoryPath);
                    this.Container.InvokeUsersMovedEvent(authentication, items, oldPaths, oldCategoryPaths);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task DeleteAsync(Authentication authentication)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(Delete), this);
                    this.ValidateDelete(authentication);
                    this.Sign(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var container = this.Container;
                    container.InvokeUserDelete(authentication, this);
                    base.Delete(authentication);
                    container.InvokeUsersDeletedEvent(authentication, items, oldPaths);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<Authentication> LoginAsync(SecureString password)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(Authentication.System, this, nameof(LoginAsync), this);
                    this.ValidateLogin(password);

                    var users = new User[] { this };
                    var authentication = new Authentication(new UserAuthenticationProvider(this), Guid.NewGuid());
                    this.Sign(authentication);

                    if (this.Authentication != null)
                    {
                        var message = "다른 기기에서 동일한 아이디로 접속하였습니다.";
                        var closeInfo = new CloseInfo(CloseReason.Reconnected, message);
                        this.Authentication.InvokeExpiredEvent(this.ID, message);
                        this.Container.InvokeUsersLoggedOutEvent(this.Authentication, users, closeInfo);
                    }

                    this.Authentication = authentication;
                    this.IsOnline = true;
                    this.Container.InvokeUsersStateChangedEvent(this.Authentication, users);
                    this.Container.InvokeUsersLoggedInEvent(this.Authentication, users);
                    return this.Authentication;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task LogoutAsync(Authentication authentication)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LogoutAsync), this);
                    this.ValidateLogout(authentication);
                    this.Sign(authentication);
                    var users = new User[] { this };
                    this.Authentication.InvokeExpiredEvent(authentication.ID, string.Empty);
                    this.Authentication = null;
                    this.IsOnline = false;
                    this.Container.InvokeUsersStateChangedEvent(authentication, users);
                    this.Container.InvokeUsersLoggedOutEvent(authentication, users, CloseInfo.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task BanAsync(Authentication authentication, string comment)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(Ban), this, comment);
                    this.ValidateBan(authentication, comment);
                    this.Sign(authentication);
                    var users = new User[] { this };
                    var comments = Enumerable.Repeat(comment, users.Length).ToArray();
                    var banInfo = new BanInfo() { Path = this.Path, Comment = comment, SignatureDate = authentication.SignatureDate };
                    var isOnline = this.IsOnline;
                    var userInfo = new UserSerializationInfo(this.SerializationInfo)
                    {
                        BanInfo = (BanSerializationInfo)banInfo
                    };
                    this.Container.InvokeUserBan(authentication, this, userInfo);
                    base.Ban(authentication, banInfo);
                    this.IsOnline = false;
                    this.Container.InvokeUsersBannedEvent(authentication, users, comments);
                    if (isOnline == true)
                    {
                        this.Authentication.InvokeExpiredEvent(authentication.ID, comment);
                        this.Authentication = null;
                        this.Container.InvokeUsersStateChangedEvent(authentication, users);
                        this.Container.InvokeUsersLoggedOutEvent(authentication, users, new CloseInfo(CloseReason.Banned, comment));
                    }
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task UnbanAsync(Authentication authentication)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(Unban), this);
                    this.ValidateUnban(authentication);
                    this.Sign(authentication);
                    var userInfo = new UserSerializationInfo(this.SerializationInfo)
                    {
                        BanInfo = (BanSerializationInfo)BanInfo.Empty
                    };
                    this.Container.InvokeUserUnban(authentication, this, userInfo);
                    base.Unban(authentication);
                    this.Container.InvokeUsersUnbannedEvent(authentication, new User[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task KickAsync(Authentication authentication, string comment)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(KickAsync), this, comment);
                    this.ValidateKick(authentication, comment);
                    this.Sign(authentication);
                    var users = new User[] { this };
                    var comments = Enumerable.Repeat(comment, users.Length).ToArray();
                    this.Container.InvokeUserKick(authentication, this, comment);
                    this.IsOnline = false;
                    this.Authentication.InvokeExpiredEvent(authentication.ID, comment);
                    this.Authentication = null;
                    this.Container.InvokeUsersKickedEvent(authentication, users, comments);
                    this.Container.InvokeUsersStateChangedEvent(authentication, users);
                    this.Container.InvokeUsersLoggedOutEvent(authentication, users, new CloseInfo(CloseReason.Kicked, comment));
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task ChangeUserInfoAsync(Authentication authentication, SecureString password, SecureString newPassword, string userName, Authority? authority)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ChangeUserInfoAsync), this, password, newPassword, userName, authority);
                    this.ValidateUserInfoChange(authentication, password, newPassword, userName, authority);
                    this.Sign(authentication);
                    var serializationInfo = this.SerializationInfo;
                    if (newPassword != null)
                        serializationInfo.Password = UserContext.SecureStringToString(newPassword).Encrypt();
                    if (userName != null)
                        serializationInfo.Name = userName;
                    if (authority.HasValue)
                        serializationInfo.Authority = authority.Value;
                    if (object.Equals(serializationInfo, this.SerializationInfo) == true)
                        return;
                    var items = EnumerableUtility.One(this).ToArray();
                    serializationInfo.ModificationInfo = new SignatureDate(authentication.ID);
                    this.Container.InvokeUserChange(authentication, this, serializationInfo);
                    if (newPassword != null)
                        this.Password = UserContext.StringToSecureString(serializationInfo.Password);
                    base.UpdateUserInfo((UserInfo)serializationInfo);
                    this.Container.InvokeUsersChangedEvent(authentication, items);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SendMessageAsync(Authentication authentication, string message)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateSendMessage(authentication, message);
                    this.Sign(authentication);
                    this.Container.InvokeSendMessageEvent(authentication, this, message);
                });
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

        public string ItemPath => this.Context.GenerateUserPath(this.Category.Path, base.Name);

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

            if (this.IsOnline == true)
                throw new InvalidOperationException(Resources.Exception_LoggedInUserCannotDelete);

            if (this.ID == authentication.ID)
                throw new InvalidOperationException(Resources.Exception_CannotDeleteYourself);

            if (base.UserInfo.ID == Authentication.AdminID)
                throw new InvalidOperationException(Resources.Exception_AdminCannotDeleted);

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
                throw new ArgumentNullException(nameof(comment), Resources.Exception_EmptyStringIsNotAllowed);
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
                throw new ArgumentNullException(nameof(comment), Resources.Exception_EmptyStringIsNotAllowed);
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (this.IsOnline == false)
                throw new InvalidOperationException(Resources.Exception_OfflineUserCannotKicked);
            if (authentication.ID == this.ID)
                throw new PermissionDeniedException(Resources.Exception_CannotKickYourself);
        }

        private void Sign(Authentication authentication)
        {
            authentication.Sign();
        }

        #region IUser

        string IUser.ID => this.ID;

        IUserCategory IUser.Category => this.Category;

        #endregion

        #region IUserItem

        string IUserItem.Name => this.ID;

        IUserItem IUserItem.Parent => this.Category;

        IEnumerable<IUserItem> IUserItem.Childs => Enumerable.Empty<IUserItem>();

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
