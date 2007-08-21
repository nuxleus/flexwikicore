using System;

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
