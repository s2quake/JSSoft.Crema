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
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Domains
{
    class DomainUser : DomainUserBase, IDomainUser
    {
        private readonly Domain domain;
        readonly bool metadata;

        public DomainUser(Domain domain, DomainUserInfo domainUserInfo, DomainUserState domainUserState, bool metadata)
        {
            this.domain = domain;
            base.DomainUserInfo = domainUserInfo;
            base.DomainUserState = domainUserState;
            this.metadata = metadata;
        }

        public Task BeginEditAsync(Authentication authentication, DomainLocationInfo location)
        {
            return this.domain.BeginUserEditAsync(authentication, location);
        }

        public Task EndEditAsync(Authentication authentication)
        {
            return this.domain.EndUserEditAsync(authentication);
        }

        public Task SetLocationAsync(Authentication authentication, DomainLocationInfo location)
        {
            return this.domain.SetUserLocationAsync(authentication, location);
        }

        public Task KickAsync(Authentication authentication, string comment)
        {
            return this.domain.KickAsync(authentication, base.DomainUserInfo.UserID, comment);
        }

        public Task SetOwnerAsync(Authentication authentication)
        {
            return this.domain.SetOwnerAsync(authentication, base.DomainUserInfo.UserID);
        }

        public DomainUserMetaData GetMetaData(Authentication authentication)
        {
            this.Dispatcher.VerifyAccess();

            var metaData = new DomainUserMetaData()
            {
                DomainUserInfo = base.DomainUserInfo,
                DomainUserState = base.DomainUserState,
            };

            return metaData;
        }

        public string ID => base.DomainUserInfo.UserID;

        public CremaDispatcher Dispatcher => this.domain.Dispatcher;

        public new event EventHandler DomainUserInfoChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                base.DomainUserInfoChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                base.DomainUserInfoChanged -= value;
            }
        }

        public new event EventHandler DomainUserStateChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                base.DomainUserStateChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                base.DomainUserStateChanged -= value;
            }
        }

        #region Invisibles

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetDomainUserInfo(DomainUserInfo domainUserInfo)
        {
            base.DomainUserInfo = domainUserInfo;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetDomainUserState(DomainUserState domainUserState)
        {
            base.DomainUserState = domainUserState;
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IDomain))
                return this.domain;
            return (this.domain as IServiceProvider).GetService(serviceType);
        }

        #endregion
    }
}
