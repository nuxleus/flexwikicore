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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using FlexWiki.Collections; 
using FlexWiki.Formatting;
using FlexWiki.Security; 

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class FederationTests
    {
        [Test]
        public void AllQualifiedTopicNamesThatExist()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.ImportingReferencingSet);

            QualifiedTopicNameCollection topics =
                federation.AllQualifiedTopicNamesThatExist(new TopicName("NoSuchTopic"), "NamespaceOne");

            Assert.AreEqual(0, topics.Count, "Ensuring that nonexistent topics return no hits.");

            // Check that an explicit reference doesn't pull in any imports
            topics =
                federation.AllQualifiedTopicNamesThatExist(new TopicName("NamespaceOne.ReferencedTopic"), "NamespaceOne");

            WikiTestUtilities.AssertTopicsCorrectUnordered(topics,
                new TopicName("NamespaceOne.ReferencedTopic")); 

            // Check that a local name in both namespaces pulls in imports
            topics =
                federation.AllQualifiedTopicNamesThatExist(new TopicName("ReferencedTopic"), "NamespaceOne");
            WikiTestUtilities.AssertTopicsCorrectUnordered(topics,
                new TopicName("NamespaceOne.ReferencedTopic"),
                new TopicName("NamespaceTwo.ReferencedTopic")); 

            // Check that a local name in the other namespace pulls in imports
            topics =
                federation.AllQualifiedTopicNamesThatExist(new TopicName("OtherTopic"), "NamespaceOne");
            WikiTestUtilities.AssertTopicsCorrectUnordered(topics,
                new TopicName("NamespaceTwo.OtherTopic")); 

            // Check that a local name in a nonimporting namespace doesn't pull in imports
            topics =
                federation.AllQualifiedTopicNamesThatExist(new TopicName("ReferencedTopic"), "NamespaceTwo");
            WikiTestUtilities.AssertTopicsCorrectUnordered(topics,
                new TopicName("NamespaceTwo.ReferencedTopic")); 

            // Check that a local name resolved relative to a nonexistent namespace returns nothing
            topics =
                federation.AllQualifiedTopicNamesThatExist(new TopicName("ReferencedTopic"), "NoSuchNamespace");
            Assert.AreEqual(0, topics.Count, 
                "Checking that a local name resolved relative to a nonexistent namespace returns nothing.");

            // Check that a plural form resolves to the singular form
            topics =
                federation.AllQualifiedTopicNamesThatExist(
                    new TopicName("OtherTopics"), 
                    "NamespaceOne", 
                    AlternatesPolicy.IncludeAlternates);
            WikiTestUtilities.AssertTopicsCorrectUnordered(topics,
                new TopicName("NamespaceTwo.OtherTopic")); 

            // Check that the singular form also resolves correctly
            topics =
                federation.AllQualifiedTopicNamesThatExist(
                    new TopicName("OtherTopic"),
                    "NamespaceOne",
                    AlternatesPolicy.IncludeAlternates);
            WikiTestUtilities.AssertTopicsCorrectUnordered(topics,
                new TopicName("NamespaceTwo.OtherTopic")); 

            // Check that a local name that resolves to more than one qualified name
            // works given the plural form
            topics =
                federation.AllQualifiedTopicNamesThatExist(
                    new TopicName("ReferencedTopics"),
                    "NamespaceOne",
                    AlternatesPolicy.IncludeAlternates);
            WikiTestUtilities.AssertTopicsCorrectUnordered(topics,
                new TopicName("NamespaceOne.ReferencedTopic"), 
                new TopicName("NamespaceTwo.ReferencedTopic")); 

            // Check that an absolute plural name resolves only in the specified namespace
            topics =
                federation.AllQualifiedTopicNamesThatExist(
                    new TopicName("NamespaceTwo.ReferencedTopics"),
                    "NamespaceOne",
                    AlternatesPolicy.IncludeAlternates);
            WikiTestUtilities.AssertTopicsCorrectUnordered(topics,
                new TopicName("NamespaceTwo.ReferencedTopic")); 


        }
        [Test]
        public void GetTopicChanges()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            TopicChangeCollection actualChanges = federation.GetTopicChanges(new TopicName("NamespaceOne.TopicOne"));

            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");
            TopicChangeCollection expectedChanges = manager.AllChangesForTopic("TopicOne");

            AssertChangesAreEqual(expectedChanges, actualChanges); 
        }
        [Test]
        public void GetTopicChangesNonexistentNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            Assert.IsNull(federation.GetTopicChanges(new TopicName("NoSuchNamespace.TopicOne")));
        }
        [Test]
        public void GetTopicChangesNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            Assert.IsNull(federation.GetTopicChanges(new TopicName("NamespaceOne.NoSuchTopic"))); 
        }
        [Test]
        public void GetTopicCreationTime()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.SingleTopicNoImports);

            Assert.AreEqual(new DateTime(2004, 10, 28, 14, 11, 02),
                federation.GetTopicCreationTime(new QualifiedTopicName("NamespaceOne.TopicOne")),
                "Checking that creation time is correct."); 
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "Could not find the namespace NoSuchNamespace")]
        public void GetTopicCreationTimeNonexistentNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.SingleTopicNoImports);

            DateTime creationTime = federation.GetTopicCreationTime(new QualifiedTopicName("NoSuchNamespace.TopicOne"));
        }
        [Test]
        public void GetTopicLastModificationTimeLatestVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            DateTime modificationTime = federation.GetTopicLastModificationTime(
                new TopicName("TopicOne", "NamespaceOne"));

            DateTime expectedModificationTime = new DateTime(2004, 10, 28, 14, 11, 09);
            Assert.AreEqual(expectedModificationTime, modificationTime, "Checking that modification time was correct.");
        }
        [Test]
        [ExpectedException(typeof(FlexWikiException), "The namespace NamespaceTwo does not exist.")]
        public void GetTopicLastModificationTimeNonExistentNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            DateTime modificationTime = federation.GetTopicLastModificationTime(
                new TopicName("NoSuchTopic", "NamespaceTwo"));
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "Could not locate a topic named NamespaceOne.NoSuchTopic")]
        public void GetTopicLastModificationTimeNonExistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            DateTime modificationTime = federation.GetTopicLastModificationTime(
                new TopicName("NoSuchTopic", "NamespaceOne"));
        }
        [Test]
        [ExpectedException(typeof(FlexWikiException))]
        public void GetTopicLastModificationTimeRelativeName()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            DateTime modificationTime = federation.GetTopicLastModificationTime(
                new TopicName("TopicOne"));

        }
        [Test]
        public void GetTopicModificationTime()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            TopicChangeCollection changes = 
                federation.NamespaceManagerForNamespace("NamespaceOne").AllChangesForTopic("TopicOne"); 

            QualifiedTopicRevision middleRevision = changes[1].TopicRevision;

            DateTime actualModificationTime = federation.GetTopicModificationTime(middleRevision);
            DateTime expectedModificationTime = changes[1].Modified;

            Assert.AreEqual(expectedModificationTime, actualModificationTime,
                "Checking that Federation.GetTopicModificationTime returns the right modification time."); 
        }
        [Test]
        public void GetTopicModificationTimeNonexistentNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            Assert.AreEqual(DateTime.MinValue, 
                federation.GetTopicModificationTime(new TopicName("NoSuchNamespace.TopicOne")), 
                "Checking that DateTime.MinValue is returned when a topic in a nonexistent namespace is requested."); 
        }
        [Test]
        public void GetTopicModificationTimeNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            Assert.AreEqual(DateTime.MinValue, 
                federation.GetTopicModificationTime(new TopicName("NamespaceOne.NoSuchTopic")), 
                "Checking that DateTime.MinValue is returned when a topic in a nonexistent topic is requested."); 
        }
        [Test]
        public void GetTopicModificationTimeNullVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleVersions);

            TopicChangeCollection changes =
                federation.NamespaceManagerForNamespace("NamespaceOne").AllChangesForTopic("TopicOne");

            DateTime actualModificationTime = federation.GetTopicModificationTime(new TopicName("NamespaceOne.TopicOne"));
            DateTime expectedModificationTime = changes.Latest.Modified;

            Assert.AreEqual(expectedModificationTime, actualModificationTime,
                "Checking that a null version corresponds to the latest modification.");
        }
        [Test]
        public void GetTopicProperty()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleTopicsWithProperties);

            TopicProperty property = federation.GetTopicProperty(new TopicName("TopicOne", "NamespaceOne"),
                "PropertyOne");

            Assert.AreEqual(1, property.Values.Count, "Checking that the property has one value.");
            Assert.AreEqual("Value one", property.LastValue, "Checking that the value is correct."); 
        }
        [Test]
        [ExpectedException(typeof(FlexWikiException), "The namespace NoSuchNamespace does not exist.")]
        public void GetTopicPropertyNonExistentNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleTopicsWithProperties);

            TopicProperty property = federation.GetTopicProperty(new TopicName("TopicOne", "NoSuchNamespace"),
                "PropertyOne");
        }
        [Test]
        public void GetTopicPropertyNonExistentProperty()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleTopicsWithProperties);

            TopicProperty property = federation.GetTopicProperty(new TopicName("TopicOne", "NamespaceOne"), 
                "NoSuchProperty");

            Assert.AreEqual(0, property.Values.Count,
                "Checking that zero property values are returned for nonexistent property.");

        }
        [Test]
        public void GetTopicPropertyNonExistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleTopicsWithProperties);

            TopicProperty property = federation.GetTopicProperty(new TopicName("NoSuchTopic", "NamespaceOne"), 
                "PropertyOne");

            Assert.IsNull(property, "Checking that property comes back null when topic does not exist.");
        }
        [Test]
        [ExpectedException(typeof(FlexWikiException), "A topic name without a namespace was specified where a fully-qualified topic name was expected. The topic name was TopicOne")]
        public void GetTopicPropertyRelativeName()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://FederationTests",
                TestContentSets.MultipleTopicsWithProperties);

            TopicProperty property = federation.GetTopicProperty(new TopicName("TopicOne"),
                "PropertyOne");
        }
        [Test]
        public void NamespaceManagerForTopicNegativeNonExistentNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests",
                TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForTopic(
                new QualifiedTopicRevision("TopicOne", "NoSuchNamespace"));

            Assert.IsNull(manager, "Checking that a null manager is returned when namespace does not exist.");
        }
        [Test]
        public void NamespaceManagerForTopicNegativeNonExistentContentStore()
        {
            // If a content provider returns false from Exists, the namespace shouldn't show up in the list
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests",
                TestContentSets.SingleTopicNoImports, MockSetupOptions.StoreDoesNotExist);
            NamespaceManager manager = federation.NamespaceManagerForTopic(
                new QualifiedTopicRevision("TopicOne", "NamespaceOne"));

            Assert.IsNull(manager, "Checking that a null manager is returned when content store does not exist.");
            
        }

        [Test]
        public void NamespaceManagerForTopicNegativeNullTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests",
                TestContentSets.SingleTopicNoImports);

            NamespaceManager manager = federation.NamespaceManagerForTopic((QualifiedTopicRevision) null);
            Assert.IsNull(manager, "Checking that a null manager is returned when topic key is null.");

            manager = federation.NamespaceManagerForTopic((TopicName) null);
            Assert.IsNull(manager, "Checking that a null manager is returned when topic name is null.");
        }

        [Test]
        public void NamespaceManagerForTopicPositive()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests",
                TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForTopic(
                new QualifiedTopicRevision("TopicOne", "NamespaceOne"));

            Assert.IsNotNull(manager, "Checking that a non-null manager was returned.");
            Assert.AreEqual("NamespaceOne", manager.Namespace, "Checking that the correct manager was returned."); 
        }

        [Test]
        public void NamespaceManagerForNonExistentContentStore()
        {
            // If a content provider returns false from Exists, the namespace shouldn't show up in the list
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests",
                TestContentSets.SingleTopicNoImports, MockSetupOptions.StoreDoesNotExist);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsNull(manager, "Checking that a null manager is returned when content store does not exist.");
        }

        [Test]
        public void NamespaceManagerForNullNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests",
                TestContentSets.SingleEmptyNamespace); 
            Assert.IsNull(federation.NamespaceManagerForNamespace(null),
              "Checking that NamespaceManagerForNamespace returns null rather than throwing an exception when passed a null namespace.");
        }

        [Test]
        public void NamespaceManagers()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests", new TestContentSet());
            NamespaceManager namespaceOne = federation.RegisterNamespace(
                new MockContentStore(), "NamespaceOne");
            NamespaceManager namespaceTwo = federation.RegisterNamespace(
                new MockContentStore(MockSetupOptions.StoreDoesNotExist), "NamespaceTwo");

            int count = 0; 
            NamespaceManager last = null;
            foreach (NamespaceManager manager in federation.NamespaceManagers)
            {
                last = manager;
                ++count; 
            }

            Assert.AreEqual(1, count, "Checking that only existing namespaces were returned.");
            Assert.AreEqual(last, namespaceOne, "Checking that the nonexistent manager was not returned."); 
        }

        [Test]
        public void Namespaces()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests", new TestContentSet());
            NamespaceManager namespaceOne = federation.RegisterNamespace(
                new MockContentStore(), "NamespaceOne");
            NamespaceManager namespaceTwo = federation.RegisterNamespace(
                new MockContentStore(MockSetupOptions.StoreDoesNotExist), "NamespaceTwo");

            int count = 0;
            string last = null;
            foreach (string ns in federation.Namespaces)
            {
                last = ns;
                ++count;
            }

            Assert.AreEqual(1, count, "Checking that only existing namespaces were returned.");
            Assert.AreEqual(last, "NamespaceOne", "Checking that the nonexistent namespace was not returned.");
        }

        [Test]
        public void RegisterNamespace()
        {
            MockWikiApplication application = new MockWikiApplication(
                new FederationConfiguration(),
                new LinkMaker("test://federationtests"),
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1))); 

            Federation federation = new Federation(application);
            MockContentStore store = new MockContentStore();
            NamespaceManager storeManager = federation.RegisterNamespace(store, "MockStore");

            Assert.IsNotNull(storeManager, "Checking that a NamespaceManager was created.");
            Assert.AreSame(storeManager, store.NamespaceManager,
              "Checking that the MockContentStore is at the end of the content store chain.");
        }


        private void AssertChangesAreEqual(TopicChangeCollection expectedChanges, TopicChangeCollection actualChanges)
        {
            Assert.AreEqual(expectedChanges.Count, actualChanges.Count,
                "Number of changes are different.");

            for (int i = 0; i < expectedChanges.Count; i++)
            {
                TopicChange expectedChange = expectedChanges[i];
                TopicChange actualChange = actualChanges[i];

                Assert.AreEqual(0, actualChange.CompareTo(expectedChange), 
                    "Checking that changes are equivalent for " + 
                    actualChange.TopicRevision.DottedNameWithVersion + 
                    " and " +
                    expectedChange.TopicRevision.DottedNameWithVersion); 
            }
        }


    }
}
