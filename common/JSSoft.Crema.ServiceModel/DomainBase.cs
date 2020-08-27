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

using JSSoft.Crema.ServiceModel.Properties;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.Library.ObjectModel;
using System;

namespace JSSoft.Crema.ServiceModel
{
    internal abstract class DomainBase<_I, _C, _IC, _CC, _CT> : ItemBase<_I, _C, _IC, _CC, _CT>
        where _I : DomainBase<_I, _C, _IC, _CC, _CT>
        where _C : DomainCategoryBase<_I, _C, _IC, _CC, _CT>, new()
        where _IC : ItemContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CC : CategoryContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CT : ItemContext<_I, _C, _IC, _CC, _CT>
    {
        private DomainInfo domainInfo;
        private DomainState domainState;
        private bool isLoaded;

        public DomainBase()
        {

        }

        public void UpdateDomainInfo(DomainInfo domainInfo)
        {
            this.domainInfo = domainInfo;
            this.OnDomainInfoChanged(EventArgs.Empty);
        }

        public void Initialize(DomainInfo domainInfo)
        {
            if (this.isLoaded == true)
                throw new InvalidOperationException();
            this.domainInfo = domainInfo;
            this.isLoaded = true;
            this.OnDomainInfoChanged(EventArgs.Empty);
        }

        public DomainInfo DomainInfo => this.domainInfo;

        public DomainState DomainState
        {
            get => this.domainState;
            set
            {
                if (this.domainState == value)
                    return;
                this.domainState = value;
                this.OnDomainStateChanged(EventArgs.Empty);
            }
        }

        public bool IsModified
        {
            get => this.DomainState.HasFlag(DomainState.IsModified);
            set
            {
                if (value == true)
                    this.DomainState |= DomainState.IsModified;
                else
                    this.DomainState &= ~DomainState.IsModified;
            }
        }

        public bool IsActivated
        {
            get => this.DomainState.HasFlag(DomainState.IsActivated);
            set
            {
                if (value == true)
                    this.DomainState |= DomainState.IsActivated;
                else
                    this.DomainState &= ~DomainState.IsActivated;
            }
        }

        public event EventHandler DomainInfoChanged;

        public event EventHandler DomainStateChanged;

        protected void UpdateModificationInfo(SignatureDate signatureDate)
        {
            this.domainInfo.ModificationInfo = signatureDate;
            this.OnDomainInfoChanged(EventArgs.Empty);
        }

        protected void Move(IAuthentication _, string categoryPath)
        {
            this.Category = this.Context.Categories[categoryPath];
        }

        protected void Delete(IAuthentication _)
        {
            base.Dispose();
        }

        protected virtual void OnDomainInfoChanged(EventArgs e)
        {
            this.DomainInfoChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainStateChanged(EventArgs e)
        {
            this.DomainStateChanged?.Invoke(this, e);
        }

        protected override void OnPathChanged(string oldPath, string newPath)
        {
            base.OnPathChanged(oldPath, newPath);

            if (this.Category != null)
            {
                this.domainInfo.CategoryPath = this.Category == null ? PathUtility.Separator : this.Category.Path;
                this.OnDomainInfoChanged(EventArgs.Empty);
            }
        }

        protected void ValidateMove(IAuthentication _, string categoryPath)
        {
            if (this.Category.Path == categoryPath)
                throw new ArgumentException(Resources.Exception_CannotMoveToSameFolder, nameof(categoryPath));
            var category = this.Context.Categories[categoryPath];
            if (category == null)
                throw new CategoryNotFoundException(categoryPath);
            base.ValidateMove(category);
        }
    }
}
