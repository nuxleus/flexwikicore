using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Security
{
    public class SecurityProperty
    {
        private SecurableAction _action; 
        private SecurityRulePolarity _polarity;
        private SecurityRuleWho _who;

        public SecurityProperty(SecurityRuleWhoType whoType, string who, SecurityRulePolarity polarity,
            SecurableAction action)
        {
            _who = new SecurityRuleWho(whoType, who);
            _polarity = polarity;
            _action = action; 
        }

        public SecurableAction Action
        {
            get { return _action; }
        }
        
        public SecurityRulePolarity Polarity
        {
            get { return _polarity; }
        }

        public SecurityRuleWho Who
        {
            get { return _who; }
        }

    }
}
