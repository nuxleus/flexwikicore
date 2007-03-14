using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Security
{
    public class SecurityRule : IComparable<SecurityRule>
    {
        private SecurableAction _action;
        private int _lexicalOrder;
        private SecurityRulePolarity _polarity;
        private SecurityRuleScope _scope;
        private SecurityRuleWho _who;

        public SecurityRule(SecurityRuleWho who, SecurityRulePolarity what, SecurityRuleScope scope,
            SecurableAction permission, int lexicalOrder)
        {
            if (permission == SecurableAction.ManageNamespace)
            {
                if (scope == SecurityRuleScope.Topic)
                {
                    throw new ArgumentException("ManageNamespace cannot be stated at topic scope.");
                }
            }

            _who = who; 
            _polarity = what;
            _scope = scope;
            _action = permission;
            _lexicalOrder = lexicalOrder;
        }

        public int LexicalOrder
        {
            get { return _lexicalOrder; }
        }
        public SecurableAction Action
        {
            get { return _action; }
        }
        public SecurityRulePolarity Polarity
        {
            get { return _polarity; }
        }
        public SecurityRuleScope Scope
        {
            get { return _scope; }
        }
        public SecurityRuleWho Who
        {
            get { return _who; }
        }

        private int Score
        {
            get
            {
                return (int)Scope; 
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} for {2} at {3} scope (lexical order {4})",
                Polarity, Action, Who, Scope, LexicalOrder); 
        }
        public string ToString(string format)
        {
            if (format != "T")
            {
                throw new ArgumentException("Unsupported format " + format); 
            }

            string statement = "";

            if (_polarity == SecurityRulePolarity.Allow)
            {
                statement += StringLiterals.Allow;
            }
            else if (_polarity == SecurityRulePolarity.Deny)
            {
                statement += StringLiterals.Deny;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (_action == SecurableAction.Read)
            {
                statement += StringLiterals.Read;
            }
            else if (_action == SecurableAction.Edit)
            {
                statement += StringLiterals.Edit;
            }
            else if (_action == SecurableAction.ManageNamespace)
            {
                statement += StringLiterals.ManageNamespace;
            }
            else
            {
                throw new NotImplementedException();
            }

            statement += ": ";

            statement += _who.ToString();

            return statement;
        }

        int IComparable<SecurityRule>.CompareTo(SecurityRule other)
        {
            if (this.Score == other.Score)
            {
                return _lexicalOrder.CompareTo(other._lexicalOrder); 
            }

            return this.Score.CompareTo(other.Score); 
        }

        public static bool TryParseRuleType(TopicProperty property, SecurityRuleScope scope, 
            out SecurableAction action, out SecurityRulePolarity polarity)
        {
            action = SecurableAction.Read;
            polarity = SecurityRulePolarity.Deny;
            string remainder; 
            if (property.Name.StartsWith(StringLiterals.Allow))
            {
                polarity = SecurityRulePolarity.Allow;
                remainder = property.Name.Substring(StringLiterals.Allow.Length);
            }
            else if (property.Name.StartsWith(StringLiterals.Deny))
            {
                polarity = SecurityRulePolarity.Deny;
                remainder = property.Name.Substring(StringLiterals.Deny.Length);
            }
            else
            {
                return false; 
            }

            if (remainder.Equals(StringLiterals.Read))
            {
                action = SecurableAction.Read; 
            }
            else if (remainder.Equals(StringLiterals.Edit))
            {
                action= SecurableAction.Edit; 
            }
            else if (remainder.Equals(StringLiterals.ManageNamespace))
            {
                if (scope == SecurityRuleScope.Topic)
                {
                    return false;
                }
                else
                {
                    action = SecurableAction.ManageNamespace;
                }
            }
            else
            {
                return false; 
            }

            return true; 
        }
    }
}
