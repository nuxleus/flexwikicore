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

using NUnit.Framework;

using FlexWiki; 

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class ParsingProviderTests
    {
        [Test]
        public void BuiltInProperties()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://ParsingProviderTests",
                new TestContentSet(new TestNamespace("NamespaceOne")));
            using (RequestContext.Create())
            {
                NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");

                string contents = @"Some content that tries to override one of the built-in properties
_TopicName: NotTheTopicName";
                string author = "testauthor";
                string topicName = "TopicOne";
                QualifiedTopicName qualifiedTopicName = new QualifiedTopicName(topicName, manager.Namespace);

                manager.WriteTopicAndNewVersion("TopicOne", contents, author);
                ParsingProvider provider = (ParsingProvider)manager.GetProvider(typeof(ParsingProvider));

                ParsedTopic parsedTopic = provider.GetParsedTopic(new UnqualifiedTopicRevision("TopicOne"));

                TopicChange latestRevision = manager.AllChangesForTopic(topicName).Latest;

                AssertPropertyContents(parsedTopic, "_TopicName", qualifiedTopicName.LocalName);
                AssertPropertyContents(parsedTopic, "_TopicFullName", qualifiedTopicName.DottedName);
                AssertPropertyContents(parsedTopic, "_LastModifiedBy", author);
                AssertPropertyContents(parsedTopic, "_CreationTime", latestRevision.Created.ToString());
                AssertPropertyContents(parsedTopic, "_ModificationTime", latestRevision.Modified.ToString());
                AssertPropertyContents(parsedTopic, "_Body", contents);
            }
        }

        private void AssertPropertyContents(ParsedTopic parsedTopic, string propertyName, string expectedValue)
        {
            Assert.IsTrue(parsedTopic.Properties.Contains(propertyName),
                "Topic does not contain a property named " + propertyName);

            TopicProperty property = parsedTopic.Properties[propertyName];

            Assert.AreEqual(expectedValue, property.LastValue, "Checking that property has correct value."); 
        }
    }
}
