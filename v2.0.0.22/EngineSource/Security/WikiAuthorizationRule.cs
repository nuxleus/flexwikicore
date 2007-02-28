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
    [XmlRoot("rule")]
    public class WikiAuthorizationRule : IXmlSerializable
    {
        private SecurableAction _action;
        private SecurityRulePolarity _polarity;
        private SecurityRuleWho _who; 

        public WikiAuthorizationRule()
        {
        }

        public WikiAuthorizationRule(SecurityRule rule)
        {
            if (rule.Scope != SecurityRuleScope.Wiki)
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

        public SecurityRulePolarity Polarity
        {
            get { return _polarity; }
        }
        
        public string Who
        {
            get { return _who.Who; }
        }

        public SecurityRuleWhoType WhoType
        {
            get { return _who.WhoType; }
        }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            if (!string.IsNullOrEmpty(reader.NamespaceURI))
            {
                return; 
            }

            string type = reader.GetAttribute("type"); 

            if (type == "allow")
            {
                _polarity = SecurityRulePolarity.Allow; 
            }
            else if (type == "deny")
            {
                _polarity = SecurityRulePolarity.Deny;
            }
            else
            {
                throw new NotSupportedException("Unsupported or missing rule type: " + ((type == null) ? "<<null>>" : type)); 
            }

            _action = (SecurableAction) Enum.Parse(typeof(SecurableAction), reader.GetAttribute("action"));
            _who = SecurityRuleWho.Parse(reader.GetAttribute("principal"));
            reader.Read(); 
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            if (_polarity == SecurityRulePolarity.Allow)
            {
                writer.WriteAttributeString("type", "allow"); 
            }
            else if (_polarity == SecurityRulePolarity.Deny)
            {
                writer.WriteAttributeString("type", "deny");
            }
            else
            {
                throw new NotImplementedException("Unsupported security rule polarity"); 
            }

            writer.WriteAttributeString("action", _action.ToString());
            writer.WriteAttributeString("principal", _who.ToString());
        }
    }
}
