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
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.Random;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITypeTemplateTask))]
    [TaskClass]
    class ITypeTemplateTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var authentication = context.Authentication;
            var template = context.Target as ITypeTemplate;
            if (context.IsCompleted(template) == true)
            {
                try
                {
                    var editableState = await template.Dispatcher.InvokeAsync(() => template.EditableState);
                    if (editableState == EditableState.IsBeingEdited)
                    {
                        if (template.Count > 0)
                            await template.EndEditAsync(context.Authentication);
                        else
                            await template.CancelEditAsync(context.Authentication);
                    }
                }
                catch
                {
                    if (template.EditableState == EditableState.IsBeingEdited)
                        await template.CancelEditAsync(context.Authentication);
                }

                context.Pop(template);
                context.Complete(context.Target);
            }
            else
            {
                if (await template.Dispatcher.InvokeAsync(() => template.VerifyAccessType(authentication, AccessType.Developer)) == false)
                {
                    context.Pop(template);
                    return;
                }

                var editableState = await template.Dispatcher.InvokeAsync(() => template.EditableState);
                if (editableState == EditableState.None)
                {
                    try
                    {
                        await template.BeginEditAsync(context.Authentication);
                    }
                    catch
                    {
                        context.Pop(template);
                        throw;
                    }
                }
                else if (editableState == EditableState.IsBeingEdited)
                {
                    var domain = template.Domain;
                    if (await domain.Users.ContainsAsync(authentication.ID) == false)
                    {
                        context.Pop(template);
                        return;
                    }
                }
                else
                {
                    return;
                }

                if (template.IsNew == true || template.Any() == false || RandomUtility.Within(25) == true)
                {
                    var member = await template.AddNewAsync(context.Authentication);
                    context.Push(member);
                    context.State = System.Data.DataRowState.Detached;
                }
                else
                {
                    var member = template.Random();
                    context.Push(member);
                }
            }
        }

        public Type TargetType
        {
            get { return typeof(ITypeTemplate); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        [TaskMethod(Weight = 10)]
        public async Task SetTypeNameAsync(ITypeTemplate template, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (template.EditableState != EditableState.IsBeingEdited)
                    return;
            }
            var tableName = RandomUtility.NextIdentifier();
            await template.SetTypeNameAsync(context.Authentication, tableName);
        }

        [TaskMethod(Weight = 10)]
        public async Task SetIsFlagAsync(ITypeTemplate template, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (template.EditableState != EditableState.IsBeingEdited)
                    return;
            }
            var isFlag = RandomUtility.NextBoolean();
            await template.SetIsFlagAsync(context.Authentication, isFlag);
        }

        [TaskMethod(Weight = 10)]
        public async Task SetCommentAsync(ITypeTemplate template, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (template.EditableState != EditableState.IsBeingEdited)
                    return;
            }
            var comment = RandomUtility.NextString();
            await template.SetCommentAsync(context.Authentication, comment);
        }

        private bool CanEdit(ITypeTemplate template)
        {
            var type = template.Type;
            var tables = type.GetService(typeof(ITableCollection)) as ITableCollection;

            var query = from table in tables
                        from column in table.TableInfo.Columns
                        where column.DataType == type.Path
                        where table.TableState != TableState.None
                        select table;

            if (query.Any() == true)
                return false;


            return true;
        }
    }
}
