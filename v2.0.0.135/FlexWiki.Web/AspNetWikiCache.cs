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
