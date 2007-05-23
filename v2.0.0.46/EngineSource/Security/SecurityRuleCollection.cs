using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlexWiki.Security
{
    public class SecurityRuleCollection : Collection<SecurityRule>
    {
        public void AddRange(IEnumerable<SecurityRule> items)
        {
            foreach (SecurityRule item in items)
            {
                this.Add(item); 
            }
        }
    }
}
