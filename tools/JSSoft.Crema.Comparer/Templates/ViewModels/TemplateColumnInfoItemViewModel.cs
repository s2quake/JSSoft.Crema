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

using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Diff;
using JSSoft.Library;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Comparer.Templates.ViewModels
{
    class TemplateColumnInfoItemViewModel
    {
        private ColumnInfo columnInfo;
        private string dataType;
        private string categoryName;

        public TemplateColumnInfoItemViewModel(ColumnInfo columnInfo)
        {
            this.columnInfo = columnInfo;

            if (NameValidator.VerifyItemPath(this.columnInfo.DataType) == false)
            {
                this.dataType = this.columnInfo.DataType;
                this.categoryName = string.Empty;
            }
            else
            {
                var itemName = new ItemName(this.columnInfo.DataType);
                this.dataType = itemName.Name;
                this.categoryName = itemName.CategoryPath;
            }
        }

        public string Header
        {
            get
            {
                if (this.columnInfo.Name.StartsWith(DiffUtility.DiffDummyKey) == true)
                    return string.Empty;
                return this.columnInfo.Name;
            }
        }

        public string Content
        {
            get
            {
                if (this.columnInfo.Name.StartsWith(DiffUtility.DiffDummyKey) == true)
                    return string.Empty;
                return this.columnInfo.DataType;
            }
        }

        public string Name
        {
            get { return this.columnInfo.Name; }
        }

        public string DataType
        {
            get { return this.dataType; }
        }

        public string CategoryName
        {
            get { return this.categoryName; }
        }

        public string Comment
        {
            get { return this.columnInfo.Comment; }
        }

        public TagInfo Tags
        {
            get { return this.columnInfo.DerivedTags; }
        }

        public bool IsKey
        {
            get { return this.columnInfo.IsKey; }
        }

        public bool IsUnique
        {
            get { return this.columnInfo.IsUnique; }
        }
    }
}
