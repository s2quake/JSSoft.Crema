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

using System.IO;

namespace Ntreev.Crema.Services
{
    public class RepositoryValidationSettings
    {
        private const string validationString = "validation";
        private string fileType;
        private string repositoryModule;
        private string[] dataBaseNames;

        public RepositoryValidationSettings()
        {
            this.Verbose = LogVerbose.Error;
        }

        public string BasePath
        {
            get; set;
        }

        public string FileType
        {
            get => this.fileType ?? CremaSettings.DefaultFileType;
            set => this.fileType = value;
        }

        public LogVerbose Verbose
        {
            get; set;
        }

        public string RepositoryModule
        {
            get => this.repositoryModule ?? CremaSettings.DefaultRepositoryModule;
            set => this.repositoryModule = value;
        }

        public bool Force
        {
            get; set;
        }

        public string[] DataBaseNames
        {
            get => this.dataBaseNames ?? new string[] { };
            set => this.dataBaseNames = value;
        }

        public string TempPath
        {
            get; set;
        }

        public readonly static RepositoryValidationSettings Default = new RepositoryValidationSettings();

        internal string GetTempPath(string repositoryName)
        {
            if (this.TempPath == null)
            {
                return Path.Combine(this.BasePath, validationString, repositoryName);
            }
            return Path.Combine(this.TempPath, repositoryName);
        }

        internal string GetTempPath()
        {
            if (this.TempPath == null)
            {
                return Path.Combine(this.BasePath, validationString);
            }
            return this.TempPath;
        }
    }
}
