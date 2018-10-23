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

using Ntreev.Crema.Presentation.Base.Properties;
using Ntreev.Crema.Presentation.Base.Services.ViewModels;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Base.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(ConnectionItemViewModel))]
    class ConnectionItemEditMenuItem : MenuItemBase
    {
        [Import]
        private Lazy<CremaAppHostViewModel> cremaAppHost = null;

        public ConnectionItemEditMenuItem()
        {
            this.DisplayName = Resources.MenuItem_Edit;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is ConnectionItemViewModel item && item.IsTemporary == false)
                return true;
            return false;
        }

        protected override void OnExecute(object parameter)
        {
            if (parameter is ConnectionItemViewModel connectionItem)
            {
                this.CremaAppHost.EditConnectionItem(connectionItem);
            }
        }

        private CremaAppHostViewModel CremaAppHost => this.cremaAppHost.Value;
    }
}
