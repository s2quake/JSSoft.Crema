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

using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.Collections.Generic;

namespace JSSoft.Crema.Commands.Consoles.TableTemplate
{
    class TemplateTerminal : CommandContextTerminal
    {
        private TemplateTerminal(TemplateCommandContext commandContext, string prompt)
            : base(commandContext)
        {
            this.Prompt = prompt;
        }

        public static TemplateTerminal Create(Authentication authentication, ITableTemplate template, string prompt)
        {
            var serviceProvider = template.Dispatcher.Invoke(() => template.Target as IServiceProvider);
            var commands = (serviceProvider.GetService(typeof(IEnumerable<ITemplateCommand>)) as IEnumerable<ITemplateCommand>);

            var commandContext = new TemplateCommandContext(authentication, template, commands);
            var terminal = new TemplateTerminal(commandContext, prompt) { Postfix = "$ " };

            foreach (var item in commands)
            {
                if (item is SaveCommand saveCommand)
                {
                    saveCommand.CloseAction = () => terminal.Cancel();
                }
                else if (item is CancelCommand cancelCommand)
                {
                    cancelCommand.CloseAction = () => terminal.Cancel();
                }
            }

            return terminal;
        }
    }
}
