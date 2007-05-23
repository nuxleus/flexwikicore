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

using NUnit.Framework; 

using FlexWiki.Collections; 


namespace FlexWiki.UnitTests
{
    /// <summary>
    /// Summary description for WikiTestUtilities.
    /// </summary>
    internal static class WikiTestUtilities
    {
        internal static void AssertTopicsCorrectUnordered(QualifiedTopicNameCollection actualTopics,
            params TopicName[] expectedTopics)
        {
            Assert.AreEqual(expectedTopics.Length, actualTopics.Count,
                "Checking that the right number of topics were returned.");

            for (int i = 0; i < expectedTopics.Length; i++)
            {
                TopicName expectedTopic = expectedTopics[i];
                string message = string.Format("Checking that topic {0} was present.", expectedTopic.DottedName);

                bool found = false;
                foreach (TopicName actualTopic in actualTopics)
                {
                    if (expectedTopic.DottedName == actualTopic.DottedName)
                    {
                        found = true;
                        break;
                    }
                }

                Assert.IsTrue(found, message);
            }
        }

        internal static void AssertReferencesCorrect(QualifiedTopicRevisionCollection actualTopics,
            params QualifiedTopicRevision[] expectedTopics)
        {
            Assert.AreEqual(expectedTopics.Length, actualTopics.Count, "Checking that the correct number of topics were returned.");

            for (int i = 0; i < actualTopics.Count; i++)
            {
                Assert.AreEqual(expectedTopics[i], actualTopics[i],
                  string.Format("Checking that topic {0} was correct", i));
            }
        }
        
        internal static void AssertTopicChangeCorrect(TopicChange actualChange, string expectedName,
          string expectedAuthor, DateTime expectedTimestamp, string message)
        {
            Assert.AreEqual(expectedName, actualChange.TopicRevision.LocalName, "Checking that name was correct for " + message);
            Assert.AreEqual(expectedAuthor, actualChange.Author, "Checking that author was correct for " + message);
            Assert.AreEqual(expectedTimestamp, actualChange.Created, "Checking that timestamp was correct for " + message);
        }
        
        internal static void AssertTopicPropertyCorrect(TopicProperty topicProperty, string propertyName,
            params string[] values)
        {
            Assert.AreEqual(propertyName, topicProperty.Name, "Checking that topic property name was correct.");
            Assert.AreEqual(values.Length, topicProperty.Values.Count,
                "Checking that the correct number of values are present.");

            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(values[i], topicProperty.Values[i].RawValue,
                    string.Format("Checking that value {0} is correct.", i));
            }
        }
        
        internal static void AssertTopicsCorrectOrdered(QualifiedTopicNameCollection actualTopics,
            params TopicName[] expectedTopics)
        {
            Assert.AreEqual(expectedTopics.Length, actualTopics.Count,
                "Checking that the right number of topics were returned.");

            for (int i = 0; i < expectedTopics.Length; i++)
            {
                string message = string.Format("Checking that topic {0} was present.", expectedTopics[i]);
                Assert.AreEqual(expectedTopics[i].DottedName, actualTopics[i].DottedName, message);
            }
        }
        
        internal static void AssertTopicsCorrectUnordered(ArrayList actualTopics, params TopicName[] expectedTopics)
        {
            QualifiedTopicNameCollection topics = new QualifiedTopicNameCollection();

            foreach (TopicVersionInfo actualTopic in actualTopics)
            {
                QualifiedTopicName topic = new QualifiedTopicName(actualTopic.ExposedFullname);
                topics.Add(topic);
            }

            AssertTopicsCorrectUnordered(topics, expectedTopics);
        }
        
        internal static string AuthorForLastChange(NamespaceManager manager, string topic)
        {
            TopicChangeCollection changes = manager.AllChangesForTopic(topic);
            return changes.Latest.Author;
        }
        
        internal static NamespaceManager CreateMockStore(Federation federation, string ns)
        {
            return CreateMockStore(federation, ns, MockSetupOptions.Default); 
        }

        internal static NamespaceManager CreateMockStore(Federation federation, string ns, MockSetupOptions options)
        {
            return CreateMockStore(federation, ns, options, null); 
        }

        internal static NamespaceManager CreateMockStore(Federation federation, string ns,
            NamespaceProviderParameterCollection parameters)
        {
            return CreateMockStore(federation, ns, MockSetupOptions.Default, parameters); 
        }

        internal static NamespaceManager CreateMockStore(Federation federation, string ns, 
            MockSetupOptions options, NamespaceProviderParameterCollection parameters)
        {
            MockContentStore store = new MockContentStore(options);
            return federation.RegisterNamespace(store, ns, parameters);
        }

        internal static Federation SetupFederation(string siteUrl, TestContentSet content)
        {
            return SetupFederation(siteUrl, content, MockSetupOptions.Default);
        }

        internal static Federation SetupFederation(string siteUrl, TestContentSet content,
            FederationConfiguration federationConfiguration)
        {
            return SetupFederation(siteUrl, content, MockSetupOptions.Default, federationConfiguration);
        }

        internal static Federation SetupFederation(string siteUrl, TestContentSet content, MockSetupOptions options)
        {
            return SetupFederation(siteUrl, content, options, new FederationConfiguration());
        }

        internal static Federation SetupFederation(string siteUrl, TestContentSet content, MockSetupOptions options,
            FederationConfiguration federationConfiguration)
        {
            LinkMaker linkMaker = new LinkMaker(siteUrl);
            MockWikiApplication application = new MockWikiApplication(
                federationConfiguration,
                linkMaker,
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            Federation federation = new Federation(application);

            foreach (TestNamespace ns in content.Namespaces)
            {
                NamespaceManager storeManager = CreateMockStore(federation, ns.Name, options, ns.Parameters);

                foreach (TestTopic topic in ns.Topics)
                {
                    WriteTestTopicAndNewVersion(storeManager, topic.Name, topic.Content, topic.Author);
                }
            }

            return federation;

        }

        internal static QualifiedTopicRevision WriteTestTopicAndNewVersion(NamespaceManager namespaceManager,
          string localName, string content, string author)
        {
            QualifiedTopicRevision name = new QualifiedTopicRevision(localName, namespaceManager.Namespace);
            name.Version = QualifiedTopicRevision.NewVersionStringForUser(author, 
                namespaceManager.Federation.TimeProvider.Now);
            namespaceManager.WriteTopicAndNewVersion(name.LocalName, content, author);
            return name;
        }



    }
}