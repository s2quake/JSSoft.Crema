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

using FirstFloor.ModernUI.Windows.Controls;
using JSSoft.Crema.ApplicationHost.Commands.Consoles;
using JSSoft.Crema.ApplicationHost.Controls;
using JSSoft.Crema.Services;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JSSoft.Crema.ApplicationHost
{
    /// <summary>
    /// ShellView.xaml에 대한 상호 작용 논리
    /// </summary>
    [Export]
    public partial class ShellView : ModernWindow
    {
        private readonly LogTextWriter writer;
        private readonly ICremaHost cremaHost;
        private readonly ConsoleCommandContext commandContext;

        public ShellView()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public ShellView(ICremaHost cremaHost, ConsoleCommandContext commandContext)
        {
            InitializeComponent();
            this.writer = new LogTextWriter() { TextBox = this.LogBox };
            this.cremaHost = cremaHost;
            this.cremaHost.Opening += CremaHost_Opening;
            this.cremaHost.Opened += CremaHost_Opened;
            this.cremaHost.Closed += CremaHost_Closed;
            this.commandContext = commandContext;
            this.commandContext.Out = new ConsoleWriter(this.terminal);
            this.terminal.CommandContext = this.commandContext;
            this.SetPrompt();
            CremaLog.AddRedirection(this.writer, LogLevel.Info);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.terminal.ApplyTemplate();
        }

        public async Task Run(string commandLine)
        {
            try
            {
                await this.commandContext.ExecuteAsync(this.commandContext.Name + " " + commandLine);
                this.SetPrompt();
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                if (e.InnerException != null)
                    this.terminal.AppendLine(e.InnerException.Message);
                else
                    this.terminal.AppendLine(e.Message);
            }
            catch (Exception e)
            {
                this.terminal.AppendLine(e.Message);
            }
            finally
            {
                this.terminal.InsertPrompt();
            }
        }

        public void Reset()
        {
            this.terminal.Reset();
        }

        protected void SetPrompt()
        {
            this.Dispatcher.InvokeAsync(() => this.terminal.Prompt = $"{this.commandContext.Prompt}> ");
        }

        private void CremaHost_Opening(object sender, EventArgs e)
        {
            if (this.cremaHost.GetService(typeof(ILogService)) is ILogService logService)
            {
                logService.AddRedirection(writer, LogLevel.Info);
            }
        }

        private void LogBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void LogBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Back || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        private void TerminalControl_Executed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.Run(this.terminal.Text);
        }

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.SetPrompt();
                this.terminal.AppendLine(Properties.Resources.Comment_Hello);
                this.terminal.AppendLine(Properties.Resources.Comment_AvaliableCommands);
                foreach (var item in this.commandContext.Node.Childs)
                {
                    if (item.IsEnabled == false)
                        continue;
                    this.terminal.AppendLine(" - " + item.Name);
                }
                this.terminal.AppendLine(Properties.Resources.Comment_TypeHelp + Environment.NewLine);
                this.terminal.AppendLine(Properties.Resources.Comment_TypeVersion);
            });
        }

        private async void CremaHost_Closed(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.terminal.Reset();
            });
        }

        private async void OpenService_Click(object sender, RoutedEventArgs e)
        {
            await this.SetFocusAsync();
        }

        private async void CloseService_Click(object sender, RoutedEventArgs e)
        {
            await this.SetFocusAsync();
        }

        private async void ModernWindow_Activated(object sender, EventArgs e)
        {
            await this.SetFocusAsync();
        }

        private async Task SetFocusAsync()
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.TabControl.SelectedContent is UIElement element && element.Focusable == true)
                {
                    element.Focus();
                }
            });
        }
    }
}
