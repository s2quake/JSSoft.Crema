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

using Ntreev.Library.Commands;
using System.ComponentModel;

namespace Ntreev.Crema.ApplicationHost
{
    public class AppSettings
    {
        [CommandProperty]
        public string[] PluginsPath
        {
            get; set;
        }

        /// <summary>
        /// crema://userID:password@localhost
        /// crema://userID:password@localhost/dataBase
        /// </summary>
        [CommandProperty]
        [DefaultValue("")]
        public string Address
        {
            get; set;
        }

        /// <summary>
        /// light or dark
        /// </summary>
        [CommandProperty]
        [DefaultValue("")]
        public string Theme
        {
            get; set;
        }

        /// <summary>
        /// color as #ffffff
        /// </summary>
        [CommandProperty("color")]
        [DefaultValue("")]
        public string ThemeColor
        {
            get; set;
        }

        [CommandProperty]
        [DefaultValue(false)]
        public bool ReportDetails
        {
            get; set;
        }

        [CommandProperty]
#if DEBUG
        [DefaultValue("en-US")]
#else
        [DefaultValue("")]
#endif
        public string Culture
        {
            get; set;
        }
    }
}
