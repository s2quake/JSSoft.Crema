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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript.Methods
{
    class DataBaseEventListenerContext
    {
        private readonly ICremaHost cremaHost;
        private readonly IDictionary<DataBaseEvents, DataBaseEventListenerHost> listenerHosts;
        private readonly Dictionary<DataBaseEvents, DataBaseEventListenerCollection> listeners = new();
        private readonly CremaDispatcher dispatcher;

        public DataBaseEventListenerContext(ICremaHost cremaHost, DataBaseEventListenerHost[] eventListener)
        {
            this.cremaHost = cremaHost;

            this.listenerHosts = eventListener.ToDictionary(item => item.EventName);
            this.dispatcher = new CremaDispatcher(this);
            foreach (var item in eventListener)
            {
                item.Dispatcher = this.dispatcher;
            }
            this.DataBaseContext.Dispatcher.Invoke(() => this.DataBaseContext.ItemsLoaded += DataBaseContext_ItemsLoaded);
            this.DataBaseContext.Dispatcher.Invoke(() => this.DataBaseContext.ItemsUnloaded += DataBaseContext_ItemsUnloaded);
        }

        public async Task DisposeAsync()
        {
            foreach (var item in this.listenerHosts)
            {
                await item.Value.DisposeAsync();
            }
            await this.dispatcher.DisposeAsync();
        }

        public async Task AddEventListenerAsync(DataBaseEvents eventName, DataBaseEventListener listener)
        {
            var dataBases = await this.DataBaseContext.GetDataBasesAsync(DataBaseFlags.Loaded);
            if (this.listenerHosts.ContainsKey(eventName) == true)
            {
                if (this.listeners.ContainsKey(eventName) == false)
                {
                    this.listeners[eventName] = new DataBaseEventListenerCollection();
                }

                this.listeners[eventName].Add(listener);

                var listenerHost = this.listenerHosts[eventName];
                foreach (var item in dataBases)
                {
                    await listenerHost.SubscribeAsync(item, listener);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public async Task RemoveEventListenerAsync(DataBaseEvents eventName, DataBaseEventListener listener)
        {
            var dataBases = await this.DataBaseContext.GetDataBasesAsync(DataBaseFlags.Loaded);
            if (this.listenerHosts.ContainsKey(eventName) == true)
            {
                var listenerHost = this.listenerHosts[eventName];
                foreach (var item in dataBases)
                {
                    await listenerHost.UnsubscribeAsync(item, listener);
                }
                var listeners = this.listeners[eventName];
                listeners.Remove(listener);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private async void DataBaseContext_ItemsLoaded(object sender, ItemsEventArgs<IDataBase> e)
        {
            if (sender is IDataBase dataBase)
            {
                foreach (var item in this.listenerHosts)
                {
                    var eventName = item.Key;
                    var host = item.Value;
                    var listeners = this.listeners[eventName];
                    await host.SubscribeAsync(dataBase, listeners);
                }
            }
        }

        private async void DataBaseContext_ItemsUnloaded(object sender, ItemsEventArgs<IDataBase> e)
        {
            if (sender is IDataBase dataBase)
            {
                foreach (var item in this.listenerHosts.Values)
                {
                    await item.UnsubscribeAsync(dataBase);
                }
            }
        }

        private IDataBaseContext DataBaseContext => this.cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;

        #region classes

        class ListenerItem
        {
            public DataBaseEvents EventName { get; set; }

            public DataBaseEventListener Listener { get; set; }
        }

        #endregion
    }
}
