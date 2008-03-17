using System;
using System.Collections.Generic; 

namespace FlexWiki.Tools.RenderDriver
{
    public class InMemoryCache : FlexWiki.Caching.IWikiCache
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        public InMemoryCache()
        {
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

        internal Dictionary<string, object> GetCacheContents()
        {
            return _cache;
        }

    }
}
