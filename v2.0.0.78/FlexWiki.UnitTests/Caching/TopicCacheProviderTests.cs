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
        public void GetParsedTopic()
        {
            DoTest(delegate(TestParameters parameters)
            {
                ClearCache(parameters.Cache);
                QualifiedTopicRevision topicRevision = new QualifiedTopicRevision("TopicOne", "NamespaceOne");
                ParsedTopic firstRetrieval = parameters.Provider.GetParsedTopic(topicRevision.AsUnqualifiedTopicRevision());
                AssertCacheContainsParsedTopic(parameters.Cache, firstRetrieval);
                ParsedTopic secondRetrieval = parameters.Provider.GetParsedTopic(topicRevision.AsUnqualifiedTopicRevision());
                Assert.AreSame(firstRetrieval, secondRetrieval,
                    "Checking that the cached parsed topic is returned.");
                ClearCache(parameters.Cache);
                ParsedTopic thirdRetrieval = parameters.Provider.GetParsedTopic(topicRevision.AsUnqualifiedTopicRevision());
                Assert.IsFalse(object.ReferenceEquals(firstRetrieval, thirdRetrieval),
                    "Checking that after the cache is cleared, a new parsed topic is retrieved.");

            //    Assert.Fail("Need to add test that ensures topics from other namespaces are not pulled up accidentally."); 
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
                ParsedTopic firstRetrieval = parameters.Provider.GetParsedTopic(topicRevision.AsUnqualifiedTopicRevision());
                parameters.Manager.WriteTopicAndNewVersion(topicRevision.AsUnqualifiedTopicRevision().AsUnqualifiedTopicName(),
                    "New content", "test");
                ParsedTopic secondRetrieval = parameters.Provider.GetParsedTopic(topicRevision.AsUnqualifiedTopicRevision());

                Assert.IsFalse(object.ReferenceEquals(firstRetrieval, secondRetrieval),
                    "Checking that a write flushes the cache for a parsed topic.");

                ParsedTopic thirdRetrieval = parameters.Provider.GetParsedTopic(topicRevision.AsUnqualifiedTopicRevision());
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
