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

using FlexWiki.Collections;
using FlexWiki.Formatting;
using FlexWiki.Security; 

namespace FlexWiki.UnitTests.Formatting
{
    public class FormattingTestsBase
    {
        protected NamespaceManager _namespaceManager;
        protected const string c_siteUrl = "/formattingtestswiki/";
        protected ExternalReferencesMap _externals;
        protected Federation _federation;
        protected LinkMaker _lm;
        protected string _user = "joe";

        protected Federation Federation
        {
            get { return _federation; }
            set { _federation = value; }
        }


        [SetUp]
        public void SetUp()
        {
            _lm = new LinkMaker(c_siteUrl);
            FederationConfiguration configuration = new FederationConfiguration();
            AuthorizationRule rule = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.GenericAll, null),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Wiki, SecurableAction.ManageNamespace, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(rule));
            MockWikiApplication application = new MockWikiApplication(
                configuration,
                _lm,
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));

            Federation = new Federation(application);
            Federation.WikiTalkVersion = 1;

            _namespaceManager = WikiTestUtilities.CreateMockStore(Federation, "FlexWiki");
            //_namespaceManager.Title  = "Friendly Title";

            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "_ContentBaseDefinition", "Title: Friendly Title", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "HomePage", "Home is where the heart is", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "BigPolicy", "This is ", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "BigDog", "This is ", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "BigAddress", "This is ", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "BigBox", "This is ", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "IncludeOne", "inc1", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "IncludeTwo", "!inc2", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "IncludeThree", "!!inc3", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "IncludeFour", "!inc4", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "IncludeNest", @"		{{IncludeNest1}}
			{{IncludeNest2}}", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "TopicWithColor", "Color: Yellow", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "IncludeNest1", "!hey there", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "IncludeNest2", "!black dog", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "IncludeNestURI", @"wiki://IncludeNest1 wiki://IncludeNest2 ", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "ResourceReference", @"URI: http://www.google.com/$$$", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "FlexWiki", "flex ", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "InlineTestTopic", @"aaa @@""foo""@@ zzz", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "OneMinuteWiki", "one ", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "TestIncludesBehaviors", "@@ProductName@@ somthing @@Now@@ then @@Now@@", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "_Underscore", "Underscore", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "TopicWithBehaviorProperties", @"
Face: {""hello"".Length}
one 
FaceWithArg: {arg | arg.Length }
FaceSpanningLines:{ arg |

arg.Length 

}

", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "TestTopicWithBehaviorProperties", @"
len=@@topics.TopicWithBehaviorProperties.Face@@
lenWith=@@topics.TopicWithBehaviorProperties.FaceWithArg(""superb"")@@
lenSpanning=@@topics.TopicWithBehaviorProperties.FaceSpanningLines(""parsing is wonderful"")@@
", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "TestTopicWithBehaviorProperty", 
@"lenSpanning=@@topics.TopicWithBehaviorProperties.FaceSpanningLines(""parsing is wonderful"")@@", _user);


            _externals = new ExternalReferencesMap();
        }

        [TearDown]
        public void TearDown()
        {
            //_namespaceManager.DeleteAllTopicsAndHistory();
        }

        protected void AssertStringContains(string container, string find)
        {
            Assert.IsTrue(container.IndexOf(find) != -1, "Searching for " + find + " in " + container);
        }
        protected string FormattedTestText(string inputString)
        {
            return FormattedTestText(inputString, null);
        }
        protected string FormattedTestText(string inputString, QualifiedTopicRevision top)
        {
            WikiOutput output = WikiOutput.ForFormat(OutputFormat.Testing, null);
            Formatter.Format(top, inputString, output, _namespaceManager, _lm, _externals, 0);
            string o = output.ToString();
            string o1 = o.Replace("\r", "");
            return o1;
        }
        protected string FormattedTopic(QualifiedTopicRevision top)
        {
            return FormattedTestText(Federation.NamespaceManagerForTopic(top).Read(top.LocalName), top);
        }
        protected void FormattedTopicContainsTest(QualifiedTopicRevision top, string find)
        {
            FormatTestContains(FormattedTopic(top), find);
        }
        protected void FormatTestContains(string inputString, string find)
        {
            string s = FormattedTestText(inputString);
            AssertStringContains(s, find);
        }
        protected void FormatTest(string inputString, string outputString)
        {
            FormatTest(inputString, outputString, null);
        }
        protected void FormatTest(string inputString, string outputString, QualifiedTopicRevision top)
        {
            string o1 = FormattedTestText(inputString, top);
            string o2 = outputString.Replace("\r", "");
            if (o1 != o2)
            {
                Console.Error.WriteLine("Got     : " + o1);
                Console.Error.WriteLine("Expected: " + o2);
            }
            Assert.AreEqual(o2, o1);
        }
        protected void ShouldBeTopicName(string s)
        {
            string topicName = s;
            int hashIndex = s.IndexOf("#");
            if (hashIndex > -1)
            {
                topicName = s.Substring(0, hashIndex);
            }
            FormatTest(
                @s,
                @"<p><a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor(topicName)) + @""">" + topicName + @"</a></p>
");
        }
        protected void ShouldNotBeTopicName(string s)
        {
            FormatTest(
                @s,
                @"<p>" + s + @"</p>
");
        }

    }
}
