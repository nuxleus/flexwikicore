using System;

namespace FlexWiki.Caching
{
    public interface IWikiCache
    {
        object this[string key] { get; set; }
    }
}
