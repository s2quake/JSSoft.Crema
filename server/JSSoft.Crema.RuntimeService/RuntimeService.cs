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

using JSSoft.Crema.Runtime.Generation;
using JSSoft.Crema.Runtime.Serialization;
using JSSoft.Crema.ServiceHosts.RuntimeService;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.RuntimeService
{
    [Export(typeof(IPlugin))]
    [Export(typeof(IRuntimeService))]
    [Export(typeof(RuntimeService))]
    partial class RuntimeService : IPlugin, IRuntimeService, IDisposable
    {
        public const string ServiceID = "B8CD9F7C-58B8-4BDA-B6AE-B99ED216DA22";
        private readonly ICremaHost cremaHost;
        private readonly IEnumerable<Runtime.Serialization.IDataSerializer> serializers;
        private readonly Dictionary<Guid, RuntimeServiceItem> items = new();
        private Authentication authentication;

        [ImportingConstructor]
        public RuntimeService(ICremaHost cremaHost, [ImportMany] IEnumerable<Runtime.Serialization.IDataSerializer> serializers)
        {
            this.cremaHost = cremaHost;
            this.serializers = serializers;
            this.Dispatcher = new CremaDispatcher(this);
            this.cremaHost.Opened += CremaHost_Opened;
            this.cremaHost.Closed += CremaHost_Closed;
        }

        public string Name => nameof(RuntimeService);

        public Guid ID => Guid.Parse(ServiceID);

        public async void Initialize(Authentication authentication)
        {
            this.authentication = authentication;

            if (this.cremaHost.GetService(typeof(IDataBaseContext)) is IDataBaseContext dataBaseContext)
            {
                await dataBaseContext.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in dataBaseContext)
                    {
                        var obj = new RuntimeServiceItem(item, this.Dispatcher, authentication);
                        this.items.Add(item.ID, obj);
                    }
                });
            }
        }

        public void Release()
        {

        }

        public RuntimeServiceItem GetServiceItem(Guid dataBaseID)
        {
            if (this.items.ContainsKey(dataBaseID) == false)
                return null;
            return this.items[dataBaseID];
        }

        public IDataSerializer GetSerializer(string type)
        {
            return this.serializers.FirstOrDefault(item => item.Name == type);
        }

        public async Task<ResultBase<GenerationSet>> GetCodeGenerationDataAsync(string dataBaseName, string tags, string filterExpression, string revision)
        {
            var result = new ResultBase<GenerationSet>();
            using var dataBaseItem = await UsingDataBase.SetAsync(this.cremaHost, dataBaseName, this.authentication);
            var dataBaseID = dataBaseItem.DataBase.ID;
            var project = this.GetServiceItem(dataBaseID);
            var tagInfo = new TagInfo(tags);
            result.Value = await project.GernerationAsync(tagInfo, filterExpression, revision);
            return result;
        }

        public async Task<ResultBase<SerializationSet>> GetDataGenerationDataAsync(string dataBaseName, string tags, string filterExpression, string revision)
        {
            var result = new ResultBase<SerializationSet>();
            using var dataBaseItem = await UsingDataBase.SetAsync(this.cremaHost, dataBaseName, this.authentication);
            var dataBaseID = dataBaseItem.DataBase.ID;
            var project = this.GetServiceItem(dataBaseID);
            var tagInfo = (TagInfo)tags;
            result.Value = await project.SerializeAsync(tagInfo, filterExpression, revision);
            return result;
        }

        public async Task<ResultBase<GenerationSet, SerializationSet>> GetMetaDataAsync(string dataBaseName, string tags, string filterExpression, string revision)
        {
            var result = new ResultBase<GenerationSet, SerializationSet>();
            using var dataBaseItem = await UsingDataBase.SetAsync(this.cremaHost, dataBaseName, this.authentication);
            var dataBaseID = dataBaseItem.DataBase.ID;
            var project = this.GetServiceItem(dataBaseID);
            var tagInfo = (TagInfo)tags;
            result.Value1 = await project.GernerationAsync(tagInfo, filterExpression, revision);
            result.Value2 = await project.SerializeAsync(tagInfo, filterExpression, revision);
            return result;
        }

        public async Task<ResultBase> ResetDataAsync(string dataBaseName)
        {
            var result = new ResultBase();
            using var dataBaseItem = await UsingDataBase.SetAsync(this.cremaHost, dataBaseName, this.authentication);
            var dataBaseID = dataBaseItem.DataBase.ID;
            var project = this.GetServiceItem(dataBaseID);
            await project.ResetAsync();
            return result;
        }

        public async Task<ResultBase<string>> GetRevisionAsync(string dataBaseName)
        {
            var result = new ResultBase<string>();
            using var dataBaseItem = await UsingDataBase.SetAsync(this.cremaHost, dataBaseName, this.authentication);
            var dataBase = dataBaseItem.DataBase;
            result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.DataBaseInfo.Revision);
            return result;
        }

        public CremaDispatcher Dispatcher { get; }

        private void DataBaseContext_ItemCreated(object sender, ItemsCreatedEventArgs<IDataBase> e)
        {
            foreach (var item in e.Items)
            {
                var obj = new RuntimeServiceItem(item, this.Dispatcher, authentication);
                this.items.Add(item.ID, obj);
            }
        }

        private void DataBaseContext_ItemDeleted(object sender, ItemsDeletedEventArgs<IDataBase> e)
        {
            foreach (var item in e.Items)
            {
                var obj = this.items[item.ID];
                obj.Dispose();
                this.items.Remove(item.ID);
            }
        }

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            if (this.cremaHost.GetService(typeof(IDataBaseContext)) is IDataBaseContext dataBaseContext)
            {
                await dataBaseContext.Dispatcher.InvokeAsync(() =>
                {
                    dataBaseContext.ItemsCreated += DataBaseContext_ItemCreated;
                    dataBaseContext.ItemsDeleted += DataBaseContext_ItemDeleted;
                });
            }
        }

        private void CremaHost_Closed(object sender, EventArgs e)
        {
            foreach (var item in this.items)
            {
                item.Value.Commit();
            }
            this.items.Clear();
        }

        private void Filter(ref DataBaseMetaData metaData, TagInfo tags, string filterExpression)
        {
            var tableList = new List<TableMetaData>();
            for (var i = 0; i < metaData.Tables.Length; i++)
            {
                var table = metaData.Tables[i];

                if (StringUtility.GlobMany(table.TableInfo.Name, filterExpression) == false)
                    continue;

                if ((table.TableInfo.DerivedTags & tags) != tags)
                    continue;

                var tableInfo = table.TableInfo;
                tableInfo.Columns = tableInfo.Columns.Where(item => (item.DerivedTags & tags) == tags).ToArray();
                table.TableInfo = tableInfo;

                tableList.Add(table);
            }

            metaData.Tables = tableList.ToArray();
        }

        #region IDisposable

        void IDisposable.Dispose()
        {
            this.Dispatcher.Dispose();
        }

        #endregion
    }
}
