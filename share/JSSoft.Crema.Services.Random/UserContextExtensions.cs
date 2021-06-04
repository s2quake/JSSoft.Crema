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

using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Random
{
    public static class UserContextExtensions
    {
        public static Task<IUser> GetRandomUserAsync(this IUserContext userContext)
        {
            if (userContext.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                return userCollection.Dispatcher.InvokeAsync(() => userCollection.Random());
            }
            throw new NotImplementedException();
        }

        public static Task<IUserCategory> GetRandomUserCategoryAsync(this IUserContext userContext)
        {
            if (userContext.GetService(typeof(IUserCategoryCollection)) is IUserCategoryCollection userCategoryCollection)
            {
                return userCategoryCollection.Dispatcher.InvokeAsync(() => userCategoryCollection.Random());
            }
            throw new NotImplementedException();
        }

        public static Task<IUserItem> GetRandomUserItemAsync(this IUserContext userContext)
        {
            return userContext.Dispatcher.InvokeAsync(() => userContext.Random());
        }

        public static Task<IUserItem> GetRandomUserItemAsync(this IUserContext userContext, Func<IUserItem, bool> predicate)
        {
            return userContext.Dispatcher.InvokeAsync(() => userContext.Random(predicate));
        }

        public static async Task GenerateCategoriesAsync(this IUserContext userContext, Authentication authentication, int count)
        {
            for (var i = 0; i < count; i++)
            {
                await userContext.GenerateCategoryAsync(authentication);
            }
        }

        public static async Task<bool> GenerateCategoryAsync(this IUserContext userContext, Authentication authentication)
        {
            if (RandomUtility.Within(50) == true)
            {
                await userContext.Root.AddNewCategoryAsync(authentication, RandomUtility.NextIdentifier());
            }
            else
            {
                var category = await userContext.GetRandomUserCategoryAsync();
                // if (GetLevel(category, (i) => i.Parent) > 4)
                //     return false;
                await category.AddNewCategoryAsync(authentication, RandomUtility.NextIdentifier());
            }
            return true;
        }

        public static async Task GenerateUsersAsync(this IUserContext userContext, Authentication authentication, int count)
        {
            for (var i = 0; i < count; i++)
            {
                await userContext.GenerateUserAsync(authentication);
            }
        }

        public static async Task<IUser> GenerateUserAsync(this IUserContext userContext, Authentication authentication)
        {
            var authorities = new Authority[] { Authority.Admin, Authority.Member, Authority.Guest };
            var authority = authorities.Random();
            var category = await userContext.GetRandomUserCategoryAsync();
            var newID = await userContext.GenerateUserIDAsync("user");
            var newName = newID.Replace("user", "User");
            var password = authority.ToString().ToLower().ToSecureString();
            return await category.AddNewUserAsync(authentication, newID, password, newName, authority);
        }

        public static Task<string> GenerateUserIDAsync(this IUserContext userContext)
        {
            return GenerateUserIDAsync(userContext, "user");
        }

        public static Task<string> GenerateUserIDAsync(this IUserContext userContext, string name)
        {
            return userContext.Dispatcher.InvokeAsync(() =>
            {
                return NameUtility.GenerateNewName(name, userContext.Users.Select(item => item.ID).ToArray());
            });
        }
    }
}
