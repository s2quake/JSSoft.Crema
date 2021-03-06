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

using JSSoft.Crema.Presentation.Home.Properties;
using JSSoft.Crema.Presentation.Home.Services.ViewModels;
using JSSoft.ModernUI.Framework;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Home.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(FileMenuItem))]
    class LeaveDataBaseMenuItem : MenuItemBase
    {
        private readonly CremaAppHostViewModel cremaAppHost;

        [ImportingConstructor]
        public LeaveDataBaseMenuItem(CremaAppHostViewModel cremaAppHost)
        {
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Loaded += this.InvokeCanExecuteChangedEvent;
            this.cremaAppHost.Unloaded += this.InvokeCanExecuteChangedEvent;
            this.cremaAppHost.PropertyChanged += CremaAppHost_PropertyChanged;
            this.DisplayName = Resources.MenuItem_LeaveDataBase;
        }

        protected override async void OnExecute(object parameter)
        {
            if (await AppMessageBox.ShowQuestionAsync(Resources.Message_CloseDataBase) == true)
            {
                await this.cremaAppHost.UnloadAsync();
            }
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (this.cremaAppHost.IsOpened == false)
                return false;
            if (this.cremaAppHost.IsLoaded == false)
                return false;
            return this.cremaAppHost.IsProgressing == false;
        }

        private void CremaAppHost_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CremaAppHostViewModel.IsProgressing) || e.PropertyName == string.Empty)
            {
                this.Refresh();
            }
        }
    }
}
