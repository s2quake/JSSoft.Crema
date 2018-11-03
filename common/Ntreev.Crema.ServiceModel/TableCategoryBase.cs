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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Collections.Generic;
using Ntreev.Library.ObjectModel;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.ServiceModel.Properties;
using Ntreev.Library.IO;
using Ntreev.Crema.Data.Xml.Schema;

namespace Ntreev.Crema.ServiceModel
{
    internal abstract class TableCategoryBase<_I, _C, _IC, _CC, _CT> : PermissionCategoryBase<_I, _C, _IC, _CC, _CT>
        where _I : TableBase<_I, _C, _IC, _CC, _CT>
        where _C : TableCategoryBase<_I, _C, _IC, _CC, _CT>, new()
        where _IC : ItemContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CC : CategoryContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CT : ItemContext<_I, _C, _IC, _CC, _CT>
    {
        private CategoryMetaData metaData = CategoryMetaData.Empty;

        public TableCategoryBase()
        {

        }

        public IContainer<_I> Tables => this.Items;

        public CategoryMetaData MetaData => this.metaData;

        public string FullPath => PathUtility.Separator + CremaSchema.TableDirectory + this.Path;

        protected override void OnRenamed(EventArgs e)
        {
            base.OnRenamed(e);
        }

        protected override void OnMoved(EventArgs e)
        {
            base.OnMoved(e);
        }

        protected override void OnAccessChanged(EventArgs e)
        {
            this.metaData.AccessInfo = this.AccessInfo;
            base.OnAccessChanged(e);
        }

        protected override void OnLockChanged(EventArgs e)
        {
            this.metaData.LockInfo = this.LockInfo;
            base.OnLockChanged(e);
        }

        protected override void OnPathChanged(string oldPath, string newPath)
        {
            this.metaData.Path = this.Path;
            base.OnPathChanged(oldPath, newPath);
        }
    }
}
