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
using Ntreev.Crema.Presentation.Framework.Dialogs.ViewModels;
using Ntreev.Crema.Presentation.Tables.Properties;
using Ntreev.Crema.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class MoveTableViewModel : MoveAsyncAppViewModel
    {
        private readonly Authentication authentication;
        private readonly ITable table;

        private MoveTableViewModel(Authentication authentication, ITable table, string[] targetPaths)
            : base(table, currentPath: table.Path, targetPaths: targetPaths)
        {
            this.authentication = authentication;
            this.table = table;
            this.table.Dispatcher.VerifyAccess();
            this.DisplayName = Resources.Title_MoveTable;
        }

        public static Task<MoveTableViewModel> CreateInstanceAsync(Authentication authentication, ITableDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is ITable table)
            {
                return table.Dispatcher.InvokeAsync(() =>
                {
                    var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                    var targetPaths = categories.Select(item => item.Path).ToArray();
                    return new MoveTableViewModel(authentication, table, targetPaths);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        protected async override void VerifyMove(string targetPath, Action<bool> isVerify)
        {
            var result = await this.table.Dispatcher.InvokeAsync(() =>
            {
                if (targetPath == string.Empty)
                    return false;
                var categories = this.table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                var target = categories[targetPath];
                if (target == null)
                    return false;
                return target.Categories[this.table.Name] == null;
            });

            isVerify(result);
        }

        protected override Task OnMoveAsync(string targetPath)
        {
            return this.table.MoveAsync(this.authentication, targetPath);
        }
    }
}
