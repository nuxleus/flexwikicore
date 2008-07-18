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

        public string[] Keys
        {
            // Return an empty list - we don't hold anything.
            get { return new string[0]; }
        }

        public void Clear()
        {
        }
    }
}
