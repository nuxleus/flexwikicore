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
using System.Threading; 

namespace FlexWiki
{
    /// <summary>
    /// Provides a place to track the details of one particular request
    /// </summary>
    public class RequestContext : IDisposable
    {
        private Dictionary<string, object> _dictionary = new Dictionary<string, object>();
        [ThreadStatic]
        private static RequestContext s_current; 

        private RequestContext()
        {
        }

        public static RequestContext Current
        {
            get
            {
                return s_current;
            }
        }

        public object this[string key]
        {
            get
            {
                if (!_dictionary.ContainsKey(key))
                {
                    return null;
                }
                return _dictionary[key]; 
            }
            set
            {
                _dictionary[key] = value; 
            }
        }

        public static RequestContext Create()
        {
            s_current = new RequestContext();
            return s_current; 
        }

        void IDisposable.Dispose()
        {
            s_current = null; 
        }
    }
}
