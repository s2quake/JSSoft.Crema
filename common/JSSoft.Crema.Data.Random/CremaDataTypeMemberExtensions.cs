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

using JSSoft.Library.Random;

namespace JSSoft.Crema.Data.Random
{
    public static class CremaDataTypeMemberExtensions
    {
        public static void InitializeRandom(this CremaDataTypeMember typeMember)
        {
            var dataType = typeMember.Type;
            if (dataType.IsFlag == true)
                typeMember.Value = RandomUtility.NextBit();
            else if (RandomUtility.Within(95) == true)
                typeMember.Value = (long)dataType.Members.Count;
            else
                typeMember.Value = RandomUtility.NextLong(long.MaxValue);

            if (RandomUtility.Within(50) == true)
                typeMember.Comment = RandomUtility.NextString();
        }

        public static void SetRandomValue(this CremaDataTypeMember typeMember)
        {
            var dataType = typeMember.Type;

            if (RandomUtility.Within(75) == true)
            {
                var newName = RandomUtility.NextIdentifier();
                while (newName == typeMember.Name)
                {
                    newName = RandomUtility.NextIdentifier();
                }
                typeMember.Name = newName;
            }
            else if (RandomUtility.Within(75) == true)
            {
                if (dataType.IsFlag == true)
                {
                    typeMember.Value = RandomUtility.NextBit();
                }
                else
                {
                    typeMember.Value = RandomUtility.NextLong(long.MaxValue);
                }
            }
            else if (RandomUtility.Within(50) == true)
            {
                typeMember.Comment = RandomUtility.NextString();
            }
            else
            {
                typeMember.Comment = string.Empty;
            }
        }

        public static bool SetRandomValue(this CremaDataTypeMember typeMember, int tryCount)
        {
            var count = 0;
            for (var i = 0; i < tryCount; i++)
            {
                try
                {
                    SetRandomValue(typeMember);
                    count++;
                }
                catch
                {

                }
            }
            return count > 0;
        }
    }
}