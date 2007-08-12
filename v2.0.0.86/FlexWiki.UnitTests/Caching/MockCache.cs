using System;
using System.Collections.Generic;

using FlexWiki.Caching;

namespace FlexWiki.UnitTests.Caching
{
    internal class MockCache : IWikiCache
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

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

        internal Dictionary<string, object> GetCacheContents()
        {
            return _cache;
        }
    }
}
