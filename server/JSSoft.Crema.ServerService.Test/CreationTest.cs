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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using JSSoft.Library.IO;
using JSSoft.Library;
using System.Linq;
using JSSoft.Crema.SvnModule;
using JSSoft.Library.Random;
using JSSoft.Crema.ServiceModel;

namespace JSSoft.Crema.ServerService.Test
{
    [TestClass]
    public class CreationTest
    {
        private static ICremaHost cremaHost;
        private static string tempDir;

        private TestContext testContext;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(context.TestDir));
            tempDir = Path.Combine(solutionDir, "crema_repo", "unit_test");
        }

        [TestInitialize()]
        public void Initialize()
        {
            cremaHost = TestCrema.CreateInstance(tempDir);
            cremaHost.Open();
        }

        [TestCleanup()]
        public void Cleanup()
        {
            cremaHost.Dispatcher.Invoke(() => cremaHost.Close());
            cremaHost.Dispose();
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            
        }

        [TestMethod]
        public void BaseTest()
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                var userContext = cremaHost.GetService<IUserContext>();
                var user = userContext.Users.Random(item => item.Authority != Authority.Guest);
                var authentication = cremaHost.Login(user.ID, user.Authority.ToString().ToLower());
                var dataBase = cremaHost.PrimaryDataBase;

                var tableContext = dataBase.TableContext;

                var category = tableContext.Root.AddNewCategory(authentication, "Folder1");

                var category1 = category.AddNewCategory(authentication, "Folder1");

                var template = category1.NewTable(authentication);
                template.EndEdit(authentication);

                var table = template.Table;

                var childTemplate1 = table.NewTable(authentication);
                childTemplate1.EndEdit(authentication);

                var child = childTemplate1.Table;
                child.Rename(authentication, "Child_abc");

                var childTemplate2 = table.NewTable(authentication);
                childTemplate2.EndEdit(authentication);
            });
        }

        [TestMethod]
        public void GenerateStandardTest()
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                var userContext = cremaHost.GetService<IUserContext>();
                var user = userContext.Users.Random(item => item.Authority != Authority.Guest);
                var authentication = cremaHost.Login(user.ID, user.Authority.ToString().ToLower());
                var dataBase = cremaHost.PrimaryDataBase;
                var tableContext = dataBase.TableContext;

                var time = DateTime.Now;
                
                var transaction = dataBase.BeginTransaction(authentication);
                dataBase.GenerateStandard(authentication);

                var diff = DateTime.Now - time;

                transaction.Commit();
            });
        }

        public TestContext TestContext
        {
            get { return this.testContext; }
            set { this.testContext = value; }
        }
    }
}