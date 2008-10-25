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
using System.Text;

using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class TopicInfoTest
    {

        [Test]
        public void TopicsInfoForKeywordList()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleTopicsWithKeywords);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicVersionInfo topic = manager.GetTopicInfo("TopicOne");
            ArrayList keywordList = topic.KeywordsList;

            Assert.AreEqual(3, keywordList.Count, "Checking that three keyword items were returned.");
            Assert.AreEqual(0, keywordList.IndexOf("Test"), "Checking that the first keyword list item is 'Test'");
            Assert.IsTrue(keywordList.Contains("Data"), "Checking that the keyword list contains 'Data'");
            Assert.IsTrue(keywordList.Contains("Topic"), "Checking that the keyword list contains 'Topic'");
        }
        [Test]
        public void TopicsInfoForKeywords()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleTopicsWithKeywords);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicVersionInfo topicOne = manager.GetTopicInfo("TopicOne");

            Assert.AreEqual("Test, Data, Topic", topicOne.Keywords, "Checking that the keyword string for 'TopicOne' matches.");
            TopicVersionInfo topicTwo = manager.GetTopicInfo("TopicTwo");

            Assert.AreEqual("Data, Test", topicTwo.Keywords, "Checking that the keyword string for 'Topictwo' matches.");
        }
    }
}
