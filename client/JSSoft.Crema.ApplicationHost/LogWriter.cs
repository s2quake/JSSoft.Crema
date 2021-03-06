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

using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace JSSoft.Crema.ApplicationHost
{
    class LogWriter : TextWriter
    {
        private readonly StringBuilder sb = new();
        private TextBox textBox;

        public LogWriter()
        {

        }

        public override Encoding Encoding => Encoding.UTF8;

        public TextBox TextBox
        {
            get => this.textBox;
            set
            {
                this.textBox = value;
                if (this.textBox != null && this.sb.Length > 0)
                {
                    this.textBox.AppendText(this.sb.ToString());
                    this.sb.Clear();
                }
            }
        }

        public override void Write(char value)
        {
            if (this.textBox != null)
            {
                this.Redirect(this.textBox, $"{value}");
            }
            else
            {
                this.sb.Append(value);
            }
        }

        public override void Write(string value)
        {
            if (this.textBox != null)
            {
                this.Redirect(this.textBox, value);
            }
            else
            {
                this.sb.Append(value);
            }
        }

        public override void WriteLine(string value)
        {
            if (this.textBox != null)
            {
                this.Redirect(this.textBox, value + Environment.NewLine);
            }
            else
            {
                this.sb.AppendLine(value);
            }
        }

        private async void Redirect(TextBox textBox, string value)
        {
            await textBox.Dispatcher.InvokeAsync(() => textBox.AppendText(value));
        }
    }
}
