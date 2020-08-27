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

using JSSoft.Library.Commands;
using JSSoft.ModernUI.Framework.Controls;
using System;
using System.IO;
using System.Windows.Media;

namespace JSSoft.Crema.ApplicationHost.Controls
{
    class ConsoleWriter : StringWriter
    {
        private readonly TerminalControl control;

        public ConsoleWriter(TerminalControl control)
        {
            this.control = control;
            TerminalColor.ForegroundColorChanged += TerminalColor_ForegroundColorChanged;
            TerminalColor.BackgroundColorChanged += TerminalColor_BackgroundColorChanged;
        }

        public override void Write(char value)
        {
            base.Write(value);
            this.control.Dispatcher.Invoke(() => this.control.Append(value.ToString()));
        }

        public override void WriteLine()
        {
            base.WriteLine();
            this.control.Dispatcher.Invoke(() => this.control.AppendLine(string.Empty));
        }

        public override void Write(string value)
        {
            base.Write(value);
            this.control.Dispatcher.Invoke(() => this.control.Append(value));
        }

        private void TerminalColor_ForegroundColorChanged(object sender, EventArgs e)
        {
            var foregroundColor = TerminalColor.ForegroundColor;
            this.control.Dispatcher.Invoke(() =>
            {
                if (foregroundColor == null)
                    this.control.OutputForeground = null;
                else
                    this.control.OutputForeground = (Brush)this.control.FindResource(TerminalColors.FindForegroundKey(foregroundColor));
            });
        }

        private void TerminalColor_BackgroundColorChanged(object sender, EventArgs e)
        {
            var backgroundColor = TerminalColor.BackgroundColor;
            this.control.Dispatcher.Invoke(() =>
            {
                if (backgroundColor == null)
                    this.control.OutputBackground = null;
                else
                    this.control.OutputBackground = (Brush)this.control.FindResource(TerminalColors.FindBackgroundKey(backgroundColor));
            });
        }
    }
}
