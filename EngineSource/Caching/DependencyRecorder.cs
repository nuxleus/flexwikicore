using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Collections;
using System.IO;

namespace FlexWiki.Caching
{
    public class DependencyRecorder : RecorderBase
    {
        public DependencyRecorder(IContentProvider next)
            : base(next)
        {
        }

        public override bool Exists
        {
            get 
            {
                AddDependency(new NamespaceExistenceDependency(Namespace)); 
                return Next.Exists; 
            }
        }

        public override bool IsReadOnly
        {
            get 
            {
                AddDependency(new NamespacePropertiesDependency(Namespace));
                return Next.IsReadOnly; 
            }
        }

        public override TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            AddDependency(new TopicContentsDependency(topic.ResolveRelativeTo(Namespace))); 
            return Next.AllChangesForTopicSince(topic, stamp); 
        }
        public override QualifiedTopicNameCollection AllTopics()
        {
            AddDependency(new TopicListDependency(Namespace));
            return Next.AllTopics(); 
        }
        public override void DeleteAllTopicsAndHistory()
        {
            Next.DeleteAllTopicsAndHistory(); 
        }
        public override void DeleteTopic(UnqualifiedTopicName topic, bool removeHistory)
        {
            Next.DeleteTopic(topic, removeHistory); 
        }
        public override ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            AddDependency(new TopicContentsDependency(topicRevision.ResolveRelativeTo(Namespace).AsQualifiedTopicName())); 
            return Next.GetParsedTopic(topicRevision); 
        }
        public override bool HasNamespacePermission(NamespacePermission permission)
        {
            AddDependency(new NamespacePermissionsDependency(Namespace));
            return Next.HasNamespacePermission(permission); 
        }
        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            AddDependency(new TopicPermissionsDependency(topic.ResolveRelativeTo(Namespace)));
            return Next.HasPermission(topic, permission); 
        }
        public override void LockTopic(UnqualifiedTopicName topic)
        {
            Next.LockTopic(topic); 
        }
        public override TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            AddDependency(new TopicContentsDependency(topicRevision.ResolveRelativeTo(Namespace).AsQualifiedTopicName()));
            return Next.TextReaderForTopic(topicRevision); 
        }
        public override bool TopicExists(UnqualifiedTopicName name)
        {
            AddDependency(new TopicExistenceDependency(name.ResolveRelativeTo(Namespace))); 
            return Next.TopicExists(name); 
        }
        public override bool TopicIsReadOnly(UnqualifiedTopicName name)
        {
            AddDependency(new TopicPermissionsDependency(name.ResolveRelativeTo(Namespace)));
            return Next.TopicIsReadOnly(name); 
        }
        public override void UnlockTopic(UnqualifiedTopicName topic)
        {
            Next.UnlockTopic(topic); 
        }
        public override void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            Next.WriteTopic(topicRevision, content); 
        }

        private void AddDependency(Dependency dependency)
        {
            if (RequestContext.Current != null)
            {
                RequestContext.Current.Dependencies.Add(dependency);
            }
        }
    }
}
