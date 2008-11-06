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

using log4net;
using log4net.Config;
using log4net.Core;

using FlexWiki.Caching; 

namespace FlexWiki.Web
{
    internal class AspNetWikiCache : IWikiCache
    {
		public string[] Keys
        {
            get
            {
				if (HttpContext.Current == null) return new string[0];

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
                if (HttpContext.Current == null)
                {
                    return null;
                }
                return HttpContext.Current.Cache[key]; 
            }
            set
            {
                if (HttpContext.Current == null)
                {
                    return;
                }

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
			if (HttpContext.Current == null) return;

            string[] keys = Keys;
            foreach (string key in keys)
            {
                HttpContext.Current.Cache.Remove(key);
            }
        }
		public void ClearTopic(string name)
        {
			if (HttpContext.Current == null) return;
			string cacheKey = "cache://"+name.Replace(".","/") ;

            LogDebug(this.GetType().ToString(),
                     "Clearing Cache for Topic = "+cacheKey);            	
			
			string[] keys = Keys;
            foreach (string key in keys)
            {
            	if(key.Contains(cacheKey))
            	{
                    // LogDebug(this.GetType().ToString(),
                    //     "Removing Topic key = "+key);                  		
                    HttpContext.Current.Cache.Remove(key);          		
            	}
             }
        }	
        public void LogDebug(string source, string message)
        {
            LogManager.GetLogger(source).Debug(message);
        }
        public void LogError(string source, string message)
        {
            LogManager.GetLogger(source).Error(message);
        }
        public void LogInfo(string source, string message)
        {
            LogManager.GetLogger(source).Info(message);
        }
        public void LogWarning(string source, string message)
        {
            LogManager.GetLogger(source).Warn(message);
        }		
    }
}
