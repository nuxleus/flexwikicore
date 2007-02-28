using System;
using System.Collections.Generic;
using System.Security.Principal;

using FlexWiki.Security; 

namespace FlexWiki.UnitTests.Security
{
    internal static class SecurityRules
    {
        internal static IEnumerable<SecurityRule> GetAll(string user, string role, int lexicalOrder)
        {
            SecurityRuleWho[] whos = 
            { 
                new SecurityRuleWho(SecurityRuleWhoType.User, user), 
                new SecurityRuleWho(SecurityRuleWhoType.Role, role), 
                new SecurityRuleWho(SecurityRuleWhoType.GenericAuthenticated, null), 
                new SecurityRuleWho(SecurityRuleWhoType.GenericAnonymous, null), 
                new SecurityRuleWho(SecurityRuleWhoType.GenericAll, null),
            };

            SecurityRulePolarity[] whats = 
            {
                SecurityRulePolarity.Deny, 
                SecurityRulePolarity.Allow,
            };

            SecurityRuleScope[] wheres = 
            {
                SecurityRuleScope.Topic, 
                SecurityRuleScope.Namespace, 
                SecurityRuleScope.Wiki, 
            };

            SecurableAction[] permissions = 
            {
                SecurableAction.Read,
                SecurableAction.Edit, 
                SecurableAction.ManageNamespace,
            };

            foreach (SecurityRuleWho who in whos)
            {
                foreach (SecurityRulePolarity what in whats)
                {
                    foreach (SecurityRuleScope where in wheres)
                    {
                        foreach (SecurableAction permission in permissions)
                        {
                            if (IsValidCombination(who, what, where, permission))
                            {
                                yield return new SecurityRule(who, what, where, permission, lexicalOrder);
                            }
                        }
                    }
                }
            }
        }
        internal static bool GetGrantExpectation(TopicPermission topicPermission, TestIdentity identity, params SecurityRule[] rules)
        {
            List<SecurityRule> sortedRules = new List<SecurityRule>();
            sortedRules.AddRange(rules);
            sortedRules.Sort();

            bool granted = false; 
            foreach (SecurityRule rule in sortedRules)
            {
                if (DoesRuleApply(rule, identity))
                {
                    if (rule.Polarity == SecurityRulePolarity.Allow)
                    {
                        if ((int)topicPermission >= (int)rule.Action)
                        {
                            granted = true;
                        }
                    }
                    else if (rule.Polarity == SecurityRulePolarity.Deny)
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

        private static bool DoesRuleApply(SecurityRule rule, TestIdentity identity)
        {
            if (rule.Who.WhoType == SecurityRuleWhoType.GenericAll)
            {
                return true; 
            }
            else if (rule.Who.WhoType == SecurityRuleWhoType.GenericAnonymous)
            {
                return !identity.IsAuthenticated; 
            }
            else if (rule.Who.WhoType == SecurityRuleWhoType.GenericAuthenticated)
            {
                return identity.IsAuthenticated; 
            }
            else if (rule.Who.WhoType == SecurityRuleWhoType.Role)
            {
                return identity.Roles.Contains(rule.Who.Who); 
            }
            else if (rule.Who.WhoType == SecurityRuleWhoType.User)
            {
                return identity.Name == rule.Who.Who;
            }
            else
            {
                throw new NotImplementedException(); 
            }
        }

        private static bool IsValidCombination(SecurityRuleWho who, SecurityRulePolarity what, SecurityRuleScope where,
            SecurableAction permission)
        {
            if ((where == SecurityRuleScope.Topic) && (permission == SecurableAction.ManageNamespace))
            {
                return false;
            }

            return true;
        }

    }
}
