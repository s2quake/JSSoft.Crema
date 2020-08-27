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

using FirstFloor.ModernUI.Windows;
using JSSoft.Crema.Presentation.Controls;
using JSSoft.Crema.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using JSSoft.Crema.Designer.ViewModels;
using JSSoft.ModernUI.Framework;
using System.ComponentModel.Composition.Hosting;
using JSSoft.Library;

namespace JSSoft.Crema.Designer.Types.Views
{
    /// <summary>
    /// Types.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TypesView : UserControl, IPartImportsSatisfiedNotification
    {
        [Import]
        private IAppConfiguration configs = null;

        [Import]
        private IShell shell = null;

        public TypesView()
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

        private void Expander_Loaded(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if (expander.DataContext == null)
                return;

            //if (this.configs.TryGetValue<bool>(expander.DataContext.GetType(), nameof(expander.IsExpanded), out var isExpanded) == true)
            //{
            //    expander.IsExpanded = isExpanded;
            //}
        }

        private void Expander_Unloaded(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if (expander.DataContext == null)
                return;

            //this.configs[expander.DataContext.GetType(), nameof(expander.IsExpanded)] = expander.IsExpanded;
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
