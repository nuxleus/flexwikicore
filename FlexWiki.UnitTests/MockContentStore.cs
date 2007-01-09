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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

using FlexWiki.Collections;

namespace FlexWiki.UnitTests
{
    /// <summary>
    /// A content store whose purpose is to provide storage during unit tests.
    /// </summary>
    internal class MockContentStore : ContentProviderBase
    {
        private DateTime _created;
        private MockSetupOptions _options;
        private readonly MockTopicCollection _topics = new MockTopicCollection();

        internal MockContentStore()
            : this(MockSetupOptions.Default)
        {
        }

        internal MockContentStore(MockSetupOptions options)
            : base(null)
        {
            _options = options;
        }

        public DateTime Created
        {
            get
            {
                return _created;
            }
        }

        public override bool Exists
        {
            get
            {
                return (_options & MockSetupOptions.StoreDoesNotExist) == 0;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return ((_options & MockSetupOptions.ReadOnlyStore) != 0);
            }
        }

        private Federation Federation
        {
            get { return NamespaceManager.Federation; }
        }

        public override TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topicName, DateTime stamp)
        {
            MockTopic topic = GetTopic(topicName, ExistencePolicy.ExistingOnly);

            // If the topicName does not exist, return a null list
            if (topic == null)
            {
                return null;
            }

            TopicChangeCollection changes = new TopicChangeCollection();

            foreach (MockTopicRevision topicHistory in topic.History)
            {
                if (topicHistory.Created >= stamp)
                {
                    QualifiedTopicRevision namespaceQualifiedTopic = new QualifiedTopicRevision(topicName.LocalName, Namespace);
                    namespaceQualifiedTopic.Version = TopicRevision.NewVersionStringForUser(topicHistory.Author, topicHistory.Created);
                    changes.Insert(0, new TopicChange(namespaceQualifiedTopic, topicHistory.Created, topicHistory.Author));
                }
            }

            return changes;
        }

        public override QualifiedTopicNameCollection AllTopics()
        {
            QualifiedTopicNameCollection topics = new QualifiedTopicNameCollection();

            foreach (MockTopic topic in AllTopics(ExistencePolicy.ExistingOnly))
            {
                topics.Add(new QualifiedTopicName(topic.Name, this.Namespace));
            }

            return topics;
        }

        public override void DeleteAllTopicsAndHistory()
        {
            _topics.Clear();
        }

        public override void DeleteTopic(UnqualifiedTopicName topicName)
        {
            MockTopic topic = GetTopic(topicName, ExistencePolicy.All);

            topic.DeleteLatest();
        }

        //public IList<TopicProperty> GetTopicProperty(LocalTopicName topicName, string propertyName)
        //{
        //  string content = GetTopicContents(topicName); 

        //  if (content != null)
        //  {
        //    TopicParser parser = new TopicParser();
        //    ParsedTopic parsedTopic = parser.Parse(content);

        //    return parsedTopic.GetProperty(propertyName);
        //  }

        //  return null;
        //}

        public override void Initialize(NamespaceManager manager)
        {
            base.Initialize(manager); 
            _created = manager.Federation.TimeProvider.Now;
        }

        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            MockTopic mockTopic = GetTopic(topic, ExistencePolicy.ExistingOnly);
            if (mockTopic == null)
            {
                return false;
            }

            if (permission == TopicPermission.Edit)
            {
                return mockTopic.Latest.CanWrite; 
            }
            else if (permission == TopicPermission.Read)
            {
                return mockTopic.Latest.CanRead;
            }
            else
            {
                throw new ArgumentException("Unrecognized TopicPermission " + permission.ToString()); 
            }
        }

        public override void MakeTopicReadOnly(UnqualifiedTopicName topic)
        {
            MockTopic mockTopic = GetTopic(topic, ExistencePolicy.ExistingOnly);
            mockTopic.Latest.IsReadOnly = true; 
        }

        public override void MakeTopicWritable(UnqualifiedTopicName topic)
        {
            MockTopic mockTopic = GetTopic(topic, ExistencePolicy.ExistingOnly);
            mockTopic.Latest.IsReadOnly = false;
        }

        public override bool TopicExists(UnqualifiedTopicName name)
        {
            return GetTopic(name, ExistencePolicy.ExistingOnly) != null;
        }

        public override TextReader TextReaderForTopic(UnqualifiedTopicRevision revision)
        {
            MockTopic topic = GetTopic(revision.LocalName, ExistencePolicy.ExistingOnly);
            if (topic == null)
            {
                return null;
            }

            MockTopicRevision history = topic[revision.Version];

            if (history == null)
            {
                return null;
            }

            return new StringReader(history.Contents);
        }

        public override void WriteTopic(UnqualifiedTopicRevision revision, string content)
        {
            MockTopic topic = RetrieveOrCreateTopic(revision.LocalName);

            MockTopicRevision revisionToWrite = null;

            if (revision.Version == null)
            {
                if (topic.IsDeleted)
                {
                    revisionToWrite = new MockTopicRevision(content, "", Federation.TimeProvider.Now);
                    topic.WriteLatest(revisionToWrite);
                }
                else
                {
                    revisionToWrite = topic.Latest; 
                }
            }
            else
            {
                revisionToWrite = topic[revision.Version];

                if (revisionToWrite == null)
                {
                    VersionInfo versionInfo = TopicRevision.ParseVersion(revision.Version); 
                    revisionToWrite = new MockTopicRevision(content, versionInfo.Author, versionInfo.Timestamp);
                    topic.History.Add(revisionToWrite); 
                }

                revisionToWrite.Modified = Federation.TimeProvider.Now;
            }

            revisionToWrite.Contents = content; 
        }
        
        internal void DeleteHistory(string topicName)
        {
            MockTopic topic = GetTopic(topicName, ExistencePolicy.ExistingOnly);
            topic.History.Clear(); 
        }

        private IEnumerable<MockTopic> AllTopics(ExistencePolicy existencePolicy)
        {
            foreach (MockTopic topic in _topics)
            {
                if (existencePolicy == ExistencePolicy.All || topic.IsDeleted == false)
                {
                    yield return topic;
                }
            }
        }

        private MockTopic GetTopic(UnqualifiedTopicName topicName, ExistencePolicy existencePolicy)
        {
            return GetTopic(topicName.LocalName, existencePolicy); 
        }

        private MockTopic GetTopic(string topicName, ExistencePolicy existencePolicy)
        {
            if (!_topics.Contains(topicName))
            {
                return null;
            }

            MockTopic topic = _topics[topicName];

            if (topic.IsDeleted && existencePolicy == ExistencePolicy.ExistingOnly)
            {
                return null;
            }

            return topic;
        }

        private MockTopic RetrieveOrCreateTopic(string topicName)
        {
            MockTopic topic = GetTopic(topicName, ExistencePolicy.All);

            if (topic == null)
            {
                topic = new MockTopic(topicName);
                _topics.Add(topic);
            }
            return topic;
        }
    }
}
