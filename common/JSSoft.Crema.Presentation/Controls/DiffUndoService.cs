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

using JSSoft.ModernUI.Framework;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace JSSoft.Crema.Presentation.Controls
{
    public static class DiffUndoService
    {
        public static readonly DependencyProperty UndoServiceProperty =
            DependencyProperty.RegisterAttached(nameof(UndoService), typeof(IUndoService), typeof(DiffUndoService),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty SelectorProperty =
            DependencyProperty.RegisterAttached(nameof(Selector), typeof(Selector), typeof(DiffUndoService),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static IUndoService GetUndoService(DependencyObject obj)
        {
            return (IUndoService)obj.GetValue(UndoServiceProperty);
        }

        public static void SetUndoService(DependencyObject obj, IUndoService value)
        {
            obj.SetValue(UndoServiceProperty, value);
        }

        public static Selector GetSelector(DependencyObject obj)
        {
            return (Selector)obj.GetValue(SelectorProperty);
        }

        public static void SetSelector(DependencyObject obj, Selector value)
        {
            obj.SetValue(SelectorProperty, value);
        }
    }
}
