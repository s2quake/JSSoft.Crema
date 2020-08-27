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

using JSSoft.Communication;
using JSSoft.Crema.ServiceHosts;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Users;
using JSSoft.Library.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    public class CremaBootstrapper : IServiceProvider, IDisposable
    {
        public static int DefaultPort = 4004;
        public const string DefaultDataBase = "default";
        private const string pluginsString = "plugins";

        private CremaSettings settings;
        private CompositionContainer container;

        static CremaBootstrapper()
        {
#if DEBUG
            DefaultInactivityTimeout = TimeSpan.MaxValue;
#else
            DefaultInactivityTimeout = new TimeSpan(0, 1, 0);
#endif
        }

        public CremaBootstrapper()
        {
            this.Initialize();
        }

        public static async Task<bool> IsOnlineAsync(string address, string userID, SecureString password)
        {
            var serviceHost = new CremaHostServiceHost();
            var clientContext = new ClientContext(serviceHost)
            {
                Host = AddressUtility.GetIPAddress(address),
                Port = AddressUtility.GetPort(address)
            };
            var token = Guid.Empty;
            try
            {
                token = await clientContext.OpenAsync();
                return await serviceHost.IsOnlineAsync(userID, UserContext.Encrypt(userID, password));
            }
            finally
            {
                await clientContext.CloseAsync(token);
            }
        }

        public static async Task<DataBaseInfo[]> GetDataBasesAsync(string address)
        {
            var serviceHost = new CremaHostServiceHost();
            var clientContext = new ClientContext(serviceHost)
            {
                Host = AddressUtility.GetIPAddress(address),
                Port = AddressUtility.GetPort(address)
            };
            var token = Guid.Empty;
            try
            {
                token = await clientContext.OpenAsync();
                return await serviceHost.GetDataBaseInfosAsync();
            }
            finally
            {
                await clientContext.CloseAsync(token);
            }
        }

        public object GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
                return this;

            if (typeof(IEnumerable).IsAssignableFrom(serviceType) && serviceType.GenericTypeArguments.Length == 1)
            {
                var itemType = serviceType.GenericTypeArguments.First();
                var contractName = AttributedModelServices.GetContractName(itemType);
                var items = this.container.GetExportedValues<object>(contractName);
                var listGenericType = typeof(List<>);
                var list = listGenericType.MakeGenericType(itemType);
                var ci = list.GetConstructor(new System.Type[] { typeof(int) });
                var instance = ci.Invoke(new object[] { items.Count(), }) as IList;
                foreach (var item in items)
                {
                    instance.Add(item);
                }
                return instance;
            }
            else
            {
                var contractName = AttributedModelServices.GetContractName(serviceType);
                return this.container.GetExportedValue<object>(contractName);
            }
        }

        // bootstrap가 여러개 생기고 닫힐때 CremaLog.Release(); 에서 예외 발생
        public void Dispose()
        {
            this.container.Dispose();
            this.container = null;
            this.OnDisposed(EventArgs.Empty);
        }

        public LogVerbose Verbose
        {
            get => this.settings.Verbose;
            set => this.settings.Verbose = value;
        }

        public event EventHandler Disposed;

        public virtual IEnumerable<Tuple<System.Type, object>> GetParts()
        {
            yield return new Tuple<System.Type, object>(typeof(CremaBootstrapper), this);
            yield return new Tuple<System.Type, object>(typeof(IServiceProvider), this);
        }

        private static readonly Dictionary<string, Assembly> assembliesByPath = new Dictionary<string, Assembly>();

        public virtual IEnumerable<Assembly> GetAssemblies()
        {
            var assemblyList = new List<Assembly>();

            if (Assembly.GetEntryAssembly() != null)
            {
                assemblyList.Add(Assembly.GetEntryAssembly());
            }

            var query = from directory in EnumerableUtility.Friends(AppDomain.CurrentDomain.BaseDirectory, this.SelectPath())
                        let catalog = new DirectoryCatalog(directory)
                        from file in catalog.LoadedFiles
                        select file;

            foreach (var item in query)
            {
                try
                {
                    if (assembliesByPath.ContainsKey(item) == false)
                    {
                        var assembly = Assembly.LoadFrom(item);
                        assemblyList.Add(assembly);
                        assembliesByPath.Add(item, assembly);
                        CremaLog.Debug(assembly.Location);
                    }
                    else
                    {
                        assemblyList.Add(assembliesByPath[item]);
                    }
                }
                catch
                {

                }
            }

            return assemblyList.Distinct();
        }

        public string Culture
        {
            get => $"{System.Globalization.CultureInfo.CurrentCulture}";
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (value != string.Empty)
                {
                    System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo(value);
                    System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo(value);
                }
            }
        }

        protected virtual IEnumerable<string> SelectPath()
        {
            var dllPath = AppDomain.CurrentDomain.BaseDirectory;
            var rootPath = Path.GetDirectoryName(dllPath);
            var pluginsPath = Path.Combine(rootPath, pluginsString);
            if (Directory.Exists(pluginsPath) == true)
            {
                foreach (var item in Directory.GetDirectories(pluginsPath))
                {
                    yield return item;
                }
            }
        }

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }

        private void Initialize()
        {
            var catalog = new AggregateCatalog();

            foreach (var item in this.GetAssemblies())
            {
                catalog.Catalogs.Add(new AssemblyCatalog(item));
            }

            this.container = new CompositionContainer(catalog);

            var batch = new CompositionBatch();
            foreach (var item in this.GetParts())
            {
                var contractName = AttributedModelServices.GetContractName(item.Item1);
                var typeIdentity = AttributedModelServices.GetTypeIdentity(item.Item1);
                batch.AddExport(new Export(contractName, new Dictionary<string, object>
                {
                    {
                        "ExportTypeIdentity",
                        typeIdentity
                    }
                }, () => item.Item2));
            }

            this.container.Compose(batch);
            this.settings = this.container.GetExportedValue<CremaSettings>();
        }

        internal static TimeSpan DefaultInactivityTimeout { get; set; }

        #region CremaHostServiceHost

        class CremaHostServiceHost : ClientServiceHostBase<ICremaHostService>
        {
            private ICremaHostService service;

            public CremaHostServiceHost()
            {
            }

            public async Task<DataBaseInfo[]> GetDataBaseInfosAsync()
            {
                return (await this.service.GetDataBaseInfosAsync()).Value;
            }

            public async Task<bool> IsOnlineAsync(string userID, byte[] password)
            {
                return (await this.service.IsOnlineAsync(userID, password)).Value;
            }

            protected override void OnServiceCreated(ICremaHostService service)
            {
                base.OnServiceCreated(service);
                this.service = service;
            }
        }

        #endregion
    }
}
