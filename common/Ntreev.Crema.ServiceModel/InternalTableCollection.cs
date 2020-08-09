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

using Ntreev.Library.ObjectModel;
using System;

namespace Ntreev.Crema.ServiceModel
{
    class InternalTableCollection<_I, _C, _IC, _CC, _CT> : ContainerBase<_I>
        where _I : TableBase<_I, _C, _IC, _CC, _CT>
        where _C : TableCategoryBase<_I, _C, _IC, _CC, _CT>, new()
        where _IC : ItemContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CC : CategoryContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CT : ItemContext<_I, _C, _IC, _CC, _CT>
    {
        private readonly bool isLocalName;

        public InternalTableCollection()
            : this(false)
        {

        }

        public InternalTableCollection(bool isLocalName)
        {
            this.isLocalName = isLocalName;
        }

        public void Add(_I item)
        {
            if (this.isLocalName == true)
                this.AddBase(item.TableName, item);
            else
                this.AddBase(item.Name, item);
            item.Renamed += Item_Renamed;
            item.Moved += Item_Moved;
        }

        public void Remove(_I item)
        {
            var oldKey = this.GetKey(item);
            if (oldKey != null)
            {
                this.RemoveBase(oldKey);
                item.Renamed -= Item_Renamed;
                item.Moved -= Item_Moved;
            }
        }

        private string GetKey(_I item)
        {
            foreach (var i in this.GetKeyValues())
            {
                if (i.Value == item)
                {
                    return i.Key;
                }
            }
            return null;
        }

        private void Item_Moved(object sender, EventArgs e)
        {
            var item = sender as _I;
            var oldKey = this.GetKey(item);
            if (this.isLocalName == true)
                this.ReplaceKeyBase(oldKey, item.TableName);
            else
                this.ReplaceKeyBase(oldKey, item.Name);
        }

        private void Item_Renamed(object sender, EventArgs e)
        {
            var item = sender as _I;
            var oldKey = this.GetKey(item);
            if (this.isLocalName == true)
                this.ReplaceKeyBase(oldKey, item.TableName);
            else
                this.ReplaceKeyBase(oldKey, item.Name);
        }
    }
}
