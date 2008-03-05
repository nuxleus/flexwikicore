using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Collections;
using System.IO;

namespace FlexWiki.Caching
{
    public class ModificationRecorder : RecorderBase
    {
        // NamespaceExistenceDependency, NamespacePropertiesDependency, TopicContentsDependency, 
        // TopicListDependency, NamespacePermissionsDependency, TopicPermissionsDependency, 
        // TopicExistenceDependency

        public ModificationRecorder(IContentProvider next)
            : base(next)
        {
        }

        public override bool Exists
        {
            get { return Next.Exists; }
        }
        public override bool IsReadOnly
        {
            get { return Next.IsReadOnly; }
        }

        private IWikiApplication Application
        {
            get { return NamespaceManager.Federation.Application; }
        }

        public override TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            return Next.AllChangesForTopicSince(topic, stamp); 
        }
        public override QualifiedTopicNameCollection AllTopics()
        {
            return Next.AllTopics(); 
        }
        public override void DeleteAllTopicsAndHistory()
        {
            RecordModification(new NamespaceContentsDeletedModification(Namespace)); 
            Next.DeleteAllTopicsAndHistory(); 
        }
        public override void DeleteTopic(UnqualifiedTopicName topic)
        {
            RecordModification(new TopicDeletedModification(topic.ResolveRelativeTo(Namespace))); 
            Next.DeleteTopic(topic); 
        }
        public override ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            return Next.GetParsedTopic(topicRevision); 
        }
        public override bool HasNamespacePermission(NamespacePermission permission)
        {
            return Next.HasNamespacePermission(permission); 
        }
        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            return Next.HasPermission(topic, permission); 
        }
        public override void LockTopic(UnqualifiedTopicName topic)
        {
            RecordModification(new TopicPermissionsModification(topic.ResolveRelativeTo(Namespace))); 
            Next.LockTopic(topic); 
        }
        public override TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            return Next.TextReaderForTopic(topicRevision); 
        }
        public override bool TopicExists(UnqualifiedTopicName name)
        {
            return Next.TopicExists(name); 
        }
        public override bool TopicIsReadOnly(UnqualifiedTopicName name)
        {
            return Next.TopicIsReadOnly(name); 
        }
        public override void UnlockTopic(UnqualifiedTopicName topic)
        {
            RecordModification(new TopicPermissionsModification(topic.ResolveRelativeTo(Namespace))); 
            Next.UnlockTopic(topic); 
        }
        public override void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            if (topicRevision.AsUnqualifiedTopicName().Equals(new UnqualifiedTopicName(NamespaceManager.DefinitionTopicLocalName)))
            {
                RecordModification(new NamespacePropertiesModification(Namespace));
            }
            RecordModification(new TopicContentsModification(topicRevision.AsUnqualifiedTopicName().ResolveRelativeTo(Namespace))); 
            Next.WriteTopic(topicRevision, content); 
        }

        private void RecordModification(Modification modification)
        {
            Application.NoteModification(modification); 
        }

    }
}
