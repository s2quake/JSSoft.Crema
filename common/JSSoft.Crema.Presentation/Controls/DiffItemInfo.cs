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

using System.Collections.Generic;
using Xceed.Wpf.DataGrid;

namespace JSSoft.Crema.Presentation.Controls
{
    struct DiffItemInfo
    {
        public DiffItemInfo(DataGridContext gridContext, object item)
        {
            this.GridContext = gridContext;
            this.Item = item;
        }

        public DataGridContext GridContext { get; set; }

        public object Item { get; set; }

        public static IEnumerable<DiffItemInfo> GetVisibleItems(DataGridContext gridContext)
        {
            foreach (var item in gridContext.Headers)
            {
                yield return new DiffItemInfo(gridContext, item);
            }

            foreach (var item in gridContext.Items)
            {
                yield return new DiffItemInfo(gridContext, item);
            }

            foreach (var item in gridContext.Footers)
            {
                yield return new DiffItemInfo(gridContext, item);
            }
        }

        public static IEnumerable<DiffItemInfo> GetItems(DataGridContext gridContext)
        {
            for (var i = 0; i < gridContext.Items.Count; i++)
            {
                var item = gridContext.Items.GetItemAt(i);
                yield return new DiffItemInfo(gridContext, item);
            }
        }

        public static IEnumerable<DiffItemInfo> GetReverseItems(DataGridContext gridContext)
        {
            for (var i = gridContext.Items.Count - 1; i >= 0; i--)
            {
                var item = gridContext.Items.GetItemAt(i);
                yield return new DiffItemInfo(gridContext, item);
            }
        }
    }
}
