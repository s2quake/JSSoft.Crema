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

using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class TypeMember : DomainBasedRow, ITypeMember
    {
        private readonly TypeTemplateBase template;

        public TypeMember(TypeTemplateBase template, DataRow row)
            : base(template.Domain, row)
        {
            this.template = template;
        }

        public TypeMember(TypeTemplateBase template, DataTable table)
            : base(template.Domain, table)
        {
            this.template = template;
            var query = from DataRow item in table.Rows
                        select item.Field<string>(CremaSchema.Name);

            var newName = NameUtility.GenerateNewName("Type", query);
            this.SetField(null, CremaSchema.Name, newName);
        }

        public void SetIndex(Authentication authentication, int value)
        {
            try
            {
                this.ValidateDispatcher(authentication);
                this.SetField(authentication, CremaSchema.Index, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public void SetName(Authentication authentication, string value)
        {
            try
            {
                this.ValidateDispatcher(authentication);
                this.SetField(authentication, CremaSchema.Name, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public void SetValue(Authentication authentication, long value)
        {
            try
            {
                this.ValidateDispatcher(authentication);
                this.SetField(authentication, CremaSchema.Value, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public void SetComment(Authentication authentication, string value)
        {
            try
            {
                this.ValidateDispatcher(authentication);
                this.SetField(authentication, CremaSchema.Comment, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public int Index
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.GetField<int>(CremaSchema.Index);
            }
        }

        public string Name
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.GetField<string>(CremaSchema.Name);
            }
        }

        public long Value
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.GetField<long>(CremaSchema.Value);
            }
        }

        public string Comment
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.GetField<string>(CremaSchema.Comment);
            }
        }

        public override DataBase DataBase => this.template.DataBase;

        public override CremaDispatcher Dispatcher => this.template.Dispatcher;

        public override CremaHost CremaHost => this.template.CremaHost;

        private void ValidateDispatcher(Authentication authentication)
        {
            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);
            this.Dispatcher.VerifyAccess();
        }

        #region ITypeTemplate

        ITypeTemplate ITypeMember.Template
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.template;
            }
        }

        #endregion
    }
}
