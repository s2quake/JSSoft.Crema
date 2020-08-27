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

#pragma warning disable 0618

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JSSoft.Crema.Commands.Consoles.Serializations
{
    static class JsonSchemaUtility
    {
        private static readonly Dictionary<Type, JSchema> schemas = new Dictionary<Type, JSchema>();

        static JsonSchemaUtility()
        {

        }

        public static JSchema CreateSchema(Type type)
        {
            var generator = new JSchemaGenerator();
            var schema = generator.Generate(type);
            return schema;
        }

        public static JSchema CreateSchema(string[] enums)
        {
            var schema = CreateSchema(typeof(string));
            schema.SetEnums(enums);
            return schema;
        }

        public static void SetEnums(this JSchema schema, string[] items)
        {
            schema.Enum.Clear();
            foreach (var item in items)
            {
                schema.Enum.Add(JValue.CreateString(item));
            }
        }

        public static void SetEnums(this JSchema schema, string propertyName, string[] items)
        {
            var prop = schema.Properties[propertyName];
            prop.SetEnums(items);
        }

        internal static JSchema GetSchema(Type type)
        {
            if (schemas.ContainsKey(type) == false)
            {
                var generator = new JSchemaGenerator();
                var schema = generator.Generate(type);
                WriteDescription(schema, type);
                schemas.Add(type, schema);
            }

            return schemas[type];
        }

        private static void WriteDescription(JSchema schema, Type type)
        {
            if (schema.Properties != null)
            {
                foreach (var item in schema.Properties)
                {
                    var prop = GetPropertyDescriptor(item.Key);
                    if (prop != null && prop.Description != string.Empty)
                        item.Value.Description = prop.Description;

                    WriteDescription(item.Value, prop.PropertyType);
                }
            }

            PropertyDescriptor GetPropertyDescriptor(string propertyName)
            {
                foreach (PropertyDescriptor item in TypeDescriptor.GetProperties(type))
                {
                    if (!(item.Attributes[typeof(JsonPropertyAttribute)] is JsonPropertyAttribute attr))
                        continue;
                    var name = attr.PropertyName ?? item.Name;
                    if (name == propertyName)
                        return item;
                }
                return null;
            }
        }
    }
}
