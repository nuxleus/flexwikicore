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

using FlexWiki.Collections;

namespace FlexWiki.Security
{
    public class TransportSecurityRequirementProvider : IContentProvider
    {
        private const string c_recursionContextKey = "TransportSecurityRequirementProviderRecursionContextKey";

        private NamespaceManager _namespaceManager;
        private IContentProvider _next;

        public TransportSecurityRequirementProvider(IContentProvider next)
        {
            _next = next;
        }

        public bool Exists
        {
            get
            {
                using (CreateRecursionContext())
                {
                    return _next.Exists;
                }
            }
        }
        public bool IsReadOnly
        {
            get
            {
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
        public static string RequirementPropertyName
        {
            get { return "RequireTransportSecurityFor"; }
        }

        private bool IsRecursing
        {
            get { return RecursionContextHelper.IsRecursing(c_recursionContextKey); }
        }
        private IWikiApplication WikiApplication
        {
            get { return _namespaceManager.Federation.Application; }
        }

        public TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            using (CreateRecursionContext())
            {
                return _next.AllChangesForTopicSince(topic, stamp);
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
            using (CreateRecursionContext())
            {
                _next.DeleteAllTopicsAndHistory();
            }
        }
        public void DeleteTopic(UnqualifiedTopicName topic)
        {
            using (CreateRecursionContext())
            {
                _next.DeleteTopic(topic);
            }
        }
        public ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            using (CreateRecursionContext())
            {
                return _next.GetParsedTopic(topicRevision);
            }
        }
        public bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            using (CreateRecursionContext())
            {
                return _next.HasPermission(topic, permission);
            }
        }
        public void Initialize(NamespaceManager namespaceManager)
        {
            _namespaceManager = namespaceManager;
            _next.Initialize(namespaceManager);
        }
        public void LockTopic(UnqualifiedTopicName topic)
        {
            using (CreateRecursionContext())
            {
                _next.LockTopic(topic);
            }
        }
        public TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            AssertTransportSecurityRequirement(TransportSecurityContext.Content);
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
            using (CreateRecursionContext())
            {
                _next.UnlockTopic(topic);
            }
        }
        public void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            using (CreateRecursionContext())
            {
                _next.WriteTopic(topicRevision, content);
            }
        }

        private void AssertTransportSecurityRequirement(TransportSecurityContext context)
        {
            if (IsRecursing)
            {
                return;
            }

            if (IsTransportSecurityRequired(context))
            {
                if (!WikiApplication.IsTransportSecure)
                {
                    throw new TransportSecurityRequirementException();
                }
            }
        }
        private IDisposable CreateRecursionContext()
        {
            return new RecursionContextHelper(c_recursionContextKey);
        }
        private bool IsTransportSecurityRequired(TransportSecurityContext context)
        {
            TransportSecurityRequiredFor requiredOn = WikiApplication.FederationConfiguration.RequireTransportSecurityFor;

            using (CreateRecursionContext())
            {
                TopicProperty property = _namespaceManager.GetTopicProperty(
                    _namespaceManager.DefinitionTopicName.LocalName,
                    RequirementPropertyName);

                if (property != null)
                {
                    if (property.HasValue)
                    {
                        TransportSecurityRequiredFor value;
                        if (TryParseValue(property.LastValue, out value))
                        {
                            requiredOn = value;
                        }
                    }
                }
            }

            if (requiredOn == TransportSecurityRequiredFor.Content)
            {
                return true; 
            }

            return false; 
        }

        private bool TryParseValue(string input, out TransportSecurityRequiredFor value)
        {
            value = TransportSecurityRequiredFor.None;
            
            try
            {
                value = (TransportSecurityRequiredFor) Enum.Parse(typeof(TransportSecurityRequiredFor), input, true);
                return true; 
            }
            catch (ArgumentException)
            {
                return false;
            }

        }


    }
}
