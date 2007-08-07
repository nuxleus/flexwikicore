using System;
using System.Web; 
using System.Web.Caching;

using FlexWiki.Caching; 

namespace FlexWiki.Web
{
    internal class AspNetWikiCache : IWikiCache
    {
        public object this[string key]
        {
            get
            {
                return HttpContext.Current.Cache[key]; 
            }
            set
            {
                if (value == null)
                {
                    HttpContext.Current.Cache.Remove(key);
                }
                else
                {
                    HttpContext.Current.Cache[key] = value;
                }
            }
        }
    }
}
