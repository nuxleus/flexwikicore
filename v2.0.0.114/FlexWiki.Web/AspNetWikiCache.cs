using System;
using System.Collections; 
using System.Collections.Generic; 
using System.Web; 
using System.Web.Caching;

using FlexWiki.Caching; 

namespace FlexWiki.Web
{
    internal class AspNetWikiCache : IWikiCache
    {
		
		public string[] Keys
        {
            get
            {
                List<string> keys = new List<string>(); 
                foreach (DictionaryEntry entry in HttpContext.Current.Cache)
                {
                    keys.Add(entry.Key as string); 
                }

                return keys.ToArray(); 
            }
        }
		
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
		

		public void Clear()
        {
            string[] keys = Keys;
            foreach (string key in keys)
            {
                HttpContext.Current.Cache.Remove(key);
            }
        }
		

    }
}
