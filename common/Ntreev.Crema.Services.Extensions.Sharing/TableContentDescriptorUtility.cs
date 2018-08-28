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

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Extensions
{
    public static class TableContentDescriptorUtility
    {
        public static async Task BeginEditAsync(Authentication authentication, ITableContentDescriptor descriptor)
        {
            if (descriptor.Target is ITableContent content)
            {
                var domain = await content.Dispatcher.InvokeAsync(() =>
                {
                    if (content.Domain == null)
                    {
                        content.BeginEdit(authentication);
                    }
                    return content.Domain;
                });
                var isEntered = await domain.Dispatcher.InvokeAsync(() =>
                {
                    return domain.Users.Contains(authentication.ID);
                });
                await content.Dispatcher.InvokeAsync(() =>
                {
                    if (isEntered == false)
                    {
                        content.EnterEdit(authentication);
                    }
                });
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task<bool> EndEditAsync(Authentication authentication, ITableContentDescriptor descriptor)
        {
            if (descriptor.Target is ITableContent content)
            {
                var domain = await content.Dispatcher.InvokeAsync(() =>
                {
                    content.LeaveEdit(authentication);
                    return content.Domain;
                });
                var isEmpty = await domain.Dispatcher.InvokeAsync(() => domain.Users.Any() == false);
                await content.Dispatcher.InvokeAsync(() =>
                {
                    if (isEmpty == true)
                    {
                        content.EndEdit(authentication);
                    }
                });
                return true;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
