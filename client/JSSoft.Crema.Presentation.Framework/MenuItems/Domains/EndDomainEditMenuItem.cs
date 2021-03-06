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
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Framework.MenuItems.Domains
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(DomainTreeItemBase))]
    [ParentType(typeof(DomainListItemBase))]
    class EndDomainEditMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public EndDomainEditMenuItem(Authenticator authenticator)
        {
            this.authenticator = authenticator;
            this.DisplayName = Resources.MenuItem_EndEdit;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is IDomainDescriptor descriptor && descriptor.Target is IDomain && this.authenticator.Authority == Authority.Admin)
            {
                return descriptor.IsModified && descriptor.IsActivated;
            }
            return false;
        }

        protected async override void OnExecute(object parameter)
        {
            if (parameter is IDomainDescriptor descriptor && descriptor.Target is IDomain domain && this.authenticator.Authority == Authority.Admin)
            {
                if (await AppMessageBox.ShowProceedAsync("편집을 종료합니다. 계속하시겠습니까?") == false)
                    return;

                try
                {
                    await domain.DeleteAsync(this.authenticator, false);
                }
                catch (Exception e)
                {
                    await AppMessageBox.ShowErrorAsync(e);
                }
            }
        }
    }
}
