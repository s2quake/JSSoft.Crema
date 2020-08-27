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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JSSoft.Crema.Data.Test
{
    [TestClass]
    public class CremaDataColumn_ChangeUInt8ToOtherTypeTest : CremaDataColumn_ChangeTypeTestBase
    {
        public CremaDataColumn_ChangeUInt8ToOtherTypeTest()
            : base(typeof(byte))
        {

        }

        [TestMethod]
        public void DBNullUInt8ToOther()
        {
            this.AddRows(DBNull.Value);
            foreach (var item in CremaDataTypeUtility.GetBaseTypes().Where(item => item != this.column.DataType))
            {
                try
                {
                    column.DataType = typeof(byte);
                }
                catch (FormatException)
                {

                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void UInt8ToBoolean_Fail()
        {
            this.AddRows((byte)0);
            column.DataType = typeof(bool);
        }

        [TestMethod]
        public void UInt8ToSingle()
        {
            this.AddRows(byte.MinValue, byte.MaxValue);
            column.DataType = typeof(float);
        }

        [TestMethod]
        public void UInt8ToDouble()
        {
            this.AddRows(byte.MinValue, byte.MaxValue);
            column.DataType = typeof(double);
        }

        [TestMethod]
        public void UInt8ToInt8()
        {
            this.AddRows((byte)0, (byte)sbyte.MaxValue);
            column.DataType = typeof(sbyte);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void UInt8ToInt8_Fail()
        {
            this.AddRows(byte.MaxValue);
            column.DataType = typeof(sbyte);
        }

        [TestMethod]
        public void UInt8ToInt16()
        {
            this.AddRows(byte.MinValue, byte.MaxValue);
            column.DataType = typeof(short);
        }

        [TestMethod]
        public void UInt8ToUInt16()
        {
            this.AddRows((byte)0, byte.MaxValue);
            column.DataType = typeof(ushort);
        }

        [TestMethod]
        public void UInt8ToInt32()
        {
            this.AddRows(byte.MinValue, byte.MaxValue);
            column.DataType = typeof(int);
        }

        [TestMethod]
        public void UInt8ToUInt32()
        {
            this.AddRows((byte)0, byte.MaxValue);
            column.DataType = typeof(uint);
        }

        [TestMethod]
        public void UInt8ToInt64()
        {
            this.AddRows(byte.MinValue, byte.MaxValue);
            column.DataType = typeof(long);
        }

        [TestMethod]
        public void UInt8ToUInt64()
        {
            this.AddRows((byte)0, byte.MaxValue);
            column.DataType = typeof(ulong);
        }

        [TestMethod]
        public void UInt8ToDateTime()
        {
            this.AddRows((byte)0, byte.MaxValue);
            column.DataType = typeof(DateTime);
        }

        [TestMethod]
        public void UInt8ToTimeSpan()
        {
            this.AddRows(byte.MinValue, byte.MaxValue);
            column.DataType = typeof(TimeSpan);
        }

        [TestMethod]
        public void UInt8ToString()
        {
            this.AddRows(byte.MinValue, byte.MaxValue);
            column.DataType = typeof(string);
        }
    }
}
