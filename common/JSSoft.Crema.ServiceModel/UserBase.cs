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

using JSSoft.Crema.ServiceModel.Properties;
using JSSoft.Library.IO;
using JSSoft.Library.ObjectModel;
using System;
using System.Text.RegularExpressions;

namespace JSSoft.Crema.ServiceModel
{
    internal abstract class UserBase<_I, _C, _IC, _CC, _CT> : ItemBase<_I, _C, _IC, _CC, _CT>
        where _I : UserBase<_I, _C, _IC, _CC, _CT>
        where _C : UserCategoryBase<_I, _C, _IC, _CC, _CT>, new()
        where _IC : ItemContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CC : CategoryContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CT : ItemContext<_I, _C, _IC, _CC, _CT>
    {
        private UserInfo userInfo;
        private UserState userState;
        private UserFlags userFlags;
        private BanInfo banInfo = BanInfo.Empty;
        private bool isLoaded;

        public UserBase()
        {

        }

        public void UpdateUserInfo(UserInfo userInfo)
        {
            this.userInfo = userInfo;
            this.UpdateUserFlags();
            this.OnUserInfoChanged(EventArgs.Empty);
        }

        public void Initialize(UserInfo userInfo, BanInfo banInfo)
        {
            if (this.isLoaded == true)
                throw new InvalidOperationException();
            this.userInfo = userInfo;
            this.banInfo = banInfo;
            this.isLoaded = true;
            this.UpdateUserFlags();
            this.OnUserInfoChanged(EventArgs.Empty);
            this.OnUserBanInfoChanged(EventArgs.Empty);
        }

        public UserInfo UserInfo => this.userInfo;

        public BanInfo BanInfo
        {
            get
            {
                if (this.banInfo.UserID != string.Empty)
                {
                    this.banInfo.Path = this.Path;
                }
                return this.banInfo;
            }
            protected set
            {
                this.banInfo = value;
                this.UpdateUserFlags();
                this.OnUserBanInfoChanged(EventArgs.Empty);
            }
        }

        public UserState UserState
        {
            get => this.userState;
            set
            {
                if (this.userState == value)
                    return;
                this.userState = value;
                this.UpdateUserFlags();
                this.OnUserStateChanged(EventArgs.Empty);
            }
        }

        public UserFlags UserFlags => this.userFlags;

        public Authority Authority => this.userInfo.Authority;

        public Guid Guid { get; set; }

        public bool IsOnline
        {
            get => this.UserState.HasFlag(UserState.Online);
            set
            {
                if (value == true)
                    this.UserState |= UserState.Online;
                else
                    this.UserState &= ~UserState.Online;
            }
        }

        public event EventHandler UserInfoChanged;

        public event EventHandler UserStateChanged;

        public event EventHandler UserBanInfoChanged;

        protected void Move(IAuthentication authentication, string categoryPath)
        {
            this.Category = this.Context.Categories[categoryPath];
        }

        protected void Delete(IAuthentication authentication)
        {
            base.Dispose();
        }

        protected void Ban(IAuthentication authentication, BanInfo banInfo)
        {
            this.banInfo = banInfo;
            this.UpdateUserFlags();
            this.OnUserBanInfoChanged(EventArgs.Empty);
        }

        protected void Unban(IAuthentication authentication)
        {
            this.banInfo = BanInfo.Empty;
            this.UpdateUserFlags();
            this.OnUserBanInfoChanged(EventArgs.Empty);
        }

        protected virtual void OnUserInfoChanged(EventArgs e)
        {
            this.UserInfoChanged?.Invoke(this, e);
        }

        protected virtual void OnUserStateChanged(EventArgs e)
        {
            this.UserStateChanged?.Invoke(this, e);
        }

        protected virtual void OnUserBanInfoChanged(EventArgs e)
        {
            this.UserBanInfoChanged?.Invoke(this, e);
        }

        protected override void OnPathChanged(string oldPath, string newPath)
        {
            base.OnPathChanged(oldPath, newPath);

            if (this.Category != null)
            {
                this.userInfo.CategoryPath = this.Category == null ? PathUtility.Separator : this.Category.Path;
                this.OnUserInfoChanged(EventArgs.Empty);
            }

            if (this.banInfo.UserID != string.Empty)
            {
                this.banInfo.Path = Regex.Replace(this.banInfo.Path, "^" + oldPath, newPath);
                this.OnUserBanInfoChanged(EventArgs.Empty);
            }
        }

        protected void ValidateMove(IAuthentication authentication, string categoryPath)
        {
            NameValidator.ValidateCategoryPath(categoryPath);
            if (this.Category.Path == categoryPath)
                throw new ArgumentException(Resources.Exception_CannotMoveToSameFolder, nameof(categoryPath));
            var category = this.Context.Categories[categoryPath];
            if (category == null)
                throw new CategoryNotFoundException(categoryPath);
            base.ValidateMove(category);
        }

        private void UpdateUserFlags()
        {
            this.userFlags = UserFlags.None;
            if (this.userInfo.Authority == Authority.Admin)
                this.userFlags |= UserFlags.Admin;
            else if (this.userInfo.Authority == Authority.Member)
                this.userFlags |= UserFlags.Member;
            else if (this.userInfo.Authority == Authority.Guest)
                this.userFlags |= UserFlags.Guest;
            if (this.userState == UserState.Online)
                this.userFlags |= UserFlags.Online;
            else
                this.userFlags |= UserFlags.Offline;
            if (this.banInfo.IsBanned == true)
                this.userFlags |= UserFlags.Banned;
            else
                this.userFlags |= UserFlags.NotBanned;
        }
    }
}
