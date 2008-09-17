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
using System.Collections.Generic;

using FlexWiki.Caching;

namespace FlexWiki.Web.UnitTests.Caching
{
    internal class MockCache : IWikiCache
    {

        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();


        public string[] Keys
        {
            get
            {
                List<string> keys = new List<string>();

                foreach (string key in _cache.Keys)
                {
                    keys.Add(key);
                }

                return keys.ToArray();
            }
        }

        public object this[string key]
        {
            get
            {
                if (_cache.ContainsKey(key))
                {
                    return _cache[key];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _cache[key] = value;
            }
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void ClearTopic(string topic)
        { 
            _cache.Remove(topic);       	
        }

        internal Dictionary<string, object> GetCacheContents()
        {
            return _cache;
        }


    }
}
