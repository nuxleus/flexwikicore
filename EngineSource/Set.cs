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

namespace FlexWiki
{
	public class Set<T> : IEnumerable<T>
	{
		private Dictionary<T, object> _hash = new Dictionary<T, object>();

        public int Count
        {
            get
            {
                return _hash.Count;
            }
        }
        public T First
        {
            get
            {
                foreach (T item in _hash.Keys)
                {
                    return item;
                }
                return default(T);
            }
        }

		public void Add(T item)
		{
            if (_hash.ContainsKey(item))
            {
                return;
            }
			_hash[item] = null;
		}
		public void AddRange(ICollection c)
		{
            foreach (object obj in c)
            {
                Add((T)obj);
            }
		}
		public bool Contains(T item)
		{
			return _hash.ContainsKey(item);
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _hash.Keys.GetEnumerator();
		}
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _hash.Keys.GetEnumerator(); 
        }
		public void Remove(T item)
		{
			_hash.Remove(item);
		}
	}
}