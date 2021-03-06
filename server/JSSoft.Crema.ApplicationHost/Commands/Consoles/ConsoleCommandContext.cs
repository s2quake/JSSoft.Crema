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

using JSSoft.Crema.Commands.Consoles;
using JSSoft.Crema.ServiceHosts;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.ApplicationHost.Commands.Consoles
{
    [Export(typeof(ConsoleCommandContext))]
    public class ConsoleCommandContext : ConsoleCommandContextBase
    {
        private readonly ICremaHost cremaHost;
        private readonly CremaService service;
        private Authentication authentication;

        static ConsoleCommandContext()
        {
            CommandSettings.IsConsoleMode = false;
        }

        [ImportingConstructor]
        public ConsoleCommandContext(ICremaHost cremaHost, CremaService service,
            [ImportMany] IEnumerable<IConsoleDrive> rootItems,
            [ImportMany] IEnumerable<IConsoleCommand> commands)
            : base(rootItems, commands)
        {
            this.cremaHost = cremaHost;
            this.cremaHost.Opened += CremaHost_Opened;
            this.service = service;
        }

        public async Task LoginAsync(string userID, SecureString password)
        {
            if (this.authentication != null)
                throw new Exception("이미 로그인되어 있습니다.");
            var token = await this.CremaHost.LoginAsync(userID, password);
            this.authentication = await this.CremaHost.AuthenticateAsync(token);
            this.authentication.Expired += Authentication_Expired;
            this.Initialize(authentication);
        }

        public async Task LogoutAsync()
        {
            if (this.authentication == null)
                throw new Exception("로그인되어 있지 않습니다.");
            this.Release();
            this.authentication.Expired -= Authentication_Expired;
            await this.CremaHost.LogoutAsync(this.authentication);
            this.authentication = null;
        }

        public override ICremaHost CremaHost => this.cremaHost;

        public override string Address => AddressUtility.GetDisplayAddress($"localhost:{this.service.Port}");

        public bool IsOpen => this.service.ServiceState == ServiceState.Open;

        public override Dispatcher Dispatcher => this.service.Dispatcher;

        private void CremaHost_Opened(object sender, EventArgs e)
        {
            if (this.service.GetService(typeof(AppSettings)) is AppSettings settings)
            {
                this.BaseDirectory = System.IO.Path.Combine(settings.BasePath, "Documents");
            }
        }

        private void Authentication_Expired(object sender, EventArgs e)
        {
            this.authentication = null;
        }
    }
}
