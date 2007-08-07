using System;

namespace FlexWiki.Caching
{
    /// <summary>
    /// Provides a null implementation of caching for environments where caching is 
    /// not needed. 
    /// </summary>
    public class NullCache : IWikiCache
    {
        public object this[string key]
        {
            get
            {
                return null; 
            }
            set
            {
                // Do nothing - we simply discard the cached item so it appears
                // that the cache item has always expired the next time someone
                // asks for it. 
            }
        }
    }
}
