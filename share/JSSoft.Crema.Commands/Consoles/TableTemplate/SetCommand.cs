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

using JSSoft.Crema.Commands.Consoles.Serializations;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles.TableTemplate
{
    [Export(typeof(ITemplateCommand))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [CommandStaticProperty(typeof(TextEditorHost))]
    class SetCommand : TemplateCommandBase, ITemplateCommand
    {
        public SetCommand()
            : base("set")
        {

        }

        public ITableTemplate Template => this.CommandContext.Template;

        public Authentication Authentication => this.CommandContext.Authentication;

        [CommandProperty(InitValue = "")]
        public string ColumnName
        {
            get; set;
        }

        protected override Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            return this.EditMemberAsync();
        }

        public override string[] GetCompletions(string find)
        {
            return base.GetCompletions(find);
        }

        private async Task EditMemberAsync()
        {
            var value = new JsonColumnInfo();
            var column = this.Template.Dispatcher.Invoke(() => this.Template[this.ColumnName]);
            var dataTypes = this.Template.Dispatcher.Invoke(() => this.Template.SelectableTypes);
            var columnCount = this.Template.Dispatcher.Invoke(() => this.Template.Count);

            this.Template.Dispatcher.Invoke(() =>
            {
                value.Name = column.Name;
                value.Index = column.Index;
                value.IsKey = column.IsKey;
                value.DataType = column.DataType;
                value.Comment = column.Comment;
            });

            var schema = JsonSchemaUtility.CreateSchema(typeof(JsonColumnInfo));
            var typeSchema = schema.Properties[nameof(JsonColumnInfo.DataType)];
            typeSchema.SetEnums(dataTypes);
            var indexSchema = schema.Properties[nameof(JsonColumnInfo.Index)];
            indexSchema.Minimum = 0;
            indexSchema.Maximum = columnCount - 1;

            using var editor = new JsonEditorHost(value, schema);
            if (editor.Execute() == true)
            {
                value = editor.Read<JsonColumnInfo>();
                if (column.Name != value.Name)
                    await column.SetNameAsync(this.Authentication, value.Name);
            }
        }
    }
}
