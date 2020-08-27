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

using JSSoft.Library.Random;
using System;

namespace JSSoft.Crema.Data.Random
{
    public static class CremaDataAttributeExtensions
    {
        public static object GetRandomValue(this CremaAttribute attribute)
        {
            return RandomUtility.Next(attribute.DataType);
        }

        public static void InitializeRandom(this CremaAttribute attribute)
        {
            attribute.DataType = CremaDataTypeUtility.GetBaseTypes().Random();

            if (RandomUtility.Within(25) == true)
            {
                attribute.Comment = RandomUtility.NextString();
            }

            if (RandomUtility.Within(25) == true)
            {
                attribute.DefaultValue = RandomUtility.Next(attribute.DataType);
            }

            if (RandomUtility.Within(25) == true && CremaDataTypeUtility.CanUseAutoIncrement(attribute.DataType) == true && attribute.DefaultValue == DBNull.Value)
            {
                attribute.AutoIncrement = RandomUtility.NextBoolean();
            }
        }
    }
}
