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
using JSSoft.Library.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JSSoft.Crema.Services.Random;

namespace JSSoft.Crema.Services.Test.DispatcherTest
{
    [TestClass]
    public class ITableCategoryCollection_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IDataBase dataBase;
        private static ITableCategoryCollection categories;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(ITableCategoryCollection_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;

            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.GetRandomDataBaseAsync();
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.InitializeAsync(authentication);
            categories = dataBase.TableContext.Categories;
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await dataBase.UnloadAsync(authentication);
            await cremaHost.StopAsync(authentication);
            app.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Contains()
        {
            categories.Contains(PathUtility.Separator);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Root()
        {
            Console.Write(categories.Root);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Indexer()
        {
            Console.Write(categories[PathUtility.Separator]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesCreated()
        {
            categories.CategoriesCreated += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesRenamed()
        {
            categories.CategoriesRenamed += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesMoved()
        {
            categories.CategoriesMoved += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesDeleted()
        {
            categories.CategoriesDeleted += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Count()
        {
            Console.Write(categories.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetEnumerator()
        {
            foreach (var item in categories as IEnumerable)
            {
                Console.Write(item);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetEnumeratorGeneric()
        {
            foreach (var item in categories as IEnumerable<ITableCategory>)
            {
                Console.Write(item);
            }
        }

        [TestMethod]
        public void GetService()
        {
            Console.Write(categories.GetService(typeof(ICremaHost)));
        }
    }
}