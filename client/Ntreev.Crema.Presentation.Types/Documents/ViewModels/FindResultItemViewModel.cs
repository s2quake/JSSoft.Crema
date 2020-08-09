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

using Ntreev.Crema.ServiceModel;
using Ntreev.Library.IO;
using Ntreev.Library.ObjectModel;
using Ntreev.ModernUI.Framework;

namespace Ntreev.Crema.Presentation.Types.Documents.ViewModels
{
    class FindResultItemViewModel : PropertyChangedBase
    {
        private readonly FindResultInfo resultInfo;
        private readonly string categoryName;
        private readonly string typeName;

        public FindResultItemViewModel(FindResultInfo resultInfo)
        {
            this.resultInfo = resultInfo;
            var itemName = new ItemName(resultInfo.Path);
            this.categoryName = itemName.CategoryPath.Trim(PathUtility.SeparatorChar);
            this.typeName = itemName.Name;
        }

        public string CategoryName => this.categoryName;

        public string TypeName => this.typeName;

        public string ColumnName => this.resultInfo.ColumnName;

        public int Row => this.resultInfo.Row;

        public string Value => this.resultInfo.Value;
    }
}
