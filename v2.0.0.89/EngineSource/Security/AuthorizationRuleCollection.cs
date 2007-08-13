using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlexWiki.Security
{
    public class AuthorizationRuleCollection : Collection<AuthorizationRule>
    {
        public void AddRange(IEnumerable<AuthorizationRule> items)
        {
            foreach (AuthorizationRule item in items)
            {
                this.Add(item); 
            }
        }
    }
}
