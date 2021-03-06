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

using JSSoft.Library.Commands;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
    class MoveCommand : ConsoleCommandAsyncBase
    {
        public MoveCommand()
            : base("mv")
        {

        }

        public override string[] GetCompletions(CommandCompletionContext completionContext)
        {
            if (completionContext.MemberDescriptor.DescriptorName == nameof(SourcePath) == true)
                return this.CommandContext.GetCompletion(completionContext.Find, true);
            else if (completionContext.MemberDescriptor.DescriptorName == nameof(DestPath) == true)
                return this.CommandContext.GetCompletion(completionContext.Find);
            return base.GetCompletions(completionContext);
        }

        [CommandPropertyRequired]
        public string SourcePath
        {
            get; set;
        }

        [CommandPropertyRequired]
        public string DestPath
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.IsOnline;

        protected override Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var sourcePath = this.CommandContext.GetAbsolutePath(this.SourcePath);
            var destPath = this.CommandContext.GetAbsolutePath(this.DestPath);
            this.ValidateMove(sourcePath, destPath);
            return this.MoveAsync(sourcePath, destPath);
        }

        private void ValidateMove(string sourcePath, string destPath)
        {
            var sourceRoot = this.CommandContext.GetDrive(sourcePath);
            var destRoot = this.CommandContext.GetDrive(destPath);
            if (sourceRoot != destRoot)
                throw new ArgumentException($"cannot move '{sourceRoot}' to '{destPath}'");
        }

        private Task MoveAsync(string sourcePath, string destPath)
        {
            var root = this.CommandContext.GetDrive(sourcePath);
            var sourceLocalPath = this.CommandContext.GetAbsolutePath(sourcePath);
            var destLocalPath = this.CommandContext.GetAbsolutePath(destPath);
            var authentication = this.CommandContext.GetAuthentication(this);
            return root.MoveAsync(authentication, sourceLocalPath, destLocalPath);
        }
    }
}
