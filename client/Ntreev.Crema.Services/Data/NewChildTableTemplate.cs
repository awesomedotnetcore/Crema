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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.DataBaseService;
using Ntreev.Crema.Services.Domains;
using System;

namespace Ntreev.Crema.Services.Data
{
    class NewChildTableTemplate : TableTemplateBase
    {
        private readonly Table parent;
        private Table table;

        public NewChildTableTemplate(Table parent)
        {
            this.parent = parent;
            this.IsNew = true;
        }

        public override Type GetType(string typeName)
        {
            this.Dispatcher.VerifyAccess();
            var typeContext = this.parent.GetService(typeof(TypeContext)) as TypeContext;
            return typeContext[typeName] as Type;
        }

        public override ITable Table
        {
            get
            {
                this.Dispatcher.VerifyAccess();
                return this.table;
            }
        }

        public override DomainContext DomainContext => this.parent.GetService(typeof(DomainContext)) as DomainContext;

        public override string ItemPath => this.parent.Path;

        public override CremaHost CremaHost => this.parent.CremaHost;

        public override CremaDispatcher Dispatcher => this.parent.Dispatcher;

        public override DataBase DataBase => this.parent.DataBase;

        public override IPermission Permission => this.parent;

        public IDataBaseService Service => this.parent.Service;

        protected override void OnBeginEdit(Authentication authentication, DomainMetaData metaData)
        {
            base.OnBeginEdit(authentication, metaData);
        }

        protected override void OnEndEdit(Authentication authentication, TableInfo tableInfo)
        {
            base.OnEndEdit(authentication, tableInfo);
            this.table = this.parent.AddNew(authentication, tableInfo);
        }

        protected override void OnCancelEdit(Authentication authentication)
        {
            base.OnCancelEdit(authentication);
        }

        protected override ResultBase<DomainMetaData> BeginDomain(Authentication authentication)
        {
            return this.Service.BeginNewTable(this.parent.Path);
        }

        protected override ResultBase<TableInfo> EndDomain(Authentication authentication, Guid domainID)
        {
            return this.Service.EndTableTemplateEdit(domainID);
        }

        protected override ResultBase CancelDomain(Authentication authentication, Guid domainID)
        {
            return this.Service.CancelTableTemplateEdit(domainID);
        }
    }
}
