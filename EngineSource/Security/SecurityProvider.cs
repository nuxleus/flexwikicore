using System;
using System.IO;
using System.Security.Principal;
using System.Threading;

using FlexWiki.Collections;

namespace FlexWiki.Security
{
    public class SecurityProvider : IContentProvider
    {
        private NamespaceManager _namespaceManager;
        private IContentProvider _next;

        public SecurityProvider(IContentProvider next)
        {
            _next = next;
        }

        public bool Exists
        {
            get { return _next.Exists; }
        }
        public bool IsReadOnly
        {
            get { return _next.IsReadOnly; }
        }
        public IContentProvider Next
        {
            get
            {
                return _next;
            }
            set
            {
                _next = value;
            }
        }

        private Federation Federation
        {
            get { return _namespaceManager.Federation; }
        }
        private string Namespace
        {
            get { return _namespaceManager.Namespace; }
        }

        public TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            AssertPermission(topic, TopicPermission.Read);
            return Next.AllChangesForTopicSince(topic, stamp);
        }

        private void AssertPermission(UnqualifiedTopicName topic, TopicPermission topicPermission)
        {
            // We don't throw if the topic doesn't exist.
            if (!_next.TopicExists(topic))
            {
                return;
            }

            if (!HasPermission(topic, TopicPermission.Read))
            {
                throw new FlexWikiSecurityException(SecurableAction.Read, SecurityRuleScope.Topic,
                    new QualifiedTopicName(topic.LocalName, Namespace).DottedName);
            }
        }
        public QualifiedTopicNameCollection AllTopics()
        {
            return _next.AllTopics();
        }
        public void DeleteAllTopicsAndHistory()
        {
            _next.DeleteAllTopicsAndHistory();
        }
        public void DeleteTopic(UnqualifiedTopicName topic)
        {
            _next.DeleteTopic(topic);
        }
        public ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            return _next.GetParsedTopic(topicRevision);
        }
        public bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
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
                new UnqualifiedTopicRevision(_namespaceManager.DefinitionTopicName.LocalName));
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
        public void Initialize(NamespaceManager namespaceManager)
        {
            _namespaceManager = namespaceManager;
            _next.Initialize(namespaceManager);
        }
        public void MakeTopicReadOnly(UnqualifiedTopicName topic)
        {
            _next.MakeTopicReadOnly(topic);
        }
        public void MakeTopicWritable(UnqualifiedTopicName topic)
        {
            _next.MakeTopicWritable(topic);
        }
        public TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            return _next.TextReaderForTopic(topicRevision);
        }
        public bool TopicExists(UnqualifiedTopicName name)
        {
            return _next.TopicExists(name);
        }
        public void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            _next.WriteTopic(topicRevision, content);
        }

        private bool CaseInsenstiveEquivalent(string s1, string s2)
        {
            return string.Compare(s1, s2, true) == 0;
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
