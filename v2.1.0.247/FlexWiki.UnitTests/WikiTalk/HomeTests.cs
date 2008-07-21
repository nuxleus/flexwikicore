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
using System.Collections.Generic;
using System.Text;

using NUnit.Framework; 

namespace FlexWiki.UnitTests.WikiTalk
{
    [TestFixture]
    public class HomeTests
    {


        [Test]
        public void HomeFederationApplicationKey()
        {
            MockWikiApplication application = new MockWikiApplication(
                new FederationConfiguration(),
                new LinkMaker("test://federationtests"),
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));

            Federation federation = new Federation(application);
            MockContentStore store = new MockContentStore();
            NamespaceManager storeManager = federation.RegisterNamespace(store, "MockStore");
            //ExecutionContext ctx = new ExecutionContext();
            bool result = (bool) federation.Application["DisableWikiEmoticons"];
            Assert.AreEqual(false, result, "Checking that DisableWikiEmoticons is false");
            result = (bool) federation.Application["RemoveListItemWhitespace"];
            Assert.AreEqual(false, result, "Checking that RemoveListItemWhitespace is false");
            string overrideCss = (string)federation.Application["OverrideStylesheet"];
            Assert.AreEqual("wiki-override.css", overrideCss, "Checking that OverrideStylesheet is wiki-override.css");
        }

        [Test]
        public void HomeTopicFormattedName()
        {
            MockWikiApplication application = new MockWikiApplication(
                new FederationConfiguration(),
                new LinkMaker("test://federationtests"),
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));

            Federation federation = new Federation(application);
            MockContentStore store = new MockContentStore();
            NamespaceManager storeManager = federation.RegisterNamespace(store, "MockStore");
            QualifiedTopicRevision topic = new QualifiedTopicRevision("MockStore.NewTopicForTest");
            string result = topic.FormattedName;
            Assert.AreEqual("New Topic For Test", result, "checking that the TopicInfo.FormattedName has correct spaces");
        }
    }
}
