using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Security
{
    public class FlexWikiSecurityException : Exception
    {
        private SecurableAction _action;
        private string _name; 
        private SecurityRuleScope _scope; 

        public FlexWikiSecurityException(SecurableAction action, SecurityRuleScope scope, string name)
        {
            _action = action;
            _scope = scope;
            _name = name; 
        }

        public SecurableAction Action
        {
            get { return _action; }
        }
        
        public override string Message
        {
            get
            {
                return string.Format("Permission to {0} {1} {2} is denied.",
                    _action, _scope, _name); 
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public SecurityRuleScope Scope
        {
            get { return _scope; }
        }
    }
}
