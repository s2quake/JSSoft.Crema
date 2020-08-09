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

using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Types.BrowserItems.ViewModels;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System.ComponentModel.Composition;
using System.Windows.Input;
using TypeDescriptor = Ntreev.Crema.Presentation.Framework.TypeDescriptor;

namespace Ntreev.Crema.Presentation.Types.PropertyItems.ViewModels
{
    class TypeListBoxItemViewModel : TypeListItemBase
    {
        private readonly ICommand selectInBrowserCommand;
        [Import]
        private readonly TypeBrowserViewModel browser = null;
        [Import]
        private readonly IShell shell = null;
        [Import]
        private readonly TypeServiceViewModel service = null;

        public TypeListBoxItemViewModel(Authentication authentication, IType type, object owner)
            : base(authentication, new TypeDescriptor(authentication, type, DescriptorTypes.IsSubscriptable, owner), owner)
        {
            this.selectInBrowserCommand = new DelegateCommand(item => this.SelectInBrowser());
        }

        public TypeListBoxItemViewModel(Authentication authentication, ITypeDescriptor descriptor, object owner)
            : base(authentication, new TypeDescriptor(authentication, descriptor, true, owner), owner)
        {
            this.selectInBrowserCommand = new DelegateCommand(item => this.SelectInBrowser());
        }

        public async void SelectInBrowser()
        {
            if (this.browser != null && this.shell != null && this.service != null)
            {
                await this.Dispatcher.InvokeAsync(() => shell.SelectedService = this.service);
                await this.Dispatcher.InvokeAsync(() => browser.Select(this.descriptor));
            }
        }

        public ICommand SelectInBrowserCommand => this.selectInBrowserCommand;

        public override string DisplayName => this.descriptor.TypeName;
    }
}