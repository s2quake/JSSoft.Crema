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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Types.BrowserItems.ViewModels
{
    public class TypeTreeViewItemViewModel : TypeTreeItemBase
    {
        public TypeTreeViewItemViewModel(Authentication authentication, IType type)
            : this(authentication, type, null)
        {

        }

        public TypeTreeViewItemViewModel(Authentication authentication, IType type, object owner)
            : this(authentication, new TypeDescriptor(authentication, type, DescriptorTypes.All, owner), owner)
        {

        }

        public TypeTreeViewItemViewModel(Authentication authentication, TypeDescriptor descriptor)
            : this(authentication, descriptor, null)
        {

        }

        public TypeTreeViewItemViewModel(Authentication authentication, TypeDescriptor descriptor, object owner)
            : base(authentication, descriptor, owner)
        {
            this.RenameCommand = new DelegateCommand(async item => await this.RenameAsync(), item => this.CanRename);
            this.DeleteCommand = new DelegateCommand(async item => await this.DeleteAsync(), item => this.CanDelete);
            this.ViewCommand = new DelegateCommand(async item => await this.ViewTemplateAsync(), item => this.CanViewTemplate);
        }

        public async Task EditTemplateAsync()
        {
            await TypeUtility.EditTemplateAsync(this.authentication, this.descriptor);
        }

        public async Task ViewTemplateAsync()
        {
            await TypeUtility.ViewTemplateAsync(this.authentication, this.descriptor);
        }

        public async Task CopyAsync()
        {
            if (await TypeUtility.CopyAsync(this.authentication, this.descriptor) is string newTypeName)
            {

            }
        }

        public async Task RenameAsync()
        {
            if (await TypeUtility.RenameAsync(this.authentication, this.descriptor) == false)
                return;

            if (this.Owner is ISelector selector)
                selector.SelectedItem = this;
        }

        public async Task MoveAsync()
        {
            if (await TypeUtility.MoveAsync(this.authentication, this.descriptor) == false)
                return;

            if (this.Owner is ISelector)
                this.ExpandAncestors();
        }

        public async Task DeleteAsync()
        {
            await TypeUtility.DeleteAsync(this.authentication, this.descriptor);
        }

        public async Task ViewLogAsync()
        {
            await TypeUtility.ViewLogAsync(this.authentication, this.descriptor);
        }

        [DescriptorProperty]
        public bool CanCopy => TypeUtility.CanCopy(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanViewLog => TypeUtility.CanViewLog(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanRename => TypeUtility.CanRename(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanMove => TypeUtility.CanMove(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanDelete => TypeUtility.CanDelete(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanEditTemplate => TypeUtility.CanEditTemplate(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanViewTemplate => TypeUtility.CanViewTemplate(this.authentication, this.descriptor);

        public ICommand RenameCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand ViewCommand { get; }

        public override string DisplayName => this.descriptor.TypeName;
    }
}
