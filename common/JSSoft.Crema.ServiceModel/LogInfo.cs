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

using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace JSSoft.Crema.ServiceModel
{
    [DataContract(Namespace = SchemaUtility.Namespace)]
    [Serializable]
    public struct LogInfo
    {
        [DataMember]
        public string Revision { get; set; }

        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public DateTime DateTime { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public LogPropertyInfo[] Properties { get; set; }

        public IDictionary<string, object> ToDictionary()
        {
            var props = new Dictionary<string, object>
            {
                { nameof(this.Revision), this.Revision },
                { nameof(this.UserID), this.UserID },
                { nameof(this.DateTime), $"{this.DateTime}" },
                { nameof(this.Comment), this.Comment },
            };
            return props;
        }

        internal bool ContainsProperty(string key)
        {
            if (this.Properties == null)
                return false;

            var query = from item in this.Properties
                        where item.Key == key
                        select item;
            return query.Any();
        }

        internal string GetPropertyString(string key)
        {
            if (this.Properties == null)
                return null;

            var query = from item in this.Properties
                        where item.Key == key
                        select item;
            if (query.Any() == true)
                return query.First().Value;
            return null;
        }

        public static readonly LogInfo Empty = new LogInfo();
    }
}
