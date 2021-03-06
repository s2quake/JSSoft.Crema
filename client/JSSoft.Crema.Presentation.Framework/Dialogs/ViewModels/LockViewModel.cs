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

using JSSoft.Crema.Presentation.Framework.Properties;
using System;

namespace JSSoft.Crema.Presentation.Framework.Dialogs.ViewModels
{
    public abstract class LockViewModel : ModalDialogAppBase
    {
        private string comment;
        private readonly Func<string, bool> predicate;

        public LockViewModel()
            : this(item => true)
        {
#if DEBUG
            this.Dispatcher.InvokeAsync(() => this.Comment = "lock test");
#endif
        }

        public LockViewModel(Func<string, bool> predicate)
        {
            this.predicate = predicate;
            this.DisplayName = Resources.Title_SettingLock;
        }

        public virtual void Rename()
        {
            this.TryCloseAsync(this.CanLock);
        }

        public string Comment
        {
            get => this.comment ?? string.Empty;
            set
            {
                this.comment = value;
                this.NotifyOfPropertyChange(nameof(this.CanLock));
                this.NotifyOfPropertyChange(nameof(this.Comment));
            }
        }

        public virtual bool CanLock
        {
            get
            {
                if (this.IsProgressing == true)
                    return false;
                if (this.Comment == string.Empty)
                    return false;
                return this.VerifyLock(this.Comment);
            }
        }

        protected virtual bool VerifyLock(string comment)
        {
            if (this.predicate != null)
                return this.predicate(comment);
            return true;
        }
    }
}
