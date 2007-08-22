using System;
using System.IO; 

using NUnit.Framework;

using FlexWiki.Caching;
using FlexWiki.Collections;

namespace FlexWiki.UnitTests.Caching
{
    [TestFixture]
    public class TopicCacheProviderTests
    {
        private class TestParameters
        {
            private MockCache _cache;
            private Federation _federation;
            private NamespaceManager _manager;
            private TopicCacheProvider _provider;
            private MockContentStore _store;

            public MockCache Cache
            {
                get { return _cache; }
                set { _cache = value; }
            }

            public Federation Federation
            {
                get { return _federation; }
                set { _federation = value; }
            }

            public NamespaceManager Manager
            {
                get { return _manager; }
                set { _manager = value; }
            }

            public TopicCacheProvider Provider
            {
                get { return _provider; }
                set { _provider = value; }
            }

            public MockContentStore Store
            {
                get { return _store; }
                set { _store = value; }
            }
        }

        [Test]
        public void AllChangesForTopicSince()
        {
            DoTest(delegate(TestParameters parameters)
            {
                ClearCache(parameters.Cache);
                UnqualifiedTopicName topicName = new UnqualifiedTopicName("TopicOne");

                parameters.Store.AllChangesForTopicSinceCalled = false;

                DateTime now = parameters.Federation.TimeProvider.Now;
                TopicChangeCollection firstRetrieval = parameters.Provider.AllChangesForTopicSince(
                    topicName,
                    DateTime.MinValue);
                Assert.IsTrue(parameters.Store.AllChangesForTopicSinceCalled,
                    "Checking that cache called store the first time.");

                parameters.Store.AllChangesForTopicSinceCalled = false;
                TopicChangeCollection secondRetrieval = parameters.Provider.AllChangesForTopicSince(
                    topicName,
                    DateTime.MinValue);
                Assert.IsFalse(parameters.Store.AllChangesForTopicSinceCalled,
                    "Checking that second retrieval came from cache.");

                parameters.Manager.WriteTopicAndNewVersion(topicName, "Changed content", "FlexWiki");

                parameters.Store.AllChangesForTopicSinceCalled = false;
                TopicChangeCollection thirdRetrieval = parameters.Provider.AllChangesForTopicSince(
                    topicName,
                    DateTime.MinValue);
                Assert.IsTrue(parameters.Store.AllChangesForTopicSinceCalled,
                    "Checking that third retrieval (after topic is written to) did not come from cache.");

                parameters.Store.AllChangesForTopicSinceCalled = false;
                TopicChangeCollection fourthRetrieval = parameters.Provider.AllChangesForTopicSince(
                    topicName,
                    now);
                Assert.IsFalse(parameters.Store.AllChangesForTopicSinceCalled,
                    "Checking that fourth retrieval did came from cache.");
                Assert.AreEqual(1, fourthRetrieval.Count,
                    "Checking that correct number of changes were returned.");

                ClearCache(parameters.Cache);
                parameters.Store.AllChangesForTopicSinceCalled = false;
                TopicChangeCollection fifthRetrieval = parameters.Provider.AllChangesForTopicSince(
                    topicName,
                    DateTime.MinValue);
                Assert.IsTrue(parameters.Store.AllChangesForTopicSinceCalled,
                    "Checking that the fifth retrieval (after a cache clear) did not come from cache.");

                parameters.Store.AllChangesForTopicSinceCalled = false;
                TopicChangeCollection sixthRetrieval = parameters.Provider.AllChangesForTopicSince(
                    topicName,
                    now);
                Assert.IsFalse(parameters.Store.AllChangesForTopicSinceCalled,
                    "Checking that the sixth retrieval came from cache even though the timestamp was newer.");
                Assert.AreEqual(1, sixthRetrieval.Count,
                    "Checking that the correct number of changes were returned.");

                ClearCache(parameters.Cache);
                parameters.Store.AllChangesForTopicSinceCalled = false;
                TopicChangeCollection seventhRetrieval = parameters.Provider.AllChangesForTopicSince(
                    topicName,
                    now);
                Assert.IsTrue(parameters.Store.AllChangesForTopicSinceCalled,
                    "Checking that the seventh retrieval (after cache clear) did not come from cache.");

                parameters.Store.AllChangesForTopicSinceCalled = false;
                TopicChangeCollection eigthRetrieval = parameters.Provider.AllChangesForTopicSince(
                    topicName,
                    DateTime.MinValue);
                Assert.IsTrue(parameters.Store.AllChangesForTopicSinceCalled,
                    "Checking that the eigth retrieval did not come from cache, because new enough information was not present.");


            }
            );
        }
        [Test]
        public void AllTopics()
        {
            DoTest(delegate(TestParameters parameters)
            {
                ClearCache(parameters.Cache);
                parameters.Store.AllTopicsCalled = false;
                QualifiedTopicNameCollection firstRetrieval = parameters.Provider.AllTopics();
                Assert.IsTrue(parameters.Store.AllTopicsCalled,
                    "Checking that first retrieval does not come from cache.");

                parameters.Store.AllTopicsCalled = false;
                QualifiedTopicNameCollection secondRetrieval = parameters.Provider.AllTopics();
                Assert.IsFalse(parameters.Store.AllTopicsCalled,
                    "Checking that second retrieval comes from cache.");

                parameters.Manager.WriteTopicAndNewVersion("NewTopic", "New content", "test");

                parameters.Store.AllTopicsCalled = false;
                QualifiedTopicNameCollection thirdRetrieval = parameters.Provider.AllTopics();
                Assert.IsTrue(parameters.Store.AllTopicsCalled,
                    "Checking that a retrieval after a write of a new topic does not come from cache.");

                parameters.Manager.DeleteTopic("NewTopic");

                parameters.Store.AllTopicsCalled = false;
                QualifiedTopicNameCollection fourthRetrieval = parameters.Provider.AllTopics();
                Assert.IsTrue(parameters.Store.AllTopicsCalled,
                    "Checking that a retrieval after a delete of a topic does not come from cache.");

                parameters.Manager.DeleteAllTopicsAndHistory();

                parameters.Store.AllTopicsCalled = false;
                QualifiedTopicNameCollection fifthRetrieval = parameters.Provider.AllTopics();
                Assert.IsTrue(parameters.Store.AllTopicsCalled,
                    "Checking that a retrieval after a delete of all namespace content does not come from cache.");


            });
        }
        [Test]
        public void DeleteAllTopicsAndHistory()
        {
            DoTest(delegate(TestParameters parameters)
            {
                ClearCache(parameters.Cache);
                QualifiedTopicRevision topicRevision = new QualifiedTopicRevision("TopicOne", "NamespaceOne");
                ParsedTopic firstRetrieval = parameters.Provider.GetParsedTopic(
                    topicRevision.AsUnqualifiedTopicRevision());
                AssertCacheContainsParsedTopic(parameters.Cache, firstRetrieval);
                parameters.Provider.DeleteAllTopicsAndHistory();
                AssertCacheDoesNotContainsParsedTopic(parameters.Cache, firstRetrieval);
            }
            );
        }
        [Test]
        public void DeleteTopic()
        {
            DoTest(delegate(TestParameters parameters)
            {
                ClearCache(parameters.Cache);
                QualifiedTopicRevision topicRevision = new QualifiedTopicRevision("TopicOne", "NamespaceOne");
                ParsedTopic firstRetrieval = parameters.Provider.GetParsedTopic(
                    topicRevision.AsUnqualifiedTopicRevision());
                AssertCacheContainsParsedTopic(parameters.Cache, firstRetrieval);
                parameters.Provider.DeleteTopic(
                    topicRevision.AsUnqualifiedTopicRevision().AsUnqualifiedTopicName());
                AssertCacheDoesNotContainsParsedTopic(parameters.Cache, firstRetrieval);
            }
            );
        }
        [Test]
        public void GetParsedTopic()
        {
            DoTest(delegate(TestParameters parameters)
            {
                ClearCache(parameters.Cache);
                QualifiedTopicRevision topicRevision = new QualifiedTopicRevision("TopicOne", "NamespaceOne");
                ParsedTopic firstRetrieval = parameters.Provider.GetParsedTopic(
                    topicRevision.AsUnqualifiedTopicRevision());
                AssertCacheContainsParsedTopic(parameters.Cache, firstRetrieval);
                ParsedTopic secondRetrieval = parameters.Provider.GetParsedTopic(
                    topicRevision.AsUnqualifiedTopicRevision());
                Assert.AreSame(firstRetrieval, secondRetrieval,
                    "Checking that the cached parsed topic is returned.");
                ClearCache(parameters.Cache);
                ParsedTopic thirdRetrieval = parameters.Provider.GetParsedTopic(
                    topicRevision.AsUnqualifiedTopicRevision());
                Assert.IsFalse(object.ReferenceEquals(firstRetrieval, thirdRetrieval),
                    "Checking that after the cache is cleared, a new parsed topic is retrieved.");

                // There was a bug in early versions where the TopicCacheProvider would cache
                // topics without a namespace, serving up the wrong topic at times. 
                NamespaceManager otherManager = parameters.Federation.NamespaceManagerForNamespace("NamespaceTwo");
                TopicCacheProvider otherProvider = otherManager.GetProvider(typeof(TopicCacheProvider)) as
                    TopicCacheProvider;

                ParsedTopic otherParsedTopic = otherProvider.GetParsedTopic(
                    topicRevision.AsUnqualifiedTopicRevision());

                Assert.IsFalse(object.ReferenceEquals(thirdRetrieval, otherParsedTopic),
                    "Checking that the provider does not accidentially cache topics from two namespaces under the same key");
            }
            );
        }
        [Test]
        public void HasPermission()
        {
            DoTest(delegate(TestParameters parameters)
            {
                ClearCache(parameters.Cache);
                UnqualifiedTopicName topicName = new UnqualifiedTopicName("TopicOne");

                AssertHasPermission(parameters, topicName, "Initial retrieval");

                parameters.Provider.LockTopic(topicName);
                AssertHasPermission(parameters, topicName, "After LockTopic");

                parameters.Provider.UnlockTopic(topicName);
                AssertHasPermission(parameters, topicName, "After UnlockTopic");

                parameters.Provider.WriteTopic(new UnqualifiedTopicRevision(topicName), "New content");
                AssertHasPermission(parameters, topicName, "After WriteTopic");

                parameters.Provider.DeleteTopic(topicName);
                AssertHasPermission(parameters, topicName, "After DeleteTopic");

                parameters.Provider.DeleteAllTopicsAndHistory();
                AssertHasPermission(parameters, topicName, "After DeleteAllTopicsAndHistory");

            }); 
        }
        [Test]
        public void TextReaderForTopic()
        {
            DoTest(delegate(TestParameters parameters)
            {
                UnqualifiedTopicRevision revision = new UnqualifiedTopicRevision("TopicOne");

                ClearCache(parameters.Cache);
                parameters.Store.TextReaderForTopicCalled = false;
                string firstRetrieval = parameters.Provider.TextReaderForTopic(revision).ToString();
                Assert.IsTrue(parameters.Store.TextReaderForTopicCalled,
                    "Checking that first retrieval does not come from cache.");

                parameters.Store.TextReaderForTopicCalled = false;
                string secondRetrieval = parameters.Provider.TextReaderForTopic(revision).ToString();
                Assert.IsFalse(parameters.Store.TextReaderForTopicCalled,
                    "Checking that second retrieval comes from cache.");

                parameters.Manager.WriteTopicAndNewVersion(revision.AsUnqualifiedTopicName(), "New content", "test");

                parameters.Store.TextReaderForTopicCalled = false;
                string thirdRetrieval = parameters.Provider.TextReaderForTopic(revision).ToString();
                Assert.IsTrue(parameters.Store.TextReaderForTopicCalled,
                    "Checking that a retrieval after a write of a new topic does not come from cache.");

                parameters.Manager.DeleteTopic(revision.AsUnqualifiedTopicName());

                parameters.Store.TextReaderForTopicCalled = false;
                TextReader fourthRetrieval = parameters.Provider.TextReaderForTopic(revision);
                Assert.IsTrue(parameters.Store.TextReaderForTopicCalled,
                    "Checking that a retrieval after a delete of a topic does not come from cache.");

                parameters.Manager.DeleteAllTopicsAndHistory();

                parameters.Store.TextReaderForTopicCalled = false;
                TextReader fifthRetrieval = parameters.Provider.TextReaderForTopic(revision);
                Assert.IsTrue(parameters.Store.TextReaderForTopicCalled,
                    "Checking that a retrieval after a delete of all namespace content does not come from cache.");

            });
        }
        [Test]
        public void TopicExists()
        {
            DoTest(delegate(TestParameters parameters)
            {
                ClearCache(parameters.Cache);
                UnqualifiedTopicName topicName = new UnqualifiedTopicName("TopicOne");

                parameters.Store.TopicExistsCalled = false;

                bool firstRetrieval = parameters.Provider.TopicExists(topicName);
                Assert.IsTrue(parameters.Store.TopicExistsCalled,
                    "Checking that cache called store the first time.");

                parameters.Store.TopicExistsCalled = false;
                bool secondRetrieval = parameters.Provider.TopicExists(topicName);
                Assert.IsFalse(parameters.Store.TopicExistsCalled,
                    "Checking that second retrieval came from cache.");

                parameters.Manager.WriteTopicAndNewVersion(topicName, "Changed content", "FlexWiki");

                parameters.Store.TopicExistsCalled = false;
                bool thirdRetrieval = parameters.Provider.TopicExists(topicName);
                Assert.IsTrue(parameters.Store.TopicExistsCalled,
                    "Checking that third retrieval (after topic is written to) did not come from cache.");

                parameters.Store.TopicExistsCalled = false;
                bool fourthRetrieval = parameters.Provider.TopicExists(topicName);
                Assert.IsFalse(parameters.Store.TopicExistsCalled,
                    "Checking that fourth retrieval did came from cache.");

                parameters.Manager.DeleteTopic(topicName);

                parameters.Store.TopicExistsCalled = false;
                bool fifthRetrieval = parameters.Provider.TopicExists(topicName);
                Assert.IsTrue(parameters.Store.TopicExistsCalled,
                    "Checking that fifth retrieval (after topic is deleted) did not come from cache.");

                parameters.Manager.DeleteAllTopicsAndHistory();

                parameters.Store.TopicExistsCalled = false;
                bool sixthRetrieval = parameters.Provider.TopicExists(topicName);
                Assert.IsTrue(parameters.Store.TopicExistsCalled,
                    "Checking that sixth retrieval (after all namespace content created) did not come from cache.");
            });

        }
        [Test]
        public void WriteTopic()
        {
            DoTest(delegate(TestParameters parameters)
            {
                ClearCache(parameters.Cache);
                QualifiedTopicRevision topicRevision = new QualifiedTopicRevision("TopicOne", "NamespaceOne");
                ParsedTopic firstRetrieval = parameters.Provider.GetParsedTopic(
                    topicRevision.AsUnqualifiedTopicRevision());
                parameters.Manager.WriteTopicAndNewVersion(
                    topicRevision.AsUnqualifiedTopicRevision().AsUnqualifiedTopicName(),
                    "New content", "test");
                ParsedTopic secondRetrieval = parameters.Provider.GetParsedTopic(
                    topicRevision.AsUnqualifiedTopicRevision());

                Assert.IsFalse(object.ReferenceEquals(firstRetrieval, secondRetrieval),
                    "Checking that a write flushes the cache for a parsed topic.");

                ParsedTopic thirdRetrieval = parameters.Provider.GetParsedTopic(
                    topicRevision.AsUnqualifiedTopicRevision());
                Assert.AreSame(secondRetrieval, thirdRetrieval,
                    "Checking that subsequent retrievals return from cache.");
            }
            );
        }

        private void AssertCacheContainsParsedTopic(MockCache cache, ParsedTopic parsedTopic)
        {
            Assert.IsTrue(cache.GetCacheContents().ContainsValue(parsedTopic),
                "Checking that cache contains parsed topic.");
        }
        private void AssertCacheDoesNotContainsParsedTopic(MockCache cache, ParsedTopic parsedTopic)
        {
            Assert.IsFalse(cache.GetCacheContents().ContainsValue(parsedTopic),
                "Checking that cache does not contain parsed topic.");
        }
        private void AssertCacheDoesNotContainTopicChanges(MockCache mockCache, QualifiedTopicName qualifiedTopicName)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        private void AssertHasPermission(TestParameters parameters, UnqualifiedTopicName topicName, string messageTag)
        {
            AssertHasPermissionNotFromCache(parameters, topicName, messageTag);
            AssertHasPermissionFromCache(parameters, topicName, messageTag);
        }
        private void AssertHasPermissionFromCache(
            TestParameters parameters, 
            UnqualifiedTopicName topicName, 
            string messageTag)
        {
            AssertHasPermissionFromCache(parameters, topicName, TopicPermission.Read, messageTag);
            AssertHasPermissionFromCache(parameters, topicName, TopicPermission.Edit, messageTag);
        }
        private void AssertHasPermissionFromCache(
            TestParameters parameters, 
            UnqualifiedTopicName topicName, 
            TopicPermission topicPermission, 
            string messageTag)
        {
            string messageFormat = "Checking that retrieval comes from cache: {0} {1}";
            parameters.Store.HasPermissionCalled = false;
            bool thirteenthRetrieval = parameters.Provider.HasPermission(topicName, topicPermission);
            Assert.IsFalse(parameters.Store.HasPermissionCalled,
                string.Format(messageFormat, topicPermission, messageTag));
        }
        private void AssertHasPermissionNotFromCache(
            TestParameters parameters, 
            UnqualifiedTopicName topicName, 
            string messageTag)
        {
            AssertHasPermissionNotFromCache(parameters, topicName, TopicPermission.Read, messageTag);
            AssertHasPermissionNotFromCache(parameters, topicName, TopicPermission.Edit, messageTag);
        }
        private static void AssertHasPermissionNotFromCache(
            TestParameters parameters, 
            UnqualifiedTopicName topicName,
            TopicPermission topicPermission, 
            string messageTag)
        {
            string messageFormat = "Checking that retrieval does not come from cache: {0} {1}";
            parameters.Store.HasPermissionCalled = false;
            bool thirteenthRetrieval = parameters.Provider.HasPermission(topicName, topicPermission);
            Assert.IsTrue(parameters.Store.HasPermissionCalled,
                string.Format(messageFormat, topicPermission, messageTag));
        }
        private void ClearCache(MockCache cache)
        {
            cache.GetCacheContents().Clear();
        }
        private void DoTest(Action<TestParameters> test)
        {
            TestParameters parameters = new TestParameters();
            parameters.Federation = WikiTestUtilities.SetupFederation("test://TopicCacheProviderTests",
                TestContentSets.MultipleTopicsWithProperties);
            parameters.Manager = parameters.Federation.NamespaceManagerForNamespace("NamespaceOne");
            parameters.Provider = (TopicCacheProvider)parameters.Manager.GetProvider(typeof(TopicCacheProvider));
            parameters.Store = (MockContentStore)parameters.Manager.GetProvider(typeof(MockContentStore));
            parameters.Cache = GetCache(parameters.Federation);

            test(parameters);
        }
        private MockCache GetCache(Federation federation)
        {
            return (MockCache)federation.Application.Cache;
        }
    }
}
