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

namespace JSSoft.Crema.Repository.Git
{
    class GitCommandItem : CommandOption
    {
        public GitCommandItem(string name)
            : base(name)
        {

        }

        public GitCommandItem(char name)
            : base(name)
        {

        }

        public GitCommandItem(string name, object value)
            : base(name, value)
        {

        }

        public GitCommandItem(char name, object value)
            : base(name, value)
        {

        }

        public static GitCommandItem FromMessage(string message)
        {
            return new GitCommandItem('m', (GitString)message);
        }

        public static GitCommandItem FromFile(string path)
        {
            return new GitCommandItem("file", (GitPath)path);
        }

        public static GitCommandItem FromAuthor(string author)
        {
            return FromAuthor((GitAuthor)author);
        }

        public static GitCommandItem FromAuthor(GitAuthor author)
        {
            return new GitCommandItem("author", (GitString)$"{author}");
        }

        public static GitCommandItem FromPretty(string format)
        {
            return new GitCommandItem($"pretty={format}");
        }

        public static GitCommandItem FromMaxCount(int count)
        {
            return new GitCommandItem($"max-count={count}");
        }

        public static readonly GitCommandItem Separator = new(string.Empty);

        public static readonly GitCommandItem Global = new("global");

        public static readonly GitCommandItem ShowNotes = new("show-notes");

        public static readonly GitCommandItem NoPatch = new("no-patch");
    }
}
