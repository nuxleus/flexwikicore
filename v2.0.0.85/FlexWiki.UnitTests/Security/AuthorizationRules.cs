using System;
using System.Collections.Generic;
using System.Security.Principal;

using FlexWiki.Security; 

namespace FlexWiki.UnitTests.Security
{
    internal static class AuthorizationRules
    {
        internal static IEnumerable<AuthorizationRule> GetAll(string user, string role, int lexicalOrder)
        {
            AuthorizationRuleWho[] whos = 
            { 
                new AuthorizationRuleWho(AuthorizationRuleWhoType.User, user), 
                new AuthorizationRuleWho(AuthorizationRuleWhoType.Role, role), 
                new AuthorizationRuleWho(AuthorizationRuleWhoType.GenericAuthenticated, null), 
                new AuthorizationRuleWho(AuthorizationRuleWhoType.GenericAnonymous, null), 
                new AuthorizationRuleWho(AuthorizationRuleWhoType.GenericAll, null),
            };

            AuthorizationRulePolarity[] whats = 
            {
                AuthorizationRulePolarity.Deny, 
                AuthorizationRulePolarity.Allow,
            };

            AuthorizationRuleScope[] wheres = 
            {
                AuthorizationRuleScope.Topic, 
                AuthorizationRuleScope.Namespace, 
                AuthorizationRuleScope.Wiki, 
            };

            SecurableAction[] permissions = 
            {
                SecurableAction.Read,
                SecurableAction.Edit, 
                SecurableAction.ManageNamespace,
            };

            foreach (AuthorizationRuleWho who in whos)
            {
                foreach (AuthorizationRulePolarity what in whats)
                {
                    foreach (AuthorizationRuleScope where in wheres)
                    {
                        foreach (SecurableAction permission in permissions)
                        {
                            if (IsValidCombination(who, what, where, permission))
                            {
                                yield return new AuthorizationRule(who, what, where, permission, lexicalOrder);
                            }
                        }
                    }
                }
            }
        }
        internal static bool GetGrantExpectation(TopicPermission topicPermission, TestIdentity identity, params AuthorizationRule[] rules)
        {
            List<AuthorizationRule> sortedRules = new List<AuthorizationRule>();
            sortedRules.AddRange(rules);
            sortedRules.Sort();

            bool granted = false; 
            foreach (AuthorizationRule rule in sortedRules)
            {
                if (DoesRuleApply(rule, identity))
                {
                    if (rule.Polarity == AuthorizationRulePolarity.Allow)
                    {
                        if ((int)topicPermission >= (int)rule.Action)
                        {
                            granted = true;
                        }
                    }
                    else if (rule.Polarity == AuthorizationRulePolarity.Deny)
                    {
                        if ((int)topicPermission <= (int)rule.Action)
                        {
                            granted = false;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            return granted;
        }

        private static bool DoesRuleApply(AuthorizationRule rule, TestIdentity identity)
        {
            if (rule.Who.WhoType == AuthorizationRuleWhoType.GenericAll)
            {
                return true; 
            }
            else if (rule.Who.WhoType == AuthorizationRuleWhoType.GenericAnonymous)
            {
                return !identity.IsAuthenticated; 
            }
            else if (rule.Who.WhoType == AuthorizationRuleWhoType.GenericAuthenticated)
            {
                return identity.IsAuthenticated; 
            }
            else if (rule.Who.WhoType == AuthorizationRuleWhoType.Role)
            {
                return identity.Roles.Contains(rule.Who.Who); 
            }
            else if (rule.Who.WhoType == AuthorizationRuleWhoType.User)
            {
                return identity.Name == rule.Who.Who;
            }
            else
            {
                throw new NotImplementedException(); 
            }
        }

        private static bool IsValidCombination(AuthorizationRuleWho who, AuthorizationRulePolarity what, AuthorizationRuleScope where,
            SecurableAction permission)
        {
            if ((where == AuthorizationRuleScope.Topic) && (permission == SecurableAction.ManageNamespace))
            {
                return false;
            }

            return true;
        }

    }
}
