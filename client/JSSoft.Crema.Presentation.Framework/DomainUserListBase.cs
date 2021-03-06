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

using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace JSSoft.Crema.Presentation.Framework
{
    public class DomainUserListBase : PropertyChangedBase
    {
        private readonly ObservableCollection<DomainUserListItemBase> users = new();
        private readonly Authentication authentication;
        private readonly DomainDescriptor descriptor;
        private readonly object owner;

        public DomainUserListBase(Authentication authentication, IDomain domain, bool isSubscriptable, object owner)
            : base(domain)
        {
            this.authentication = authentication;
            this.descriptor = new DomainDescriptor(authentication, domain, isSubscriptable == true ? DescriptorTypes.All : DescriptorTypes.IsRecursive, owner);
            this.IsSubscriptable = isSubscriptable;
            this.owner = owner;
            this.Users = new ReadOnlyObservableCollection<DomainUserListItemBase>(this.users);
            domain.ExtendedProperties[this.owner] = this;

            foreach (var item in this.descriptor.Users)
            {
                this.AddDescriptor(item);
            }

            if (this.descriptor.Users is INotifyCollectionChanged users)
            {
                users.CollectionChanged += Users_CollectionChanged;
            }
        }

        public ReadOnlyObservableCollection<DomainUserListItemBase> Users { get; private set; }

        public bool IsSubscriptable { get; }

        protected virtual DomainUserListItemBase CreateInstance(Authentication authentication, DomainUserDescriptor descriptor, object owner)
        {
            return new DomainUserListItemBase(authentication, descriptor, this.owner);
        }

        private void Users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (item is DomainUserDescriptor descriptor)
                            {
                                this.AddDescriptor(descriptor);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var item in e.OldItems)
                        {
                            if (item is DomainUserDescriptor descriptor)
                            {
                                this.RemoveDescriptor(descriptor);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        this.users.Clear();
                    }
                    break;
            }
        }

        private void AddDescriptor(DomainUserDescriptor descriptor)
        {
            var viewModel = descriptor.Host == null ? this.CreateInstance(this.authentication, descriptor, this.owner) : descriptor.Host as DomainUserListItemBase;
            this.users.Add(viewModel);
        }

        private void RemoveDescriptor(DomainUserDescriptor descriptor)
        {
            foreach (var item in this.users.ToArray())
            {
                if (item is DomainUserListItemBase viewModel && viewModel.Descriptor == descriptor)
                {
                    this.users.Remove(viewModel);
                }
            }
        }
    }
}
