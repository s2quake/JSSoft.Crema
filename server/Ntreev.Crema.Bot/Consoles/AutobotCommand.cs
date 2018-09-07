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

using Ntreev.Crema.Commands.Consoles;
using Ntreev.Crema.Services;
using Ntreev.Library.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Bot.Consoles
{
    [Export(typeof(IConsoleCommand))]
    class AutobotCommand : ConsoleCommandMethodBase
    {
        private readonly ICremaHost cremaHost;
        [Import]
        private Lazy<AutobotService> autobotService = null;

        [ImportingConstructor]
        public AutobotCommand(ICremaHost cremaHost)
            : base("autobot")
        {
            this.cremaHost = cremaHost;
        }

        [CommandMethod]
        public async Task StartAsync()
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            await this.AutobotService.StartAsync(authentication);
        }

        [CommandMethod]
        public async Task StopAsync()
        {
            await this.AutobotService.StopAsync();
        }

        public override bool IsEnabled
        {
            get
            {
                if (this.CommandContext.IsOnline == false)
                    return false;
                return this.CommandContext.Authority == ServiceModel.Authority.Admin;
            }
        }

        protected override bool IsMethodEnabled(CommandMethodDescriptor descriptor)
        {
            if (descriptor.DescriptorName == nameof(StartAsync))
            {
                return this.autobotService.Value.IsPlaying == false;
            }
            else if (descriptor.DescriptorName == nameof(StopAsync))
            {
                return this.autobotService.Value.IsPlaying == true;
            }
            return base.IsMethodEnabled(descriptor);
        }

        private AutobotService AutobotService => this.autobotService.Value;
    }
}
