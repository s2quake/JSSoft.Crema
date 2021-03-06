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
using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using JSSoft.Library.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace JSSoft.Crema.Services.Domains
{
    class TableContentDomain : Domain
    {
        private readonly Dictionary<string, DataView> views = new();
        private readonly Dictionary<DataView, CremaDataTable> tables = new();

        public TableContentDomain(DomainInfo domainInfo)
            : base(domainInfo)
        {

        }

        public override object Source => this.DataSet;

        public CremaDataSet DataSet { get; private set; }

        protected override byte[] SerializeSource()
        {
            var xml = XmlSerializerUtility.GetString(this.DataSet);
            return Encoding.UTF8.GetBytes(xml.Compress());
        }

        protected override void DerializeSource(byte[] data)
        {
            var xml = Encoding.UTF8.GetString(data).Decompress();
            this.DataSet = XmlSerializerUtility.ReadString<CremaDataSet>(xml);

            foreach (var item in this.DataSet.Tables)
            {
                var view = item.AsDataView();
                this.views.Add(item.Name, view);
                this.tables.Add(view, item);
            }
        }

        protected override void OnInitialize(byte[] data)
        {
            base.OnInitialize(data);

            var xml = Encoding.UTF8.GetString(data).Decompress();
            this.DataSet = XmlSerializerUtility.ReadString<CremaDataSet>(xml);
            this.DataSet.AcceptChanges();
            this.views.Clear();
            foreach (var item in this.DataSet.Tables)
            {
                var view = item.AsDataView();
                //view.AllowDelete = false;
                //view.AllowEdit = false;
                //view.AllowNew = false;
                this.views.Add(item.Name, view);
                this.tables.Add(view, item);
            }
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            this.DataSet = null;
            this.views.Clear();
        }

        protected override void OnDeleted(EventArgs e)
        {
            base.OnDeleted(e);
            if (this.DataSet != null)
            {
                this.DataSet.Dispose();
                this.DataSet = null;
            }
        }

        protected override void OnNewRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            this.DataSet.BeginLoad();
            foreach (var item in rows)
            {
                var view = this.views[item.TableName];
                var table = view.Table;
                CremaDomainUtility.AddNew(view, item.Fields);
                this.tables[view].ContentsInfo = signatureDate;
            }
            this.DataSet.EndLoad();
            this.DataSet.AcceptChanges();
        }

        protected override void OnRemoveRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            this.DataSet.BeginLoad();
            foreach (var item in rows.Reverse())
            {
                var view = this.views[item.TableName];
                if (DomainRowInfo.ClearKey.SequenceEqual(item.Keys) == true)
                {
                    view.Table.Clear();
                }
                else
                {
                    CremaDomainUtility.Delete(view, item.Keys);
                }
                this.tables[view].ContentsInfo = signatureDate;
            }
            this.DataSet.EndLoad();
            this.DataSet.AcceptChanges();
        }

        protected override void OnSetRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            this.DataSet.BeginLoad();
            foreach (var item in rows)
            {
                var view = this.views[item.TableName];
                CremaDomainUtility.SetFieldsForce(view, item.Keys, item.Fields);
                this.tables[view].ContentsInfo = signatureDate;
            }
            this.DataSet.EndLoad();
            this.DataSet.AcceptChanges();
        }
    }
}
