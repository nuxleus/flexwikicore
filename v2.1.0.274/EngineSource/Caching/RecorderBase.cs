using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Collections;
using System.IO;

namespace FlexWiki.Caching
{
    // It's important that we derive from IContentProvider and not ContentProviderBase, 
    // as we don't want a new method on IContentProvider to be implemented for us - 
    // that could lead to problems with the cache not getting updated properly. By
    // the same token, unless there's a *really* good reason, all operations on this
    // class should be abstract. 
    public abstract class RecorderBase : IContentProvider
    {
        private NamespaceManager _namespaceManager;
        private IContentProvider _next;

        public RecorderBase(IContentProvider next)
        {
            _next = next; 
        }

        public abstract bool Exists
        {
            get;
        }
        public abstract bool IsReadOnly
        {
            get;
        }
        public IContentProvider Next
        {
            get { return _next; }
            set { _next = value; }
        }

        protected string Namespace
        {
            get { return _namespaceManager.Namespace; }
        }

        protected NamespaceManager NamespaceManager
        {
            get { return _namespaceManager; }
        }

        public abstract TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp);
        public abstract QualifiedTopicNameCollection AllTopics(); 
        public abstract void DeleteAllTopicsAndHistory();
        public abstract void DeleteTopic(UnqualifiedTopicName topic, bool removeHistory);
        public abstract ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision);
        public abstract bool HasNamespacePermission(NamespacePermission permission);
        public abstract bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission);
        public virtual void Initialize(NamespaceManager namespaceManager)
        {
            _namespaceManager = namespaceManager;
            _next.Initialize(namespaceManager); 
        }
        public abstract void LockTopic(UnqualifiedTopicName topic);
        public abstract TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision); 
        public abstract bool TopicExists(UnqualifiedTopicName name);
        public abstract bool TopicIsReadOnly(UnqualifiedTopicName name);
        public abstract void UnlockTopic(UnqualifiedTopicName topic); 
        public abstract void WriteTopic(UnqualifiedTopicRevision topicRevision, string content);
    }
}
