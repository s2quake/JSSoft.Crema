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

using JSSoft.Crema.ServiceHosts.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using System;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts
{
    class CremaHostService : CremaServiceItemBase<ICremaHostEventCallback>, ICremaHostService
    {
        private bool isSubscribed;
        private Guid authenticationToken;
        private Authentication authentication;
        private long index = 0;

        public CremaHostService(CremaService service, ICremaHostEventCallback callback)
            : base(service, callback)
        {
            this.LogService.Debug($"{nameof(CremaHostService)} Constructor");
            this.OwnerID = nameof(CremaHostService);
        }

        public async Task<ResultBase> SubscribeAsync(string version, string platformID, string culture)
        {
            var result = new ResultBase();
            var serverVersion = typeof(ICremaHost).Assembly.GetName().Version;
            var clientVersion = new Version(version);
            if (clientVersion < serverVersion)
                throw new ArgumentException(Resources.Exception_LowerVersion, nameof(version));

            this.isSubscribed = true;
            result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            this.LogService.Debug($"[{this.OwnerID}] {nameof(CremaHostService)} {nameof(SubscribeAsync)}");
            return result;
        }

        public async Task<ResultBase<Guid>> LoginAsync(string userID, byte[] password)
        {
            var result = new ResultBase<Guid>();
            try
            {
                this.authenticationToken = await this.CremaHost.LoginAsync(userID, ToSecureString(userID, password));
                this.authentication = await this.CremaHost.AuthenticateAsync(this.authenticationToken);
                this.OwnerID = this.authentication.ID;
                result.Value = this.authenticationToken;
                result.SignatureDate = this.authentication.SignatureDate;
                this.LogService.Debug($"[{this.OwnerID}] {nameof(CremaHostService)} {nameof(LoginAsync)}");
            }
            catch (Exception e)
            {
                this.OwnerID = $"{userID} - failed";
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> LogoutAsync()
        {
            await this.CremaHost.LogoutAsync(this.authentication);
            this.authentication = null;
            this.OwnerID = nameof(CremaHostService);
            this.LogService.Debug($"[{this.OwnerID}] {nameof(CremaHostService)} {nameof(LogoutAsync)}");
            return new ResultBase()
            {
                SignatureDate = new SignatureDateProvider(this.OwnerID).Provide()
            };
        }

        public async Task<ResultBase> UnsubscribeAsync()
        {
            var result = new ResultBase();
            try
            {
                this.isSubscribed = await Task.Run(() => false);
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
                this.LogService.Debug($"[{this.OwnerID}] {nameof(CremaHostService)} {nameof(UnsubscribeAsync)}");
            }
            catch (Exception e)
            {
                this.OwnerID = nameof(CremaHostService);
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<ServiceInfo>> GetServiceInfoAsync()
        {
            var result = new ResultBase<ServiceInfo>();
            try
            {
                result.Value = await Task.Run(() => this.Service.ServiceInfo);
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase<DataBaseInfo[]>> GetDataBaseInfosAsync()
        {
            var result = new ResultBase<DataBaseInfo[]>();
            try
            {
                result.Value = await this.DataBaseContext.Dispatcher.InvokeAsync(() => this.DataBaseContext.Select(item => item.DataBaseInfo).ToArray());
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase<string>> GetVersionAsync()
        {
            var result = new ResultBase<string>();
            try
            {
                result.Value = await Task.Run(() => AppUtility.ProductVersion.ToString());
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase<bool>> IsOnlineAsync(string userID, byte[] password)
        {
            var result = new ResultBase<bool>();
            try
            {
                var userContext = this.CremaHost.GetService(typeof(IUserContext)) as IUserContext;
                var text = Encoding.UTF8.GetString(password);
                result.Value = await userContext.IsOnlineUserAsync(userID, StringUtility.ToSecureString(StringUtility.Decrypt(text, userID)));
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> ShutdownAsync(int milliseconds, ShutdownType shutdownType, string message)
        {
            var result = new ResultBase();
            try
            {
                await this.CremaHost.ShutdownAsync(this.authentication, milliseconds, shutdownType, message);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> CancelShutdownAsync()
        {
            var result = new ResultBase();
            try
            {
                await this.CremaHost.CancelShutdownAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<bool> IsAliveAsync()
        {
            if (this.authentication == null)
                return false;
            this.LogService.Debug($"[{this.authentication}] {nameof(CremaHostService)}.{nameof(IsAliveAsync)} : {DateTime.Now}");
            await Task.Delay(1);
            return true;
        }

        private async void AuthenticationUtility_Disconnected(object sender, EventArgs e)
        {
            var authentication = sender as Authentication;
            if (this.authentication != null && this.authentication == authentication)
            {
                await this.CremaHost.LogoutAsync(this.authentication);
            }
        }

        protected override void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = signatureDate };
            this.Callback?.OnServiceClosed(callbackInfo, closeInfo);
        }

        private static SecureString ToSecureString(string userID, byte[] password)
        {
            var text = Encoding.UTF8.GetString(password);
            return StringToSecureString(StringUtility.Decrypt(text, userID));
        }

        private static SecureString StringToSecureString(string value)
        {
            var secureString = new SecureString();
            foreach (var item in value)
            {
                secureString.AppendChar(item);
            }
            return secureString;
        }

        private IDataBaseContext DataBaseContext => this.CremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;

        #region ICremaServiceItem

        protected override async Task OnCloseAsync(bool disconnect)
        {
            await Task.Delay(1);
            if (this.authentication != null)
            {
                this.authentication = null;
            }
        }

        #endregion
    }
}
