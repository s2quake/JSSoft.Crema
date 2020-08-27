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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Framework
{
    public static class TableDescriptorUtility
    {
        public static bool IsBeingEdited(Authentication authentication, ITableDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            return descriptor.TableState == TableState.IsBeingEdited;
        }

        //public static bool IsContentEditor(Authentication authentication, ITableDescriptor descriptor)
        //{
        //    if (authentication == null)
        //        throw new ArgumentNullException(nameof(authentication));
        //    if (descriptor == null)
        //        throw new ArgumentNullException(nameof(descriptor));
        //    return descriptor.TableState.HasFlag(TableState.IsBeingEdited | TableState.IsMember);
        //}

        public static bool IsBeingSetup(Authentication authentication, ITableDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            return descriptor.TableState == TableState.IsBeingSetup;
        }

        //public static bool IsTemplateEditor(Authentication authentication, ITableDescriptor descriptor)
        //{
        //    if (authentication == null)
        //        throw new ArgumentNullException(nameof(authentication));
        //    if (descriptor == null)
        //        throw new ArgumentNullException(nameof(descriptor));
        //    return descriptor.TableState.HasFlag(TableState.IsBeingSetup | TableState.IsMember);
        //}

        public static bool IsInherited(Authentication authentication, ITableDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            return descriptor.TableInfo.TemplatedParent != string.Empty;
        }

        public static bool IsBaseTemplate(Authentication authentication, ITableDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            return descriptor.TableAttribute.HasFlag(TableAttribute.BaseTable);
        }

        public static async Task<LogInfo[]> GetLogAsync(Authentication authentication, ITableDescriptor descriptor, string revision)
        {
            if (descriptor.Target is ITable table)
            {
                return await table.GetLogAsync(authentication, revision);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task<FindResultInfo[]> FindAsync(Authentication authentication, ITableDescriptor descriptor, string text, FindOptions options)
        {
            if (descriptor.Target is ITable table)
            {
                return await table.FindAsync(authentication, text, options);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task<CremaDataSet> GetDataAsync(Authentication authentication, ITableDescriptor descriptor, string revision)
        {
            if (descriptor.Target is ITable table)
            {
                return await table.GetDataSetAsync(authentication, revision);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
