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

namespace FlexWiki.Security
{
    public class AuthorizationRule : IComparable<AuthorizationRule>
    {
        private SecurableAction _action;
        private int _lexicalOrder;
        private AuthorizationRulePolarity _polarity;
        private AuthorizationRuleScope _scope;
        private AuthorizationRuleWho _who;

        public AuthorizationRule(AuthorizationRuleWho who, AuthorizationRulePolarity what, AuthorizationRuleScope scope,
            SecurableAction permission, int lexicalOrder)
        {
            if (permission == SecurableAction.ManageNamespace)
            {
                if (scope == AuthorizationRuleScope.Topic)
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
        public AuthorizationRulePolarity Polarity
        {
            get { return _polarity; }
        }
        public AuthorizationRuleScope Scope
        {
            get { return _scope; }
        }
        public AuthorizationRuleWho Who
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

            if (_polarity == AuthorizationRulePolarity.Allow)
            {
                statement += StringLiterals.Allow;
            }
            else if (_polarity == AuthorizationRulePolarity.Deny)
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

        int IComparable<AuthorizationRule>.CompareTo(AuthorizationRule other)
        {
            if (this.Score == other.Score)
            {
                return _lexicalOrder.CompareTo(other._lexicalOrder); 
            }

            return this.Score.CompareTo(other.Score); 
        }

        public static bool TryParseRuleType(TopicProperty property, AuthorizationRuleScope scope, 
            out SecurableAction action, out AuthorizationRulePolarity polarity)
        {
            action = SecurableAction.Read;
            polarity = AuthorizationRulePolarity.Deny;
            string remainder; 
            if (property.Name.StartsWith(StringLiterals.Allow))
            {
                polarity = AuthorizationRulePolarity.Allow;
                remainder = property.Name.Substring(StringLiterals.Allow.Length);
            }
            else if (property.Name.StartsWith(StringLiterals.Deny))
            {
                polarity = AuthorizationRulePolarity.Deny;
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
                if (scope == AuthorizationRuleScope.Topic)
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
