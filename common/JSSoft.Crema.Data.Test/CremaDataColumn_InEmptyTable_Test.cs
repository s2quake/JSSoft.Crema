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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSSoft.Crema.Data.Random;
using JSSoft.Library.Random;
using System;
using System.Linq;

namespace JSSoft.Crema.Data.Test
{
    [TestClass]
    public class CremaDataColumn_InEmptyTable_Test
    {
        private CremaDataSet dataSet;
        private CremaDataColumn[] columns;

        public CremaDataColumn_InEmptyTable_Test()
        {
            this.Reset();
        }

        private void Reset()
        {
            Random.CremaDataTableExtensions.MinRowCount = 0;
            Random.CremaDataTableExtensions.MaxRowCount = 0;
            this.dataSet = new CremaDataSet();
            this.dataSet.AddRandoms(10);
            this.columns = this.dataSet.Tables.SelectMany(item => item.Columns).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetNullName_Fail()
        {
            var column = this.RandomOrDefault();
            column.ColumnName = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetEmptyName_Fail()
        {
            var column = this.RandomOrDefault();
            column.ColumnName = string.Empty;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetInvalidName_Fail()
        {
            var column = this.RandomOrDefault();
            var columnName = RandomUtility.NextInvalidIdentifier();
            column.ColumnName = columnName;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetExistedName_Fail()
        {
            var column = this.RandomOrDefault();
            var columnName = this.RandomOrDefault(item => item.Table == column.Table && item != column).ColumnName;
            column.ColumnName = columnName;
        }

        [TestMethod]
        public void SetName()
        {
            var column = this.RandomOrDefault();
            var columnName = RandomUtility.NextIdentifier();
            column.ColumnName = columnName;
            Assert.AreEqual(columnName, column.ColumnName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetCremaTypeNotInDataSet_Fail()
        {
            var column = this.RandomOrDefault();
            var columnType = CremaDataTypeExtensions.CreateRandomType();
            column.CremaType = columnType;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetCremaTypeNotInDataSetToNotStringTypeColumn_Fail()
        {
            var columnType = CremaDataTypeExtensions.CreateRandomType();
            var column = this.RandomOrDefault(item => item.DataType != typeof(string));
            column.CremaType = columnType;
        }

        [TestMethod]
        public void SetCremaType()
        {
            var column = this.RandomOrDefault(item => item.CremaType == null);
            if (column == null)
                return;
            var columnType = this.dataSet.Types.Random();
            column.CremaType = columnType;
            Assert.AreEqual(typeof(string), column.DataType);
            Assert.AreEqual(DBNull.Value, column.DefaultValue);
            Assert.AreEqual(false, column.AutoIncrement);
            Assert.AreEqual(columnType, column.CremaType);
        }

        [TestMethod]
        public void SetCremaTypeToNotStringTypeColumn()
        {
            var columnType = this.dataSet.Types.Random();
            var column = this.RandomOrDefault(item => item.DataType != typeof(string));

            column.CremaType = columnType;
            Assert.AreEqual(typeof(string), column.DataType);
            Assert.AreEqual(columnType, column.CremaType);
            Assert.AreEqual(DBNull.Value, column.DefaultValue);
        }

        [TestMethod]
        public void SetNullCremaTypeToCremaTypeColumn()
        {
            var column = this.RandomOrDefault(item => item.CremaType != null);
            if (column == null)
                return;

            column.CremaType = null;
            Assert.AreEqual(typeof(string), column.DataType);
            Assert.IsNull(column.CremaType);
        }

        [TestMethod]
        public void SetDataTypeToCremaTypeColumn()
        {
            var column = this.RandomOrDefault(item => item.CremaType != null);
            if (column == null)
                return;

            if (column.DefaultValue is DBNull == false)
                column.DefaultValue = DBNull.Value;

            var cremaType = column.CremaType;
            var refCount = cremaType.ReferencedColumns.Length;
            var dataType = CremaDataTypeUtility.GetBaseTypes().Random();
            column.DataType = dataType;
            Assert.AreEqual(dataType, column.DataType);
            Assert.IsNull(column.CremaType);
            Assert.AreNotEqual(refCount, cremaType.ReferencedColumns.Length);
        }

        [TestMethod]
        public void SetKeyTrue()
        {
            var column = this.RandomOrDefault(item => item.IsKey == false);
            column.IsKey = true;
            Assert.AreEqual(true, column.IsKey);
        }

        [TestMethod]
        public void SetKeyFalse()
        {
            var column = this.RandomOrDefault(item => item.IsKey == true);
            column.IsKey = false;
            Assert.AreEqual(false, column.IsKey);
        }

        [TestMethod]
        public void SetUniqueTrue()
        {
            var column = this.RandomOrDefault(item => item.Unique == false);
            column.Unique = true;
            Assert.AreEqual(true, column.Unique);
        }

        [TestMethod]
        public void SetUniqueFalse()
        {
            var column = this.RandomOrDefault(item => item.IsKey == false && item.Unique == true);
            column.Unique = false;
            Assert.AreEqual(false, column.Unique);
        }

        [TestMethod]
        public void SetComment()
        {
            var column = this.RandomOrDefault(item => item.Comment == string.Empty);
            var comment = RandomUtility.NextString();
            column.Comment = comment;
            Assert.AreEqual(comment, column.Comment);
        }

        [TestMethod]
        public void SetNullComment()
        {
            var column = this.RandomOrDefault(item => item.Comment == string.Empty);
            column.Comment = null;
            Assert.AreEqual(string.Empty, column.Comment);
        }

        [TestMethod]
        public void SetReadOnlyTrue()
        {
            var column = this.RandomOrDefault(item => item.ReadOnly == false);
            column.ReadOnly = true;
            Assert.AreEqual(true, column.ReadOnly);
        }

        [TestMethod]
        public void SetReadOnlyFalse()
        {
            var column = this.RandomOrDefault(item => item.ReadOnly == true);
            column.ReadOnly = false;
            Assert.AreEqual(false, column.ReadOnly);
        }

        [TestMethod]
        public void SetDefaultValue()
        {
            var column = this.RandomOrDefault(item => item.DefaultValue == DBNull.Value && item.AutoIncrement == false);
            var defaultValue = column.CremaType != null ? column.CremaType.GetRandomValue() : RandomUtility.Next(column.DataType);
            column.DefaultValue = defaultValue;
            Assert.AreEqual(defaultValue, column.DefaultValue);
        }

        [TestMethod]
        public void SetDefaultValueAsString()
        {
            var column = this.RandomOrDefault(item => item.DefaultValue == DBNull.Value && item.AutoIncrement == false);
            column.DataType = CremaDataTypeUtility.GetBaseTypes().Random();
            var defaultValue = RandomUtility.Next(column.DataType);
            column.DefaultValue = CremaConvert.ChangeType(defaultValue, typeof(string));
            if (column.DataType == typeof(DateTime))
                Assert.AreEqual(defaultValue.ToString(), column.DefaultValue.ToString());
            else
                Assert.AreEqual(defaultValue, column.DefaultValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDefaultValue_Fail()
        {
            var column = this.RandomOrDefault(item => item.DataType != typeof(string) && item.AutoIncrement == false);
            column.DefaultValue = RandomUtility.NextWord();
        }

        [TestMethod]
        public void SetDefaultValueNull()
        {
            var column = this.RandomOrDefault(item => item.DefaultValue != DBNull.Value);
            column.DefaultValue = null;
            Assert.AreEqual(DBNull.Value, column.DefaultValue);
        }

        [TestMethod]
        public void SetDefaultValueDBNull()
        {
            var column = this.RandomOrDefault(item => item.DefaultValue != DBNull.Value);
            column.DefaultValue = DBNull.Value;
            Assert.AreEqual(DBNull.Value, column.DefaultValue);
        }

        [TestMethod]
        public void SetDefaultValueToCremaTypeColumn()
        {
            var column = this.RandomOrDefault(item => item.CremaType != null);
            var columnType = column.CremaType;
            var defaultValue = columnType.GetRandomValue();
            column.DefaultValue = defaultValue;
            Assert.AreEqual(defaultValue, column.DefaultValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDefaultValueToCremaTypeColumn_Fail()
        {
            var column = this.RandomOrDefault(item => item.CremaType != null);
            var columnType = dataSet.AddRandomType();
            column.DefaultValue = RandomUtility.NextString();
        }

        [TestMethod]
        public void SetDefaultValueDBNullToCremaTypeColumn()
        {
            var column = this.RandomOrDefault(item => item.CremaType != null);
            column.DefaultValue = DBNull.Value;
            Assert.AreEqual(DBNull.Value, column.DefaultValue);
        }

        [TestMethod]
        public void SetDefaultValueNullToCremaTypeColumn()
        {
            var column = this.RandomOrDefault(item => item.CremaType != null);
            var columnType = dataSet.AddRandomType();
            column.DefaultValue = null;
            Assert.AreEqual(DBNull.Value, column.DefaultValue);
        }

        [TestMethod]
        public void SetAutoIncrementTrue()
        {
            var column = this.RandomOrDefault(item => CremaDataTypeUtility.CanUseAutoIncrement(item.DataType) && item.AutoIncrement == false && item.DefaultValue == DBNull.Value);
            column.AutoIncrement = true;
            Assert.AreEqual(true, column.AutoIncrement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetAutoIncrementTrueToDefaultValueColumn_Fail()
        {
            var column = this.RandomOrDefault(item => CremaDataTypeUtility.CanUseAutoIncrement(item.DataType) && item.AutoIncrement == false && item.DefaultValue != DBNull.Value);
            column.AutoIncrement = true;
        }

        [TestMethod]
        public void SetAutoIncrementFalse()
        {
            var column = this.RandomOrDefault(item => item.AutoIncrement == true);
            column.AutoIncrement = false;
            Assert.AreEqual(false, column.AutoIncrement);
        }

        [TestMethod]
        public void SetAutoIncrementToInvalidIncrementType()
        {
            var column = this.RandomOrDefault(item => CremaDataTypeUtility.CanUseAutoIncrement(item.DataType) == false && item.CremaType == null && item.DefaultValue == DBNull.Value);
            column.AutoIncrement = true;
            Assert.AreEqual(typeof(int), column.DataType);
            Assert.AreEqual(true, column.AutoIncrement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetAutoIncrementToCremaTypeColumn_Fail()
        {
            var column = this.RandomOrDefault(item => item.CremaType != null);
            if (column == null)
                return;
            column.AutoIncrement = true;
        }

        [TestMethod]
        public void SetIndex()
        {
            var column = this.RandomOrDefault();
            var index = RandomUtility.Next(column.Table.Columns.Count);
            column.Index = index;
            Assert.AreEqual(index, column.Index);
            Assert.AreEqual(column, column.Table.Columns[index]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetIndexUnderValue_Fail()
        {
            var column = this.RandomOrDefault();
            column.Index = -1;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetIndexOverValue_Fail()
        {
            var column = this.RandomOrDefault();
            column.Index = column.Table.Columns.Count;
        }

        private CremaDataColumn RandomOrDefault()
        {
            return this.RandomOrDefault(item => true);
        }

        private CremaDataColumn RandomOrDefault(Func<CremaDataColumn, bool> predicate)
        {
            for (var i = 0; i < 5; i++)
            {
                var column = this.columns.RandomOrDefault(predicate);
                if (column != null)
                    return column;
                this.Reset();
            }
            return null;
        }

    }
}
