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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Data;
using Ntreev.Crema.Services;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Random;
using System;
using System.Collections.Generic;
using System.Text;
using Ntreev.Library;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Random
{
    public static class DataBaseExtensions
    {
        static DataBaseExtensions()
        {

        }

        public static Task InitializeRandomItemsAsync(this IDataBase dataBase, Authentication authentication)
        {
            return InitializeRandomItemsAsync(dataBase, authentication, false);
        }

        public static async Task InitializeRandomItemsAsync(this IDataBase dataBase, Authentication authentication, bool transaction)
        {
            if (transaction == true)
                await InitializeRandomItemsTransactionAsync(dataBase, authentication);
            else
                await InitializeRandomItemsStandardAsync(dataBase, authentication);
        }

        private static async Task InitializeRandomItemsTransactionAsync(this IDataBase dataBase, Authentication authentication)
        {
            var trans = await dataBase.BeginTransactionAsync(authentication);
            await dataBase.TypeContext.AddRandomItemsAsync(authentication);
            await dataBase.TableContext.AddRandomItemsAsync(authentication);
            await trans.CommitAsync(authentication);
        }

        private static async Task InitializeRandomItemsStandardAsync(this IDataBase dataBase, Authentication authentication)
        {
            await dataBase.TypeContext.AddRandomItemsAsync(authentication);
            await dataBase.TableContext.AddRandomItemsAsync(authentication);
        }
    }
}