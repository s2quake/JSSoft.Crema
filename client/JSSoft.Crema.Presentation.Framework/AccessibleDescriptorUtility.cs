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

using Ntreev.Crema.Presentation.Framework.Dialogs.ViewModels;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Framework
{
    public static class AccessibleDescriptorUtility
    {
        public static bool IsPrivate(Authentication authentication, IAccessibleDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            return descriptor.AccessInfo.IsPrivate;
        }

        public static bool IsAccessOwner(Authentication authentication, IAccessibleDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            return descriptor.AccessInfo.IsOwner(authentication.ID);
        }

        public static bool IsAccessMember(Authentication authentication, IAccessibleDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            return descriptor.AccessInfo.IsMember(authentication.ID);
        }

        public static bool IsAccessInherited(Authentication authentication, IAccessibleDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            return descriptor.AccessInfo.IsInherited;
        }

        public static bool CanSetPrivate(Authentication authentication, IAccessibleDescriptor descriptor)
        {
            if (descriptor.AccessInfo.IsPrivate == true && descriptor.AccessInfo.IsInherited == false)
                return false;
            return descriptor.AccessInfo.GetAccessType(authentication.ID) >= AccessType.Owner;
        }

        public static bool CanSetPublic(Authentication authentication, IAccessibleDescriptor descriptor)
        {
            if (descriptor.AccessInfo.IsPrivate == false || descriptor.AccessInfo.IsInherited == true)
                return false;

            return descriptor.AccessInfo.GetAccessType(authentication.ID) >= AccessType.Owner;
        }

        public static bool CanSetAuthority(Authentication authentication, IAccessibleDescriptor descriptor)
        {
            if (descriptor.AccessInfo.IsPrivate == false || descriptor.AccessInfo.IsInherited == true)
                return false;

            return descriptor.AccessInfo.GetAccessType(authentication.ID) >= AccessType.Owner;
        }

        public static async Task<bool> SetPrivateAsync(Authentication authentication, IAccessibleDescriptor descriptor)
        {
            if (descriptor.Target is IAccessible accessible)
            {
                try
                {
                    await accessible.SetPrivateAsync(authentication);
                    return true;
                }
                catch (Exception e)
                {
                    await AppMessageBox.ShowErrorAsync(e);
                    return false;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task<bool> SetPublicAsync(Authentication authentication, IAccessibleDescriptor descriptor)
        {
            if (descriptor.Target is IAccessible accessible)
            {
                try
                {
                    await accessible.SetPublicAsync(authentication);
                    return true;
                }
                catch (Exception e)
                {
                    await AppMessageBox.ShowErrorAsync(e);
                    return false;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task<bool> SetAuthorityAsync(Authentication authentication, IAccessibleDescriptor descriptor)
        {
            var dialog = await AccessViewModel.CreateInstanceAsync(authentication, descriptor);
            return await dialog?.ShowDialogAsync() == true;
        }
    }
}
