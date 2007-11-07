using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using FlexWiki.Caching; 

namespace FlexWiki.UnitTests.Caching
{
    [TestFixture]
    public class DependencyRecorderTests
    {
        [Test]
        public void AllChangesForTopicSince()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("NamespaceOne.TopicOne"); 

            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.AllChangesForTopicSince(new UnqualifiedTopicName(topicName.LocalName), DateTime.MinValue);
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new TopicContentsDependency(topicName)),
                    "Making sure that dependency on correct topic was recorded");
            });
        }

        [Test]
        public void AllTopics()
        {
            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.AllTopics();
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new TopicListDependency("NamespaceOne")), 
                    "Making sure that dependency on topic list was returned."); 
            });             
        }

        [Test]
        public void DeleteAllTopicsAndHistory()
        {
            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.DeleteAllTopicsAndHistory();
                Assert.AreEqual(0, RequestContext.Current.Dependencies.Count,
                    "Making sure that namespace deletion results in no dependencies.");
            });
        }

        [Test]
        public void DeleteTopic()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("NamespaceOne.TopicOne");

            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.DeleteTopic(new UnqualifiedTopicName(topicName.LocalName));
                Assert.AreEqual(0, RequestContext.Current.Dependencies.Count,
                    "Making sure that topic deletion results in no dependencies.");
            });
        }

        [Test]
        public void Exists()
        {
            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                bool exists = parameters.Provider.Exists;
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new NamespaceExistenceDependency("NamespaceOne")),
                    "Making sure that namespace existence query results a namespace existence dependencies.");
            });
        }

        [Test]
        public void GetParsedTopic()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("NamespaceOne.TopicOne");

            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.GetParsedTopic(new UnqualifiedTopicRevision(topicName.LocalName));
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new TopicContentsDependency(topicName)),
                    "Making sure that dependency on correct topic was recorded.");
            });
        }

        [Test]
        public void HasNamespacePermission()
        {
            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.HasNamespacePermission(NamespacePermission.Manage); 
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(
                    new NamespacePermissionsDependency("NamespaceOne"))); 
            }); 
        }

        [Test]
        public void HasPermission()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("NamespaceOne.TopicOne");

            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.HasPermission(new UnqualifiedTopicName(topicName.LocalName), TopicPermission.Edit);
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new TopicPermissionsDependency(topicName)),
                    "Making sure that dependency on correct topic was recorded.");
            });
        }

        [Test]
        public void IsReadOnly()
        {
            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                bool isReadOnly = parameters.Provider.IsReadOnly;
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new NamespacePropertiesDependency("NamespaceOne")),
                    "Making sure that namespace existence query results a namespace existence dependencies.");
            });
        }

        [Test]
        public void LockTopic()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("NamespaceOne.TopicOne");

            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.LockTopic(new UnqualifiedTopicName(topicName.LocalName));
                Assert.AreEqual(0, RequestContext.Current.Dependencies.Count,
                    "Making sure that topic locking results in no dependencies.");
            });
        }

        [Test]
        public void TextReaderForTopic()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("NamespaceOne.TopicOne");

            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.TextReaderForTopic(new UnqualifiedTopicRevision(topicName.LocalName));
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new TopicContentsDependency(topicName)),
                    "Making sure that dependency on correct topic was recorded.");
            });
        }

        [Test]
        public void TopicExists()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("NamespaceOne.TopicOne");

            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.TopicExists(new UnqualifiedTopicName(topicName.LocalName));
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new TopicExistenceDependency(topicName)),
                    "Making sure that dependency on correct topic was recorded.");
            });
        }

        [Test]
        public void TopicIsReadOnly()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("NamespaceOne.TopicOne");

            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.TopicIsReadOnly(new UnqualifiedTopicName(topicName.LocalName));
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new TopicPermissionsDependency(topicName)),
                    "Making that that dependency on topic permissions was recorded.");
            }); 
        }

        [Test]
        public void UnlockTopic()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("NamespaceOne.TopicOne");

            DoTest(delegate(TestParameters<DependencyRecorder> parameters)
            {
                parameters.Provider.UnlockTopic(new UnqualifiedTopicName(topicName.LocalName));
                Assert.AreEqual(0, RequestContext.Current.Dependencies.Count,
                    "Making sure that topic unlocking results in no dependencies.");
            });
        }

        private void DoTest(Action<TestParameters<DependencyRecorder>> test)
        {
            WikiTestUtilities.DoProviderTest(
                TestContentSets.MultipleVersions, 
                "test://DependencyRecorderTests/", 
                test); 
        }
    }
}
