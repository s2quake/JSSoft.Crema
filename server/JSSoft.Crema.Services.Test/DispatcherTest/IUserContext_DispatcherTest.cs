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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSSoft.Library.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.DispatcherTest
{
    [TestClass]
    public class IUserContext_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IUserContext userContext;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(IUserContext_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            userContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await cremaHost.StopAsync(authentication);
            app.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task NotifyMessageAsync()
        {
            await userContext.NotifyMessageAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Contains()
        {
            userContext.Contains(PathUtility.Separator);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Users()
        {
            Console.Write(userContext.Users);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Categories()
        {
            Console.Write(userContext.Categories);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Root()
        {
            Console.Write(userContext.Root);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Indexer()
        {
            Console.Write(userContext[PathUtility.Separator]);
        }

        [TestMethod]
        public void Dispatcher()
        {
            Console.Write(userContext.Dispatcher);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsCreated()
        {
            userContext.ItemsCreated += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsRenamed()
        {
            userContext.ItemsRenamed += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsMoved()
        {
            userContext.ItemsMoved += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsDeleted()
        {
            userContext.ItemsDeleted += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsChanged()
        {
            userContext.ItemsChanged += (s, e) => Assert.Inconclusive();
        }

#if SERVER
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Login()
        {
            userContext.Login("admin", Utility.AdminPassword);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Logout()
        {
            userContext.Logout(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Authenticate()
        {
            userContext.Authenticate(Guid.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IsAuthenticated()
        {
            userContext.IsAuthenticated(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IsOnlineUser()
        {
            userContext.IsOnlineUser(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetMetaData()
        {
            userContext.GetMetaData(null);
        }
#endif

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetEnumerator()
        {
            foreach (var item in userContext as IEnumerable)
            {
                Console.Write(item);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetEnumeratorGeneric()
        {
            foreach (var item in userContext as IEnumerable<IUserItem>)
            {
                Console.Write(item);
            }
        }

        [TestMethod]
        public void GetService()
        {
            Console.Write(userContext.GetService(typeof(ICremaHost)));
        }
    }
}
