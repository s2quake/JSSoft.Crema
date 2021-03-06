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

using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JSSoft.Crema.Comparer.Templates.Views
{
    /// <summary>
    /// TemplatesView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TemplatesView : UserControl, IPartImportsSatisfiedNotification
    {
        [Import]
        private IAppConfiguration configs = null;

        [Import]
        private IShell shell = null;

        public TemplatesView()
        {
            InitializeComponent();
        }

        [ConfigurationProperty]
        private bool IsBrowserExpanded
        {
            get { return this.contentService.IsBrowserExpanded; }
            set { this.contentService.IsBrowserExpanded = value; }
        }

        [ConfigurationProperty]
        private double BrowserDistance
        {
            get { return this.contentService.BrowserDistance; }
            set { this.contentService.BrowserDistance = value; }
        }

        [ConfigurationProperty]
        private bool IsPropertyExpanded
        {
            get { return this.contentService.IsPropertyExpanded; }
            set { this.contentService.IsPropertyExpanded = value; }
        }

        [ConfigurationProperty]
        private double PropertyDistance
        {
            get { return this.contentService.PropertyDistance; }
            set { this.contentService.PropertyDistance = value; }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.properties.LoadSettings(this.configs);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.properties.SaveSettings(this.configs);
        }

        #region IPartImportsSatisfiedNotification

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            this.configs.Update(this);
            this.shell.Closing += (s, e) => this.configs.Commit(this);
        }

        #endregion
    }
}
