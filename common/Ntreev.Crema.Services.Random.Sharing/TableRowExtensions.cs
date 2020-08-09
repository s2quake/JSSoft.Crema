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

using Ntreev.Crema.Data;
using Ntreev.Library.Random;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Random
{
    public static class TableRowExtensions
    {
        public static async Task<bool> InitializeRandomAsync(this ITableRow tableRow, Authentication authentication)
        {
            var content = tableRow.Content;
            var table = content.Table;
            foreach (var item in table.TableInfo.Columns)
            {
                if (tableRow[item.Name] != null && item.IsUnique == false)
                    continue;
                if (await SetRandomValueAsync(tableRow, authentication, item.Name) == false)
                    return false;
            }
            return true;
        }

        public static async Task<bool> SetRandomValueAsync(this ITableRow tableRow, Authentication authentication)
        {
            var content = tableRow.Content;
            var table = content.Table;
            var columnInfo = table.TableInfo.Columns.Random();
            var value = await GetRandomValueAsync(tableRow, columnInfo.Name);
            if (value == null)
                return false;
            await tableRow.SetFieldAsync(authentication, columnInfo.Name, value);
            return true;
        }

        public static async Task<bool> SetRandomValueAsync(this ITableRow tableRow, Authentication authentication, int tryCount)
        {
            var count = 0;
            for (var i = 0; i < tryCount; i++)
            {
                try
                {
                    if (await SetRandomValueAsync(tableRow, authentication) == true)
                        count++;
                }
                catch
                {

                }
            }
            return count > 0;
        }

        public static async Task<bool> SetRandomValueAsync(this ITableRow tableRow, Authentication authentication, string columnName)
        {
            var value = await GetRandomValueAsync(tableRow, columnName);
            if (value == null)
                return false;

            var domain = tableRow.Content.Domain;
            var table = tableRow.Content.Table;
            var dataSet = domain.Source as CremaDataSet;
            var dataTable = dataSet.Tables[table.Name, table.Category.Path];

            if (dataTable.Columns[columnName].Unique == true)
            {
                var expression = CremaDataExtensions.GenerateFieldExpression(columnName, value);
                var items = dataTable.Select(expression);
                if (items.Any() == true)
                    return false;
            }

            await tableRow.SetFieldAsync(authentication, columnName, value);
            return true;
        }

        private static async Task<object> GetRandomValueAsync(this ITableRow tableRow, string columnName)
        {
            var content = tableRow.Content;
            var table = content.Table;
            var tableInfo = await table.Dispatcher.InvokeAsync(() => table.TableInfo);
            var columnInfo = tableInfo.Columns.Where(item => item.Name == columnName).First();

            if (CremaDataTypeUtility.IsBaseType(columnInfo.DataType) == true)
            {
                var type = CremaDataTypeUtility.GetType(columnInfo.DataType);
                return RandomUtility.Next(type);
            }
            else
            {
                var typeContext = table.GetService(typeof(ITypeContext)) as ITypeContext;
                var type = await typeContext.Dispatcher.InvokeAsync(() => typeContext[columnInfo.DataType] as IType);
                return await type.GetRandomValueAsync();
            }
        }
    }
}