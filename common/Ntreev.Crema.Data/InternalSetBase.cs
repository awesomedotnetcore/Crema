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

using Ntreev.Crema.Data.Properties;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Data
{
    abstract class InternalSetBase : DataSet
    {
        private object target;

        private SignatureDateProvider signatureDateProvider;
        private SignatureDate signatureDate;

        protected InternalSetBase(string dataSetName)
            : base(dataSetName)
        {
            this.Tables.CollectionChanging += Tables_CollectionChanging;
            this.Tables.CollectionChanged += Tables_CollectionChanged;
        }

        private void Tables_CollectionChanging(object sender, CollectionChangeEventArgs e)
        {
            switch (e.Action)
            {
                case CollectionChangeAction.Add:
                    {
                        if (e.Element is InternalTableBase dataTable)
                        {
                            //if (dataTable.Parent != null)
                            //{
                            //    var parentTable = dataTable.Parent;
                            //    var relationName = InternalSetBase.GenerateRelationName(parentTable.LocalName, dataTable.LocalName, parentTable.Namespace);
                            //    this.Relations.Add(relationName, parentTable.RelationColumn, dataTable.RelationColumn);
                            //}
                        }
                    }
                    break;
                case CollectionChangeAction.Remove:
                    {
                        if (e.Element is InternalTableBase dataTable)
                        {
                            if (dataTable.Parent != null)
                            {
                                var parentTable = dataTable.Parent;
                                var relationName = InternalSetBase.GenerateRelationName(parentTable.LocalName, dataTable.LocalName, parentTable.Namespace);
                                dataTable.Constraints.Remove(relationName);
                                this.Relations.Remove(relationName);
                            }
                        }
                    }
                    break;
            }
        }

        private void Tables_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            switch (e.Action)
            {
                case CollectionChangeAction.Add:
                    {
                        if (e.Element is InternalTableBase dataTable)
                        {
                            if (dataTable.Parent != null)
                            {
                                var parentTable = dataTable.Parent;
                                var relationName = InternalSetBase.GenerateRelationName(parentTable.LocalName, dataTable.LocalName, parentTable.Namespace);
                                this.Relations.Add(relationName, parentTable.ColumnRelation, dataTable.ParentRelation);
                            }
                            dataTable.PropertyChanged += DataTable_PropertyChanged;
                        }
                    }
                    break;
                case CollectionChangeAction.Remove:
                    {
                        if (e.Element is InternalTableBase dataTable)
                        {
                            dataTable.PropertyChanged -= DataTable_PropertyChanged;
                            if (dataTable.TemplatedParent != null)
                            {
                                dataTable.InternalTemplatedParent = null;
                            }
                            else
                            {
                                foreach (var item in dataTable.DerivedItems.ToArray())
                                {
                                    item.InternalTemplatedParent = null;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void DataTable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is InternalTableBase dataTable)
            {
                if (e.PropertyName == nameof(InternalTableBase.LocalName))
                {
                    this.RefreshRelation(dataTable);
                }
            }
        }

        public object Target
        {
            get { return this.target; }
            set { this.target = value; }
        }

        public SignatureDateProvider SignatureDateProvider
        {
            get { return this.signatureDateProvider ?? SignatureDateProvider.Default; }
            set { this.signatureDateProvider = value; }
        }

        public SignatureDate SignatureDate
        {
            get { return this.signatureDate; }
        }

        public static void AddRelation(InternalSetBase dataSet, InternalTableBase parent, InternalTableBase table)
        {
            var relationName = InternalSetBase.GenerateRelationName(parent.LocalName, table.LocalName, parent.Namespace);

            if (dataSet != null)
            {
                dataSet.Relations.Add(relationName, parent.ColumnRelation, table.ParentRelation);
            }
        }

        public static void RemoveRelation(InternalSetBase dataSet, InternalTableBase parent, InternalTableBase table)
        {
            var relationName = InternalSetBase.GenerateRelationName(parent.LocalName, table.LocalName, parent.Namespace);
            if (dataSet != null)
            {
                dataSet.Relations.Remove(relationName);
            }
        }

        public void RefreshRelation(InternalTableBase table)
        {
            for (var i = 0; i < this.Relations.Count; i++)
            {
                var item = this.Relations[i];
                if (item.ParentTable == table || item.ChildTable == table)
                {
                    if (item.ParentTable is InternalTableBase parentTable && item.ChildTable is InternalTableBase childTable)
                    {
                        item.RelationName = InternalSetBase.GenerateRelationName(parentTable.LocalName, childTable.LocalName, parentTable.Namespace);
                    }
                }
            }
        }

        public static string GenerateRelationName(string parentName, string childName, string parentNamespace)
        {
            return System.Xml.Linq.XName.Get(string.Format("{0}.{1}", parentName, childName), parentNamespace).ToString();
        }

        public static string GenerateRelationName(InternalTableBase parent, InternalTableBase table)
        {
            return GenerateRelationName(parent.LocalName, table.LocalName, parent.Namespace);
        }

        public static void ValidateSetLocalName(InternalSetBase dataSet, string localName)
        {
            if (dataSet == null && string.IsNullOrEmpty(localName) == false && CremaDataSet.VerifyName(localName) == false)
                throw new ArgumentException(Resources.Exception_InvalidName);
            if (dataSet != null && localName == null)
                throw new ArgumentNullException(nameof(localName));
            if (dataSet != null && CremaDataSet.VerifyName(localName) == false)
                throw new ArgumentException(Resources.Exception_InvalidName);
            //if (dataTable != null && dataTable.Childs.Where(item => item.TableName == columnName).Any() == true)
            //    throw new ArgumentException($"{columnName} 은(는) 자식 테이블의 이름으로 사용되고 있으므로 사용할 수 없습니다.");
            //if (dataTable != null && dataTable.TemplateNamespace != string.Empty)
            //    throw new ArgumentException("상속받은 테이블의 열의 이름은 변경할 수 없습니다.");
        }

        protected SignatureDate Sign()
        {
            this.ValidateSign();
            this.signatureDate = this.SignatureDateProvider.Provide();
            return this.signatureDate;
        }

        private void ValidateSign()
        {
            //if (this.OmitSignatureDate == true)
            //    throw new InvalidOperationException();
        }
    }
}
