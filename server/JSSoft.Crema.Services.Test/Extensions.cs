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
using System;
using System.Collections.Generic;

namespace JSSoft.Crema.Services.Test
{
    static class Extensions
    {
        private static readonly object lockobj = new object();
#if CLIENT
        private static int port = 4006;
#endif

        private static readonly Dictionary<ICremaHost, int> cremaHostToPort = new Dictionary<ICremaHost, int>();
        private static readonly Dictionary<Authentication, Guid> authenticationToToken = new Dictionary<Authentication, Guid>();

        public static void Initialize(this CremaBootstrapper boot, TestContext context, string name)
        {
#if SERVER
            var repositoryPath = DirectoryUtility.Prepare(context.TestRunDirectory + "_repo", name);
            CremaBootstrapper.CreateRepository(boot, repositoryPath, "git", "xml", true);
            boot.BasePath = repositoryPath;
#endif
#if CLIENT
            var cremaHost = boot.GetService(typeof(ICremaHost)) as ICremaHost;
            var repositoryPath = DirectoryUtility.Prepare(context.TestRunDirectory + "_repo", name);
            CremaServeHost.Run("init", repositoryPath.WrapQuot());
            var process = CremaServeHost.RunAsync("run", repositoryPath.WrapQuot(), "--port", port);
            var eventSet = new ManualResetEvent(false);
            cremaHostToPort[cremaHost] = port;
            port += 2;
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data == "종료하시려면 <Q> 키를 누르세요.")
                    eventSet.Set();
            };
            eventSet.WaitOne();
            boot.Disposed += (s, e) =>
            {
                process.StandardInput.WriteLine("exit");
                process.WaitForExit(100);
                cremaHostToPort.Remove(cremaHost);
            };
#endif
        }

        public static void Initialize(this IDataBase dataBase, Authentication authentication)
        {
#if SERVER
            dataBase.InitializeRandomItemsAsync(authentication);
#else
            dataBase.InitializeRandomItems(authentication, false);
#endif
        }

        public static Authentication Start(this ICremaHost cremaHost)
        {
#if SERVER
            var token = cremaHost.OpenAsync();
            var authentication = cremaHost.LoginAsync("admin", Utility.AdminPassword);
            authenticationToToken.Add(authentication, token);
            return authentication;
#endif
#if CLIENT
            var port = cremaHostToPort[cremaHost];
            var token = cremaHost.Open($"localhost:{port}", "admin", Utility.AdminPassword);
            var authentication = cremaHost.GetService(typeof(Authenticator)) as Authenticator;
            authenticationToToken.Add(authentication, token);
            return authentication;
#endif
        }

        public static void Stop(this ICremaHost cremaHost, Authentication authentication)
        {
            var token = authenticationToToken[authentication];
#if SERVER
            cremaHost.Logout(authentication);
#endif
            cremaHost.CloseAsync(token);
            authenticationToToken.Remove(authentication);
        }
    }
}
