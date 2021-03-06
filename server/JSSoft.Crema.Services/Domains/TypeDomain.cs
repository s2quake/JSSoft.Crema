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
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Data;
using JSSoft.Crema.Services.Domains.Serializations;
using JSSoft.Crema.Services.Properties;
using JSSoft.Library;
using JSSoft.Library.ObjectModel;
using JSSoft.Library.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace JSSoft.Crema.Services.Domains
{
    class TypeDomain : Domain
    {
        private readonly CremaDataType dataType;
        private readonly DataView view;
        private byte[] data;

        public TypeDomain(DomainSerializationInfo serializationInfo, object source)
            : base(serializationInfo, source)
        {
            this.IsNew = (bool)serializationInfo.GetProperty(nameof(IsNew));
            this.dataType = source as CremaDataType;
            this.view = this.dataType.View;

            var dataSet = this.dataType.DataSet;
            var itemPaths = (string)serializationInfo.GetProperty(nameof(ItemPaths));
            dataSet.SetItemPaths(StringUtility.Split(itemPaths, ';'));
        }

        public TypeDomain(Authentication authentication, CremaDataType dataType, DataBase dataBase, string itemPath, string itemType)
            : base(authentication.ID, dataType, dataBase.ID, itemPath, itemType)
        {
            this.dataType = dataType;
            this.view = this.dataType.View;
        }

        public bool IsNew { get; set; }

        public string[] ItemPaths => this.dataType.DataSet.GetItemPaths();

        protected override byte[] SerializeSource(object source)
        {
            if (this.data == null)
            {
                if (source is CremaDataType dataType)
                {
                    var text = dataType.Path + ";" + XmlSerializerUtility.GetString(dataType.DataSet);
                    this.data = Encoding.UTF8.GetBytes(text.Compress());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return this.data;
        }

        protected override object DerializeSource(byte[] data)
        {
            var text = Encoding.UTF8.GetString(data).Decompress();
            var index = text.IndexOf(";");
            var path = text.Remove(index);
            var itemName = new ItemName(path);
            var xml = text.Substring(index + 1);
            var dataSet = XmlSerializerUtility.ReadString<CremaDataSet>(xml);
            return dataSet.Types[itemName.Name];
        }

        protected override void OnSerializaing(IDictionary<string, object> properties)
        {
            base.OnSerializaing(properties);
            properties.Add(nameof(this.IsNew), this.IsNew);
            properties.Add(nameof(this.ItemPaths), string.Join(";", this.ItemPaths));
        }

        protected override DomainRowInfo[] OnNewRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            this.dataType.SignatureDateProvider = signatureProvider;
            try
            {
                for (var i = 0; i < rows.Length; i++)
                {
                    var rowView = CremaDomainUtility.AddNew(this.view, rows[i].Fields);
                    rows[i].Keys = CremaDomainUtility.GetKeys(rowView);
                    rows[i].Fields = CremaDomainUtility.GetFields(rowView);
                }
                this.dataType.AcceptChanges();
                this.data = null;
                return rows;
            }
            catch
            {
                this.dataType.RejectChanges();
                throw;
            }
        }

        protected override DomainRowInfo[] OnSetRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            this.dataType.SignatureDateProvider = signatureProvider;
            try
            {
                for (var i = 0; i < rows.Length; i++)
                {
                    rows[i].Fields = CremaDomainUtility.SetFields(this.view, rows[i].Keys, rows[i].Fields);
                }
                this.dataType.AcceptChanges();
                this.data = null;
                return rows;
            }
            catch
            {
                this.dataType.RejectChanges();
                throw;
            }
        }

        protected override DomainRowInfo[] OnRemoveRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            this.dataType.SignatureDateProvider = signatureProvider;
            try
            {
                foreach (var item in rows)
                {
                    CremaDomainUtility.Delete(this.view, item.Keys);
                }
                this.dataType.AcceptChanges();
                this.data = null;
                return rows;
            }
            catch
            {
                this.dataType.RejectChanges();
                throw;
            }
        }

        protected override void OnSetProperty(DomainUser domainUser, string propertyName, object value, SignatureDateProvider signatureProvider)
        {
            if (propertyName == CremaSchema.TypeName)
            {
                if (this.IsNew == false)
                    throw new InvalidOperationException(Resources.Exception_CannotRename);
                this.dataType.TypeName = (string)value;
            }
            else if (propertyName == CremaSchema.IsFlag)
            {
                this.dataType.IsFlag = (bool)value;
            }
            else if (propertyName == CremaSchema.Comment)
            {
                this.dataType.Comment = (string)value;
            }
            else
            {
                throw new NotSupportedException();
            }
            this.data = null;
        }
    }
}
