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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.ModernUI.Framework.Dialogs.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Home.Dialogs.ViewModels
{
    public class RenameDataBaseViewModel : RenameAsyncViewModel
    {
        private readonly Authentication authentication;
        private readonly IDataBase dataBase;

        private RenameDataBaseViewModel(Authentication authentication, IDataBase dataBase)
            : base(dataBase.Name)
        {
            this.authentication = authentication;
            this.authentication.Expired += Authentication_Expired;
            this.dataBase = dataBase;
            this.dataBase.Dispatcher.VerifyAccess();
            this.DisplayName = "이름 변경";
        }

        public static Task<RenameDataBaseViewModel> CreateInstanceAsync(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is IDataBase dataBase)
            {
                return dataBase.Dispatcher.InvokeAsync(() =>
                {
                    return new RenameDataBaseViewModel(authentication, dataBase);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        protected async override void VerifyRename(string newName, Action<bool> isVerify)
        {
            var dataBases = this.dataBase.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            var result = await dataBases.ContainsAsync(newName) == false;
            isVerify(result);
        }

        protected override Task OnRenameAsync(string newName)
        {
            return this.dataBase.RenameAsync(this.authentication, newName);
        }

        protected async override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
            if (close == true)
            {
                this.authentication.Expired -= Authentication_Expired;
            }
        }

        private async void Authentication_Expired(object sender, EventArgs e)
        {
            await this.TryCloseAsync();
        }
    }
}
