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

using JSSoft.ModernUI.Framework.Controls;
using System.Windows;

namespace JSSoft.Crema.Presentation.Assets.AvalonDocks
{
    partial class Theme : ResourceDictionary
    {
        private void IconImage_Loaded(object sender, RoutedEventArgs e)
        {
            var iconImage = sender as IconImage;
            iconImage.Loaded -= IconImage_Loaded;

            //var binding = new Binding("IsMouseOver")
            //{
            //    RelativeSource = new RelativeSource() { AncestorType = typeof(MenuItem), },
            //    Converter = new InverseBooleanConverter(),
            //};
            //BindingOperations.SetBinding(iconImage, IconImage.UseColorProperty, binding);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
