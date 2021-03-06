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
using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Console
{
    [Export(typeof(ConsoleCommandContext))]
    public class ConsoleCommandContext : ConsoleCommandContextBase, IDisposable
    {
        private readonly ICremaHost cremaHost;
        private readonly ICremaAppHost cremaAppHost;

        static ConsoleCommandContext()
        {
            CommandSettings.IsConsoleMode = false;
        }

        [ImportingConstructor]
        public ConsoleCommandContext(ICremaHost cremaHost, ICremaAppHost cremaAppHost,
            [ImportMany] IEnumerable<IConsoleDrive> driveItems,
            [ImportMany] IEnumerable<IConsoleCommand> commands)
            : base(driveItems, commands)
        {
            this.cremaHost = cremaHost;
            this.cremaHost.Opened += CremaHost_Opened;
            this.cremaHost.Closed += CremaHost_Closed;
            this.cremaAppHost = cremaAppHost;
            this.BaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.Dispatcher = new Dispatcher(this);
        }

        public override ICremaHost CremaHost => this.cremaHost;

        public override string Address => this.cremaAppHost.Address;

        public override Dispatcher Dispatcher { get; }

        private void CremaHost_Opened(object sender, EventArgs e)
        {
            this.Initialize(this.cremaHost.GetService(typeof(Authenticator)) as Authenticator);
        }

        private void CremaHost_Closed(object sender, ClosedEventArgs e)
        {
            this.Release();
        }

        void IDisposable.Dispose()
        {
            this.Dispatcher.Dispose();
        }
    }
}