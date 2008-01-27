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
using System.Text;

namespace FlexWiki.Collections
{
    public class ExternalReferencesMap : Dictionary<string, string>
    {
        /// <summary>
        /// Indexer gets or sets the value for a given key. 
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <returns>The requested item, or null if it is not found.</returns>
        /// <remarks>This implementation hides the base class implementation because
        /// we want to return null when the key is not a member of the collection
        /// rather than throwing an exception. </remarks>
        public new string this[string key]
        {
            get
            {
                string value;
                bool found = this.TryGetValue(key, out value);

                if (!found)
                {
                    return null;
                }
                else
                {
                    return value; 
                }
            }
            set
            {
                base[key] = value; 
            }
        }

        public void AddRange(ExternalReferencesMap items)
        {
            foreach (KeyValuePair<string, string> item in items)
            {
                this.Add(item.Key, item.Value); 
            }
        }
    }
}
