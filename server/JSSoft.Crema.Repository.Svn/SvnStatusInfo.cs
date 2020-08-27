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

using JSSoft.Crema.Services;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace JSSoft.Crema.Repository.Svn
{
    struct SvnStatusInfo
    {
        public string Path { get; set; }

        public string OldPath { get; set; }

        public RepositoryItemStatus Status { get; set; }

        public static SvnStatusInfo[] Run(params string[] paths)
        {
            var statusCommand = new SvnCommand("status")
            {
                SvnCommandItem.Xml
            };
            foreach (var item in paths)
            {
                statusCommand.Add((SvnPath)item);
            }
            return Parse(statusCommand.Run());
        }

        public static SvnStatusInfo[] Parse(string text)
        {
            using var sr = new StringReader(text);
            var doc = XDocument.Load(sr);
            var itemList = new List<SvnStatusInfo>();

            foreach (var element in doc.XPathSelectElements("/status/target/entry"))
            {
                var path = element.Attribute("path").Value;
                var status = element.XPathSelectElement("wc-status").Attribute("item").Value;

                var item = new SvnStatusInfo()
                {
                    Path = path,
                };

                if (status == "added")
                {
                    item.Status = RepositoryItemStatus.Added;
                }
                else if (status == "deleted")
                {
                    item.Status = RepositoryItemStatus.Deleted;
                }
                else if (status == "modified")
                {
                    item.Status = RepositoryItemStatus.Modified;
                }
                else if (status == "unversioned")
                {
                    item.Status = RepositoryItemStatus.Untracked;
                }
                itemList.Add(item);
            }

            return itemList.ToArray();
        }
    }
}