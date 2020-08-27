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

using JSSoft.Crema.Data;
using JSSoft.Library;
using System;

namespace JSSoft.Crema.Services.Random
{
    public static class CremaDataSetTypesCreator
    {
        public static void CreateStandardTables(CremaDataSet dataSet)
        {
            //var table1 = CreateStandardTable(dataSet, "table_all", "/base/", TagInfo.All);
            //var table2 = CreateStandardTable(dataSet, "table_server", "/tags/", TagInfo.Server);
            //var table3 = CreateStandardTable(dataSet, "table_client", "/tags/", TagInfo.Client);
            //var table4 = CreateStandardTable(dataSet, "table_unused", "/tags/", TagInfo.Unused);

            //table1.Inherit("derived_all", "/derived_tables/", true);
            //table2.Inherit("derived_server", "/derived_tables/", true);
            //table3.Inherit("derived_client", "/derived_tables/", true);
            //table4.Inherit("derived_unused", "/derived_tables/", true);

            //table1.Copy("derived_all", "/copied_tables/", true);
            //table2.Copy("derived_server", "/copied_tables/", true);
            //table3.Copy("derived_client", "/copied_tables/", true);
            //table4.Copy("derived_unused", "/copied_tables/", true);
        }

        public static void CreateStandardColumns(CremaDataTable table)
        {
            table.Columns.Add("column_key", typeof(string)).IsKey = true;
            //table.Columns.Add("column_all", TagInfo.All);
            //table.Columns.Add("column_server", TagInfo.Server);
            //table.Columns.Add("column_client", TagInfo.Client);
            //table.Columns.Add("column_unused", TagInfo.Unused);
        }

        public static void CreateStandardChild(CremaDataTable table)
        {
            CreateStandardChild(table, "child_all", TagInfoUtility.All);
            CreateStandardChild(table, "child_server", TagInfoUtility.Server);
            CreateStandardChild(table, "child_client", TagInfoUtility.Client);
            CreateStandardChild(table, "child_unused", TagInfoUtility.Unused);
        }

        public static void FillStandardTable(CremaDataTable table)
        {
            FillStandardTable(table, TagInfoUtility.All);
            FillStandardTable(table, TagInfoUtility.Server);
            FillStandardTable(table, TagInfoUtility.Client);
            FillStandardTable(table, TagInfoUtility.Unused);
        }

        public static CremaDataRow FillStandardTable(CremaDataTable table, TagInfo tags)
        {
            var row = table.NewRow();
            row.Tags = tags;
            row["column_key"] = $"{tags}.key".ToLower();
            row["column_all"] = $"{tags}.all".ToLower();
            row["column_server"] = $"{tags}.server".ToLower();
            row["column_client"] = $"{tags}.client".ToLower();
            row["column_unused"] = $"{tags}.unused".ToLower();
            table.Rows.Add(row);

            foreach (var item in table.Childs)
            {
                FillStandardChild(item, row, TagInfoUtility.All);
                FillStandardChild(item, row, TagInfoUtility.Server);
                FillStandardChild(item, row, TagInfoUtility.Client);
                FillStandardChild(item, row, TagInfoUtility.Unused);
            }
            return row;
        }

        public static void FillStandardChild(CremaDataTable table, CremaDataRow parentRow, TagInfo tags)
        {
            var row = table.NewRow(parentRow);
            row.Tags = tags;
            row["column_key"] = $"{parentRow.Tags}.{tags}.key".ToLower();
            row["column_all"] = $"{parentRow.Tags}.{tags}.all".ToLower();
            row["column_server"] = $"{parentRow.Tags}.{tags}.server".ToLower();
            row["column_client"] = $"{parentRow.Tags}.{tags}.client".ToLower();
            row["column_unused"] = $"{parentRow.Tags}.{tags}.unused".ToLower();
            table.Rows.Add(row);
        }

        public static CremaDataTable CreateStandardTable(CremaDataSet dataSet, string tableName, string categoryPath, TagInfo tags)
        {
            var table = dataSet.Tables.Add(tableName, categoryPath);
            table.Tags = tags;
            CreateStandardColumns(table);
            CreateStandardChild(table);
            FillStandardTable(table);
            return table;
        }

        public static CremaDataColumn CreateStandardColumn(CremaDataTable table, string columnName, Type dataType)
        {
            var column = table.Columns.Add(columnName, dataType);
            return column;
        }

        public static CremaDataTable CreateStandardChild(CremaDataTable table, string childName, TagInfo tags)
        {
            var child = table.Childs.Add(childName);
            child.Tags = tags;
            CreateStandardColumns(child);
            return child;
        }
    }
}