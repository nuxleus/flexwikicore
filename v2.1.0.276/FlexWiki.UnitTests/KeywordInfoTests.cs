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
    public class KeywordInfoTests
    {

        [Test]
        public void TopicsInfoForKeywordInfo()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleTopicsWithKeywords);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            KeywordInfoArray keywordInfo = manager.KeywordInfos(new ExecutionContext());
            KeywordInfo keywordOne = (KeywordInfo) keywordInfo.Item(0);
            KeywordInfo keywordThree = (KeywordInfo) keywordInfo.Item(2);

            Assert.AreEqual(3, keywordInfo.Count, "Checking that three keyword items were returned.");
            Assert.AreEqual("Data", keywordOne.ExposedName, "Checking that the first keyword list item is 'Data'");
            Assert.AreEqual(3, keywordOne.ExposedCount, "Checking that the count of the Data keyword is 3");
            Assert.AreEqual("Topic", keywordThree.ExposedName, "Checking that the first keyword list item is 'Topic'");
            Assert.AreEqual(2, keywordThree.ExposedCount, "Checking that the count of the Topic keyword is 2");
        }
    }
}
