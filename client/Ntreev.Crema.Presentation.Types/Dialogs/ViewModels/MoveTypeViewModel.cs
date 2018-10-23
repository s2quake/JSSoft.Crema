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

using System.Linq;
using Ntreev.Crema.Presentation.Types.Properties;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using System.IO;
using System.Threading.Tasks;
using Ntreev.ModernUI.Framework.Dialogs.ViewModels;
using System.Windows.Threading;
using System;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Framework.Dialogs.ViewModels;

namespace Ntreev.Crema.Presentation.Types.Dialogs.ViewModels
{
    public class MoveTypeViewModel : MoveAsyncAppViewModel
    {
        private readonly Authentication authentication;
        private readonly IType type;

        private MoveTypeViewModel(Authentication authentication, IType type, string[] targetPaths)
            : base(type.Path, targetPaths)
        {
            this.authentication = authentication;
            this.type = type;
            this.type.Dispatcher.VerifyAccess();
            this.DisplayName = Resources.Title_MoveType;
        }

        public static Task<MoveTypeViewModel> CreateInstanceAsync(Authentication authentication, ITypeDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is IType type)
            {
                return type.Dispatcher.InvokeAsync(() =>
                {
                    var categories = type.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
                    var targetPaths = categories.Select(item => item.Path).ToArray();
                    return new MoveTypeViewModel(authentication, type, targetPaths);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        protected async override void VerifyMove(string targetPath, Action<bool> isVerify)
        {
            var result = await this.type.Dispatcher.InvokeAsync(() =>
            {
                var categories = this.type.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
                var target = categories[targetPath];
                if (target == null)
                    return false;

                return target.Categories[this.type.Name] == null;
            });
            isVerify(result);
        }

        protected override Task MoveAsync(string targetPath)
        {
            return this.type.MoveAsync(this.authentication, targetPath);
        }
    }
}
