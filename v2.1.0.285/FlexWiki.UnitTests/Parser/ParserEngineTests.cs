using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Xsl;

using NUnit.Framework;
using FlexWiki.Formatting;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class ParserEngineTests
    {
        //private IParserApplication application;
        private Federation federation;
        private ParserEngine parser;
        private XslCompiledTransform _trans;
        IMockWikiApplication testApp;

        [SetUp]
        public void SetUp()
        {
            federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleEmptyNamespace);
            testApp = (IMockWikiApplication)federation.Application;

            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            testApp.SetApplicationProperty("DisableNewParser", false);
            testApp.SetApplicationProperty("EnableNewParser", true);
            parser = new ParserEngine(federation);
            _trans = new XslCompiledTransform();
            _trans.Load(parser.XsltPath);
        }

        [Test]
        public void GetMainPathTest()
        {
            string mainpath = parser.GetMainPath;
            Assert.IsTrue(mainpath.Contains("womContext.xml"));
        }

        [Test]
        public void GrammarMultiLinePathTest()
        {
            string mainpath = parser.GrammarMultiLinePath;
            Assert.IsTrue(mainpath.Contains("womMultiLine.xml"));
        }
        [Test]
        public void GrammarIncludeTopicPathTest()
        {
            string mainpath = parser.GrammarIncludeTopicPath;
            Assert.IsTrue(mainpath.Contains("womIncludeTopic.xml"));
        }
        [Test]
        public void GrammarWomTextPathTest()
        {
            string mainpath = parser.GrammarWomTextPath;
            Assert.IsTrue(mainpath.Contains("womText.xml"));
        }
        [Test]
        public void GrammarWomCellPathTest()
        {
            string mainpath = parser.GrammarWomCellPath;
            Assert.IsTrue(mainpath.Contains("womCell.xml"));
        }
        [Test]
        public void GrammarWomTableStylePathTest()
        {
            string mainpath = parser.GrammarWomTableStylePath;
            Assert.IsTrue(mainpath.Contains("womTableStyle.xml"));
        }
        [Test]
        public void XsltPathTest()
        {
            string mainpath = parser.XsltPath;
            Assert.IsTrue(mainpath.Contains("FlexWikiWeb.xslt"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // This series of xslt test verifies the various xsl transform of a particular 
        // womDocument element into correct html 
        [Test]
        public void XsltSinglelinePropertyTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<SinglelineProperty><Name>Summary</Name>
<womPropertyText> this is some text.</womPropertyText></SinglelineProperty>
<TipHolder></TipHolder>
</WomDocument>
";

            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<fieldset class=""Property"" style=""width: auto"">
  <legend class=""PropertyName"">
    <a name=""Summary"" class=""Anchor"">Summary</a>
  </legend>
  <div class=""PropertyValue""> this is some text.</div>
</fieldset>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // This series of xslt test verifies the various xsl transform of a particular 
        // womDocument element into correct html 
        [Test]
        public void XsltSinglelinePropertyTopicExistsTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<SinglelineProperty><Name>GoodTopic</Name>
<TopicExists><Namespace>OdsWiki</Namespace><Topic>GoodTopic</Topic><TipId>id1</TipId><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExists><womPropertyText> this is some text.</womPropertyText></SinglelineProperty>
<TipHolder></TipHolder>
</WomDocument>
";

            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<fieldset class=""Property"" style=""width: auto"">
  <legend class=""PropertyName"">
    <a name=""GoodTopic"" class=""Anchor"">GoodTopic</a>
  </legend>
  <div class=""PropertyValue""> <a onmouseover=""TopicTipOn(this,'id1',event);"" onmouseout=""TopicTipOff();"" href=""/FlexWiki/default.aspx/OdsWiki/GoodTopic.html"">GoodTopic</a> this is some text.</div>
</fieldset>
<div id=""id1"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // This series of xslt test verifies the various xsl transform of a particular 
        // womDocument element into correct html 
        [Test]
        public void XsltSinglelinePropertyCreateTopicTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<SinglelineProperty><Name>BadTopic</Name><CreateNewTopic><SinglelineProperty>BadTopic</SinglelineProperty><Namespace>OdsWiki</Namespace></CreateNewTopic><womPropertyText> this is some text.</womPropertyText></SinglelineProperty>
<TipHolder></TipHolder>
</WomDocument>
";

            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<fieldset class=""Property"" style=""width: auto"">
  <legend class=""PropertyName"">
    <a name=""BadTopic"" class=""Anchor"">BadTopic</a>
  </legend>
  <div class=""PropertyValue""> <a title=""Click here to create this topic"" class=""create"" href=""/FlexWiki/WikiEdit.aspx?topic=OdsWiki.BadTopic&amp;return=OdsWiki.BadTopic""></a> this is some text.</div>
</fieldset>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltHiddenSinglelinePropertyTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<HiddenSinglelineProperty><Name>Summary</Name>
<womPropertyText> this is some text.</womPropertyText></HiddenSinglelineProperty>
<Para><paraText>This text is visible.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            // HiddenSinglelineProperty has no output
            //

            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This text is visible.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltHiddenMultilinePropertyTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<HiddenMultilineProperty><Name>Summary</Name>
<womPropertyText>
this is some text.
It is on multiple lines.</womPropertyText>
</HiddenMultilineProperty>
<Para><paraText>This text is visible.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            // HiddenSinglelineProperty has no output
            //

            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This text is visible.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltMultilinePropertyTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<MultilineProperty><Name>Summary</Name>
<womPropertyText>
this is some text.
It is on multiple lines.</womPropertyText>
</MultilineProperty>
<Para><paraText>This text is visible.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            // HiddenSinglelineProperty has no output
            //

            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<fieldset class=""Property"" style=""width: auto"">
  <legend class=""PropertyName"">
    <a name=""Summary"" class=""Anchor"">Summary</a>
  </legend>
  <div class=""PropertyValue"">
this is some text.
It is on multiple lines.</div>
</fieldset>
<p>This text is visible.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltSimpleOrderedListTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<list type=""ordered""><item>
<womListText> List item 1</womListText></item>
<item>
<womListText> List item 2</womListText></item></list>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<ol>
  <li> List item 1</li>
  <li> List item 2</li>
</ol>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltSimpleUnorderedListTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<list type=""unordered""><item>
<womListText> List item 1</womListText></item>
<item>
<womListText> List item 2</womListText></item></list>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<ul>
  <li> List item 1</li>
  <li> List item 2</li>
</ul>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltComplexUnorderedListTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<list type=""unordered""><item>
<womListText> List item 1</womListText></item>
<item>
<womListText> List item 2</womListText><list type=""unordered""><item>
<womListText> List 2 item 1</womListText></item>
<item>
<womListText> List 2 item 2</womListText><list type=""unordered""><item>
<womListText> List 3 item 1</womListText></item>
</list></item>
<item>
<womListText> List 2 item 3</womListText></item>
<item>
<womListText> List 2 item 4</womListText></item>
</list></item>
<item>
<womListText> List item 3</womListText><list type=""unordered""><item>
<womListText> New List 2 item 1</womListText></item></list>
</item></list>
<Para><paraText>This is some other text after the list.</paraText></Para><TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<ul>
  <li> List item 1</li>
  <li> List item 2<ul><li> List 2 item 1</li><li> List 2 item 2<ul><li> List 3 item 1</li></ul></li><li> List 2 item 3</li><li> List 2 item 4</li></ul></li>
  <li> List item 3<ul><li> New List 2 item 1</li></ul></li>
</ul>
<p>This is some other text after the list.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltComplexOrderedListTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<list type=""ordered""><item>
<womListText> List item 1</womListText></item>
<item>
<womListText> List item 2</womListText><list type=""ordered""><item>
<womListText> List 2 item 1</womListText></item>
<item>
<womListText> List 2 item 2</womListText><list type=""ordered""><item>
<womListText> List 3 item 1</womListText></item>
</list></item>
<item>
<womListText> List 2 item 3</womListText></item>
<item>
<womListText> List 2 item 4</womListText></item>
</list></item>
<item>
<womListText> List item 3</womListText><list type=""ordered""><item>
<womListText> New List 2 item 1</womListText></item></list>
</item></list>
<Para><paraText>This is some other text after the list.</paraText></Para><TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<ol>
  <li> List item 1</li>
  <li> List item 2<ol><li> List 2 item 1</li><li> List 2 item 2<ol><li> List 3 item 1</li></ol></li><li> List 2 item 3</li><li> List 2 item 4</li></ol></li>
  <li> List item 3<ol><li> New List 2 item 1</li></ol></li>
</ol>
<p>This is some other text after the list.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltComplexMixedListTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<list type=""ordered""><item>
<womListText> List item 1</womListText></item>
<item>
<womListText> List item 2</womListText><list type=""unordered""><item>
<womListText> List 2 item 1</womListText></item>
<item>
<womListText> List 2 item 2</womListText><list type=""ordered""><item>
<womListText> List 3 item 1</womListText></item>
</list></item>
<item>
<womListText> List 2 item 3</womListText></item>
<item>
<womListText> List 2 item 4</womListText></item>
</list></item>
<item>
<womListText> List item 3</womListText><list type=""ordered""><item>
<womListText> New List 2 item 1</womListText></item></list>
</item></list>
<Para><paraText>This is some other text after the list.</paraText></Para><TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<ol>
  <li> List item 1</li>
  <li> List item 2<ul><li> List 2 item 1</li><li> List 2 item 2<ol><li> List 3 item 1</li></ol></li><li> List 2 item 3</li><li> List 2 item 4</li></ul></li>
  <li> List item 3<ol><li> New List 2 item 1</li></ol></li>
</ol>
<p>This is some other text after the list.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltSimpleHeaderTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<Header level=""1"">
<womHeaderText> Header One text</womHeaderText><AnchorText>_1__Header_One_text</AnchorText></Header>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            //TODO: Fix anchor text so that it is without spaces and other format details
            string expected = @"<p>This is some text.</p>
<h1 id=""_1__Header_One_text""> Header One text</h1>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFirstMultipleHeaderTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<Header level=""2"">
<womHeaderText> Header Two text</womHeaderText><AnchorText>_1__Header_Two_text</AnchorText></Header>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Header level=""3"">
<womHeaderText> Header Three text</womHeaderText><AnchorText>_2__Header_Three_text</AnchorText></Header>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Header level=""5"">
<womHeaderText> Header Five text</womHeaderText><AnchorText>_3__Header_Five_text</AnchorText></Header>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<h2 id=""_1__Header_Two_text""> Header Two text</h2>
<p />
<p>Here is some more text.</p>
<h3 id=""_2__Header_Three_text""> Header Three text</h3>
<p />
<p>Here is some more text.</p>
<h5 id=""_3__Header_Five_text""> Header Five text</h5>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltSecondMultipleHeaderTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<Header level=""4"">
<womHeaderText> Header Four text</womHeaderText><AnchorText>_1__Header_Four_text</AnchorText></Header>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Header level=""6"">
<womHeaderText> Header Six text</womHeaderText><AnchorText>_2__Header_Six_text</AnchorText></Header>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Header level=""7"">
<womHeaderText> Header Seven text</womHeaderText><AnchorText>_3__Header_Seven_text</AnchorText></Header>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<h4 id=""_1__Header_Four_text""> Header Four text</h4>
<p />
<p>Here is some more text.</p>
<h6 id=""_2__Header_Six_text""> Header Six text</h6>
<p />
<p>Here is some more text.</p>
<h6 id=""_3__Header_Seven_text""> Header Seven text</h6>
<p />
<p>Here is some more text.</p>";
            //Note: h7 does not exist - level=7 is converted to h6
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltImageDisplayLinksTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<Para><HttpImageDisplayJpg>http://www.flexwiki.com/fwlogo.jpg</HttpImageDisplayJpg></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpImageDisplayGif>http://io9.com/test.gif</HttpImageDisplayGif></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpImageDisplayJpeg>http://127.0.0.1/mypic.jpeg</HttpImageDisplayJpeg></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpImageDisplayPng>http://www.example.com:8080/my_graphic.png</HttpImageDisplayPng></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpsImageDisplayJpg>https://www.flexwiki.com/somedir/fwlogo.jpg</HttpsImageDisplayJpg></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpsImageDisplayGif>https://io9.com/first/second/third/fourth/fifth/sixth/test.gif</HttpsImageDisplayGif></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpsImageDisplayJpeg>https://127.0.0.1/mypic.jpeg</HttpsImageDisplayJpeg></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpsImageDisplayPng>https://www.example.com/mygraphic.png</HttpsImageDisplayPng></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p>
  <img src=""http://www.flexwiki.com/fwlogo.jpg"" alt=""http://www.flexwiki.com/fwlogo.jpg"" />
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""http://io9.com/test.gif"" alt=""http://io9.com/test.gif"" />
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""http://127.0.0.1/mypic.jpeg"" alt=""http://127.0.0.1/mypic.jpeg"" />
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""http://www.example.com:8080/my_graphic.png"" alt=""http://www.example.com:8080/my_graphic.png"" />
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""https://www.flexwiki.com/somedir/fwlogo.jpg"" alt=""https://www.flexwiki.com/somedir/fwlogo.jpg"" />
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""https://io9.com/first/second/third/fourth/fifth/sixth/test.gif"" alt=""https://io9.com/first/second/third/fourth/fifth/sixth/test.gif"" />
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""https://127.0.0.1/mypic.jpeg"" alt=""https://127.0.0.1/mypic.jpeg"" />
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""https://www.example.com/mygraphic.png"" alt=""https://www.example.com/mygraphic.png"" />
</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFreeLinkImageDisplayLinksTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<Para><FreeLinkToHttpImageDisplayJpg><HttpImageDisplayJpg>http://www.flexwiki.com/fwlogo.jpg</HttpImageDisplayJpg><WebLink>https://www.flexwiki.com</WebLink></FreeLinkToHttpImageDisplayJpg></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpImageDisplayGif><HttpImageDisplayGif>http://io9.com/test.gif</HttpImageDisplayGif><WebLink>http://io9.com</WebLink></FreeLinkToHttpImageDisplayGif></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpImageDisplayJpeg><HttpImageDisplayJpeg>http://127.0.0.1/mypic.jpeg</HttpImageDisplayJpeg><WebLink>http://localhost/webpage.html</WebLink></FreeLinkToHttpImageDisplayJpeg></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpImageDisplayPng><HttpImageDisplayPng>http://www.example.com:8080/my_graphic.png</HttpImageDisplayPng><WebLink>http://www.example.com:8080/</WebLink></FreeLinkToHttpImageDisplayPng></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpsImageDisplayJpg><HttpsImageDisplayJpg>https://www.flexwiki.com/somedir/fwlogo.jpg</HttpsImageDisplayJpg><WebLink>https://www.flexwiki.com</WebLink></FreeLinkToHttpsImageDisplayJpg></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpsImageDisplayGif><HttpsImageDisplayGif>https://io9.com/first/second/third/fourth/fifth/sixth/test.gif</HttpsImageDisplayGif><WebLink>https://io9.com/first/second/third/fourth/fifth/sixth/test.gif</WebLink></FreeLinkToHttpsImageDisplayGif></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpsImageDisplayJpeg><HttpsImageDisplayJpeg>https://127.0.0.1/mypic.jpeg</HttpsImageDisplayJpeg><WebLink>http://localhost/mypic.png</WebLink></FreeLinkToHttpsImageDisplayJpeg></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpsImageDisplayPng><HttpsImageDisplayPng>https://www.example.com/mygraphic.png</HttpsImageDisplayPng><WebLink>http://www.example.com/text.htm</WebLink></FreeLinkToHttpsImageDisplayPng></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p>
  <img src=""http://www.flexwiki.com/fwlogo.jpg"" alt=""http://www.flexwiki.com/fwlogo.jpg"" />
  <a class=""externalLink"" href=""https://www.flexwiki.com"">https://www.flexwiki.com</a>
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""http://io9.com/test.gif"" alt=""http://io9.com/test.gif"" />
  <a class=""externalLink"" href=""http://io9.com"">http://io9.com</a>
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""http://127.0.0.1/mypic.jpeg"" alt=""http://127.0.0.1/mypic.jpeg"" />
  <a class=""externalLink"" href=""http://localhost/webpage.html"">http://localhost/webpage.html</a>
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""http://www.example.com:8080/my_graphic.png"" alt=""http://www.example.com:8080/my_graphic.png"" />
  <a class=""externalLink"" href=""http://www.example.com:8080/"">http://www.example.com:8080/</a>
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""https://www.flexwiki.com/somedir/fwlogo.jpg"" alt=""https://www.flexwiki.com/somedir/fwlogo.jpg"" />
  <a class=""externalLink"" href=""https://www.flexwiki.com"">https://www.flexwiki.com</a>
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""https://io9.com/first/second/third/fourth/fifth/sixth/test.gif"" alt=""https://io9.com/first/second/third/fourth/fifth/sixth/test.gif"" />
  <a class=""externalLink"" href=""https://io9.com/first/second/third/fourth/fifth/sixth/test.gif"">https://io9.com/first/second/third/fourth/fifth/sixth/test.gif</a>
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""https://127.0.0.1/mypic.jpeg"" alt=""https://127.0.0.1/mypic.jpeg"" />
  <a class=""externalLink"" href=""http://localhost/mypic.png"">http://localhost/mypic.png</a>
</p>
<p />
<p>Here is some more text.</p>
<p>
  <img src=""https://www.example.com/mygraphic.png"" alt=""https://www.example.com/mygraphic.png"" />
  <a class=""externalLink"" href=""http://www.example.com/text.htm"">http://www.example.com/text.htm</a>
</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltAnchorLinkTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<BaseImage>/FlexWiki/images/</BaseImage>
<SiteUrl>/FlexWiki/</SiteUrl>
<Para><paraText>This is some text.</paraText></Para>
<Para><TopicExistsAnchor><Namespace>OdsWiki</Namespace><Topic>GoodTopic</Topic><Anchor>Anchor</Anchor><TipId>id1</TipId><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExistsAnchor></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<Para><TopicExistsAnchor><Namespace>OdsWiki</Namespace><Topic>AnotherGoodTopic</Topic><Anchor>Placeholder</Anchor><TipId>id2</TipId><DisplayText>Anchor Display Text</DisplayText><TipData><TipIdData>id2</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExistsAnchor></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p> <a onmouseover=""TopicTipOn(this,'id1',event);"" onmouseout=""TopicTipOff();"" href=""OdsWiki/GoodTopic.html#Anchor"">GoodTopic#Anchor</a></p>
<p />
<p>Here is some more text.</p>
<p> <a onmouseover=""TopicTipOn(this,'id2',event);"" onmouseout=""TopicTipOff();"" href=""OdsWiki/AnotherGoodTopic.html#Placeholder"">Anchor Display Text</a></p>
<p />
<p>Here is some more text.</p>
<div id=""id1"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>
<div id=""id2"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltGoodTopicLinksTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line contains a link to a </paraText><TopicExists><Namespace>OdsWiki</Namespace><Topic>GoodTopic</Topic><TipId>id1</TipId><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExists><paraText> in it</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text with a link </paraText><TopicExists><Namespace>OdsWiki</Namespace><Topic>MULTIcapsGoodTopic</Topic><TipId>id2</TipId><TipData><TipIdData>id2</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExists><paraText> in the line</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para><TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line contains a link to a  <a onmouseover=""TopicTipOn(this,'id1',event);"" onmouseout=""TopicTipOff();"" href=""/FlexWiki/default.aspx/OdsWiki/GoodTopic.html"">GoodTopic</a> in it</p>
<p />
<p>Here is some more text with a link  <a onmouseover=""TopicTipOn(this,'id2',event);"" onmouseout=""TopicTipOff();"" href=""/FlexWiki/default.aspx/OdsWiki/MULTIcapsGoodTopic.html"">MULTIcapsGoodTopic</a> in the line</p>
<p />
<p>Here is some more text.</p>
<div id=""id1"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>
<div id=""id2"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltBadTopicLinksTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line contains a link to a </paraText><CreateNewTopic><StartsWithOneCap>BadTopic</StartsWithOneCap><Namespace>OdsWiki</Namespace></CreateNewTopic><paraText> in it</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text with a link </paraText><CreateNewTopic><StartsWithMulticaps>MULTIcapsBadTopic</StartsWithMulticaps><Namespace>OdsWiki</Namespace></CreateNewTopic><paraText> in the line</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line contains a link to a  <a title=""Click here to create this topic"" class=""create"" href=""/FlexWiki/WikiEdit.aspx?topic=OdsWiki.BadTopic&amp;return=OdsWiki.BadTopic"">BadTopic</a> in it</p>
<p />
<p>Here is some more text with a link  <a title=""Click here to create this topic"" class=""create"" href=""/FlexWiki/WikiEdit.aspx?topic=OdsWiki.MULTIcapsBadTopic&amp;return=OdsWiki.MULTIcapsBadTopic"">MULTIcapsBadTopic</a> in the line</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltGoodNamespaceTopicLinksTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line contains a link to a </paraText><NamespaceTopicExists><Namespace>OdsWiki</Namespace><Topic>GoodTopic</Topic><TipId>id1</TipId><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></NamespaceTopicExists><paraText> in it</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text with a link </paraText><NamespaceTopicExists><Namespace>OdsWiki</Namespace><Topic>MULTIcapsGoodTopic</Topic><TipId>id2</TipId><TipData><TipIdData>id2</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></NamespaceTopicExists><paraText> in the line</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line contains a link to a  <a onmouseover=""TopicTipOn(this,'id1',event);"" onmouseout=""TopicTipOff();"" href=""/FlexWiki/default.aspx/OdsWiki/GoodTopic.html"">GoodTopic</a> in it</p>
<p />
<p>Here is some more text with a link  <a onmouseover=""TopicTipOn(this,'id2',event);"" onmouseout=""TopicTipOff();"" href=""/FlexWiki/default.aspx/OdsWiki/MULTIcapsGoodTopic.html"">MULTIcapsGoodTopic</a> in the line</p>
<p />
<p>Here is some more text.</p>
<div id=""id1"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>
<div id=""id2"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltGoodNamespaceBadTopicLinksTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line contains a link to a </paraText><CreateNamespaceTopic><NamespaceTopic>OdsWiki.BadTopic</NamespaceTopic><CreateTopic>BadTopic</CreateTopic></CreateNamespaceTopic><paraText> in it</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text with a link </paraText><CreateNamespaceTopic><NamespaceMulticapsTopic>OdsWiki.MULTIcapsBadTopic</NamespaceMulticapsTopic><CreateTopic>MULTIcapsBadTopic</CreateTopic></CreateNamespaceTopic><paraText> in the line</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line contains a link to a <a title=""Click here to create this topic"" class=""create"" href=""OdsWiki.BadTopic&amp;return=OdsWiki.BadTopic"">OdsWiki.BadTopic</a> in it</p>
<p />
<p>Here is some more text with a link <a title=""Click here to create this topic"" class=""create"" href=""OdsWiki.MULTIcapsBadTopic&amp;return=OdsWiki.MULTIcapsBadTopic"">OdsWiki.MULTIcapsBadTopic</a> in the line</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltBadNamespaceTopicLinksTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line contains a link to a </paraText><BadNamespaceTopic>BadWiki.GoodTopic</BadNamespaceTopic><paraText> in it</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text with a link </paraText><BadNamespaceTopic>BadWiki.MULTIcapsGoodTopic</BadNamespaceTopic><paraText> in the line</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line contains a link to a BadWiki.GoodTopic in it</p>
<p />
<p>Here is some more text with a link BadWiki.MULTIcapsGoodTopic in the line</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTextileInLineTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line (1) contains </paraText><Italics>italic text</Italics><paraText> and a </paraText><Strong>strong text</Strong><paraText> in it</paraText></Para>
<Para><paraText>This line (2) contains </paraText><Italics>italic text with embedded *strong text* in</Italics><paraText> it</paraText></Para>
<Para><paraText>This line (3) has</paraText><TextileStrongInLine>strong text</TextileStrongInLine><paraText> by itself</paraText></Para>
<Para><paraText>This line (4) has</paraText><TextileSuperscriptInLine>superscript text with *strong text* embedded</TextileSuperscriptInLine><paraText> in it</paraText></Para>
<Para><paraText>This line (5) contains a</paraText><TextileCitationInLine>citation text</TextileCitationInLine><paraText> in it</paraText></Para>
<Para><paraText>This line (6) has a section of</paraText><TextileDeletionInLine>deleted terxt</TextileDeletionInLine><paraText> and a section of</paraText><TextileInsertedInLine>inserted text</TextileInsertedInLine><paraText> in it</paraText></Para>
<Para><paraText>This line (7) has</paraText><TextileEmphasisInLine>emphasized text</TextileEmphasisInLine><paraText> in it</paraText></Para>
<Para><paraText>While this (8) line has a mix of</paraText><TextileSuperscriptInLine>superscript</TextileSuperscriptInLine><paraText> and</paraText><TextileSubscriptInLine>subscript</TextileSubscriptInLine><paraText> in it</paraText></Para>
<Para><paraText>This (9) is a</paraText><TextileCodeLineInLine>section of code</TextileCodeLineInLine><paraText> in the text</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line (1) contains <i>italic text</i> and a <strong>strong text</strong> in it</p>
<p>This line (2) contains <i>italic text with embedded *strong text* in</i> it</p>
<p>This line (3) has<strong>strong text</strong> by itself</p>
<p>This line (4) has<sup>superscript text with *strong text* embedded</sup> in it</p>
<p>This line (5) contains a<cite>citation text</cite> in it</p>
<p>This line (6) has a section of<del>deleted terxt</del> and a section of<ins>inserted text</ins> in it</p>
<p>This line (7) has<em>emphasized text</em> in it</p>
<p>While this (8) line has a mix of<sup>superscript</sup> and<sub>subscript</sub> in it</p>
<p>This (9) is a<code> section of code</code> in the text</p>
<p />
<p>Here is some more text.</p>";
            // Line 2 is correct and requires implementation of a post process in WomDocument to check for 
            // and action such occurences. This will be similar to Emoticon processing and will run immediately
            // before the emoticon process
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTextileStartLineTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><TextileStrongLineStart>strong text</TextileStrongLineStart><paraText> by itself on line (1)</paraText></Para>
<Para><TextileSuperscriptLineStart>superscript text with *strong text* embedded</TextileSuperscriptLineStart><paraText> in it on line (2)</paraText></Para>
<Para><TextileCitationLineStart>citation text</TextileCitationLineStart><paraText> on line (3)</paraText></Para>
<Para><TextileDeletionLineStart>deleted terxt</TextileDeletionLineStart><paraText> at the start of line (4)</paraText></Para>
<Para><TextileInsertedLineStart>inserted text</TextileInsertedLineStart><paraText> in line (5)</paraText></Para>
<Para><TextileEmphasisLineStart>emphasized text</TextileEmphasisLineStart><paraText> in line (6)</paraText></Para>
<Para><TextileSubscriptLineStart>subscript</TextileSubscriptLineStart><paraText> in line (7)</paraText></Para>
<Para><TextileCodeLineStart>@section of code@</TextileCodeLineStart><paraText> in the text of line (8)</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>
  <strong>strong text</strong> by itself on line (1)</p>
<p>
  <sup>superscript text with *strong text* embedded</sup> in it on line (2)</p>
<p>
  <cite>citation text</cite> on line (3)</p>
<p>
  <del>deleted terxt</del> at the start of line (4)</p>
<p>
  <ins>inserted text</ins> in line (5)</p>
<p>
  <em>emphasized text</em> in line (6)</p>
<p>
  <sub>subscript</sub> in line (7)</p>
<p>
  <code> @section of code@</code> in the text of line (8)</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltPageRuleTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line contains a link to a </paraText><CreateNewTopic><StartsWithOneCap>BadTopic</StartsWithOneCap><Namespace>OdsWiki</Namespace></CreateNewTopic><paraText> in it</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text with a page rule (hr) below</paraText></Para>
<PageRule/>
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line contains a link to a  <a title=""Click here to create this topic"" class=""create"" href=""/FlexWiki/WikiEdit.aspx?topic=OdsWiki.BadTopic&amp;return=OdsWiki.BadTopic"">BadTopic</a> in it</p>
<p />
<p>Here is some more text with a page rule (hr) below</p>
<hr class=""Rule"" />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltWikiTalkMethodCreateTopicTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<WikiTalkMethod><Name>TopicForMethodCreate</Name><CreateNewTopic><WikiTalkMethod>TopicForMethodCreate</WikiTalkMethod><Namespace>OdsWiki</Namespace></CreateNewTopic><wikiTalkMultiline>{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }
}</wikiTalkMultiline></WikiTalkMethod>
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<fieldset class=""Property"" style=""width: auto"">
  <legend class=""PropertyName"">
    <a name=""TopicForMethodCreate"" class=""Anchor"">TopicForMethodCreate</a>
  </legend>
  <div class=""PropertyValue""> <a title=""Click here to create this topic"" class=""create"" href=""/FlexWiki/WikiEdit.aspx?topic=OdsWiki.TopicForMethodCreate&amp;return=OdsWiki.TopicForMethodCreate""></a>{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }
}</div>
</fieldset>
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltWikiTalkMethodTopicExistsTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<WikiTalkMethod><Name>TopicForMethodExists</Name><TopicExists><Namespace>OdsWiki</Namespace><Topic>TopicForMethodExists</Topic><TipId>id1</TipId><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExists><wikiTalkMultiline>{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }
}</wikiTalkMultiline></WikiTalkMethod>
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<fieldset class=""Property"" style=""width: auto"">
  <legend class=""PropertyName"">
    <a name=""TopicForMethodExists"" class=""Anchor"">TopicForMethodExists</a>
  </legend>
  <div class=""PropertyValue""> <a onmouseover=""TopicTipOn(this,'id1',event);"" onmouseout=""TopicTipOff();"" href=""/FlexWiki/default.aspx/OdsWiki/TopicForMethodExists.html"">TopicForMethodExists</a>{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }
}</div>
</fieldset>
<p>Here is some more text.</p>
<div id=""id1"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltHiddenWikiTalkMethodTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<HiddenWikiTalkMethod><Name>ShowNamespaceSelect</Name>
<wikiTalkMultiline>{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }</wikiTalkMultiline>
</HiddenWikiTalkMethod>
<Para><paraText>Here is some more text.</paraText></Para><TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltHttpLinkTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a link to </paraText><HttpLink>http://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html</HttpLink></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a link to <a class=""externalLink"" href=""http://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html"">http://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html</a></p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltHttpsLinkTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a link to </paraText><HttpsLink>https://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html</HttpsLink></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a link to <a class=""externalLink"" href=""https://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html"">https://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html</a></p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFreeLinkToHttpLinkTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a link to </paraText><FreeLinkToHttpLink><FreeLink>FlexWiki HomePage</FreeLink><HttpLink>http://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html</HttpLink></FreeLinkToHttpLink></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a link to <a class=""externalLink"" title="""" href=""http://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html"">FlexWiki HomePage</a></p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFreeLinkToHttpsLinkTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a link to </paraText><FreeLinkToHttpsLink><FreeLink>Secure FlexWiki HomePage</FreeLink><HttpsLink>https://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html</HttpsLink></FreeLinkToHttpsLink></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a link to <a class=""externalLink"" title="""" href=""https://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html"">Secure FlexWiki HomePage</a></p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFreeLinkToTopicExistsTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a link to </paraText><TopicExists><Namespace>OdsWiki</Namespace><Topic>GoodTopic</Topic><TipId>id1</TipId><DisplayText>FlexWiki TopicExists</DisplayText><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExists></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a link to  <a onmouseover=""TopicTipOn(this,'id1',event);"" onmouseout=""TopicTipOff();"" href=""/FlexWiki/default.aspx/OdsWiki/GoodTopic.html"">FlexWiki TopicExists</a></p>
<p />
<p>Here is some more text.</p>
<div id=""id1"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFreeLinkToTopicCreateTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a link to </paraText><CreateNewTopic><Topic>BadTopic</Topic><Namespace>OdsWiki</Namespace><DisplayText>FlexWiki CreateTopic</DisplayText></CreateNewTopic></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a link to  <a title=""Click here to create this topic"" class=""create"" href=""/FlexWiki/WikiEdit.aspx?topic=OdsWiki.BadTopic&amp;return=OdsWiki.BadTopic"">FlexWiki CreateTopic</a></p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFreeLinkToMultiCapsTopicExistsTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a link to </paraText><TopicExists><Namespace>OdsWiki</Namespace><Topic>MULTIcapsGoodTopic</Topic><TipId>id1</TipId><DisplayText>FlexWiki TopicExists</DisplayText><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExists></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a link to  <a onmouseover=""TopicTipOn(this,'id1',event);"" onmouseout=""TopicTipOff();"" href=""/FlexWiki/default.aspx/OdsWiki/MULTIcapsGoodTopic.html"">FlexWiki TopicExists</a></p>
<p />
<p>Here is some more text.</p>
<div id=""id1"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFreeLinkToMultiCapsTopicCreateTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a link to </paraText><CreateNewTopic><Topic>MULTIcapsBadTopic</Topic><Namespace>OdsWiki</Namespace><DisplayText>FlexWiki CreateTopic</DisplayText></CreateNewTopic></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a link to  <a title=""Click here to create this topic"" class=""create"" href=""/FlexWiki/WikiEdit.aspx?topic=OdsWiki.MULTIcapsBadTopic&amp;return=OdsWiki.MULTIcapsBadTopic"">FlexWiki CreateTopic</a></p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltEscapedNoFormatTextTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a </paraText><EscapedNoFormatText>NoFormatText</EscapedNoFormatText><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a NoFormatText in it.</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltLinkToAnchorTopicExistsTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has an anchor to </paraText><TopicExistsAnchor><Namespace>OdsWiki</Namespace><Topic>GoodTopic</Topic><Anchor>Summary</Anchor><TipId>id1</TipId><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExistsAnchor><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has an anchor to  <a onmouseover=""TopicTipOn(this,'id1',event);"" onmouseout=""TopicTipOff();"" href=""OdsWiki/GoodTopic.html#Summary"">GoodTopic#Summary</a> in it.</p>
<p />
<p>Here is some more text.</p>
<div id=""id1"" style=""display: none"">This is just some text<div class=""TopicTipStats"">5/22/2008 12:06:58 PM - -76.70.99.195</div></div>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFreeLinkToNamespaceTopicBadNamespaceTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has an freelink to </paraText><FreeLinkToNamespaceTopic>""My FreeLink"":FlexWiki.GoodTopic</FreeLinkToNamespaceTopic><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has an freelink to ""My FreeLink"":FlexWiki.GoodTopic in it.</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFreeLinkToNamespaceMalformedTopicBadNamespaceTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has an freelink to </paraText><FreeLinkToNamespaceMalformedTopic>""My FreeLink"":FlexWiki.[goodtopic]</FreeLinkToNamespaceMalformedTopic><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has an freelink to ""My FreeLink"":FlexWiki.[goodtopic] in it.</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltMailtoLinkTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a mailto to </paraText><MailtoLink>mailto:jwdavidson@gmail.com</MailtoLink><paraText> in it.</paraText></Para>
<Para><paraText>This line has a mailto to </paraText><MailtoLink>mailto:jwdavidson@gmail.com;somebodyelse@example.com</MailtoLink><paraText> in it.</paraText></Para>
<Para><paraText>This line has a mailto to </paraText><MailtoLink>mailto:jwdavidson@gmail.com?subject=FlexWiki%20Development</MailtoLink><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a mailto to <a class=""externalLink"" href=""mailto:jwdavidson@gmail.com"">mailto:jwdavidson@gmail.com</a> in it.</p>
<p>This line has a mailto to <a class=""externalLink"" href=""mailto:jwdavidson@gmail.com;somebodyelse@example.com"">mailto:jwdavidson@gmail.com;somebodyelse@example.com</a> in it.</p>
<p>This line has a mailto to <a class=""externalLink"" href=""mailto:jwdavidson@gmail.com?subject=FlexWiki%20Development"">mailto:jwdavidson@gmail.com?subject=FlexWiki%20Development</a> in it.</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFreeLinkToMailtoTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a mailto to </paraText><FreeLinkToMailto><FreeLinkMail>JW Davidson</FreeLinkMail><Mailto>mailto:jwdavidson@gmail.com</Mailto></FreeLinkToMailto><paraText> in it.</paraText></Para>
<Para><paraText>This line has a mailto to </paraText><FreeLinkToMailto><FreeLinkMail>JW Davidson + Somebodyelse</FreeLinkMail><Mailto>mailto:jwdavidson@gmail.com;somebodyelse@example.com</Mailto></FreeLinkToMailto><paraText> in it.</paraText></Para>
<Para><paraText>This line has a mailto to </paraText><FreeLinkToMailto><FreeLinkMail>FlexWiki Development</FreeLinkMail><Mailto>mailto:jwdavidson@gmail.com?subject=FlexWiki%20Development</Mailto></FreeLinkToMailto><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a mailto to <a class=""externalLink"" href=""mailto:jwdavidson@gmail.com"">JW Davidson</a> in it.</p>
<p>This line has a mailto to <a class=""externalLink"" href=""mailto:jwdavidson@gmail.com;somebodyelse@example.com"">JW Davidson + Somebodyelse</a> in it.</p>
<p>This line has a mailto to <a class=""externalLink"" href=""mailto:jwdavidson@gmail.com?subject=FlexWiki%20Development"">FlexWiki Development</a> in it.</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltFileLinkTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a file link to </paraText><FileLink>file:\\someserver\share</FileLink><paraText> in it.</paraText></Para>
<Para><paraText>This line has a file link to </paraText><FileLink>file:\\someserver\share\test.gif</FileLink><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a file link to <a class=""externalLink"" href=""file:\\someserver\share"">file:\\someserver\share</a> in it.</p>
<p>This line has a file link to <a class=""externalLink"" href=""file:\\someserver\share\test.gif"">file:\\someserver\share\test.gif</a> in it.</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltAltFileLinkTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a file link to </paraText><AltFileLink>file:\\someserver\share</AltFileLink><paraText> in it.</paraText></Para>
<Para><paraText>This line has a file link to </paraText><AltFileLink>file:\\someserver\share\test.gif</AltFileLink><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a file link to <a class=""externalLink"" href=""file:\\someserver\share"">file:\\someserver\share</a> in it.</p>
<p>This line has a file link to <a class=""externalLink"" href=""file:\\someserver\share\test.gif"">file:\\someserver\share\test.gif</a> in it.</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltWikiTalkStringTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<WikiTalkString>@@[
	""||{!}*Topic*||{!}*Date of Last Change*||{!}*Summary*||"", Newline,
	namespace.Topics.Select{ each | 
		each.HasProperty(""Owner"") 
		}.SortBy { each | 
			DateTime.Now.SpanBetween(each.LastModified) 
		}.Collect{ each |
			[
				""	* "", each.Name,
				"" %gray%("", each.LastModified.ToShortDateString(), "" "", each.LastModified.ToLongTimeString(), 
				"")"", Newline,
				""		* "",each.Summary,Newline,
			]
	}
]
@@</WikiTalkString><EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltWikiStylingTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>Some text </paraText><WikiStyling><StyleColor>red</StyleColor><womWikiStyledText> some text in red</womWikiStyledText></WikiStyling></Para>
<Para></Para>
<Para><paraText>Some text </paraText><WikiStyling><StyleColor>blue</StyleColor><womWikiStyledText> some text in blue</womWikiStyledText></WikiStyling>
<paraText> now it is normal</paraText></Para>
<Para><paraText>Some text </paraText><WikiStyling><StyleSizeBig/><StyleColor>blue</StyleColor><womWikiStyledText> some text in blue</womWikiStyledText></WikiStyling>
<paraText> now it is normal</paraText></Para>
<Para><paraText>Some text </paraText><WikiStyling><StyleColor>blue</StyleColor><StyleSizeSmall/><womWikiStyledText> some text in blue</womWikiStyledText></WikiStyling>
<paraText> now it is normal</paraText></Para>
<Para><paraText>Some text </paraText><WikiStyling><StyleSizeBig/><StyleColor>blue</StyleColor><StyleSizeBig/><womWikiStyledText> some text in blue</womWikiStyledText></WikiStyling>
<paraText> now it is normal</paraText></Para>
<Para><paraText>Some </paraText><WikiStyling><StyleColor>blue</StyleColor><womWikiStyledText> blue text</womWikiStyledText></WikiStyling>
<paraText> with some </paraText><WikiStyling><StyleColor>red</StyleColor><womWikiStyledText> red text.</womWikiStyledText></WikiStyling></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>Some text <span style=""color:red""> some text in red</span></p>
<p />
<p>Some text <span style=""color:blue""> some text in blue</span> now it is normal</p>
<p>Some text <big><span style=""color:blue""> some text in blue</span></big> now it is normal</p>
<p>Some text <small><span style=""color:blue""> some text in blue</span></small> now it is normal</p>
<p>Some text <big><big><span style=""color:blue""> some text in blue</span></big></big> now it is normal</p>
<p>Some <span style=""color:blue""> blue text</span> with some <span style=""color:red""> red text.</span></p>
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltSimpleTableRowTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"">
  <tr>
    <td class=""TableCell"">Region</td>
    <td class=""TableCell"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCell"">East</td>
    <td class=""TableCell"">$100</td>
  </tr>
  <tr>
    <td class=""TableCell"">West</td>
    <td class=""TableCell"">$500</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowCenterTableTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><TableCenter>T^</TableCenter></TableStyle>
<womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"" align=""center"">
  <tr>
    <td class=""TableCell"">Region</td>
    <td class=""TableCell"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCell"">East</td>
    <td class=""TableCell"">$100</td>
  </tr>
  <tr>
    <td class=""TableCell"">West</td>
    <td class=""TableCell"">$500</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowTableFloatLeftTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><TableFloatLeft>T[</TableFloatLeft></TableStyle>
<womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"" align=""left"" style="";margin-left: 0; float: left"">
  <tr>
    <td class=""TableCell"">Region</td>
    <td class=""TableCell"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCell"">East</td>
    <td class=""TableCell"">$100</td>
  </tr>
  <tr>
    <td class=""TableCell"">West</td>
    <td class=""TableCell"">$500</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowTableFloatRightTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><TableFloatRight>T]</TableFloatRight></TableStyle>
<womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"" style="";margin-left: 0; float: right"">
  <tr>
    <td class=""TableCell"">Region</td>
    <td class=""TableCell"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCell"">East</td>
    <td class=""TableCell"">$100</td>
  </tr>
  <tr>
    <td class=""TableCell"">West</td>
    <td class=""TableCell"">$500</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowTableNoBorderTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><BorderlessTable>T-</BorderlessTable></TableStyle>
<womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableWithoutBorderClass"">
  <tr>
    <td class=""TableCellNoBorder"">Region</td>
    <td class=""TableCellNoBorder"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCellNoBorder"">East</td>
    <td class=""TableCellNoBorder"">$100</td>
  </tr>
  <tr>
    <td class=""TableCellNoBorder"">West</td>
    <td class=""TableCellNoBorder"">$500</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowTableWidthTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><TableWidth>TW25</TableWidth></TableStyle>
<womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"" width=""25%"">
  <tr>
    <td class=""TableCell"">Region</td>
    <td class=""TableCell"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCell"">East</td>
    <td class=""TableCell"">$100</td>
  </tr>
  <tr>
    <td class=""TableCell"">West</td>
    <td class=""TableCell"">$500</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowSpanColumnRowTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><RowSpan>R2</RowSpan></TableStyle>
<womCell>Region</womCell></womCellText><womCellText>
<TableStyle><ColumnSpan>C2</ColumnSpan></TableStyle>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>Q1</womCell></womCellText><womCellText>
<womCell>Q2</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText><womCellText>
<womCell>$800</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText><womCellText>
<womCell>$9000</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"">
  <tr>
    <td class=""TableCell"" rowspan=""2"">Region</td>
    <td class=""TableCell"" colspan=""2"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCell"">Q1</td>
    <td class=""TableCell"">Q2</td>
  </tr>
  <tr>
    <td class=""TableCell"">East</td>
    <td class=""TableCell"">$100</td>
    <td class=""TableCell"">$800</td>
  </tr>
  <tr>
    <td class=""TableCell"">West</td>
    <td class=""TableCell"">$500</td>
    <td class=""TableCell"">$9000</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowSpanColumnLeftTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><ColumnLeft>[</ColumnLeft><ColumnSpan>C2</ColumnSpan></TableStyle>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>Q1</womCell></womCellText><womCellText>
<womCell>Q2</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$100</womCell></womCellText><womCellText>
<womCell>$800</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$500</womCell></womCellText><womCellText>
<womCell>$9000</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"">
  <tr>
    <td class=""TableCell"" colspan=""2"" align=""left"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCell"">Q1</td>
    <td class=""TableCell"">Q2</td>
  </tr>
  <tr>
    <td class=""TableCell"">$100</td>
    <td class=""TableCell"">$800</td>
  </tr>
  <tr>
    <td class=""TableCell"">$500</td>
    <td class=""TableCell"">$9000</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowSpanColumnRightTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><ColumnRight>]</ColumnRight><ColumnSpan>C2</ColumnSpan></TableStyle>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>Q1</womCell></womCellText><womCellText>
<womCell>Q2</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$100</womCell></womCellText><womCellText>
<womCell>$800</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$500</womCell></womCellText><womCellText>
<womCell>$9000</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"">
  <tr>
    <td class=""TableCell"" colspan=""2"" align=""right"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCell"">Q1</td>
    <td class=""TableCell"">Q2</td>
  </tr>
  <tr>
    <td class=""TableCell"">$100</td>
    <td class=""TableCell"">$800</td>
  </tr>
  <tr>
    <td class=""TableCell"">$500</td>
    <td class=""TableCell"">$9000</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowSpanColumnCenterTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><ColumnCenter>^</ColumnCenter><ColumnSpan>C2</ColumnSpan></TableStyle>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>Q1</womCell></womCellText><womCellText>
<womCell>Q2</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$100</womCell></womCellText><womCellText>
<womCell>$800</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$500</womCell></womCellText><womCellText>
<womCell>$9000</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"">
  <tr>
    <td class=""TableCell"" colspan=""2"" align=""center"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCell"">Q1</td>
    <td class=""TableCell"">Q2</td>
  </tr>
  <tr>
    <td class=""TableCell"">$100</td>
    <td class=""TableCell"">$800</td>
  </tr>
  <tr>
    <td class=""TableCell"">$500</td>
    <td class=""TableCell"">$9000</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowSpanColumnCellHighlightTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><ColumnCenter>^</ColumnCenter><CellHighlight>!</CellHighlight><ColumnSpan>C2</ColumnSpan></TableStyle>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>Q1</womCell></womCellText><womCellText>
<womCell>Q2</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$100</womCell></womCellText><womCellText>
<womCell>$800</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$500</womCell></womCellText><womCellText>
<TableStyle><CellHighlight>!</CellHighlight></TableStyle>
<womCell>$9000</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"">
  <tr>
    <td class=""TableCellHighlighted"" colspan=""2"" align=""center"">Sales</td>
  </tr>
  <tr>
    <td class=""TableCell"">Q1</td>
    <td class=""TableCell"">Q2</td>
  </tr>
  <tr>
    <td class=""TableCell"">$100</td>
    <td class=""TableCell"">$800</td>
  </tr>
  <tr>
    <td class=""TableCell"">$500</td>
    <td class=""TableCellHighlighted"">$9000</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowSpanColumnWidthTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><CellWidth>W75</CellWidth></TableStyle>
<womCell>Q1</womCell></womCellText><womCellText>
<womCell>Q2</womCell></womCellText><womCellText>
<womCell>Q3</womCell></womCellText><womCellText>
<womCell>Q4</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$100</womCell></womCellText><womCellText>
<womCell>$800</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText><womCellText>
<womCell>$900</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$500</womCell></womCellText><womCellText>
<womCell>$9000</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText><womCellText>
<womCell>$900</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"">
  <tr>
    <td class=""TableCell"" width=""75%"">Q1</td>
    <td class=""TableCell"">Q2</td>
    <td class=""TableCell"">Q3</td>
    <td class=""TableCell"">Q4</td>
  </tr>
  <tr>
    <td class=""TableCell"">$100</td>
    <td class=""TableCell"">$800</td>
    <td class=""TableCell"">$500</td>
    <td class=""TableCell"">$900</td>
  </tr>
  <tr>
    <td class=""TableCell"">$500</td>
    <td class=""TableCell"">$9000</td>
    <td class=""TableCell"">$500</td>
    <td class=""TableCell"">$900</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowSpanCellNoWrapTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><CellNoWrap>+</CellNoWrap></TableStyle>
<womCell> The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. </womCell></womCellText><womCellText>
<womCell> The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. </womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"">
  <tr>
    <td class=""TableCell"" style=""white-space: nowrap;""> The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. </td>
    <td class=""TableCell""> The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. </td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltTableRowCellColorTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<TableStyle><CellStyleColor>red</CellStyleColor></TableStyle>
<womCell>RED RED RED</womCell></womCellText></TableRow>
<TableRow><womCellText>
<TableStyle><CellStyleColor>lightgreen</CellStyleColor></TableStyle>
<womCell>LIGHT GREEN</womCell></womCellText></TableRow>
<TableRow><womCellText>
<TableStyle><StyleHexColor>#c0c0c0</StyleHexColor></TableStyle>
<womCell>LIGHT GREY</womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"">
  <tr>
    <td class=""TableCell"" style=""background: red;"">RED RED RED</td>
  </tr>
  <tr>
    <td class=""TableCell"" style=""background: lightgreen;"">LIGHT GREEN</td>
  </tr>
  <tr>
    <td class=""TableCell"" style=""background: #c0c0c0;"">LIGHT GREY</td>
  </tr>
</table>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltPreformattedSingleLineTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<PreformattedSingleLine>
 ||Region||Sales||
 ||East||$100||
 ||West||$500||
</PreformattedSingleLine>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<pre>
 ||Region||Sales||
 ||East||$100||
 ||West||$500||
</pre>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltAltPreformattedSingleLineTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<PreformattedSingleLine>
    ||Region||Sales||
    ||East||$100||
    ||West||$500||
</PreformattedSingleLine>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<pre>
    ||Region||Sales||
    ||East||$100||
    ||West||$500||
</pre>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltExtendedCodeTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<ExtendedCode>
<womStyledCode>   </womStyledCode><WikiStyling><StyleColor>blue</StyleColor><womWikiStyledText>public void</womWikiStyledText></WikiStyling>
<womStyledCode> Foo()
   {
       </womStyledCode><WikiStyling><StyleColor>green</StyleColor><womWikiStyledText>// comment here</womWikiStyledText></WikiStyling>
<womStyledCode>
       </womStyledCode><WikiStyling><StyleColor>blue</StyleColor><womWikiStyledText>string</womWikiStyledText></WikiStyling>
<womStyledCode> s;
      </womStyledCode><TextileStrongInLine>...</TextileStrongInLine><womStyledCode>
   }</womStyledCode>
</ExtendedCode>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<pre>
  <span style=""color:blue"">public void</span> Foo()
   {
       <span style=""color:green"">// comment here</span><span style=""color:blue"">string</span> s;
      <strong>...</strong>
   }</pre>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltPreformattedMultilineTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<PreformattedMultiline>
<womMultilineCode>your text goes
here and it does not have to start with space or tab</womMultilineCode>
</PreformattedMultiline>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<pre>your text goes
here and it does not have to start with space or tab</pre>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltPreformattedMultilineKeyedTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<PreformattedMultilineKeyed>
<womMultilineCode>

your text goes
here and it does not have to start with space or tab</womMultilineCode>
</PreformattedMultilineKeyed>
<EmptyLine />
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<pre>

your text goes
here and it does not have to start with space or tab</pre>
<p />
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltPreformattedMultilineMixedTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<PreformattedMultilineKeyed>
<womMultilineCode>

your text goes
here and it does not have to start with space or tab
{@
an embedded preformatted section
}@
this is some text</womMultilineCode>
</PreformattedMultilineKeyed>
<EmptyLine />
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<pre>

your text goes
here and it does not have to start with space or tab
{@
an embedded preformatted section
}@
this is some text</pre>
<p />
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltSimpleEmoticonsTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<BaseImage>/FlexWiki/images/</BaseImage>
<SiteUrl>/FlexWiki/</SiteUrl>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has a thumbs up at the end <Emoticon>emoticons/thumbs_up.gif</Emoticon></paraText></Para>
<Para><paraText>This line has <Emoticon>emoticons/thumbs_down.gif</Emoticon> a thumbs down in it</paraText></Para>
<Para><paraText>This line has a <Emoticon>emoticons/confused_smile.gif</Emoticon> confused smile</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has a thumbs up at the end <img src=""/FlexWiki/emoticons/thumbs_up.gif"" alt=""emoticons/thumbs_up.gif"" /></p>
<p>This line has <img src=""/FlexWiki/emoticons/thumbs_down.gif"" alt=""emoticons/thumbs_down.gif"" /> a thumbs down in it</p>
<p>This line has a <img src=""/FlexWiki/emoticons/confused_smile.gif"" alt=""emoticons/confused_smile.gif"" /> confused smile</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltMoreEmoticonsTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<BaseImage>/FlexWiki/images/</BaseImage>
<SiteUrl>/FlexWiki/</SiteUrl>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has <Emoticon>emoticons/thumbs_down.gif</Emoticon> a thumbs down and a thumbs up <Emoticon>emoticons/thumbs_up.gif</Emoticon> in it</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<p>This line has <img src=""/FlexWiki/emoticons/thumbs_down.gif"" alt=""emoticons/thumbs_down.gif"" /> a thumbs down and a thumbs up <img src=""/FlexWiki/emoticons/thumbs_up.gif"" alt=""emoticons/thumbs_up.gif"" /> in it</p>
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void XsltComplexEmoticonsTest()
        {
            string womDocIn = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
<WomDocument>
<BaseTopic>/FlexWiki/default.aspx/</BaseTopic>
<BaseEdit>/FlexWiki/WikiEdit.aspx?topic=</BaseEdit>
<BaseImage>/FlexWiki/images/</BaseImage>
<SiteUrl>/FlexWiki/</SiteUrl>
<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Table><TableRow><womCellText>
<womCell> <Emoticon>emoticons/thumbs_up.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/thumbs_down.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/beer_yum.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/martini_shaken.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/girl_handsacrossamerica.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/guy_handsacrossamerica.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/devil_smile.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/bat.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/girl_hug.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/dude_hug.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/regular_smile.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/wink_smile.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> :(</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell> </womCell><EscapedNoFormatText>(y)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(n)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(b)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(d)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(x)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(z)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(6)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>:-[</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(})</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>({)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>:-)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>;)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>:(</EscapedNoFormatText></womCellText></TableRow>
<TableRow><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>   </womCell></womCellText><womCellText>
<womCell>   </womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell> <Emoticon>emoticons/whatchutalkingabout_smile.gif</Emoticon> </womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/cry_smile.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> :-$</womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/shades_smile.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/angry_smile.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/angel_smile.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/heart.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/broken_heart.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/kiss.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/present.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/rose.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/wilted_rose.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/camera.gif</Emoticon></womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell> </womCell><EscapedNoFormatText>:|</EscapedNoFormatText><womCell> </womCell></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>:'(</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>:-$</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(H)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>:-@</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(A)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(L)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(U)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(k)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(g)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(f)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(w)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(p)</EscapedNoFormatText></womCellText></TableRow>
<TableRow><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>   </womCell></womCellText><womCellText>
<womCell>   </womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell> <Emoticon>emoticons/film.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/phone.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/phone.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/kittykay.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/coffee.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/lightbulb.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/moon.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/star.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/musical_note.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/envelope.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/cake.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/clock.gif</Emoticon></womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/messenger.gif</Emoticon></womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell> </womCell><EscapedNoFormatText>(~)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(T)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(t)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(@)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(c)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(i)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(S)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(*)</EscapedNoFormatText><womCell> </womCell></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(8)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(E)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(^)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(O)</EscapedNoFormatText></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(M)</EscapedNoFormatText></womCellText></TableRow>
<TableRow><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>   </womCell></womCellText><womCellText>
<womCell>   </womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell> <Emoticon>emoticons/tounge_smile.gif</Emoticon> </womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/clock.gif</Emoticon> </womCell></womCellText><womCellText>
<womCell> <Emoticon>emoticons/teeth_smile.gif</Emoticon>   </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>   </womCell></womCellText><womCellText>
<womCell>   </womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell> </womCell><EscapedNoFormatText>:-P</EscapedNoFormatText><womCell> </womCell></womCellText><womCellText>
<womCell> </womCell><EscapedNoFormatText>(o)</EscapedNoFormatText><womCell>   </womCell></womCellText><womCellText>
<womCell>  </womCell><EscapedNoFormatText>:-D</EscapedNoFormatText><womCell>  </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>    </womCell></womCellText><womCellText>
<womCell>   </womCell></womCellText><womCellText>
<womCell>   </womCell></womCellText></TableRow>
</Table>
<EmptyLine />
<Para><paraText>I need a little flag (red maybe <Emoticon>emoticons/regular_smile.gif</Emoticon>)</paraText></Para>
<EmptyLine />
<Para><paraText><Emoticon>emoticons/regular_smile.gif</Emoticon></paraText></Para>
<Para><paraText>:D</paraText></Para>
<Para><paraText><Emoticon>emoticons/teeth_smile.gif</Emoticon></paraText></Para>
<Para><paraText>:/</paraText></Para>
<Para><paraText>:-/</paraText></Para>
<Para><paraText>:\</paraText></Para>
<Para><paraText>:-\</paraText></Para>
<Para><paraText>:p</paraText></Para>
<Para><paraText>:-p</paraText></Para>
<Para><paraText>:P</paraText></Para>
<Para><paraText><Emoticon>emoticons/tounge_smile.gif</Emoticon></paraText></Para>
<EmptyLine />
<Para><paraText>(`) (1) (2) (3) (4) (5) <Emoticon>emoticons/devil_smile.gif</Emoticon> (7) <Emoticon>emoticons/musical_note.gif</Emoticon> (9) (0) (-) (=)</paraText></Para>
<Para><paraText>(q) <Emoticon>emoticons/wilted_rose.gif</Emoticon> <Emoticon>emoticons/envelope.gif</Emoticon> (r) <Emoticon>emoticons/phone.gif</Emoticon> <Emoticon>emoticons/thumbs_up.gif</Emoticon> <Emoticon>emoticons/broken_heart.gif</Emoticon> <Emoticon>emoticons/lightbulb.gif</Emoticon> <Emoticon>emoticons/clock.gif</Emoticon> <Emoticon>emoticons/camera.gif</Emoticon> ([) (]) (\)</paraText></Para>
<Para><paraText><Emoticon>emoticons/angel_smile.gif</Emoticon> (s) <Emoticon>emoticons/martini_shaken.gif</Emoticon> <Emoticon>emoticons/rose.gif</Emoticon> <Emoticon>emoticons/present.gif</Emoticon> <Emoticon>emoticons/shades_smile.gif</Emoticon> (j) <Emoticon>emoticons/kiss.gif</Emoticon> <Emoticon>emoticons/heart.gif</Emoticon> (<Emoticon>emoticons/wink_smile.gif</Emoticon> (')</paraText></Para>
<Para><paraText><Emoticon>emoticons/guy_handsacrossamerica.gif</Emoticon> <Emoticon>emoticons/girl_handsacrossamerica.gif</Emoticon> <Emoticon>emoticons/coffee.gif</Emoticon> (v) <Emoticon>emoticons/beer_yum.gif</Emoticon> <Emoticon>emoticons/thumbs_down.gif</Emoticon> <Emoticon>emoticons/messenger.gif</Emoticon> (,) (.) (/)</paraText></Para>
<PreformattedSingleLine>
 
</PreformattedSingleLine>
<Para><paraText>() () () () () () () () () () () () () </paraText></Para>
<EmptyLine />
<Para><paraText><Emoticon>emoticons/film.gif</Emoticon> (!) <Emoticon>emoticons/kittykay.gif</Emoticon> ($) (%) <Emoticon>emoticons/cake.gif</Emoticon> (&amp;) <Emoticon>emoticons/star.gif</Emoticon> (() ()) (_) (+)</paraText></Para>
<Para><paraText>(Q) <Emoticon>emoticons/wilted_rose.gif</Emoticon> <Emoticon>emoticons/envelope.gif</Emoticon> (R) <Emoticon>emoticons/phone.gif</Emoticon> <Emoticon>emoticons/thumbs_up.gif</Emoticon> <Emoticon>emoticons/broken_heart.gif</Emoticon> <Emoticon>emoticons/lightbulb.gif</Emoticon> <Emoticon>emoticons/clock.gif</Emoticon> <Emoticon>emoticons/camera.gif</Emoticon> <Emoticon>emoticons/dude_hug.gif</Emoticon> <Emoticon>emoticons/girl_hug.gif</Emoticon> (|)</paraText></Para>
<Para><paraText><Emoticon>emoticons/angel_smile.gif</Emoticon> <Emoticon>emoticons/moon.gif</Emoticon> <Emoticon>emoticons/martini_shaken.gif</Emoticon> <Emoticon>emoticons/rose.gif</Emoticon> <Emoticon>emoticons/present.gif</Emoticon> <Emoticon>emoticons/shades_smile.gif</Emoticon> (J) <Emoticon>emoticons/kiss.gif</Emoticon> <Emoticon>emoticons/heart.gif</Emoticon> (<Emoticon>emoticons/regular_smile.gif</Emoticon> ("")</paraText></Para>
<Para><paraText><Emoticon>emoticons/guy_handsacrossamerica.gif</Emoticon> <Emoticon>emoticons/girl_handsacrossamerica.gif</Emoticon> <Emoticon>emoticons/coffee.gif</Emoticon> (V) <Emoticon>emoticons/beer_yum.gif</Emoticon> <Emoticon>emoticons/thumbs_down.gif</Emoticon> <Emoticon>emoticons/messenger.gif</Emoticon> (&lt;) (&gt;) (?)</paraText></Para>
<EmptyLine />
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
<TipHolder></TipHolder>
</WomDocument>
";
            XmlDocument inputDoc = new XmlDocument();
            inputDoc.LoadXml(womDocIn);

            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _trans.Transform(inputDoc, null, writer);
            string xsltOut = writer.InnerWriter.ToString();
            string expected = @"<p>This is some text.</p>
<p />
<table class=""TableClass"">
  <tr>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/thumbs_up.gif"" alt=""emoticons/thumbs_up.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/thumbs_down.gif"" alt=""emoticons/thumbs_down.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/beer_yum.gif"" alt=""emoticons/beer_yum.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/martini_shaken.gif"" alt=""emoticons/martini_shaken.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/girl_handsacrossamerica.gif"" alt=""emoticons/girl_handsacrossamerica.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/guy_handsacrossamerica.gif"" alt=""emoticons/guy_handsacrossamerica.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/devil_smile.gif"" alt=""emoticons/devil_smile.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/bat.gif"" alt=""emoticons/bat.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/girl_hug.gif"" alt=""emoticons/girl_hug.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/dude_hug.gif"" alt=""emoticons/dude_hug.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/regular_smile.gif"" alt=""emoticons/regular_smile.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/wink_smile.gif"" alt=""emoticons/wink_smile.gif"" />
    </td>
    <td class=""TableCell""> :(</td>
  </tr>
  <tr>
    <td class=""TableCell"">(y)</td>
    <td class=""TableCell"">(n)</td>
    <td class=""TableCell"">(b)</td>
    <td class=""TableCell"">(d)</td>
    <td class=""TableCell"">(x)</td>
    <td class=""TableCell"">(z)</td>
    <td class=""TableCell"">(6)</td>
    <td class=""TableCell"">:-[</td>
    <td class=""TableCell"">(})</td>
    <td class=""TableCell"">({)</td>
    <td class=""TableCell"">:-)</td>
    <td class=""TableCell"">;)</td>
    <td class=""TableCell"">:(</td>
  </tr>
  <tr>
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
  </tr>
  <tr>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/whatchutalkingabout_smile.gif"" alt=""emoticons/whatchutalkingabout_smile.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/cry_smile.gif"" alt=""emoticons/cry_smile.gif"" />
    </td>
    <td class=""TableCell""> :-$</td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/shades_smile.gif"" alt=""emoticons/shades_smile.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/angry_smile.gif"" alt=""emoticons/angry_smile.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/angel_smile.gif"" alt=""emoticons/angel_smile.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/heart.gif"" alt=""emoticons/heart.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/broken_heart.gif"" alt=""emoticons/broken_heart.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/kiss.gif"" alt=""emoticons/kiss.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/present.gif"" alt=""emoticons/present.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/rose.gif"" alt=""emoticons/rose.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/wilted_rose.gif"" alt=""emoticons/wilted_rose.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/camera.gif"" alt=""emoticons/camera.gif"" />
    </td>
  </tr>
  <tr>
    <td class=""TableCell"">:|</td>
    <td class=""TableCell"">:'(</td>
    <td class=""TableCell"">:-$</td>
    <td class=""TableCell"">(H)</td>
    <td class=""TableCell"">:-@</td>
    <td class=""TableCell"">(A)</td>
    <td class=""TableCell"">(L)</td>
    <td class=""TableCell"">(U)</td>
    <td class=""TableCell"">(k)</td>
    <td class=""TableCell"">(g)</td>
    <td class=""TableCell"">(f)</td>
    <td class=""TableCell"">(w)</td>
    <td class=""TableCell"">(p)</td>
  </tr>
  <tr>
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
  </tr>
  <tr>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/film.gif"" alt=""emoticons/film.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/phone.gif"" alt=""emoticons/phone.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/phone.gif"" alt=""emoticons/phone.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/kittykay.gif"" alt=""emoticons/kittykay.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/coffee.gif"" alt=""emoticons/coffee.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/lightbulb.gif"" alt=""emoticons/lightbulb.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/moon.gif"" alt=""emoticons/moon.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/star.gif"" alt=""emoticons/star.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/musical_note.gif"" alt=""emoticons/musical_note.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/envelope.gif"" alt=""emoticons/envelope.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/cake.gif"" alt=""emoticons/cake.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/clock.gif"" alt=""emoticons/clock.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/messenger.gif"" alt=""emoticons/messenger.gif"" />
    </td>
  </tr>
  <tr>
    <td class=""TableCell"">(~)</td>
    <td class=""TableCell"">(T)</td>
    <td class=""TableCell"">(t)</td>
    <td class=""TableCell"">(@)</td>
    <td class=""TableCell"">(c)</td>
    <td class=""TableCell"">(i)</td>
    <td class=""TableCell"">(S)</td>
    <td class=""TableCell"">(*)</td>
    <td class=""TableCell"">(8)</td>
    <td class=""TableCell"">(E)</td>
    <td class=""TableCell"">(^)</td>
    <td class=""TableCell"">(O)</td>
    <td class=""TableCell"">(M)</td>
  </tr>
  <tr>
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
  </tr>
  <tr>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/tounge_smile.gif"" alt=""emoticons/tounge_smile.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/clock.gif"" alt=""emoticons/clock.gif"" />
    </td>
    <td class=""TableCell"">
      <img src=""/FlexWiki/emoticons/teeth_smile.gif"" alt=""emoticons/teeth_smile.gif"" />
    </td>
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
  </tr>
  <tr>
    <td class=""TableCell"">:-P</td>
    <td class=""TableCell"">(o)</td>
    <td class=""TableCell"">:-D</td>
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
    <td class=""TableCell"" />
  </tr>
</table>
<p />
<p>I need a little flag (red maybe <img src=""/FlexWiki/emoticons/regular_smile.gif"" alt=""emoticons/regular_smile.gif"" />)</p>
<p />
<p>
  <img src=""/FlexWiki/emoticons/regular_smile.gif"" alt=""emoticons/regular_smile.gif"" />
</p>
<p>:D</p>
<p>
  <img src=""/FlexWiki/emoticons/teeth_smile.gif"" alt=""emoticons/teeth_smile.gif"" />
</p>
<p>:/</p>
<p>:-/</p>
<p>:\</p>
<p>:-\</p>
<p>:p</p>
<p>:-p</p>
<p>:P</p>
<p>
  <img src=""/FlexWiki/emoticons/tounge_smile.gif"" alt=""emoticons/tounge_smile.gif"" />
</p>
<p />
<p>(`) (1) (2) (3) (4) (5) <img src=""/FlexWiki/emoticons/devil_smile.gif"" alt=""emoticons/devil_smile.gif"" /> (7) <img src=""/FlexWiki/emoticons/musical_note.gif"" alt=""emoticons/musical_note.gif"" /> (9) (0) (-) (=)</p>
<p>(q) <img src=""/FlexWiki/emoticons/wilted_rose.gif"" alt=""emoticons/wilted_rose.gif"" /><img src=""/FlexWiki/emoticons/envelope.gif"" alt=""emoticons/envelope.gif"" /> (r) <img src=""/FlexWiki/emoticons/phone.gif"" alt=""emoticons/phone.gif"" /><img src=""/FlexWiki/emoticons/thumbs_up.gif"" alt=""emoticons/thumbs_up.gif"" /><img src=""/FlexWiki/emoticons/broken_heart.gif"" alt=""emoticons/broken_heart.gif"" /><img src=""/FlexWiki/emoticons/lightbulb.gif"" alt=""emoticons/lightbulb.gif"" /><img src=""/FlexWiki/emoticons/clock.gif"" alt=""emoticons/clock.gif"" /><img src=""/FlexWiki/emoticons/camera.gif"" alt=""emoticons/camera.gif"" /> ([) (]) (\)</p>
<p>
  <img src=""/FlexWiki/emoticons/angel_smile.gif"" alt=""emoticons/angel_smile.gif"" /> (s) <img src=""/FlexWiki/emoticons/martini_shaken.gif"" alt=""emoticons/martini_shaken.gif"" /><img src=""/FlexWiki/emoticons/rose.gif"" alt=""emoticons/rose.gif"" /><img src=""/FlexWiki/emoticons/present.gif"" alt=""emoticons/present.gif"" /><img src=""/FlexWiki/emoticons/shades_smile.gif"" alt=""emoticons/shades_smile.gif"" /> (j) <img src=""/FlexWiki/emoticons/kiss.gif"" alt=""emoticons/kiss.gif"" /><img src=""/FlexWiki/emoticons/heart.gif"" alt=""emoticons/heart.gif"" /> (<img src=""/FlexWiki/emoticons/wink_smile.gif"" alt=""emoticons/wink_smile.gif"" /> (')</p>
<p>
  <img src=""/FlexWiki/emoticons/guy_handsacrossamerica.gif"" alt=""emoticons/guy_handsacrossamerica.gif"" />
  <img src=""/FlexWiki/emoticons/girl_handsacrossamerica.gif"" alt=""emoticons/girl_handsacrossamerica.gif"" />
  <img src=""/FlexWiki/emoticons/coffee.gif"" alt=""emoticons/coffee.gif"" /> (v) <img src=""/FlexWiki/emoticons/beer_yum.gif"" alt=""emoticons/beer_yum.gif"" /><img src=""/FlexWiki/emoticons/thumbs_down.gif"" alt=""emoticons/thumbs_down.gif"" /><img src=""/FlexWiki/emoticons/messenger.gif"" alt=""emoticons/messenger.gif"" /> (,) (.) (/)</p>
<p>() () () () () () () () () () () () () </p>
<p />
<p>
  <img src=""/FlexWiki/emoticons/film.gif"" alt=""emoticons/film.gif"" /> (!) <img src=""/FlexWiki/emoticons/kittykay.gif"" alt=""emoticons/kittykay.gif"" /> ($) (%) <img src=""/FlexWiki/emoticons/cake.gif"" alt=""emoticons/cake.gif"" /> (&amp;) <img src=""/FlexWiki/emoticons/star.gif"" alt=""emoticons/star.gif"" /> (() ()) (_) (+)</p>
<p>(Q) <img src=""/FlexWiki/emoticons/wilted_rose.gif"" alt=""emoticons/wilted_rose.gif"" /><img src=""/FlexWiki/emoticons/envelope.gif"" alt=""emoticons/envelope.gif"" /> (R) <img src=""/FlexWiki/emoticons/phone.gif"" alt=""emoticons/phone.gif"" /><img src=""/FlexWiki/emoticons/thumbs_up.gif"" alt=""emoticons/thumbs_up.gif"" /><img src=""/FlexWiki/emoticons/broken_heart.gif"" alt=""emoticons/broken_heart.gif"" /><img src=""/FlexWiki/emoticons/lightbulb.gif"" alt=""emoticons/lightbulb.gif"" /><img src=""/FlexWiki/emoticons/clock.gif"" alt=""emoticons/clock.gif"" /><img src=""/FlexWiki/emoticons/camera.gif"" alt=""emoticons/camera.gif"" /><img src=""/FlexWiki/emoticons/dude_hug.gif"" alt=""emoticons/dude_hug.gif"" /><img src=""/FlexWiki/emoticons/girl_hug.gif"" alt=""emoticons/girl_hug.gif"" /> (|)</p>
<p>
  <img src=""/FlexWiki/emoticons/angel_smile.gif"" alt=""emoticons/angel_smile.gif"" />
  <img src=""/FlexWiki/emoticons/moon.gif"" alt=""emoticons/moon.gif"" />
  <img src=""/FlexWiki/emoticons/martini_shaken.gif"" alt=""emoticons/martini_shaken.gif"" />
  <img src=""/FlexWiki/emoticons/rose.gif"" alt=""emoticons/rose.gif"" />
  <img src=""/FlexWiki/emoticons/present.gif"" alt=""emoticons/present.gif"" />
  <img src=""/FlexWiki/emoticons/shades_smile.gif"" alt=""emoticons/shades_smile.gif"" /> (J) <img src=""/FlexWiki/emoticons/kiss.gif"" alt=""emoticons/kiss.gif"" /><img src=""/FlexWiki/emoticons/heart.gif"" alt=""emoticons/heart.gif"" /> (<img src=""/FlexWiki/emoticons/regular_smile.gif"" alt=""emoticons/regular_smile.gif"" /> ("")</p>
<p>
  <img src=""/FlexWiki/emoticons/guy_handsacrossamerica.gif"" alt=""emoticons/guy_handsacrossamerica.gif"" />
  <img src=""/FlexWiki/emoticons/girl_handsacrossamerica.gif"" alt=""emoticons/girl_handsacrossamerica.gif"" />
  <img src=""/FlexWiki/emoticons/coffee.gif"" alt=""emoticons/coffee.gif"" /> (V) <img src=""/FlexWiki/emoticons/beer_yum.gif"" alt=""emoticons/beer_yum.gif"" /><img src=""/FlexWiki/emoticons/thumbs_down.gif"" alt=""emoticons/thumbs_down.gif"" /><img src=""/FlexWiki/emoticons/messenger.gif"" alt=""emoticons/messenger.gif"" /> (&lt;) (&gt;) (?)</p>
<p />
<p />
<p>Here is some more text.</p>";
            Assert.AreEqual(expected, xsltOut);
        }
    }
}
