using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Collections;
using System.IO;

namespace FlexWiki.Caching
{
    public class ModificationRecorder : RecorderBase
    {
        public ModificationRecorder(IContentProvider next)
            : base(next)
        {
        }

        public override bool Exists
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
        public override bool IsReadOnly
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override QualifiedTopicNameCollection AllTopics()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void DeleteAllTopicsAndHistory()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void DeleteTopic(UnqualifiedTopicName topic)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override bool HasNamespacePermission(NamespacePermission permission)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void LockTopic(UnqualifiedTopicName topic)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override bool TopicExists(UnqualifiedTopicName name)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override bool TopicIsReadOnly(UnqualifiedTopicName name)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void UnlockTopic(UnqualifiedTopicName topic)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            throw new Exception("The method or operation is not implemented.");
        }

    }
}
