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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JSSoft.Crema.Services
{
    /// <summary>
    /// 로그를 기록할 수 있는 도구입니다. 
    /// 이 정적 클래스는 전역으로 기록되는 로그이므로
    /// 저장소별로 로그를 기록하기 위해서는 ICremaHost 에서 ILogService 형태의 서비스를 취득하여 사용하시기 바랍니다.
    /// 전역 로그 파일은 AppData\Roaming\NtreevSoft\cremaservice
    /// 서비스 실행시 전역 로그 파일은 C:\Windows\System32\config\systemprofile\AppData\Roaming\NtreevSoft\cremaservice
    /// </summary>
    public static class CremaLog
    {
        private static LogService log;
        private static readonly List<CremaHost> references = new List<CremaHost>();

        static CremaLog()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Release();
        }

        public static void Debug(object message)
        {
            LogService.Debug(message);
        }

        public static void Info(object message)
        {
            LogService.Info(message);
        }

        public static void Error(object message)
        {
            LogService.Error(message);
        }

        public static void Warn(object message)
        {
            LogService.Warn(message);
        }

        public static void Fatal(object message)
        {
            LogService.Fatal(message);
        }

        public static void Debug(string format, params object[] args)
        {
            LogService.Debug(string.Format(format, args));
        }

        public static void Info(string format, params object[] args)
        {
            LogService.Info(string.Format(format, args));
        }

        public static void Error(string format, params object[] args)
        {
            LogService.Error(string.Format(format, args));
        }

        public static void Error(Exception e)
        {
            LogService.Error(e.ToString());
        }

        public static void Warn(string format, params object[] args)
        {
            LogService.Warn(string.Format(format, args));
        }

        public static void Fatal(string format, params object[] args)
        {
            LogService.Fatal(string.Format(format, args));
        }

        public static void AddRedirection(TextWriter writer, LogVerbose verbose)
        {
            LogService.AddRedirection(writer, verbose);
        }

        public static void RemoveRedirection(TextWriter writer)
        {
            LogService.RemoveRedirection(writer);
        }

        //public static TextWriter RedirectionWriter
        //{
        //    get => LogService.RedirectionWriter;
        //    set => LogService.RedirectionWriter = value;
        //}

        public static LogVerbose Verbose
        {
            get => LogService.Verbose;
            set => LogService.Verbose = value;
        }

        private static void Release()
        {
            log?.Dispose();
            log4net.LogManager.Shutdown();
        }

        internal static LogService LogService
        {
            get
            {
                if (log == null)
                    log = new LogService();
                return log;
            }
        }

        internal static void Attach(CremaHost cremaHost)
        {
            lock (references)
            {
                if (references.Any() == false)
                {
                    //Dispatcher = new CremaDispatcher(typeof(CremaLog));
                }
                references.Add(cremaHost);
            }
        }

        internal static void Detach(CremaHost cremaHost)
        {
            lock (references)
            {
                references.Remove(cremaHost);
                if (references.Any() == false)
                {
                    //Dispatcher.Dispose();
                    //Dispatcher = null;
                }
            }
        }

        internal static CremaDispatcher Dispatcher { get; private set; }
    }
}