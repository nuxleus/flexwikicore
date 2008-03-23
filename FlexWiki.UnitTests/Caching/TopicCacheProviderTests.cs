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

using NUnit.Framework;

using FlexWiki.Caching;
using FlexWiki.Collections;

namespace FlexWiki.UnitTests.Caching
{
    [TestFixture]
    public class TopicCacheProviderTests
    {
        [Test]
        public void AllChangesForTopicSince()
        {
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
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
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
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

                parameters.Manager.DeleteTopic("NewTopic", false);

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
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
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
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
            {
                ClearCache(parameters.Cache);
                QualifiedTopicRevision topicOneRevision = new QualifiedTopicRevision("TopicOne", "NamespaceOne");
                QualifiedTopicRevision topicTwoRevision = new QualifiedTopicRevision("TopicTwo", "NamespaceOne");
                ParsedTopic firstRetrieval = parameters.Provider.GetParsedTopic(
                    topicOneRevision.AsUnqualifiedTopicRevision());
                ParsedTopic secondRetrieval = parameters.Provider.GetParsedTopic(
                    topicTwoRevision.AsUnqualifiedTopicRevision()); 
                AssertCacheContainsParsedTopic(parameters.Cache, firstRetrieval);
                AssertCacheContainsParsedTopic(parameters.Cache, secondRetrieval);
                parameters.Provider.DeleteTopic(
                    topicOneRevision.AsUnqualifiedTopicRevision().AsUnqualifiedTopicName(), false);
                AssertCacheDoesNotContainsParsedTopic(parameters.Cache, firstRetrieval);
                AssertCacheContainsParsedTopic(parameters.Cache, secondRetrieval);
            }
            );
        }
        [Test]
        public void DeleteDefinitionTopic()
        {
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
            {
                ClearCache(parameters.Cache);
                QualifiedTopicRevision topicOneRevision = new QualifiedTopicRevision("TopicOne", "NamespaceOne");
                QualifiedTopicRevision definitionTopicRevision = new QualifiedTopicRevision("_ContentBaseDefinition", "NamespaceOne");
                ParsedTopic firstRetrieval = parameters.Provider.GetParsedTopic(
                    topicOneRevision.AsUnqualifiedTopicRevision());
                ParsedTopic secondRetrieval = parameters.Provider.GetParsedTopic(
                    definitionTopicRevision.AsUnqualifiedTopicRevision());
                AssertCacheContainsParsedTopic(parameters.Cache, firstRetrieval);
                AssertCacheContainsParsedTopic(parameters.Cache, secondRetrieval);
                parameters.Provider.DeleteTopic(
                    definitionTopicRevision.AsUnqualifiedTopicRevision().AsUnqualifiedTopicName(), false);
                AssertCacheDoesNotContainsParsedTopic(parameters.Cache, firstRetrieval);
                AssertCacheDoesNotContainsParsedTopic(parameters.Cache, secondRetrieval);
            }
            );
        }
        [Test]
        public void GetParsedTopic()
        {
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
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
        public void HasNamespacePermission()
        {
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
            {
                ClearCache(parameters.Cache);
                UnqualifiedTopicName topicName = new UnqualifiedTopicName("TopicOne");

                AssertHasNamespacePermission(parameters, "Initial retrieval");

                // Writing to a non-definition topic shouldn't flush the cache for namespace permission
                parameters.Provider.WriteTopic(new UnqualifiedTopicRevision(topicName), "New content");
                AssertHasNamespacePermissionFromCache(parameters, "After WriteTopic (normal topic)");

                // But writing to the definition topic should
                parameters.Provider.WriteTopic(new UnqualifiedTopicRevision("_ContentBaseDefinition"), "New content");
                AssertHasNamespacePermission(parameters, "After WriteTopic (definition topic)");

                parameters.Provider.DeleteTopic(topicName, false);
                AssertHasNamespacePermissionFromCache(parameters, "After DeleteTopic (normal topic)");

                parameters.Provider.DeleteTopic(new UnqualifiedTopicName("_ContentBaseDefinition"), false);
                AssertHasNamespacePermission(parameters, "After DeleteTopic (normal topic)");

                parameters.Provider.DeleteAllTopicsAndHistory();
                AssertHasNamespacePermission(parameters, "After DeleteAllTopicsAndHistory");
            }); 
        }
        [Test]
        public void HasPermission()
        {
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
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

                parameters.Provider.DeleteTopic(topicName, false);
                AssertHasPermission(parameters, topicName, "After DeleteTopic");

                parameters.Provider.DeleteAllTopicsAndHistory();
                AssertHasPermission(parameters, topicName, "After DeleteAllTopicsAndHistory");

            }); 
        }
        [Test]
        public void TopicIsReadOnly()
        {
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
            {
                ClearCache(parameters.Cache);
                UnqualifiedTopicName topicName = new UnqualifiedTopicName("TopicOne");

                AssertTopicIsReadOnly(parameters, topicName, "Initial retrieval");

                parameters.Provider.LockTopic(topicName);
                AssertTopicIsReadOnly(parameters, topicName, "After LockTopic");

                parameters.Provider.UnlockTopic(topicName);
                AssertTopicIsReadOnly(parameters, topicName, "After UnlockTopic");
            });
        }
        [Test]
        public void TextReaderForTopic()
        {
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
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

                parameters.Manager.DeleteTopic(revision.AsUnqualifiedTopicName(), false);

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
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
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

                parameters.Manager.DeleteTopic(topicName, false);

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
            DoTest(delegate(TestParameters<TopicCacheProvider> parameters)
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
        private void AssertHasNamespacePermission(TestParameters<TopicCacheProvider> parameters, string messageTag)
        {
            AssertHasNamespacePermissionNotFromCache(parameters, messageTag);
            AssertHasNamespacePermissionFromCache(parameters, messageTag); 
        }
        private void AssertHasNamespacePermissionNotFromCache(TestParameters<TopicCacheProvider> parameters, string messageTag)
        {
            string messageFormat = "Checking that retrieval does not come from cache: {0}";
            parameters.Store.HasNamespacePermissionCalled = false;
            bool hasPermission = parameters.Provider.HasNamespacePermission(NamespacePermission.Manage);
            Assert.IsTrue(parameters.Store.HasNamespacePermissionCalled,
                string.Format(messageFormat, messageTag));
        }

        private void AssertHasNamespacePermissionFromCache(TestParameters<TopicCacheProvider> parameters, string messageTag)
        {
            string messageFormat = "Checking that retrieval comes from cache: {0}";
            parameters.Store.HasNamespacePermissionCalled = false;
            bool hasPermission = parameters.Provider.HasNamespacePermission(NamespacePermission.Manage);
            Assert.IsFalse(parameters.Store.HasNamespacePermissionCalled,
                string.Format(messageFormat, messageTag));
        }
        private void AssertHasPermission(TestParameters<TopicCacheProvider> parameters, 
            UnqualifiedTopicName topicName, string messageTag)
        {
            AssertHasPermissionNotFromCache(parameters, topicName, messageTag);
            AssertHasPermissionFromCache(parameters, topicName, messageTag);
        }
        private void AssertHasPermissionFromCache(
            TestParameters<TopicCacheProvider> parameters, 
            UnqualifiedTopicName topicName, 
            string messageTag)
        {
            AssertHasPermissionFromCache(parameters, topicName, TopicPermission.Read, messageTag);
            AssertHasPermissionFromCache(parameters, topicName, TopicPermission.Edit, messageTag);
        }
        private void AssertHasPermissionFromCache(
            TestParameters<TopicCacheProvider> parameters, 
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
            TestParameters<TopicCacheProvider> parameters, 
            UnqualifiedTopicName topicName, 
            string messageTag)
        {
            AssertHasPermissionNotFromCache(parameters, topicName, TopicPermission.Read, messageTag);
            AssertHasPermissionNotFromCache(parameters, topicName, TopicPermission.Edit, messageTag);
        }
        private static void AssertHasPermissionNotFromCache(
            TestParameters<TopicCacheProvider> parameters, 
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

        private void AssertTopicIsReadOnly(TestParameters<TopicCacheProvider> parameters, UnqualifiedTopicName topicName, string messageTag)
        {
            AssertTopicIsReadOnlyNotFromCache(parameters, topicName, messageTag);
            AssertTopicIsReadOnlyFromCache(parameters, topicName, messageTag);
        }
        private void AssertTopicIsReadOnlyFromCache(
            TestParameters<TopicCacheProvider> parameters,
            UnqualifiedTopicName topicName,
            string messageTag)
        {
            string messageFormat = "Checking that retrieval comes from cache: {0}";
            parameters.Store.TopicIsReadOnlyCalled = false;
            bool fourteenthRetrieval = parameters.Provider.TopicIsReadOnly(topicName);
            Assert.IsFalse(parameters.Store.TopicIsReadOnlyCalled,
                string.Format(messageFormat, messageTag));
        }
        private static void AssertTopicIsReadOnlyNotFromCache(
            TestParameters<TopicCacheProvider> parameters,
            UnqualifiedTopicName topicName,
            string messageTag)
        {
            string messageFormat = "Checking that retrieval does not come from cache: {0}";
            parameters.Store.TopicIsReadOnlyCalled = false;
            bool fourteenthRetrieval = parameters.Provider.TopicIsReadOnly(topicName);
            Assert.IsTrue(parameters.Store.TopicIsReadOnlyCalled,
                string.Format(messageFormat, messageTag));
        }

        private void ClearCache(MockCache cache)
        {
            cache.GetCacheContents().Clear();
        }
        private void DoTest(Action<TestParameters<TopicCacheProvider>> test)
        {
            WikiTestUtilities.DoProviderTest(
                TestContentSets.MultipleTopicsWithProperties,
                "test://TopicCacheProviderTests/", 
                test);
        }
    }
}
