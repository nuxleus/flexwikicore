#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace FlexWiki
{
    [DebuggerTypeProxy(typeof(WomAttributeMapDebugView))]
    public class WomPropertyCollection : IEnumerable<WomProperty>, IEnumerable
    {
        // Fields

        private List<WomProperty> _list;

        // Constructors

        public WomPropertyCollection()
        {
            _list = new List<WomProperty>();
        }

        public WomPropertyCollection(int capacity)
        {
            _list = new List<WomProperty>(capacity);
        }

        public WomPropertyCollection(WomPropertyCollection other)
            : this()
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            for (int i = 0; i < other._list.Count; i++)
            {
                _list.Add(new WomProperty(other._list[i]));
            }
        }

        // Properties

        public int Count
        {
            get { return _list.Count; }
        }

        // Methods

        public void Add(WomProperty attribute)
        {
            int index = IndexOf(attribute._name);
            if (index >= 0)
            {
                _list[index]._value = attribute._value;
            }
            else
            {
                _list.Add(attribute);
            }
        }

        public override bool Equals(object obj)
        {
            WomPropertyCollection other = obj as WomPropertyCollection;
            if (other == null)
            {
                return false;
            }
            if (this._list.Count != other._list.Count)
            {
                return false;
            }
            for (int i = 0; i < this._list.Count; i++)
            {
                int index = other.InternalIndexOf(this._list[i]._name);
                if (index == -1 || this._list[i]._value != other._list[index]._value)
                {
                    return false;
                }
            }
            return true;
        }

        public IEnumerator<WomProperty> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            for (int i = 0; i < _list.Count; i++)
            {
                hashCode ^= _list[i].GetHashCode();
            }
            return hashCode;
        }

        public string GetName(int index)
        {
            return _list[index].Name;
        }

        public int IndexOf(string name)
        {
            return InternalIndexOf(WomNameTable.Instance.Get(name));
        }

        public string this[int index]
        {
            get { return _list[index]._value; }
            set { _list[index] = new WomProperty(_list[index]._name, value); }
        }

        public string this[string name]
        {
            get
            {
                int index = IndexOf(name);
                if (index >= 0)
                {
                    return _list[index]._value;
                }
                return String.Empty;
            }
            set
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException("name");
                }
                name = WomNameTable.Instance.Add(name);
                int index = InternalIndexOf(name);
                if (value == null)
                {
                    if (index >= 0)
                    {
                        _list.RemoveAt(index);
                    }
                }
                else if (index >= 0)
                {
                    _list[index]._value = value;
                }
                else
                {
                    _list.Add(new WomProperty(name, value));
                }
            }
        }

        public void WriteTo(XmlWriter writer)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                _list[i].WriteTo(writer);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        private int InternalIndexOf(string name)
        {
            if (name != null)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    if ((object)_list[i]._name == (object)name)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private class WomAttributeMapDebugView
        {
            private WomPropertyCollection _attributes;

            public WomAttributeMapDebugView(WomPropertyCollection attributes)
            {
                this._attributes = attributes;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public WomProperty[] Properties
            {
                get
                {
                    WomProperty[] attributes = new WomProperty[this._attributes.Count];

                    for (int i = 0; i < attributes.Length; i++)
                    {
                        attributes[i] = this._attributes._list[i];
                    }
                    return attributes;
                }
            }
        }
    }
}
