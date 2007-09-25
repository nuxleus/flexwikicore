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
using System.IO;
using System.Security.Principal;
using System.Threading;

using FlexWiki.Collections;

namespace FlexWiki.Security
{
    public class AuthorizationProvider : IContentProvider
    {
        private const string c_recursionContextKey = "AuthorizationProviderRecursionContextKey"; 

        private NamespaceManager _namespaceManager;
        private IContentProvider _next;

        public AuthorizationProvider(IContentProvider next)
        {
            _next = next;
        }

        public bool Exists
        {
            get
            {
                string key = GetKey("exists");

                if (RequestContext.Current != null)
                {
                    object cached = RequestContext.Current[key];

                    if (cached != null && cached is bool)
                    {
                        return (bool)cached; 
                    }
                }

                bool value; 
                AuthorizationRuleCollection rules = new AuthorizationRuleCollection();
                rules.AddRange(GetWikiScopeRules());
                rules.AddRange(GetNamespaceScopeRules());

                // If the user does not have read permission on the namespace, then 
                // it doesn't exist as far as they're concerned.
                if (!IsAllowed(SecurableAction.Read, rules))
                {
                    value = false;
                }
                else
                {
                    using (CreateRecursionContext())
                    {
                        value = _next.Exists;
                    }
                }

                if (RequestContext.Current != null)
                {
                    RequestContext.Current[key] = value; 
                }

                return value; 
            }
        }

        public bool IsReadOnly
        {
            get 
            {
                // Because you could mark a namespace as read-only for a particular user 
                // but still have writable topics in it (via AllowEdit commands in individual 
                // topics), we just delegate to the next provider. 
                using (CreateRecursionContext())
                {
                    return _next.IsReadOnly;
                }
            }
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
        private bool IsRecursing
        {
            get { return RecursionContextHelper.IsRecursing(c_recursionContextKey); }
        }
        private string Namespace
        {
            get { return _namespaceManager.Namespace; }
        }

        public TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            AssertTopicPermission(topic, TopicPermission.Read);
            using (CreateRecursionContext())
            {
                return Next.AllChangesForTopicSince(topic, stamp);
            }
        }
        public QualifiedTopicNameCollection AllTopics()
        {
            using (CreateRecursionContext())
            {
                return _next.AllTopics();
            }
        }
        public void DeleteAllTopicsAndHistory()
        {
            AssertNamespacePermission(SecurableAction.ManageNamespace);
            using (CreateRecursionContext())
            {
                _next.DeleteAllTopicsAndHistory();
            }
        }
        public void DeleteTopic(UnqualifiedTopicName topic)
        {
            AssertTopicPermission(topic, TopicPermission.Edit);
            using (CreateRecursionContext())
            {
                _next.DeleteTopic(topic);
            }
        }
        public ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            AssertTopicPermission(topicRevision.AsUnqualifiedTopicName(), TopicPermission.Read);
            using (CreateRecursionContext())
            {
                return _next.GetParsedTopic(topicRevision);
            }
        }
        public bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            if (permission != TopicPermission.Edit && permission != TopicPermission.Read)
            {
                throw new ArgumentException("Unrecognized topic permission " + permission.ToString());
            }

            RequestContext context = RequestContext.Current;
            string key = null; 
            if (context != null)
            {
                key = GetKey(topic, permission, "HasPermission");
                object value = context[key];

                if (value != null && value is bool)
                {
                    return (bool) value; 
                }
            }

            // Do not allow the operation if the rest of the chain denies it.
            bool nextHasPermission;
            using (CreateRecursionContext())
            {
                nextHasPermission = _next.HasPermission(topic, permission); 
            }
            if (!nextHasPermission)
            {
                return false;
            }

            bool isDefinitionTopic = false;
            using (CreateRecursionContext())
            {
                if (topic.Equals(new UnqualifiedTopicName(NamespaceManager.DefinitionTopicLocalName)))
                {
                    isDefinitionTopic = true;
                }
            }

            // Assemble the rules. First wiki, then namespace, then topic
            AuthorizationRuleCollection rules = new AuthorizationRuleCollection();
            rules.AddRange(GetWikiScopeRules());
            rules.AddRange(GetNamespaceScopeRules());

            // The namespace rules are redundant if this is the definition topic, as that's where they're stored. 
            if (!isDefinitionTopic)
            {
                rules.AddRange(GetTopicScopeRules(topic));
            }

            SecurableAction action = GetActionFromPermission(permission, isDefinitionTopic);
            bool isAllowed = IsAllowed(action, rules);

            if (context != null)
            {
                context[key] = isAllowed; 
            }

            return isAllowed; 
        }
        public void Initialize(NamespaceManager namespaceManager)
        {
            _namespaceManager = namespaceManager;
            using (CreateRecursionContext())
            {
                _next.Initialize(namespaceManager);
            }
        }
        public void LockTopic(UnqualifiedTopicName topic)
        {
            AssertNamespacePermission(SecurableAction.ManageNamespace); 
            using (CreateRecursionContext())
            {
                _next.LockTopic(topic);
            }
        }
        public TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            AssertTopicPermission(topicRevision.AsUnqualifiedTopicName(), TopicPermission.Read); 
            using (CreateRecursionContext())
            {
                return _next.TextReaderForTopic(topicRevision);
            }
        }
        public bool TopicExists(UnqualifiedTopicName name)
        {
            using (CreateRecursionContext())
            {
                return _next.TopicExists(name);
            }
        }
        public void UnlockTopic(UnqualifiedTopicName topic)
        {
            AssertNamespacePermission(SecurableAction.ManageNamespace);
            using (CreateRecursionContext())
            {
                _next.UnlockTopic(topic);
            }
        }
        public void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            string definitionTopicName = null;
            using (CreateRecursionContext())
            {
                definitionTopicName = _namespaceManager.DefinitionTopicName.LocalName; 
            }
            // You need ManageNamespace permission to edit the definition topic
            if (topicRevision.LocalName.Equals(definitionTopicName))
            {
                AssertNamespacePermission(SecurableAction.ManageNamespace);
            }
            // For all other topics, Edit will do.
            else
            {
                AssertTopicPermission(topicRevision.AsUnqualifiedTopicName(), TopicPermission.Edit);
            }
            using (CreateRecursionContext())
            {
                _next.WriteTopic(topicRevision, content);
            }
        }

        private void AssertNamespacePermission(SecurableAction action)
        {
            AuthorizationRuleCollection rules = new AuthorizationRuleCollection();
            rules.AddRange(GetWikiScopeRules());
            rules.AddRange(GetNamespaceScopeRules());

            if (!IsAllowed(action, rules))
            {
                throw new FlexWikiAuthorizationException(action, AuthorizationRuleScope.Namespace, Namespace);
            }
        }
        private void AssertTopicPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            //// We don't throw if the topic doesn't exist.
            //if (!_next.TopicExists(topic))
            //{
            //    return;
            //}

            if (!HasPermission(topic, permission))
            {
                SecurableAction action = GetActionFromPermission(permission);
                throw new FlexWikiAuthorizationException(action, AuthorizationRuleScope.Topic,
                    new QualifiedTopicName(topic.LocalName, Namespace).DottedName);
            }
        }
        private IDisposable CreateRecursionContext()
        {
            return new RecursionContextHelper(c_recursionContextKey); 
        }
        private SecurableAction GetActionFromPermission(TopicPermission permission)
        {
            return GetActionFromPermission(permission, false);
        }
        private SecurableAction GetActionFromPermission(TopicPermission permission, bool isDefinitionTopic)
        {
            if (permission == TopicPermission.Edit)
            {
                // Editing the definition topic requires ManageNamespace, not Edit. 
                if (isDefinitionTopic)
                {
                    return SecurableAction.ManageNamespace; 
                }
                else
                {
                    return SecurableAction.Edit;
                }
            }
            else if (permission == TopicPermission.Read)
            {
                return SecurableAction.Read;
            }
            else
            {
                throw new ArgumentException("Unexpected permission " + permission.ToString());
            }
        }
        private string GetKey(string tag)
        {
            return string.Format("authorizationProvider://{0}/{1}", Namespace, tag); 
        }
        private string GetKey(UnqualifiedTopicName topic, string tag)
        {
            return string.Format("authorizationProvider://{0}/{1}/{2}", Namespace, topic.LocalName, tag);
        }
        private string GetKey(UnqualifiedTopicName topic, TopicPermission permission, string tag)
        {
            return string.Format("authorizationProvider://{0}/{1}/{2}/{3}", Namespace, topic.LocalName, tag, permission); 
        }
        private AuthorizationRuleCollection GetNamespaceScopeRules()
        {
            string key = GetKey("NamespaceScopeRules");

            if (RequestContext.Current != null)
            {
                AuthorizationRuleCollection cached = RequestContext.Current[key] as AuthorizationRuleCollection;

                if (cached != null)
                {
                    return cached; 
                }
            }

            AuthorizationRuleCollection rules = new AuthorizationRuleCollection();
            ParsedTopic parsedDefinitionTopic = Next.GetParsedTopic(
                new UnqualifiedTopicRevision(_namespaceManager.DefinitionTopicName.LocalName));
            if (parsedDefinitionTopic != null)
            {
                rules.AddRange(GetSecurityRules(parsedDefinitionTopic, AuthorizationRuleScope.Namespace));
            }

            if (RequestContext.Current != null)
            {
                RequestContext.Current[key] = rules; 
            }

            return rules;
        }
        private AuthorizationRuleCollection GetSecurityRules(ParsedTopic parsedTopic, AuthorizationRuleScope scope)
        {
            int lexicalOrder = 0;
            AuthorizationRuleCollection rules = new AuthorizationRuleCollection();

            if (parsedTopic == null)
            {
                return rules; 
            }

            foreach (TopicProperty property in parsedTopic.Properties)
            {
                SecurableAction action;
                AuthorizationRulePolarity polarity;
                if (AuthorizationRule.TryParseRuleType(property, scope, out action, out polarity))
                {
                    foreach (string propertyValue in property.AsList())
                    {
                        AuthorizationRuleWho who;
                        if (AuthorizationRuleWho.TryParse(propertyValue, out who))
                        {
                            rules.Add(new AuthorizationRule(who, polarity, scope, action, lexicalOrder++));
                        }
                    }
                }
            }

            return rules;
        }
        private AuthorizationRuleCollection GetTopicScopeRules(UnqualifiedTopicName topic)
        {
            string key = GetKey(topic, "TopicScopeRules");

            if (RequestContext.Current != null)
            {
                AuthorizationRuleCollection cached = RequestContext.Current[key] as AuthorizationRuleCollection;

                if (cached != null)
                {
                    return cached;
                }
            }

            AuthorizationRuleCollection rules = new AuthorizationRuleCollection();
            ParsedTopic parsedTopic = Next.GetParsedTopic(new UnqualifiedTopicRevision(topic));
            rules.AddRange(GetSecurityRules(parsedTopic, AuthorizationRuleScope.Topic));

            if (RequestContext.Current != null)
            {
                RequestContext.Current[key] = rules;
            }

            return rules;
        }

        private AuthorizationRuleCollection GetWikiScopeRules()
        {
            AuthorizationRuleCollection rules = new AuthorizationRuleCollection();
            int lexicalOrder = 0;
            foreach (WikiAuthorizationRule wikiRule in this.Federation.Configuration.AuthorizationRules)
            {
                AuthorizationRule rule = new AuthorizationRule(new AuthorizationRuleWho(wikiRule.WhoType, wikiRule.Who),
                    wikiRule.Polarity, AuthorizationRuleScope.Wiki, wikiRule.Action, lexicalOrder++);
                rules.Add(rule);
            }
            return rules;
        }
        private bool IsAllowed(SecurableAction action, AuthorizationRuleCollection rules)
        {
            // If this is a recursive call, then we need to allow the action - otherwise we might
            // deny permission to do things like read the definition topic, which we need in order
            // to determine namespace permissions
            if (IsRecursing)
            {
                return true; 
            }

            // If the security provider is disabled, always return true. 
            if (_namespaceManager.Parameters.Contains("Security.Disabled"))
            {
                if (_namespaceManager.Parameters["Security.Disabled"].Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true; 
                }
            }

            bool allowed = false;

            foreach (AuthorizationRule rule in rules)
            {
                if (rule.Who.IsMatch(Thread.CurrentPrincipal))
                {
                    if (rule.Polarity == AuthorizationRulePolarity.Allow)
                    {
                        if ((int)action >= (int)rule.Action)
                        {
                            allowed = true;
                        }
                    }
                    else if (rule.Polarity == AuthorizationRulePolarity.Deny)
                    {
                        if ((int)action <= (int)rule.Action)
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
    }
}
