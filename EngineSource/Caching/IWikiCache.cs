using System;
using System.Collections.Generic; 

namespace FlexWiki.Caching
{
    public interface IWikiCache
    {
        object this[string key] { get; set; }
        // We use an array because we need the collection to be stable - a live
        // collection can't be iterated over and modified at the same time, which 
        // is a common situation when enumerating the cache keys. 
        string[] Keys { get; }
    }
}
