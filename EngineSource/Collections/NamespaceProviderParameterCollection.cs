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
using System.Collections.ObjectModel; 

namespace FlexWiki.Collections
{
    /// <summary>
    /// Provides a strongly-typed collection class for namespace provider parameters.
    /// </summary>
    public class NamespaceProviderParameterCollection : KeyedCollection<string, NamespaceProviderParameter>
    {
        public NamespaceProviderParameterCollection(params NamespaceProviderParameter[] parameters)
        {
            foreach (NamespaceProviderParameter parameter in parameters)
            {
                Add(parameter); 
            }
        }

        public void AddOrReplace(NamespaceProviderParameter item)
        {
            if (this.Contains(item.Name))
            {
                this.Remove(this[item.Name]); 
            }

            this.Add(item); 
        }

        protected override string GetKeyForItem(NamespaceProviderParameter item)
        {
            return item.Name; 
        }
    }
}
