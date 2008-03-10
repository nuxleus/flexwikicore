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

using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class TopicParserTests
    {
        [Test]
        public void ClosingDelimiterForOpeningMultilinePropertyDelimiter()
        {
            Assert.AreEqual("]", TopicParser.ClosingDelimiterForOpeningMultilinePropertyDelimiter("["));
            Assert.AreEqual("}", TopicParser.ClosingDelimiterForOpeningMultilinePropertyDelimiter("{"));
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void ClosingDelimiterForOpeningMultilinePropertyDelimiterIllegalCharacter()
        {
            TopicParser.ClosingDelimiterForOpeningMultilinePropertyDelimiter(".");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClosingDelimiterForOpeningMultilinePropertyDelimiterNullCharacter()
        {
            TopicParser.ClosingDelimiterForOpeningMultilinePropertyDelimiter(null); 
        }

        [Test]
        public void CountExternalLinks()
        {
            Assert.AreEqual(0, TopicParser.CountExternalLinks(null),
                "Checking that zero comes back when null is input.");
            Assert.AreEqual(0, TopicParser.CountExternalLinks(string.Empty),
                "Checking that zero comes back when an empty string is input.");

            Assert.AreEqual(0, TopicParser.CountExternalLinks("something"),
                "Checking that zero comes back when no links are present.");

            Assert.AreEqual(1, TopicParser.CountExternalLinks("http://foo.bar/whatever"),
                "Checking that one comes back when input consists of a single http link.");
            Assert.AreEqual(1, TopicParser.CountExternalLinks("https://foo.bar/whatever"),
                "Checking that one comes back when input consists of a single https link.");

            Assert.AreEqual(1, TopicParser.CountExternalLinks("This is a link: http://whatever.com/bar. And some followup."), 
                "Checking that one comes back with an embedded http link."); 
            Assert.AreEqual(1, TopicParser.CountExternalLinks("This is a link: https://whatever.com/bar. And some followup."), 
                "Checking that one comes back with an embedded https link.");

            Assert.AreEqual(5, TopicParser.CountExternalLinks(@"Some text
And then a link http://pluralsight.com/craig and some more text and another link https://www.microsoft.com
And a bit more text and another link https://www.flexwiki.com and another link http://sourceforge.net and 
maybe another link to end it http://www.microsoft.com."),
               "Checking that the right number of links are identified for long input."); 
                                                      ; 
            
        }

        [Test]
        public void IsBehaviorPropertyDelimiter()
        {
            Assert.IsTrue(TopicParser.IsBehaviorPropertyDelimiter("{"),
                "Checking that open brace is a behavior property delimiter."); 
            Assert.IsTrue(TopicParser.IsBehaviorPropertyDelimiter("}"), 
                "Checking that close brace is a behavior property delimiter.");
            Assert.IsFalse(TopicParser.IsBehaviorPropertyDelimiter("{a"),
                "Checking that something that starts with open brace is not a behavior property delimiter.");
            Assert.IsFalse(TopicParser.IsBehaviorPropertyDelimiter("a}"),
                "Checking that something that ends with close brace is not a behavior property delimiter.");
            Assert.IsFalse(TopicParser.IsBehaviorPropertyDelimiter("a{"),
                "Checking that something that ends with open brace is not a behavior property delimiter.");
            Assert.IsFalse(TopicParser.IsBehaviorPropertyDelimiter("}a"),
                "Checking that something that starts with close brace is not a behavior property delimiter.");
        }

        [Test]
        public void ParseExternalReferencesNoReferences()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"");

            Assert.AreEqual(0, parsedTopic.ExternalReferences.Count,
                "Checking that an empty topic results in zero external references."); 
        }

        [Test]
        public void ParseExternalReferencesMultipleReferences()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"@foo=http://www.google.com
@bar=http://microsoft.com/
fake=http://should.not.be/recognized");

            Assert.AreEqual(2, parsedTopic.ExternalReferences.Count,
                "Checking that two external references were parsed.");
            Assert.AreEqual("http://www.google.com", parsedTopic.ExternalReferences["foo"],
                "Checking that the 'foo' reference is correct.");
            Assert.AreEqual("http://microsoft.com/", parsedTopic.ExternalReferences["bar"],
                "Checking that the 'bar' reference is correct.");          
        }

        [Test]
        public void ParseExternalReferencesSingleReference()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"@foo=http://www.google.com");
            Assert.AreEqual(1, parsedTopic.ExternalReferences.Count,
                "Checking that a single external reference is parsed.");
            Assert.AreEqual("http://www.google.com", parsedTopic.ExternalReferences["foo"],
                "Checking that the reference was parsed correctly."); 
        }

        [Test]
        public void ParseHeaders()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"!Header 1
a line of text
!!Header 2
more text 
and another line of text
!A Second Header1
more text again");

            Assert.AreEqual(3, parsedTopic.Headers.Split(new char[] { '\n' }).Length - 1,
                "Checking that all headers were properly extracted");
            Assert.AreEqual("!Header 1", parsedTopic.Headers.Split(new char[] { '\n' }).GetValue(0).ToString().Trim(),
                "Checking that the first header content is correct");
            Assert.AreEqual("!!Header 2", parsedTopic.Headers.Split(new char[] { '\n' }).GetValue(1).ToString().Trim(),
                "Checking that the first header content is correct");
            Assert.AreEqual("!A Second Header1", parsedTopic.Headers.Split(new char[] { '\n' }).GetValue(2).ToString().Trim(),
                "Checking that the first header content is correct");

        }

        [Test]
        public void ParseMultilineProperty()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"Multiline:[
first line
foobar
second line
]
More junk here");
            Assert.AreEqual(1, parsedTopic.Properties.Count,
                "Checking that the right number of properties were processed.");
            Assert.AreEqual(1, parsedTopic.Properties["Multiline"].Values.Count,
                "Checking that the property has the right number of values."); 
            Assert.AreEqual(@"first line
foobar
second line
", parsedTopic.Properties["Multiline"].Values[0].RawValue,
            "Checking that the property value is correct."); 
        }

        [Test]
        public void ParseMultilinePropertyMultipleValues()
        {
            string input = @"Name:[value
value
]
Name:[
value
value
value
]
Name:[
value
value
value
]"; 
            ParsedTopic parsedTopic = TopicParser.Parse(input);

            Assert.AreEqual(1, parsedTopic.Properties.Count, "Checking that there's only one property.");
            Assert.AreEqual("Name", parsedTopic.Properties[0].Name, "Checking that the property name is correct.");
            Assert.AreEqual(3, parsedTopic.Properties[0].Values.Count, "Checking that the property has three values.");
            Assert.AreEqual(@"value
value
value
", parsedTopic.Properties[0].Values[1].RawValue,
                "Checking that the second property value is correct."); 
        }

        [Test]
        public void ParseMultilinePropertyWithBraces()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"Multiline:{
Some literal
multiline 
value. 
}");
            Assert.AreEqual(1, parsedTopic.Properties.Count,
                "Checking that the right number of properties were processed.");
            Assert.AreEqual(1, parsedTopic.Properties["Multiline"].Values.Count,
                "Checking that the property has the right number of values.");
            Assert.AreEqual(@"{
Some literal
multiline 
value. 
}", parsedTopic.Properties["Multiline"].Values[0].RawValue,
                "Checking that braces are preserved when used as a delimiter."); 
        }

        [Test]
        public void ParseMultipleProperties()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"PropertyOne: ValueA
PropertyTwo: ValueB
PropertyOne: ValueC");

            Assert.AreEqual(2, parsedTopic.Properties.Count,
              "Checking that the right number of property were returned.");
            AssertPropertyCorrect(parsedTopic.Properties[0], "PropertyOne", "ValueA", "ValueC");
            AssertPropertyCorrect(parsedTopic.Properties[1], "PropertyTwo", "ValueB");
        }

        [Test]
        public void ParseNestedBraces()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"Multiline:{
Some literal
multiline 
  { 
    with nested
    braces
  }
value. 
}");
            Assert.AreEqual(1, parsedTopic.Properties.Count,
                "Checking that the right number of properties were processed.");
            Assert.AreEqual(1, parsedTopic.Properties["Multiline"].Values.Count,
                "Checking that the property has the right number of values.");
            Assert.AreEqual(@"{
Some literal
multiline 
  { 
    with nested
    braces
  }
value. 
}", parsedTopic.Properties["Multiline"].Values[0].RawValue,
                "Checking that nested braces don't screw things up.");
        }

        [Test]
        public void ParseSimpleProperty()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"Property: Value");

            Assert.AreEqual(1, parsedTopic.Properties.Count, "Checking that the right number of property were parsed.");
            AssertPropertyCorrect(parsedTopic.Properties[0], "Property", "Value");
        }

        [Test]
        public void ParseTopicLinksForeignNamespace()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"This is a topic with 
a link to WikiTopicOne and WikiTopicTwo and ForeignNamespace.WikiTopicOne");

            AssertTopicLinksCorrect(parsedTopic.TopicLinks,
                new TopicRevision("WikiTopicOne"),
                new TopicRevision("WikiTopicTwo"),
                new TopicRevision("WikiTopicOne", "ForeignNamespace"));
        }
        
        [Test]
        public void ParseTopicLinksNoLinks()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"This is a page with no links.");

            Assert.AreEqual(0, parsedTopic.TopicLinks.Count, "Checking that an empty list of links was returned.");
        }

        [Test]
        public void ParseTopicLinksNoRepeats()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"This is a topic with 
a link to WikiTopicOne, another link to WikiTopicOne, and a link to WikiTopicTwo.");

            AssertTopicLinksCorrect(parsedTopic.TopicLinks,
                new TopicRevision("WikiTopicOne"),
                new TopicRevision("WikiTopicTwo"));
        }

        [Test]
        public void ParseTopicLinksSimple()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"This is a page that links
to WikiTopicOne and WikiTopicTwo");

            AssertTopicLinksCorrect(parsedTopic.TopicLinks,
                new TopicRevision("WikiTopicOne"),
                new TopicRevision("WikiTopicTwo")
                );
        }

    
        private static void AssertPropertyCorrect(TopicProperty property, string expectedName, params string[] expectedValues)
        {
            Assert.AreEqual(expectedName, property.Name, "Checking that property name was correct.");

            Assert.AreEqual(expectedValues.Length, property.Values.Count, 
                "Checking that property had the right number of values.");

            for (int i = 0; i < expectedValues.Length; i++)
			{
                string message = string.Format("Checking that value {0} of property {1} is correct.", 
                    i, property.Name); 
			    Assert.AreEqual(expectedValues[i], property.Values[i].RawValue, message); 
			}
        }

        private static void AssertTopicLinksCorrect(IList<TopicRevision> actualLinks,
            params TopicRevision[] expectedLinks)
        {
            Assert.AreEqual(expectedLinks.Length, actualLinks.Count,
                "Checking that the right number of links were returned.");

            for (int i = 0; i < actualLinks.Count; i++)
            {
                Assert.AreEqual(expectedLinks[i], actualLinks[i],
                    string.Format("Checking that link {0} is correct.", i));
            }
        }
    }
}
