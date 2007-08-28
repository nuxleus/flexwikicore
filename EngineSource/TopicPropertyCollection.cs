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

namespace FlexWiki
{
    public class TopicPropertyCollection : KeyedCollection<string, TopicProperty>
    {
        public IList<string> Names
        {
            get
            {
                List<string> names = new List<string>();

                foreach (TopicProperty property in this)
                {
                    names.Add(property.Name); 
                }

                return names; 
            }
        }

        public void AddRange(IEnumerable<TopicProperty> properties)
        {
            foreach (TopicProperty property in properties)
            {
                this.Add(property); 
            }
        }

        protected override string GetKeyForItem(TopicProperty item)
        {
            return item.Name; 
        }
    }
}
