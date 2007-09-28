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

namespace FlexWiki
{
    public class WomElementCollection : IList<WomElement>, ICollection<WomElement>, IEnumerable<WomElement>, IEnumerable
    {
        // Fields

        private WomElement _owner;
        private List<WomElement> _list = new List<WomElement>();

        // Constructors

        public WomElementCollection(WomElement owner)
        {
            this._owner = owner;
        }

        // Properties

        public int Count
        {
            get { return _list.Count; ; }
        }

        public WomElement Owner
        {
            get { return _owner; }
        }

        public WomElement this[int index]
        {
            get { return _list[index]; }
            set
            {
                if (value._parent != null)
                {
                    throw new InvalidOperationException("Item already has a parent");
                }
                _list[index]._parent = null;
                _list[index] = value;
                value._parent = _owner;
            }
        }

        bool ICollection<WomElement>.IsReadOnly
        {
            get { return (_list as ICollection<WomElement>).IsReadOnly; }
        }

        // Methods

        public void Add(WomElement item)
        {
            if (item._parent != null)
            {
                throw new InvalidOperationException("Item already has a parent");
            }
            _list.Add(item);
            item._parent = _owner;
        }

        public void Clear()
        {
            for (int i = 0; i < _list.Count; i++)
            {
                _list[i]._parent = null;
            }
            _list.Clear();
        }

        public bool Contains(WomElement item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(WomElement[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public override bool Equals(object obj)
        {
            WomElementCollection other = obj as WomElementCollection;
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
                if (!this._list[i].Equals(other._list[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public IEnumerator<WomElement> GetEnumerator()
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

        public int IndexOf(WomElement item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, WomElement item)
        {
            if (item._parent != null)
            {
                throw new InvalidOperationException("Item already has a parent");
            }
            _list.Insert(index, item);
            item._parent = _owner;
        }

        public bool Remove(WomElement item)
        {
            if (_list.Remove(item))
            {
                item._parent = null;
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            _list[index]._parent = null;
            _list.RemoveAt(index);
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

    }
}
