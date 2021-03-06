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
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript.Methods.DataBase
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(DataBase))]
    class GetTableInfoByTagsMethod : ScriptFuncTaskBase<string, string, string, IDictionary<string, object>>
    {
        [ImportingConstructor]
        public GetTableInfoByTagsMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override async Task<IDictionary<string, object>> OnExecuteAsync(string dataBaseName, string tableName, string tags)
        {
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            var table = await dataBase.GetTableAsync(tableName);
            return await table.Dispatcher.InvokeAsync(() =>
            {
                var tableInfo = table.TableInfo.Filter((TagInfo)tags);
                var props = new Dictionary<string, object>
                {
                    { nameof(tableInfo.ID), tableInfo.ID },
                    { nameof(tableInfo.Name), tableInfo.Name },
                    { nameof(tableInfo.TableName), tableInfo.TableName },
                    { nameof(tableInfo.Tags), $"{tableInfo.Tags}" },
                    { nameof(tableInfo.DerivedTags), $"{tableInfo.DerivedTags}" },
                    { nameof(tableInfo.Comment), tableInfo.Comment },
                    { nameof(tableInfo.TemplatedParent), tableInfo.TemplatedParent },
                    { nameof(tableInfo.ParentName), tableInfo.ParentName },
                    { nameof(tableInfo.CategoryPath), tableInfo.CategoryPath },
                    { nameof(tableInfo.HashValue), tableInfo.HashValue },
                    { CremaSchema.Creator, tableInfo.CreationInfo.ID },
                    { CremaSchema.CreatedDateTime, tableInfo.CreationInfo.DateTime },
                    { CremaSchema.Modifier, tableInfo.ModificationInfo.ID },
                    { CremaSchema.ModifiedDateTime, tableInfo.ModificationInfo.DateTime },
                    { CremaSchema.ContentsModifier, tableInfo.ContentsInfo.ID },
                    { CremaSchema.ContentsModifiedDateTime, tableInfo.ContentsInfo.DateTime },
                    { nameof(tableInfo.Columns), this.GetColumnsInfo(tableInfo.Columns) }
                };

                return props;
            });
        }

        private object[] GetColumnsInfo(ColumnInfo[] columns)
        {
            var props = new object[columns.Length];
            for (var i = 0; i < columns.Length; i++)
            {
                props[i] = this.GetColumnInfo(columns[i]);
            }
            return props;
        }

        private IDictionary<string, object> GetColumnInfo(ColumnInfo columnInfo)
        {
            var props = new Dictionary<string, object>
            {
                { nameof(columnInfo.ID), columnInfo.ID },
                { nameof(columnInfo.IsKey), columnInfo.IsKey },
                { nameof(columnInfo.IsUnique), columnInfo.IsUnique },
                { nameof(columnInfo.AllowNull), columnInfo.AllowNull },
                { nameof(columnInfo.Name), columnInfo.Name },
                { nameof(columnInfo.DataType), columnInfo.DataType },
                { nameof(columnInfo.DefaultValue), this.GetDefaultValue(columnInfo) },
                { nameof(columnInfo.Comment), columnInfo.Comment },
                { nameof(columnInfo.AutoIncrement), columnInfo.AutoIncrement },
                { nameof(columnInfo.ReadOnly), columnInfo.ReadOnly },
                { nameof(columnInfo.Tags), $"{columnInfo.Tags}" },
                { nameof(columnInfo.DerivedTags), $"{columnInfo.DerivedTags}" },
                { CremaSchema.Creator, columnInfo.CreationInfo.ID },
                { CremaSchema.CreatedDateTime, columnInfo.CreationInfo.DateTime },
                { CremaSchema.Modifier, columnInfo.ModificationInfo.ID },
                { CremaSchema.ModifiedDateTime, columnInfo.ModificationInfo.DateTime }
            };
            return props;
        }

        private object GetDefaultValue(ColumnInfo columnInfo)
        {
            if (columnInfo.DefaultValue != null && CremaDataTypeUtility.IsBaseType(columnInfo.DataType) == true)
            {
                var type = CremaDataTypeUtility.GetType(columnInfo.DataType);
                return CremaConvert.ChangeType(columnInfo.DefaultValue, type);
            }
            return columnInfo.DefaultValue;
        }
    }
}
