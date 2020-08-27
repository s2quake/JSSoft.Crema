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
using System.IO;

namespace JSSoft.Crema.Services
{
    public class RepositoryConfiguration : ConfigurationBase, IRepositoryConfiguration, IConfigurationCommitter
    {
        private readonly string itemName;
        private readonly IConfigurationSerializer serializer = new ConfigurationSerializer();

        public RepositoryConfiguration(string itemName, IEnumerable<IConfigurationPropertyProvider> propertiesProvider)
            : base(typeof(IRepositoryConfiguration), propertiesProvider)
        {
            this.itemName = itemName;
            try
            {
                var filename = this.itemName + ".xml";
                using var stream = File.OpenRead(filename);
                this.Read(stream, this.serializer);
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
            }
        }

        public override string Name => "CremaConfigs";

        public void Commit()
        {
            try
            {
                var filename = this.itemName + ".xml";
                using var stream = File.OpenWrite(filename);
                this.Write(stream, this.serializer);
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
        }
    }
}
