using System;
using System.Collections.Generic;
using System.Xml.Serialization; 

namespace FlexWiki.Security
{
    /*
     <allow action="Read" who="user:candera" />
     <allow action="Edit" who="role:editors" />
     <deny action="ManageNamespace" who="all" />
     */
    [XmlRoot("Rule")]
    public class WikiAuthorizationRule : IXmlSerializable
    {
        private SecurableAction _action;
        private AuthorizationRulePolarity _polarity;
        private AuthorizationRuleWho _who; 

        public WikiAuthorizationRule()
        {
        }

        public WikiAuthorizationRule(AuthorizationRule rule)
        {
            if (rule.Scope != AuthorizationRuleScope.Wiki)
            {
                throw new ArgumentException("Rule must be a wiki-level rule."); 
            }

            _action = rule.Action;
            _polarity = rule.Polarity;
            _who = rule.Who;
        }

        public SecurableAction Action
        {
            get { return _action; }
        }

        public AuthorizationRulePolarity Polarity
        {
            get { return _polarity; }
        }
        
        public string Who
        {
            get { return _who.Who; }
        }

        public AuthorizationRuleWhoType WhoType
        {
            get { return _who.WhoType; }
        }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
			return null;
        }

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            if (!string.IsNullOrEmpty(reader.NamespaceURI))
            {
                return; 
            }

            string type = reader.GetAttribute("Type"); 

            if (type == StringLiterals.Allow)
            {
                _polarity = AuthorizationRulePolarity.Allow; 
            }
            else if (type == StringLiterals.Deny)
            {
                _polarity = AuthorizationRulePolarity.Deny;
            }
            else
            {
                throw new NotSupportedException("Unsupported or missing rule type: " + ((type == null) ? "<<null>>" : type)); 
            }

            _action = (SecurableAction) Enum.Parse(typeof(SecurableAction), reader.GetAttribute("Action"));
            _who = AuthorizationRuleWho.Parse(reader.GetAttribute("Principal"));
            reader.Read(); 
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            if (_polarity == AuthorizationRulePolarity.Allow)
            {
                writer.WriteAttributeString("Type", StringLiterals.Allow); 
            }
            else if (_polarity == AuthorizationRulePolarity.Deny)
            {
                writer.WriteAttributeString("Type", StringLiterals.Deny);
            }
            else
            {
                throw new NotImplementedException("Unsupported security rule polarity"); 
            }

            writer.WriteAttributeString("Action", _action.ToString());
            writer.WriteAttributeString("Principal", _who.ToString());
        }
    }
}
