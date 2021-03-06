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

using JSSoft.Crema.Commands.Consoles.Properties;
using JSSoft.Crema.Commands.Consoles.Serializations;
using JSSoft.Crema.Commands.Consoles.TableTemplate;
using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library;
using JSSoft.Library.Commands;
using JSSoft.Library.IO;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
    class TableCommand : ConsoleCommandMethodBase
    {
        [ImportingConstructor]
        public TableCommand(ICremaHost cremaHost)
        {
            this.CremaHost = cremaHost;
        }

        public override string[] GetCompletions(CommandMethodDescriptor methodDescriptor, CommandMemberDescriptor memberDescriptor, string find)
        {
            return base.GetCompletions(methodDescriptor, memberDescriptor, find);
        }

        [CommandMethod]
        public async Task RenameAsync([CommandCompletion(nameof(GetTableNamesAsync))] string tableName, string newTableName)
        {
            var table = await this.GetTableAsync(tableName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await table.RenameAsync(authentication, newTableName);
        }

        [CommandMethod]
        public async Task MoveAsync([CommandCompletion(nameof(GetTableNamesAsync))] string tableName, [CommandCompletion(nameof(GetCategoryPathsAsync))] string categoryPath)
        {
            var table = await this.GetTableAsync(tableName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await table.MoveAsync(authentication, categoryPath);
        }

        [CommandMethod]
        public async Task DeleteAsync([CommandCompletion(nameof(GetTableNamesAsync))] string tableName)
        {
            var table = await this.GetTableAsync(tableName);
            var authentication = this.CommandContext.GetAuthentication(this);
            if (this.CommandContext.ConfirmToDelete() == true)
            {
                await table.DeleteAsync(authentication);
            }
        }

        [CommandMethod]
        public async Task SetTagsAsync([CommandCompletion(nameof(GetTableNamesAsync))] string tableName, string tags)
        {
            var table = await this.GetTableAsync(tableName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var template = table.Template;
            await template.BeginEditAsync(authentication);
            try
            {
                await template.SetTagsAsync(authentication, (TagInfo)tags);
                await template.EndEditAsync(authentication);
            }
            catch
            {
                await template.CancelEditAsync(authentication);
                throw;
            }
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(CategoryPath), nameof(CopyContent))]
        public async Task CopyAsync([CommandCompletion(nameof(GetTableNamesAsync))] string tableName, string newTableName)
        {
            var table = await this.GetTableAsync(tableName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var categoryPath = this.CategoryPath ?? this.GetCurrentDirectory();
            await table.CopyAsync(authentication, newTableName, categoryPath, this.CopyContent);
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(CategoryPath), nameof(CopyContent))]
        public async Task InheritAsync([CommandCompletion(nameof(GetTableNamesAsync))] string tableName, string newTableName)
        {
            var table = await this.GetTableAsync(tableName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var categoryPath = this.CategoryPath ?? this.GetCurrentDirectory();
            await table.InheritAsync(authentication, newTableName, categoryPath, this.CopyContent);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task ViewAsync([CommandCompletion(nameof(GetPathsAsync))] string tableItemName, string revision = null)
        {
            var sb = new StringBuilder();
            var tableItem = await this.GetTableItemAsync(tableItemName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var dataSet = await tableItem.GetDataSetAsync(authentication, revision);
            var props = dataSet.ToDictionary(true, false);
            var format = FormatProperties.Format;
            sb.AppendLine(props, format);
            await this.Out.WriteAsync(sb.ToString());
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task LogAsync([CommandCompletion(nameof(GetPathsAsync))] string tableItemName, string revision = null)
        {
            var sb = new StringBuilder();
            var tableItem = await this.GetTableItemAsync(tableItemName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var logs = await tableItem.GetLogAsync(authentication, revision);
            var format = FormatProperties.Format;
            foreach (var item in logs)
            {
                var props = item.ToDictionary();
                sb.AppendLine(props, format);
                sb.AppendLine();
            }
            await this.Out.WriteAsync(sb.ToString());
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        [CommandMethodStaticProperty(typeof(TagsProperties))]
        public async Task ListAsync()
        {
            var sb = new StringBuilder();
            var tableNames = await this.GetTableNamesAsync((TagInfo)TagsProperties.Tags, FilterProperties.FilterExpression);
            foreach (var item in tableNames)
            {
                sb.AppendLine(item);
            }
            await this.Out.WriteAsync(sb.ToString());
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task InfoAsync([CommandCompletion(nameof(GetTableNamesAsync))] string tableName)
        {
            var sb = new StringBuilder();
            var table = await this.GetTableAsync(tableName);
            var tableInfo = await table.Dispatcher.InvokeAsync(() => table.TableInfo);
            var props = tableInfo.ToDictionary(true);
            var format = FormatProperties.Format;
            sb.AppendLine(props, format);
            await this.Out.WriteAsync(sb.ToString());
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task ColumnList([CommandCompletion(nameof(GetTableNamesAsync))] string tableName)
        {
            var sb = new StringBuilder();
            var table = await this.GetTableAsync(tableName);
            var tableInfo = await table.Dispatcher.InvokeAsync(() => table.TableInfo);
            var columnList = tableInfo.Columns.Where(item => StringUtility.GlobMany(item.Name, FilterProperties.FilterExpression))
                                              .Select(item => item).ToArray();

            foreach (var item in columnList)
            {
                sb.AppendLine($"{item}");
            }
            this.Out.Write(sb.ToString());
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        public async Task ColumnInfo([CommandCompletion(nameof(GetTableNamesAsync))] string tableName)
        {
            var sb = new StringBuilder();
            var table = await this.GetTableAsync(tableName);
            var tableInfo = await table.Dispatcher.InvokeAsync(() => table.TableInfo);
            var columns = new Dictionary<string, object>(tableInfo.Columns.Length);
            var format = FormatProperties.Format;
            var query = from item in tableInfo.Columns
                        where StringUtility.GlobMany(item.Name, FilterProperties.FilterExpression)
                        select item;
            foreach (var item in query)
            {
                columns.Add(item.Name, item.ToDictionary());
            }
            sb.AppendLine(columns, format);
            await this.Out.WriteAsync(sb.ToString());
        }

        [ConsoleModeOnly]
        [CommandMethod]
        [CommandMethodProperty(nameof(ParentPath))]
        public async Task CreateAsync()
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            var tableNames = this.GetTableNamesAsync();
            var template = await CreateTemplateAsync();

            var dataTypes = template.Dispatcher.Invoke(() => template.SelectableTypes);
            var tableName = template.Dispatcher.Invoke(() => template.TableName);
            var tableInfo = JsonTableInfo.Default;
            if (tableInfo.TableName == string.Empty)
                tableInfo.TableName = tableName;

            var schema = JsonSchemaUtility.GetSchema(typeof(JsonTableInfo));
            var itemsSchema = schema.Properties[nameof(JsonTableInfo.Columns)];
            var itemSchema = itemsSchema.Items.First();
            itemSchema.SetEnums(nameof(JsonTableInfo.JsonTableColumnInfo.DataType), dataTypes);
            itemSchema.SetEnums(nameof(JsonTableInfo.JsonTableColumnInfo.Tags), TagInfoUtility.Names);

            try
            {
                if (JsonEditorHost.TryEdit(ref tableInfo, schema) == false)
                    return;
                if (this.CommandContext.ReadYesOrNo($"do you want to create table '{tableInfo.TableName}'?") == false)
                    return;

                await SetDataAsync();
                template = null;
            }
            finally
            {
                if (template != null)
                {
                    await template.CancelEditAsync(authentication);
                }
            }

            async Task<ITableTemplate> CreateTemplateAsync()
            {
                if (this.ParentPath == string.Empty)
                {
                    var category = await this.GetCategoryAsync(this.GetCurrentDirectory());
                    return await category.NewTableAsync(authentication);
                }
                else if (NameValidator.VerifyCategoryPath(this.ParentPath) == true)
                {
                    var category = await this.GetCategoryAsync(this.ParentPath);
                    return await category.NewTableAsync(authentication);
                }
                else if (await this.GetTableAsync(this.ParentPath) is ITable table)
                {
                    return await table.NewTableAsync(authentication);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            async Task SetDataAsync()
            {
                await template.SetTableNameAsync(authentication, tableInfo.TableName);
                await template.SetTagsAsync(authentication, (TagInfo)tableInfo.Tags);
                await template.SetCommentAsync(authentication, tableInfo.Comment);
                foreach (var item in tableInfo.Columns)
                {
                    var column = await template.AddNewAsync(authentication);
                    await column.SetNameAsync(authentication, item.Name);
                    await column.SetIsKeyAsync(authentication, item.IsKey);
                    await column.SetCommentAsync(authentication, item.Comment);
                    await column.SetDataTypeAsync(authentication, item.DataType);
                    await column.SetIsUniqueAsync(authentication, item.IsUnique);
                    await column.SetAutoIncrementAsync(authentication, item.AutoIncrement);
                    await column.SetDefaultValueAsync(authentication, item.DefaultValue);
                    await column.SetTagsAsync(authentication, (TagInfo)item.Tags);
                    await column.SetIsReadOnlyAsync(authentication, item.IsReadOnly);
                    await template.EndNewAsync(authentication, column);
                }
                await template.EndEditAsync(authentication);
            }
        }

        [ConsoleModeOnly]
        [CommandMethod]
        public async Task EditTemplateAsync([CommandCompletion(nameof(GetTableNamesAsync))] string tableName)
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            var table = await this.GetTableAsync(tableName);
            var template = table.Dispatcher.Invoke(() => table.Template);
            var domain = template.Dispatcher.Invoke(() => template.Domain);
            var contains = domain != null && await domain.Users.ContainsAsync(authentication.ID);

            if (contains == false)
                await template.BeginEditAsync(authentication);

            if (await TemplateEditor.EditColumnsAsync(template, authentication) == false)
            {
                await template.CancelEditAsync(authentication);
            }
            else
            {
                try
                {
                    await template.EndEditAsync(authentication);
                }
                catch
                {
                    await template.CancelEditAsync(authentication);
                    throw;
                }
            }
        }

        [ConsoleModeOnly]
        [CommandMethod]
        public async Task EditAsync([CommandCompletion(nameof(GetTableNamesAsync))] string tableName)
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            var table = await this.GetTableAsync(tableName);
            var content = table.Dispatcher.Invoke(() => table.Content);
            if (content.Domain == null)
                await content.BeginEditAsync(authentication);
            var domain = content.Domain;
            var contains = await domain.Users.ContainsAsync(authentication.ID);
            if (contains == false)
                await content.EnterEditAsync(authentication);
            domain.Dispatcher.Invoke(() => domain.UserRemoved += Domain_UserRemoved);

            throw new NotImplementedException("dotnet");
            // this.CommandContext.Category = nameof(ITableContent);
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
            this.CommandContext.Target = content;
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
        }

        [CommandProperty("force")]
        public bool IsForce
        {
            get; set;
        }

        [CommandProperty]
        [CommandCompletion(nameof(GetCategoryPathsAsync))]
        public string CategoryPath
        {
            get; set;
        }

        [CommandProperty("parent")]
        [CommandCompletion(nameof(GetPathsAsync))]
        public string ParentPath
        {
            get; set;
        }

        [CommandProperty]
        public bool CopyContent
        {
            get; set;
        }

        [CommandProperty("quiet", 'q')]
        public bool IsQuiet
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.Drive is DataBasesConsoleDrive drive && drive.DataBaseName != string.Empty;

        protected override bool IsMethodEnabled(CommandMethodDescriptor descriptor)
        {
            return base.IsMethodEnabled(descriptor);
        }

        private void Domain_UserRemoved(object sender, DomainUserRemovedEventArgs e)
        {
            if (sender is IDomain domain && e.RemoveInfo.Reason == RemoveReason.Kick && e.DomainUserInfo.UserID == this.CommandContext.UserID)
            {
                domain.UserRemoved -= Domain_UserRemoved;
                throw new NotImplementedException("dotnet");
                // this.CommandContext.Category = null;
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                this.CommandContext.Target = null;
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }
        }

        private async Task<ITable> GetTableAsync(string tableName)
        {
            var dataBase = await this.DataBaseContext.GetDataBaseAsync(this.Drive.DataBaseName);
            var tableContext = dataBase.GetService(typeof(ITableContext)) as ITableContext;
            var table = await dataBase.Dispatcher.InvokeAsync(() =>
            {
                if (NameValidator.VerifyItemPath(tableName) == true)
                    return tableContext[tableName] as ITable;
                return tableContext.Tables[tableName];
            });
            if (table == null)
                throw new TableNotFoundException(tableName);
            return table;
        }

        private async Task<ITableCategory> GetCategoryAsync(string categoryPath)
        {
            var dataBase = await this.DataBaseContext.GetDataBaseAsync(this.Drive.DataBaseName);
            return await dataBase.GetTableCategoryAsync(categoryPath);
        }

        private async Task<ITableItem> GetTableItemAsync(string tableItemName)
        {
            var dataBase = await this.DataBaseContext.GetDataBaseAsync(this.Drive.DataBaseName);
            return await dataBase.GetTableItemAsync(tableItemName);
        }

        private Task<string[]> GetTableNamesAsync()
        {
            return GetTableNamesAsync(TagInfo.All, null);
        }

        private async Task<string[]> GetTableNamesAsync(TagInfo tags, string filterExpress)
        {
            var dataBase = await this.DataBaseContext.GetDataBaseAsync(this.Drive.DataBaseName);
            var tables = await dataBase.GetTablesAsync();
            var query = from item in tables
                        where StringUtility.GlobMany(item.Name, filterExpress)
                        where (item.TableInfo.DerivedTags & tags) == tags
                        orderby item.Name
                        select item.Name;

            return query.ToArray();
        }

        private async Task<string[]> GetCategoryPathsAsync()
        {
            var dataBase = await this.DataBaseContext.GetDataBaseAsync(this.Drive.DataBaseName);
            var tableCategories = await dataBase.GetTableCategoriesAsync();
            var query = from item in tableCategories
                        orderby item.Path
                        select item.Path;
            return query.ToArray();
        }

        private async Task<string[]> GetPathsAsync()
        {
            var dataBase = await this.DataBaseContext.GetDataBaseAsync(this.Drive.DataBaseName);
            var tableItems = await dataBase.GetTableItemsAsync();
            var query = from item in tableItems
                        orderby item.Path
                        select item is ITable ? item.Name : item.Path;
            return query.ToArray();
        }

        private string GetCurrentDirectory()
        {
            if (this.CommandContext.Drive is DataBasesConsoleDrive)
            {
                var dataBasePath = new DataBasePath(this.CommandContext.Path);
                if (dataBasePath.ItemPath != string.Empty)
                    return dataBasePath.ItemPath;
            }
            return PathUtility.Separator;
        }

        private DataBasesConsoleDrive Drive => this.CommandContext.Drive as DataBasesConsoleDrive;

        private ICremaHost CremaHost { get; }

        private IDataBaseContext DataBaseContext => this.CremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
    }
}
