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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Windows.Media;
using System.Security;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Framework
{
    public interface ICremaAppHost : IServiceProvider
    {
        Task LoginAsync(string address, string userID, string password, string dataBaseName);

        Task LogoutAsync();

        void SelectDataBase(string dataBaseName);
        
        bool IsLoaded { get; }

        bool IsOpened { get; }

        Color ThemeColor { get; set; }

        string Theme { get; set; }

        string DataBaseName { get; }

        string Address { get; }

        Authority Authority { get; }

        IEnumerable<IDataBaseDescriptor> DataBases { get; }

        IEnumerable<IConnectionItem> ConnectionItems { get; }

        IConnectionItem ConnectionItem { get; set; }

        ICremaConfiguration UserConfigs { get; }

        event EventHandler Loaded;

        event EventHandler Unloaded;

        event EventHandler Resetting;

        event EventHandler Reset;

        event EventHandler Opened;

        event CloseRequestedEventHandler CloseRequested;

        event EventHandler Closed;
    }
}
