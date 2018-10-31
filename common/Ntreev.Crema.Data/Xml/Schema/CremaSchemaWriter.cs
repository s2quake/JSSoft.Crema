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

using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Ntreev.Crema.Data.Xml.Schema
{
    public sealed class CremaSchemaWriter
    {
        private CremaDataSet dataSet;
        private readonly CremaDataTable dataTable;
        private readonly CremaDataType dataType;
        private readonly ItemName itemName;

        private static readonly XmlWriterSettings settings = new XmlWriterSettings()
        {
            Encoding = Encoding.UTF8,
            Indent = true,
        };

        public CremaSchemaWriter(CremaDataSet dataSet)
        {
            this.dataSet = dataSet ?? throw new ArgumentNullException(nameof(dataSet));
        }

        public CremaSchemaWriter(CremaDataTable dataTable)
            : this(dataTable, null)
        {

        }

        public CremaSchemaWriter(CremaDataTable dataTable, ItemName itemName)
        {
            Validate();
            this.itemName = itemName;
            this.dataTable = dataTable;

            void Validate()
            {
                if (dataTable == null)
                    throw new ArgumentNullException(nameof(dataTable));
                if (dataTable.TableName == string.Empty)
                    throw new InvalidOperationException("테이블 이름이 설정되지 않았습니다.");
                if (itemName == null && dataTable.TemplateNamespace != string.Empty)
                    throw new CremaDataException("상속된 테이블은 독립적으로 스키마를 작성할 수 없습니다.");
            }
        }

        public CremaSchemaWriter(CremaDataType dataType)
        {
            this.dataType = dataType ?? throw new ArgumentNullException("dataType");
        }

        public void Write(string filename)
        {
            using (var writer = XmlWriter.Create(filename, settings))
            {
                this.Write(writer);
            }
        }

        public void Write(Stream stream)
        {
            using (var writer = XmlWriter.Create(stream, settings))
            {
                this.Write(writer);
            }
        }

        public void Write(TextWriter writer)
        {
            using (var xmlWriter = XmlWriter.Create(writer, settings))
            {
                this.Write(xmlWriter);
            }
        }

        public void Write(XmlWriter writer)
        {
            var schema = new XmlSchema();

            if (this.dataSet != null)
            {
                this.WriteSchemaAttribute(schema, this.dataSet.Namespace);
                this.WriteGuidType(schema);
                this.WriteDataTypes(schema, dataSet.Types.OrderBy(item => item.Name));
                this.WriteTables(schema, dataSet.DataSetName, dataSet.Tables);
            }
            else if (this.dataTable != null)
            {
                var contentName = this.dataTable.DataSet != null ? this.dataTable.DataSet.DataSetName : CremaDataSet.DefaultDataSetName;
                var tableNamespace = this.itemName == null ? this.dataTable.Namespace : CremaSchema.TableNamespace + this.itemName.CategoryPath + this.itemName.Name;
                this.WriteSchemaAttribute(schema, tableNamespace);
                this.WriteGuidType(schema);

                var tables = this.IsRecursive == true ? EnumerableUtility.FamilyTree(dataTable, item => item.Childs) : Enumerable.Repeat(dataTable, 1);

                if (this.dataTable.DataSet != null)
                {
                    var columns = tables.SelectMany(item => item.Columns).Distinct();
                    var query = from item in this.dataTable.DataSet.Types
                                join column in columns on item.Path equals column.DataTypeName
                                select item;

                    var index = 0;
                    foreach (var item in query.Distinct())
                    {
                        var typeSchema = new XmlSchema();
                        WriteSchemaAttribute(typeSchema, item.Namespace);
                        WriteDataType(typeSchema, item);

                        var import = new XmlSchemaImport()
                        {
                            Schema = typeSchema,
                            Namespace = item.Namespace,
                            SchemaLocation = UriUtility.MakeRelative(tableNamespace, item.Namespace) + CremaSchema.SchemaExtension
                        };
                        schema.Includes.Add(import);
                        index++;
                    }
                }

                this.WriteTables(schema, contentName, tables);
            }
            else if (this.dataType != null)
            {
                this.WriteSchemaAttribute(schema, this.dataType.Namespace);
                this.WriteDataTypes(schema, new CremaDataType[] { this.dataType });
            }

            schema.Write(writer);
        }

        public bool IsRecursive { get; internal set; }

        private void WriteSchemaAttribute(XmlSchema schema, string targetNamespace)
        {
            schema.TargetNamespace = targetNamespace;
            schema.Namespaces.Add(string.Empty, targetNamespace);
            schema.Namespaces.Add(CremaSchema.TableTypePrefix, targetNamespace);
            schema.Version = CremaSchema.VersionValue;
            schema.ElementFormDefault = XmlSchemaForm.Qualified;
        }

        private void WriteTables(XmlSchema schema, string contentName, IEnumerable<CremaDataTable> tables)
        {
            var element = new XmlSchemaElement()
            {
                Name = contentName
            };
            var complexType = new XmlSchemaComplexType();
            {
                var attribute = new XmlSchemaAttribute()
                {
                    Name = CremaSchema.Version,
                    Use = XmlSchemaUse.Required,
                    SchemaTypeName = GetSystemQualifiedName(typeof(string))
                };
                complexType.Attributes.Add(attribute);
            }

            var sequence = new XmlSchemaSequence();

            foreach (var item in tables)
            {
                this.WriteHeaderAttribute(schema, complexType, item);
            }

            foreach (var item in tables)
            {
                this.WriteElement(schema, sequence, item);
                this.WriteDataTable(schema, item);

                this.WriteKeys(schema, element, item);
                this.WriteUniques(schema, element, item);
            }

            complexType.Particle = sequence;
            element.SchemaType = complexType;
            schema.Items.Add(element);
            CompileSchema(schema);
        }

        private void CompileSchema(XmlSchema schema)
        {
            var schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += ValidationCallbackOne;
            schemaSet.Add(schema);
            schemaSet.Compile();
        }

        private XmlQualifiedName GetXmlQualifiedName(XmlSchema schema, CremaDataColumn dataColumn)
        {
            if (dataColumn.CremaType != null)
            {
                var cremaType = dataColumn.CremaType;
                foreach (var item in schema.Includes)
                {
                    if (item is XmlSchemaImport == true)
                    {
                        var n = (item as XmlSchemaImport).Namespace;
                        if (cremaType.Namespace == n)
                            return new XmlQualifiedName(cremaType.TypeName, cremaType.Namespace);
                    }
                }

                if (this.dataSet == null)
                {
                    using (var sr = new StringReader(cremaType.GetXmlSchema()))
                    {
                        var typeSchema = XmlSchema.Read(sr, ValidationCallbackOne);
                        var schemaLocation = UriUtility.MakeRelative(dataColumn.Namespace, cremaType.Namespace) + CremaSchema.SchemaExtension;
                        schema.Includes.Add(new XmlSchemaImport()
                        {
                            Schema = typeSchema,
                            SchemaLocation = schemaLocation,
                            Namespace = cremaType.Namespace,
                        });
                    }

                    return new XmlQualifiedName(cremaType.TypeName, cremaType.Namespace);
                }

                return new XmlQualifiedName(cremaType.TypeName, schema.TargetNamespace);
            }
            else if (string.IsNullOrEmpty(dataColumn.Validation) == false)
            {
                return new XmlQualifiedName(CremaSchema.GenerateColumnTypeName(dataColumn.ColumnName), schema.TargetNamespace);
            }
            else if (dataColumn.DataType == typeof(Guid))
            {
                return new XmlQualifiedName(typeof(Guid).GetSchemaTypeName(), schema.TargetNamespace);
            }
            else
            {
                return GetSystemQualifiedName(dataColumn.DataType);
            }
        }

        private void WriteHeaderAttribute(XmlSchema schema, XmlSchemaComplexType rootType, CremaDataTable dataTable)
        {
            var baseNamespace = dataTable.DataSet != null ? dataTable.DataSet.Namespace : CremaSchema.BaseNamespace;

            rootType.Attributes.Add(new XmlSchemaAttribute()
            {
                Name = dataTable.Name + CremaSchema.CreatedDateTimeExtension,
                SchemaTypeName = GetSystemQualifiedName(typeof(DateTime)),
                Use = XmlSchemaUse.Optional
            });

            rootType.Attributes.Add(new XmlSchemaAttribute()
            {
                Name = dataTable.Name + CremaSchema.CreatorExtension,
                SchemaTypeName = GetSystemQualifiedName(typeof(string)),
                Use = XmlSchemaUse.Optional
            });

            rootType.Attributes.Add(new XmlSchemaAttribute()
            {
                Name = dataTable.Name + CremaSchema.ModifiedDateTimeExtension,
                SchemaTypeName = GetSystemQualifiedName(typeof(DateTime)),
                Use = XmlSchemaUse.Optional
            });

            rootType.Attributes.Add(new XmlSchemaAttribute()
            {
                Name = dataTable.Name + CremaSchema.ModifierExtension,
                SchemaTypeName = GetSystemQualifiedName(typeof(string)),
                Use = XmlSchemaUse.Optional
            });

            rootType.Attributes.Add(new XmlSchemaAttribute()
            {
                Name = dataTable.Name + CremaSchema.CountExtension,
                SchemaTypeName = GetSystemQualifiedName(typeof(int)),
                Use = XmlSchemaUse.Optional
            });

            rootType.Attributes.Add(new XmlSchemaAttribute()
            {
                Name = dataTable.Name + CremaSchema.IDExtension,
                SchemaTypeName = GetSystemQualifiedName(typeof(string)),
                Use = XmlSchemaUse.Optional
            });
        }

        private void WriteElement(XmlSchema schema, XmlSchemaSequence sequence, CremaDataTable dataTable)
        {
            var element = new XmlSchemaElement()
            {
                Name = dataTable.GetXmlName(schema.TargetNamespace),
                MinOccursString = "0",
                MaxOccursString = "unbounded",
                SchemaTypeName = new XmlQualifiedName(dataTable.TableTypeName, schema.TargetNamespace)
            };

            element.Namespaces.Add(string.Empty, schema.TargetNamespace);
            sequence.Items.Add(element);
        }

        private void WriteDataTable(XmlSchema schema, CremaDataTable dataTable)
        {
            var tableNamespace = schema.TargetNamespace;
            var complexType = new XmlSchemaComplexType()
            {
                Name = dataTable.TableTypeName
            };
            complexType.WriteAppInfo(CremaSchema.TableInfo, CremaSchema.Creator, dataTable.CreationInfo.ID, tableNamespace);
            complexType.WriteAppInfo(CremaSchema.TableInfo, CremaSchema.CreatedDateTime, dataTable.CreationInfo.DateTime, tableNamespace);
            complexType.WriteAppInfo(CremaSchema.TableInfo, CremaSchema.Modifier, dataTable.ModificationInfo.ID, tableNamespace);
            complexType.WriteAppInfo(CremaSchema.TableInfo, CremaSchema.ModifiedDateTime, dataTable.ModificationInfo.DateTime, tableNamespace);
            complexType.WriteAppInfo(CremaSchema.TableInfo, CremaSchema.Tags, dataTable.Tags, tableNamespace);
            complexType.WriteAppInfo(CremaSchema.TableInfo, CremaSchema.ID, dataTable.TableID, tableNamespace);
            complexType.WriteAppInfo(CremaSchema.TableInfo, CremaSchema.CategoryPath, dataTable.CategoryPath, tableNamespace, PathUtility.Separator);
            if (dataTable.TemplateNamespace != string.Empty)
                complexType.WriteAppInfo(CremaSchema.TableInfo, CremaSchema.TemplateNamespace, dataTable.TemplateNamespace, tableNamespace);

            complexType.WriteDescription(dataTable.Comment);

            if (this.itemName != null || dataTable.TemplateNamespace == string.Empty || dataTable.TemplatedParent == null)
            {
                var sequence = new XmlSchemaSequence();

                foreach (var item in dataTable.Attributes)
                {
                    if (item.IsVisible == false)
                        continue;
                    this.WriteAttribute(schema, complexType, item);
                }

                foreach (var item in dataTable.Columns)
                {
                    this.WriteDataColumn(schema, item);
                    this.WriteElement(schema, sequence, item);
                }

                if (dataTable.ColumnRelation != null)
                {
                    var attribute = new XmlSchemaAttribute()
                    {
                        Name = CremaSchema.RelationID,
                        SchemaTypeName = GetSystemQualifiedName(typeof(string))
                    };
                    complexType.Attributes.Add(attribute);
                }

                if (dataTable.ParentRelation != null)
                {
                    var attribute = new XmlSchemaAttribute()
                    {
                        Name = CremaSchema.ParentID,
                        SchemaTypeName = GetSystemQualifiedName(typeof(string))
                    };
                    complexType.Attributes.Add(attribute);
                }

                complexType.Particle = sequence;
            }
            else
            {
                var content = new XmlSchemaComplexContent();
                var extension = new XmlSchemaComplexContentExtension()
                {
                    BaseTypeName = new XmlQualifiedName(dataTable.TemplateTypeName, tableNamespace)
                };
                content.Content = extension;
                complexType.ContentModel = content;
            }

            schema.Items.Add(complexType);
        }

        private void WriteElement(XmlSchema schema, XmlSchemaSequence sequence, CremaDataColumn dataColumn)
        {
            var element = new XmlSchemaElement()
            {
                Name = dataColumn.ColumnName,
                SchemaTypeName = this.GetXmlQualifiedName(schema, dataColumn)
            };
            var defaultValue = CremaXmlConvert.ToString(dataColumn.DefaultValue, dataColumn.DataType);
            if (string.IsNullOrEmpty(defaultValue) == false)
                element.DefaultValue = defaultValue;
            if (dataColumn.AllowDBNull == true && dataColumn.IsKey == false)
                element.MinOccursString = "0";

            element.WriteAppInfo(CremaSchema.ColumnInfo, CremaSchema.Creator, dataColumn.CreationInfo.ID, schema.TargetNamespace);
            element.WriteAppInfo(CremaSchema.ColumnInfo, CremaSchema.CreatedDateTime, dataColumn.CreationInfo.DateTime, schema.TargetNamespace);
            element.WriteAppInfo(CremaSchema.ColumnInfo, CremaSchema.Modifier, dataColumn.ModificationInfo.ID, schema.TargetNamespace);
            element.WriteAppInfo(CremaSchema.ColumnInfo, CremaSchema.ModifiedDateTime, dataColumn.ModificationInfo.DateTime, schema.TargetNamespace);
            element.WriteAppInfo(CremaSchema.ColumnInfo, CremaSchema.AutoIncrement, dataColumn.AutoIncrement, schema.TargetNamespace);
            element.WriteAppInfo(CremaSchema.ColumnInfo, CremaSchema.ID, dataColumn.ColumnID, schema.TargetNamespace);
            element.WriteAppInfo(CremaSchema.ColumnInfo, CremaSchema.Tags, dataColumn.Tags, schema.TargetNamespace);
            element.WriteAppInfo(CremaSchema.ColumnInfo, CremaSchema.ReadOnly, dataColumn.ReadOnly, schema.TargetNamespace);

            element.WriteDescription(dataColumn.Comment);
            sequence.Items.Add(element);
        }

        private void WriteAttribute(XmlSchema schema, XmlSchemaComplexType complexType, CremaAttribute attribute)
        {
            var schemaAttribute = new XmlSchemaAttribute()
            {
                Name = attribute.AttributeName,
                SchemaTypeName = GetSystemQualifiedName(attribute.DataType, schema.TargetNamespace)
            };
            if (attribute.AllowDBNull == false)
            {
                if (attribute.DefaultValue == DBNull.Value)
                    schemaAttribute.Use = XmlSchemaUse.Required;
                else
                    schemaAttribute.Use = XmlSchemaUse.Optional;
            }

            var defaultValue = attribute.DefaultValue;
            if (defaultValue != DBNull.Value)
                schemaAttribute.DefaultValue = CremaXmlConvert.ToString(defaultValue, attribute.DataType);

            schemaAttribute.WriteAppInfo(CremaSchema.AttributeInfo, CremaSchema.AutoIncrement, attribute.AutoIncrement, schema.TargetNamespace);
            schemaAttribute.WriteAppInfo(CremaSchema.AttributeInfo, CremaSchema.Comment, attribute.Comment, schema.TargetNamespace);

            complexType.Attributes.Add(schemaAttribute);
        }

        private void WriteDataColumn(XmlSchema schema, CremaDataColumn dataColumn)
        {
            if (dataColumn.Validation != string.Empty)
            {
                var simpleType = new XmlSchemaSimpleType()
                {
                    Name = CremaSchema.GenerateColumnTypeName(dataColumn.ColumnName),
                };
                {
                    var simpleTypeRestriction = new XmlSchemaSimpleTypeRestriction()
                    {
                        BaseTypeName = GetSystemQualifiedName(dataColumn.DataType)
                    };
                    {
                        var facet = new XmlSchemaPatternFacet()
                        {
                            Value = dataColumn.Validation
                        };
                        simpleTypeRestriction.Facets.Add(facet);
                    }
                    simpleType.Content = simpleTypeRestriction;
                }

                schema.Items.Add(simpleType);
            }
        }

        private void WriteKeys(XmlSchema schema, XmlSchemaElement element, CremaDataTable dataTable)
        {
            if (dataTable.PrimaryKey.Any() == false)
                return;

            var key = new XmlSchemaKey()
            {
                Name = dataTable.KeyTypeName,
                Selector = new XmlSchemaXPath()
            };

            key.Selector.XPath = dataTable.GetSelectorXPath(CremaSchema.TableTypePrefix, schema.TargetNamespace);

            foreach (var item in dataTable.PrimaryKey)
            {
                var field = new XmlSchemaXPath()
                {
                    XPath = CremaSchema.TableTypePrefix + ":" + item.ColumnName
                };
                key.Fields.Add(field);
            }

            element.Constraints.Add(key);
        }

        private void WriteUniques(XmlSchema schema, XmlSchemaElement element, CremaDataTable dataTable)
        {
            foreach (var item in dataTable.Columns)
            {
                if (item.Unique == false)
                    continue;

                if (item.IsKey == true && dataTable.PrimaryKey.Length == 1)
                    continue;

                var unique = new XmlSchemaUnique()
                {
                    Name = dataTable.Name + "." + item.ColumnName + "." + "Unique",
                    Selector = new XmlSchemaXPath()
                };
                unique.Selector.XPath = dataTable.GetSelectorXPath(CremaSchema.TableTypePrefix, schema.TargetNamespace);

                var field = new XmlSchemaXPath()
                {
                    XPath = CremaSchema.TableTypePrefix + ":" + item.ColumnName
                };
                unique.Fields.Add(field);
                element.Constraints.Add(unique);
            }
        }

        private void WriteTypeMembers(XmlSchema schema, XmlSchemaSimpleType simpleType, IEnumerable<CremaDataTypeMember> typeMembers)
        {
            var restriction = new XmlSchemaSimpleTypeRestriction()
            {
                BaseTypeName = GetSystemQualifiedName(typeof(string))
            };
            foreach (var item in typeMembers.OrderBy(i => i.Index))
            {
                if (item.Name == string.Empty)
                    continue;
                var facet = new XmlSchemaEnumerationFacet()
                {
                    Value = item.Name
                };
                facet.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.Value, item.Value, schema.TargetNamespace);
                facet.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.Creator, item.CreationInfo.ID, schema.TargetNamespace);
                facet.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.CreatedDateTime, item.CreationInfo.DateTime, schema.TargetNamespace);
                facet.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.Modifier, item.ModificationInfo.ID, schema.TargetNamespace);
                facet.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.ModifiedDateTime, item.ModificationInfo.DateTime, schema.TargetNamespace);
                facet.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.ID, item.MemberID, schema.TargetNamespace);
                facet.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.Tags, item.Tags, schema.TargetNamespace);
                facet.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.Enable, item.IsEnabled, schema.TargetNamespace);

                facet.WriteDescription(item.Comment);

                restriction.Facets.Add(facet);
            }

            simpleType.Content = restriction;
        }

        private void WriteDataType(XmlSchema schema, CremaDataType dataType)
        {
            var simpleType = new XmlSchemaSimpleType();

            if (dataType.IsFlag == true)
            {
                var flagType = new XmlSchemaSimpleType()
                {
                    Name = dataType.FlagTypeName
                };
                this.WriteTypeMembers(schema, flagType, dataType.Members);

                var simpleTypeList = new XmlSchemaSimpleTypeList()
                {
                    ItemTypeName = new XmlQualifiedName(dataType.FlagTypeName, schema.TargetNamespace)
                };
                simpleType.Content = simpleTypeList;

                schema.Items.Add(flagType);
                schema.Items.Add(simpleType);
            }
            else
            {
                this.WriteTypeMembers(schema, simpleType, dataType.Members);
                schema.Items.Add(simpleType);
            }

            simpleType.Name = dataType.TypeName;

            simpleType.WriteDescription(dataType.Comment);
            simpleType.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.Creator, dataType.CreationInfo.ID, schema.TargetNamespace);
            simpleType.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.CreatedDateTime, dataType.CreationInfo.DateTime, schema.TargetNamespace);
            simpleType.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.Modifier, dataType.ModificationInfo.ID, schema.TargetNamespace);
            simpleType.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.ModifiedDateTime, dataType.ModificationInfo.DateTime, schema.TargetNamespace);
            simpleType.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.ID, dataType.TypeID, schema.TargetNamespace);
            simpleType.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.Tags, dataType.Tags, schema.TargetNamespace);
            simpleType.WriteAppInfo(CremaSchema.TypeInfo, CremaSchema.CategoryPath, dataType.CategoryPath, schema.TargetNamespace, PathUtility.Separator);
        }

        private void WriteGuidType(XmlSchema schema)
        {
            var pattern = new XmlSchemaPatternFacet()
            {
                Value = @"([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}",
            };

            var restriction = new XmlSchemaSimpleTypeRestriction()
            {
                BaseTypeName = GetSystemQualifiedName(typeof(string)),
            };
            restriction.Facets.Add(pattern);

            var simpleType = new XmlSchemaSimpleType()
            {
                Name = typeof(Guid).GetSchemaTypeName(),
                Content = restriction,
            };

            schema.Items.Add(simpleType);
        }

        private void WriteDataTypes(XmlSchema schema, IEnumerable<CremaDataType> types)
        {
            foreach (var item in types.OrderBy(i => i.TypeName))
            {
                this.WriteDataType(schema, item);
            }
        }

        private void ValidationCallbackOne(object sender, ValidationEventArgs args)
        {
            Trace.WriteLine(args.Message);
        }

        internal static XmlQualifiedName GetSystemQualifiedName(Type type)
        {
            return new XmlQualifiedName(type.GetSchemaTypeName(), XmlSchema.Namespace);
        }

        internal static XmlQualifiedName GetSystemQualifiedName(Type type, string targetNamespace)
        {
            if (type == typeof(Guid))
                return new XmlQualifiedName(type.GetSchemaTypeName(), targetNamespace);
            return GetSystemQualifiedName(type);
        }
    }
}
