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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System;
using JSSoft.Crema.Services.Users.Serializations;
using System.Collections.Generic;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Random;
using JSSoft.Crema.Services.Test.Extensions;
using JSSoft.Crema.Services.Random;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSSoft.Library.IO;
using JSSoft.Crema.Data;
using JSSoft.Library;

namespace JSSoft.Crema.Services.Test
{
    partial class TestApplication : CremaBootstrapper
    {
        public async Task InitializeAsync(TestContext context)
        {
            var repositoryPath = DirectoryUtility.Prepare(context.TestRunDirectory, "repo", context.FullyQualifiedTestClassName);
            var userInfos = UserInfoGenerator.Generate(RandomUtility.Next(500, 1000), RandomUtility.Next(100, 1000));
            var dataSet = new CremaDataSet();
            await Task.Run(() => CremaBootstrapper.CreateRepositoryInternal(this, repositoryPath, "git", "xml", string.Empty, userInfos, dataSet));
            this.BasePath = repositoryPath;
            this.cremaHost = this.GetService(typeof(ICremaHost)) as ICremaHost;
        }

        public async Task ReleaseAsync()
        {
            await Task.Run(() => DirectoryUtility.Delete(this.BasePath));
            this.Dispose();
        }
    }
}
