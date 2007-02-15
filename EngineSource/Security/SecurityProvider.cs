using System;
using System.Security.Principal;
using System.Threading; 

namespace FlexWiki.Security
{
    public class SecurityProvider : ContentProviderBase
    {
        public SecurityProvider(ContentProviderBase next)
            : base(next)
        {
        }

        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            if (permission != TopicPermission.Edit && permission != TopicPermission.Read)
            {
                throw new ArgumentException("Unrecognized topic permission " + permission.ToString()); 
            }

            // Do not allow the operation if the rest of the chain denies it.
            if (!Next.HasPermission(topic, permission))
            {
                return false;
            }

            // Assemble the rules. First wiki, then namespace, then topic
            SecurityRuleCollection rules = new SecurityRuleCollection();
            int lexicalOrder = 0; 
            foreach (WikiAuthorizationRule wikiRule in this.Federation.Configuration.AuthorizationRules)
            {
                SecurityRule rule = new SecurityRule(new SecurityRuleWho(wikiRule.WhoType, wikiRule.Who),
                    wikiRule.Polarity, SecurityRuleScope.Wiki, wikiRule.Action, lexicalOrder++);
                rules.Add(rule); 
            }

            ParsedTopic parsedDefinitionTopic = Next.GetParsedTopic(
                new UnqualifiedTopicRevision(NamespaceManager.DefinitionTopicName.LocalName));
            if (parsedDefinitionTopic != null)
            {
                rules.AddRange(GetSecurityRules(parsedDefinitionTopic, SecurityRuleScope.Namespace));
            }

            ParsedTopic parsedTopic = Next.GetParsedTopic(new UnqualifiedTopicRevision(topic));
            rules.AddRange(GetSecurityRules(parsedTopic, SecurityRuleScope.Topic)); 

            bool allowed = false;

            foreach (SecurityRule rule in rules)
            {
                if (rule.Who.IsMatch(Thread.CurrentPrincipal))
                {
                    if (rule.Polarity == SecurityRulePolarity.Allow)
                    {
                        if ((int)permission >= (int)rule.Action)
                        {
                            allowed = true;
                        }
                    }
                    else if (rule.Polarity == SecurityRulePolarity.Deny)
                    {
                        if ((int)permission <= (int)rule.Action)
                        {
                            allowed = false;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            return allowed; 
        }

        private SecurityRuleCollection GetSecurityRules(ParsedTopic parsedTopic, SecurityRuleScope scope)
        {
            int lexicalOrder = 0; 
            SecurityRuleCollection rules = new SecurityRuleCollection(); 
            foreach (TopicProperty property in parsedTopic.Properties)
            {
                SecurableAction action; 
                SecurityRulePolarity polarity;
                if (SecurityRule.TryParseRuleType(property, scope, out action, out polarity))
                {
                    foreach (string propertyValue in property.AsList())
                    {
                        SecurityRuleWho who;
                        if (SecurityRuleWho.TryParse(propertyValue, out who))
                        {
                            rules.Add(new SecurityRule(who, polarity, scope, action, lexicalOrder++)); 
                        }
                    }
                }
            }

            return rules; 
        }


        private bool CaseInsenstiveEquivalent(string s1, string s2)
        {
            return string.Compare(s1, s2, true) == 0;
        }

        private bool IsPrincipalListedUnderProperty(ParsedTopic parsedTopic, IPrincipal principal, string propertyName)
        {
            TopicProperty property = null;
            if (parsedTopic.Properties.Contains(propertyName))
            {
                property = parsedTopic.Properties[propertyName];
            }

            if (property == null)
            {
                return false; 
            }

            foreach (string value in property.AsList())
            {
                if (value.ToLower().StartsWith(StringLiterals.UserPrefix))
                {
                    if (CaseInsenstiveEquivalent(value, StringLiterals.UserPrefix + principal.Identity.Name))
                    {
                        return true; 
                    }
                }
                else if (value.ToLower().StartsWith(StringLiterals.RolePrefix))
                {
                    if (principal.IsInRole(value.Substring(StringLiterals.RolePrefix.Length)))
                    {
                        return true; 
                    }
                }
                else if (CaseInsenstiveEquivalent(value, StringLiterals.All))
                {
                    return true; 
                }
                else if (CaseInsenstiveEquivalent(value, StringLiterals.Anonymous))
                {
                    return principal.Identity.IsAuthenticated == false; 
                }
                else if (CaseInsenstiveEquivalent(value, StringLiterals.Authenticated))
                {
                    return principal.Identity.IsAuthenticated == true; 
                }
            }

            return false; 

        }

    }
}
