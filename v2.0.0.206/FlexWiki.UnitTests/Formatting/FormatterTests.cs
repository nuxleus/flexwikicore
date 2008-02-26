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
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Reflection;

using NUnit.Framework;

using FlexWiki;
using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki.UnitTests.Formatting
{
    /// <summary>
    /// General tests for <see cref="FlexWiki.Formatting.Formatter"/>. If you have a 
    /// bunch of related tests rather than a single, standalone test, consider creating
    /// a new class in this folder instead of putting the tests here. 
    /// </summary>
    [TestFixture]
    public class FormatterTests : FormattingTestsBase
    {
        [Test]
        public void AllNamespacesBehavior()
        {
            FormatTestContains("@@AllNamespacesWithDetails@@", "FlexWiki");
            FormatTestContains("@@AllNamespacesWithDetails@@", "Friendly Title");
        }
        [Test]
        public void Ampersand()
        {
            // Since & sign is not a valid html character also veryify that the & sign is correct when it is not in a URL
            FormatTest(
                @"this test should make the & sign a freindly HTML element",
                @"<p>this test should make the &amp; sign a freindly HTML element</p>
");
        }
        [Test]
        public void BasicWikinames()
        {
            FormatTest(
              @"LinkThis, AndLinkThis, dontLinkThis, (LinkThis), _LinkAndEmphasisThis_ *LinkAndBold* (LinkThisOneToo)",
                @"<p><a title=""Click here to create this topic"" class=""create"" href="""
                    + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("LinkThis"))
                    + @""">LinkThis</a>, <a title=""Click here to create this topic"" class=""create"" href="""
                    + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("AndLinkThis"))
                    + @""">AndLinkThis</a>, dontLinkThis, (<a title=""Click here to create this topic"" class=""create"" href="""
                    + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("LinkThis"))
                    + @""">LinkThis</a>), <em><a title=""Click here to create this topic"" class=""create"" href="""
                    + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("LinkAndEmphasisThis"))
                    + @""">LinkAndEmphasisThis</a></em> <strong><a title=""Click here to create this topic"" class=""create"" href="""
                    + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("LinkAndBold"))
                    + @""">LinkAndBold</a></strong> (<a title=""Click here to create this topic"" class=""create"" href="""
                    + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("LinkThisOneToo")) + @""">LinkThisOneToo</a>)</p>
");
        }
        [Test]
        public void BracketedLinks()
        {
            string s = FormattedTestText(@"[BigBox] summer [eDocuments] and [e] and [HelloWorld] and [aZero123]");
            AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("BigBox")) + @""">BigBox</a>");
            AssertStringContains(s, @"<a title=""Click here to create this topic"" class=""create"" href=""" +
                _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("eDocuments")) +
                @""">eDocuments</a> and <a title=""Click here to create this topic"" class=""create"" href=""" +
                _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("e")) +
                @""">e</a> and <a title=""Click here to create this topic"" class=""create"" href=""" +
                _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("HelloWorld")) +
                @""">HelloWorld</a> and <a title=""Click here to create this topic"" class=""create"" href=""" +
                _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("aZero123")) + @""">aZero123</a>");
        }
        [Test]
        public void ContainerTests()
        {
            // Div tests.
            FormatTest("@@Presentations.ContainerStart(\"div\")@@", @"<div>
");
            FormatTest("@@Presentations.ContainerStart(\"div\", \"test\")@@", @"<div id=""test"">
");
            FormatTest("@@Presentations.ContainerStart(\"div\", \"test\", \"style\")@@", @"<div id=""test"" class=""style"">
");
            FormatTest("@@Presentations.ContainerEnd(\"div\")@@", @"</div>
");
            // Span tests.
            FormatTest("@@Presentations.ContainerStart(\"span\")@@", @"<span>
");
            FormatTest("@@Presentations.ContainerStart(\"span\", \"test\")@@", @"<span id=""test"">
");
            FormatTest("@@Presentations.ContainerStart(\"span\", \"test\", \"style\")@@", @"<span id=""test"" class=""style"">
");
            FormatTest("@@Presentations.ContainerEnd(\"span\")@@", @"</span>
");
            // Invalid type test.
            FormatTest("@@Presentations.ContainerStart(\"script\")@@", @"<span class=""ErrorMessage""><span class=""ErrorMessageBody"">Invalid 'type' parameter. Must be 'span' or 'div'.</span></span>
");
        }
        [Test]
        public void CodeFormatting()
        {
            FormatTest("@Namespace.Class@", @"<p><code>Namespace.Class</code></p>
");
            FormatTest("@Name.Space.Class@", @"<p><code>Name.Space.Class</code></p>
");
            FormatTest(
                @"These values should be code formatted, not external Wiki references @""""IObjectWithSite""""@ and @""""IViewObject""""@. This should also be (@code formatted@) inside of the parens, and @""""PropertyManager.RegisterProperty""""@ should work...",
                @"<p>These values should be code formatted, not external Wiki references <code>IObjectWithSite</code> and <code>IViewObject</code>. This should also be (<code>code formatted</code>) inside of the parens, and <code>PropertyManager.RegisterProperty</code> should work...</p>
");
            FormatTest(
                @"These values should be Wiki references and code formatted, @[IObjectWithSite]@ and @[IViewObject]@. This should also be (@code formatted@) inside of the parens, and @PropertyManager.RegisterProperty@ should work...",
                @"<p>These values should be Wiki references and code formatted, <code><a title=""Click here to create this topic"" class=""create"" href=""/formattingtestswiki/WikiEdit.aspx?topic=FlexWiki.IObjectWithSite"">IObjectWithSite</a></code> and <code><a title=""Click here to create this topic"" class=""create"" href=""/formattingtestswiki/WikiEdit.aspx?topic=FlexWiki.IViewObject"">IViewObject</a></code>. This should also be (<code>code formatted</code>) inside of the parens, and <code>PropertyManager.RegisterProperty</code> should work...</p>
");
            FormatTest(
              @"These values should be code formatted, not external Wiki references @IObjectWithSite@ and @IViewObject@. This should also be (@code formatted@) inside of the parens, and @PropertyManager.RegisterProperty@ should work...",
              @"<p>These values should be code formatted, not external Wiki references <code>IObjectWithSite</code> and <code>IViewObject</code>. This should also be (<code>code formatted</code>) inside of the parens, and <code>PropertyManager.RegisterProperty</code> should work...</p>
");
            FormatTest(
              @"The text in the parens and brackets should be bold:
(*hello*) [*world*] [*hello world*]
And the text in the parens and brackets should be code formatted:
(@hello@) [@world@] [@hello world@]",
              @"<p>The text in the parens and brackets should be bold:</p>
<p>(<strong>hello</strong>) [<strong>world</strong>] [<strong>hello world</strong>]</p>
<p>And the text in the parens and brackets should be code formatted:</p>
<p>(<code>hello</code>) [<code>world</code>] [<code>hello world</code>]</p>
");
        }
        [Test]
        public void ColorAndTextSizeTests()
        {
            FormatTest(
                @"%% by itself %% and again %%",
                @"<p>%% by itself %% and again %%</p>
");
            FormatTest(
                @"%red% red %% and now %% by itself",
                @"<p><span style=""color:red""> red </span> and now %% by itself</p>
");
            FormatTest(
                @"%red%red %blue%blue%% but this text is normal.",
                @"<p><span style=""color:red"">red </span><span style=""color:blue"">blue</span> but this text is normal.</p>
");
            FormatTest(
                @"%red%Red and no %big%closing %small%percentpercent
Normal again
",
                @"<p><span style=""color:red"">Red and no </span><big>closing </big><small>percentpercent</small></p>
<p>Normal again</p>
");
            FormatTest(
                @"Strange %#13579B%Color %dima% non-color",
                @"<p>Strange <span style=""color:#13579b"">Color </span><span style=""color:dima""> non-color</span></p>
");
            FormatTest(
                @"'''%red big%Big bold red text''' %small green%''Small green italic''%%normal",
                @"<p><strong><span style=""color:red""><big>Big bold red text</strong> </big></span><small><span style=""color:green""><em>Small green italic</em></small></span>normal</p>
");
            FormatTest(
                @"normal %big big%Very big %% normal %small small% very small %blue% normal size blue",
                @"<p>normal <big><big>Very big </big></big> normal <small><small> very small </small></small><span style=""color:blue""> normal size blue</span></p>
");
        }
        [Test]
        public void ComplexLinkTests()
        {
            FormatTestContains(
                @"[_Underscore] +and+ *@@[""after""]@@*",
                _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("_Underscore")));
            FormatTestContains(
                @"[_Underscore] +and+ after@Google",
                _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("_Underscore")));
        }
        [Test]
        public void DollarInLinkTest()
        {
            FormatTest(
              @"""Main directory"":file://servername/umuff$/folder%20name/file.txt
""Main directory"":[file://servername/umuff$/folder%20name/file.txt]
file://servername/umuff$/folder%20name/file.txt",
              @"<p><a class=""externalLink"" href=""file://servername/umuff$/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff$/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff$/folder%20name/file.txt"">file://servername/umuff$/folder%20name/file.txt</a></p>
");
        }
        [Test]
        public void EmoticonTest()
        {
            // Neither DisableWikiEmoticns or DisableNamespaceEmoticons explicitly set. Both use default value of false
            FormatTest(
              @":-) :-(",
              @"<p><img src=""" + _lm.LinkToImage("emoticons/regular_smile.gif") + @"""/> <img src=""" + _lm.LinkToImage("emoticons/sad_smile.gif") + @"""/></p>
");
            // Set DisableNamespaceEmoticons explicitly to true. DisableWikiEmoticons is default value of false.
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "_ContentBaseDefinition", @"
Title: Friendly Title
DisableNamespaceEmoticons: true
", _user);
            FormatTest(
              @":-) :-(",
              @"<p>:-) :-(</p>
");

            // Set DisableNamespaceEmoticons explicitly to false. DisableWikiEmoticons is default value of false.
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "_ContentBaseDefinition", @"
Title: Friendly Title
DisableNamespaceEmoticons: false
", _user);
            FormatTest(
              @":-) :-(",
              @"<p><img src=""" + _lm.LinkToImage("emoticons/regular_smile.gif") + @"""/> <img src=""" + _lm.LinkToImage("emoticons/sad_smile.gif") + @"""/></p>
");

            // Set DisableNamespaceEmoticons explicitly to false. Set DisableWikiEmoticons explicitly to true.
            IMockWikiApplication testApp = (IMockWikiApplication)Federation.Application;
            testApp.SetApplicationProperty("DisableWikiEmoticons", true);
            FormatTest(
              @":-) :-(",
              @"<p>:-) :-(</p>
");

            // Set DisableNamespaceEmoticons explicitly to true. Set DisableWikiEmoticons explicitly to true.
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "_ContentBaseDefinition", @"
Title: Friendly Title
DisableNamespaceEmoticons: true
", _user);
            FormatTest(
              @":-) :-(",
              @"<p>:-) :-(</p>
");

            // Set DisableNamespaceEmoticons explicitly to true. Set DisableWikiEmoticons explicitly to false.
            testApp.SetApplicationProperty("DisableWikiEmoticons", false);
            FormatTest(
              @":-) :-(",
              @"<p>:-) :-(</p>
");

            // Set DisableNamespaceEmoticons explicitly to false. Set DisableWikiEmoticons explicitly to false.
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "_ContentBaseDefinition", @"
Title: Friendly Title
DisableNamespaceEmoticons: false
", _user);
            FormatTest(
              @":-) :-(",
              @"<p><img src=""" + _lm.LinkToImage("emoticons/regular_smile.gif") + @"""/> <img src=""" + _lm.LinkToImage("emoticons/sad_smile.gif") + @"""/></p>
");

            // DisableNamespaceEmoticons is set to default value of false. Set DisableWikiEmoticons explicitly to false.
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "_ContentBaseDefinition", @"
Title: Friendly Title
", _user);
            FormatTest(
              @":-) :-(",
              @"<p><img src=""" + _lm.LinkToImage("emoticons/regular_smile.gif") + @"""/> <img src=""" + _lm.LinkToImage("emoticons/sad_smile.gif") + @"""/></p>
");

            // DisableNamespaceEmoticons is set to default value of false. Set DisableWikiEmoticons explicitly to true.
            testApp.SetApplicationProperty("DisableWikiEmoticons", true);
            FormatTest(
              @":-) :-(",
              @"<p>:-) :-(</p>
");
            
        }
        [Test]
        public void EmphasisInLinkTest()
        {
            FormatTest(
                @"""_Main directory_"":[file://servername/_default/orange_futurism/file.txt]",
                @"<p><a class=""externalLink"" href=""file://servername/_default/orange_futurism/file.txt""><em>Main directory</em></a></p>
");

            FormatTest(
                @"http://localhost/_default/orange_futurism/portal.jpg",
                @"<p><img src=""http://localhost/_default/orange_futurism/portal.jpg""/></p>
");

            FormatTest(
              @"""Main directory"":file://servername/_default/orange_futurism/file.txt",
              @"<p><a class=""externalLink"" href=""file://servername/_default/orange_futurism/file.txt"">Main directory</a></p>
");
            FormatTest(
              @"""Main directory"":[file://servername/_default/orange_futurism/file.txt]",
              @"<p><a class=""externalLink"" href=""file://servername/_default/orange_futurism/file.txt"">Main directory</a></p>
");
            FormatTest(
                @"file://servername/_default/orange_futurism/file.txt",
                @"<p><a class=""externalLink"" href=""file://servername/_default/orange_futurism/file.txt"">file://servername/_default/orange_futurism/file.txt</a></p>
");
        }
        [Test]
        public void ExtendedPreFormatting()
        {
            FormatTest(
                @"{+
%blue%public void%% '''Foo'''(%blue%string%% bar);
}+",
                @"<pre>
<span style=""color:blue"">public void</span> <strong>Foo</strong>(<span style=""color:blue"">string</span> bar);
</pre>
");
        }
        [Test]
        public void ExternalLinks()
        {
            FormatTest(
                @"A ""xxxx FooBar 0x(ToolTipText)"":http://localhost link.",
                @"<p>A <a class=""externalLink"" title=""ToolTipText"" href=""http://localhost"">xxxx FooBar 0x</a> link.</p>
");
            FormatTest(
                @"A ""xxxx FooBar 0x(ToolTipText) "":http://localhost link.",
                @"<p>A <a class=""externalLink"" href=""http://localhost"">xxxx FooBar 0x(ToolTipText)</a> link.</p>
");
            FormatTest(
                @"A ""xxxx FooBar 0x"":http://localhost link.",
                @"<p>A <a class=""externalLink"" href=""http://localhost"">xxxx FooBar 0x</a> link.</p>
");

            FormatTestContains(
              @"A ""xxxx FooBar 01"":HomePage link",
              @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("HomePage")) + @""">xxxx FooBar 01</a> link");

            FormatTestContains(
              @"A ""MyHomePage"":HomePage2 topic",
              @"href=""" + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("HomePage2")) + @""">MyHomePage</a> topic");

            FormatTest(
              @"A ""xxxx FooBar 1x"":[http://server1], ""xxxx FooBar 11"":[http://server2] link.",
              @"<p>A <a class=""externalLink"" href=""http://server1"">xxxx FooBar 1x</a>, <a class=""externalLink"" href=""http://server2"">xxxx FooBar 11</a> link.</p>
");

            FormatTest(
              @"My ""xxxx FooBar 2x"":[https://www.flexwiki.com] link",
              @"<p>My <a class=""externalLink"" href=""https://www.flexwiki.com"">xxxx FooBar 2x</a> link</p>
");

            FormatTest(
              @"""xxxx FooBar 21"":[notes:\\servername]",
              @"<p><a class=""externalLink"" href=""notes:\\servername"">xxxx FooBar 21</a></p>
");

            FormatTest(
              @"""xxxx FooBar 23"":[file:\\xyz\asdf.doc]",
              @"<p><a class=""externalLink"" href=""file:\\xyz\asdf.doc"">xxxx FooBar 23</a></p>
");

            FormatTest(
              @"""xxxx FooBar 24"":[mailto:info@flexwiki.com]",
              @"<p><a class=""externalLink"" href=""mailto:info@flexwiki.com"">xxxx FooBar 24</a></p>
");

            FormatTest(
              @"""xxxx FooBar 25"":mailto:info@flexwiki.com",
              @"<p><a class=""externalLink"" href=""mailto:info@flexwiki.com"">xxxx FooBar 25</a></p>
");

            FormatTest(
              @"mailto:info@flexwiki.com",
              @"<p><a class=""externalLink"" href=""mailto:info@flexwiki.com"">mailto:info@flexwiki.com</a></p>
");


            FormatTest(
              @"""xxxx FooBar 3x"":[notes://Server/file.nsf/viename?openView]",
              @"<p><a class=""externalLink"" href=""notes://Server/file.nsf/viename?openView"">xxxx FooBar 3x</a></p>
");

            FormatTest(
              @"""asfasdf asdasfd (asdf) asdf"":http://localhost",
              @"<p><a class=""externalLink"" href=""http://localhost"">asfasdf asdasfd (asdf) asdf</a></p>
");
        }
        [Test]
        public void FileLinks()
        {
            FormatTest(
              @"http://www.microsoft.com/download/word.doc",
              @"<p><a class=""externalLink"" href=""http://www.microsoft.com/download/word.doc"">http://www.microsoft.com/download/word.doc</a></p>
");

            FormatTest(
              @"http://www.microsoft.com/download/excel.xls",
              @"<p><a class=""externalLink"" href=""http://www.microsoft.com/download/excel.xls"">http://www.microsoft.com/download/excel.xls</a></p>
");

            FormatTest(
              @"http://www.microsoft.com/download/powerpoint.ppt",
              @"<p><a class=""externalLink"" href=""http://www.microsoft.com/download/powerpoint.ppt"">http://www.microsoft.com/download/powerpoint.ppt</a></p>
");

            FormatTest(
                @"test file:\\server\share\directory\mydoc.doc",
                @"<p>test <a class=""externalLink"" href=""file:\\server\share\directory\mydoc.doc"">file:\\server\share\directory\mydoc.doc</a></p>
");

            FormatTest(
              @"file:\\server\share\directory\mydoc.doc",
              @"<p><a class=""externalLink"" href=""file:\\server\share\directory\mydoc.doc"">file:\\server\share\directory\mydoc.doc</a></p>
");

            FormatTest(
              @"file://server\share\directory\mydoc1.jpg",
              @"<p><a class=""externalLink"" href=""file://server\share\directory\mydoc1.jpg"">file://server\share\directory\mydoc1.jpg</a></p>
");

            FormatTest(
              @"file:\\server\share\directory\mydoc2.doc",
              @"<p><a class=""externalLink"" href=""file:\\server\share\directory\mydoc2.doc"">file:\\server\share\directory\mydoc2.doc</a></p>
");

            FormatTest(
              @"file://server\share\directory\mydoc3.jpg",
              @"<p><a class=""externalLink"" href=""file://server\share\directory\mydoc3.jpg"">file://server\share\directory\mydoc3.jpg</a></p>
");

            FormatTest(
              @"notes://Server/file.nsf/viename?openView",
              @"<p><a class=""externalLink"" href=""notes://Server/file.nsf/viename?openView"">notes://Server/file.nsf/viename?openView</a></p>
");

            FormatTest(
              @"ms-help:\\Server\file.nsf\viename?openView",
              @"<p><a class=""externalLink"" href=""ms-help:\\Server\file.nsf\viename?openView"">ms-help:\\Server\file.nsf\viename?openView</a></p>
");
        }
        [Test]
        public void Hash126InLink()
        {
            FormatTest(
              @"""Main directory"":file://servername/umuff&#126;/folder%20name/file.txt
""Main directory"":[file://servername/umuff&#126;/folder%20name/file.txt]
file://servername/umuff&#126;/folder%20name/file.txt",
              @"<p><a class=""externalLink"" href=""file://servername/umuff&#126;/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff&#126;/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff&#126;/folder%20name/file.txt"">file://servername/umuff&#126;/folder%20name/file.txt</a></p>
");
        }
        [Test]
        public void HttpsImageLink()
        {
            FormatTest(
                @"https://www.microsoft.com/billgates/images/sofa-bill.jpg",
                @"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.jpg""/></p>
");
            FormatTest(
                @"https://www.microsoft.com/billgates/images/sofa-bill.JPG",
                @"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.JPG""/></p>
");
            FormatTest(
                @"https://www.microsoft.com/billgates/images/sofa-bill.png",
                @"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.png""/></p>
");
            FormatTest(
                @"https://www.microsoft.com/billgates/images/sofa-bill.PNG",
                @"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.PNG""/></p>
");
            FormatTest(
                @"https://www.microsoft.com/billgates/images/sofa-bill.gif",
                @"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.gif""/></p>
");
            FormatTest(
                @"https://www.microsoft.com/billgates/images/SOFA-BILL.GIF",
                @"<p><img src=""https://www.microsoft.com/billgates/images/SOFA-BILL.GIF""/></p>
");
            FormatTest(
                @"https://www.microsoft.com/billgates/images/sofa-bill.jpeg",
                @"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.jpeg""/></p>
");
            FormatTest(
                @"https://www.microsoft.com/billgates/images/SOFA-BILL.JPEG",
                @"<p><img src=""https://www.microsoft.com/billgates/images/SOFA-BILL.JPEG""/></p>
");
            // Make sure we really need the period before the trigger extensions...
            // Look for a hyperlink, not an <img>
            FormatTestContains(
                @"https://www.microsoft.com/billgates/images/sofa-billjpeg",
                @"<a class=""externalLink"" href");
        }
        [Test]
        public void HeadingTests()
        {
            FormatTest("!Hey Dog", @"<h1><a name=""Hey Dog"" class=""Anchor""></a>Hey Dog</h1>

");
            FormatTest("!!Hey Dog", @"<h2><a name=""Hey Dog"" class=""Anchor""></a>Hey Dog</h2>

");
            FormatTest("!!!Hey Dog", @"<h3><a name=""Hey Dog"" class=""Anchor""></a>Hey Dog</h3>

");
            FormatTest("!!!!Hey Dog", @"<h4><a name=""Hey Dog"" class=""Anchor""></a>Hey Dog</h4>

");
            FormatTest("!!!!!Hey Dog", @"<h5><a name=""Hey Dog"" class=""Anchor""></a>Hey Dog</h5>

");
            FormatTest("!!!!!!Hey Dog", @"<h6><a name=""Hey Dog"" class=""Anchor""></a>Hey Dog</h6>

");
            FormatTest("!!!!!!!Hey Dog", @"<h7><a name=""Hey Dog"" class=""Anchor""></a>Hey Dog</h7>

");
        }
        [Test]
        public void HyphensInImageLinkTest()
        {
            FormatTest(
              @"some leading text http://msdn.microsoft.com/library/en-us/dnpag/html/ch01---engineering-for-perf.gif some trailing text",
              @"<p>some leading text <img src=""http://msdn.microsoft.com/library/en-us/dnpag/html/ch01---engineering-for-perf.gif""/> some trailing text</p>
");
        }
        [Test]
        public void ImageLink()
        {
            FormatTest(
              @"http://www.microsoft.com/billgates/images/sofa-bill.jpg",
              @"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.jpg""/></p>
");
            FormatTest(
              @"http://www.microsoft.com/billgates/images/sofa-bill.JPG",
              @"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.JPG""/></p>
");
            FormatTest(
              @"http://www.microsoft.com/billgates/images/sofa-bill.png",
              @"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.png""/></p>
");
            FormatTest(
              @"http://www.microsoft.com/billgates/images/sofa-bill.PNG",
              @"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.PNG""/></p>
");
            FormatTest(
              @"http://www.microsoft.com/billgates/images/sofa-bill.gif",
              @"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.gif""/></p>
");
            FormatTest(
              @"http://www.microsoft.com/billgates/images/SOFA-BILL.GIF",
              @"<p><img src=""http://www.microsoft.com/billgates/images/SOFA-BILL.GIF""/></p>
");
            FormatTest(
              @"http://www.microsoft.com/billgates/images/sofa-bill.jpeg",
              @"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.jpeg""/></p>
");
            FormatTest(
              @"http://www.microsoft.com/billgates/images/SOFA-BILL.JPEG",
              @"<p><img src=""http://www.microsoft.com/billgates/images/SOFA-BILL.JPEG""/></p>
");
            // Make sure we really need the period before the trigger extensions...
            // Look for a hyperlink, not an <img>
            FormatTestContains(
              @"http://www.microsoft.com/billgates/images/sofa-billjpeg",
              @"<a class=""externalLink"" href");
        }
        [Test]
        public void IncludeFailure()
        {
            FormatTest(@"{{NoSuchTopic}}",
              @"<p>{{<a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("NoSuchTopic")) + @""">NoSuchTopic</a>}}</p>
");

        }
        [Test]
        public void IncludedTopicNoReadPermission()
        {
            WikiOutput output = WikiOutput.ForFormat(OutputFormat.HTML, null);
            QualifiedTopicRevision top = new QualifiedTopicRevision("TopicWithInclude", _namespaceManager.Namespace);
            Formatter.Format(top, Federation.NamespaceManagerForTopic(top).Read(top.LocalName), output,
                _namespaceManager, _lm, _externals, 0);
            string result = output.ToString();
            Assert.AreEqual("<span style=\"display:none\">.</span>\r\n", result,
                "Checking that the formatter returned no text for an included topic with DenyRead permission.");
        }
        [Test]
        public void InlineExternalReference()
        {
            FormatTest(
              @"@baf=http://www.baf.com/$$$
Again, TestIt@baf should be an external link along with TestItAgain@baf, however @this should be code formatted@.",
              @"<p>Again, <a class=""ExternalLink"" title=""External link to baf"" target=""ExternalLinks"" href=""http://www.baf.com/TestIt"">TestIt</a> should be an external link along with <a class=""ExternalLink"" title=""External link to baf"" target=""ExternalLinks"" href=""http://www.baf.com/TestItAgain"">TestItAgain</a>, however <code>this should be code formatted</code>.</p>
");
            FormatTest(
              @"@google=http://www.google.com/search?hl=en&ie=UTF-8&oe=UTF-8&q=$$$
ExternalTopic@google - verify the casing is correct.",
              @"<p><a class=""ExternalLink"" title=""External link to google"" target=""ExternalLinks"" href=""http://www.google.com/search?hl=en&ie=UTF-8&oe=UTF-8&q=ExternalTopic"">ExternalTopic</a> - verify the casing is correct.</p>
");
            FormatTest(
              @"@baf=http://www.baf.com/$$$
Let's test one that comes at the end of a sentence, such as EOSTest@baf.",
              @"<p>Let's test one that comes at the end of a sentence, such as <a class=""ExternalLink"" title=""External link to baf"" target=""ExternalLinks"" href=""http://www.baf.com/EOSTest"">EOSTest</a>.</p>
");
            FormatTest(
              @"@baf=http://www.baf.com/$$$
Test for case-insensitivity, such as CAPS@BAF, or some such nonsense.",
              @"<p>Test for case-insensitivity, such as <a class=""ExternalLink"" title=""External link to BAF"" target=""ExternalLinks"" href=""http://www.baf.com/CAPS"">CAPS</a>, or some such nonsense.</p>
");
        }
        [Test]
        public void InlineWikiTalk()
        {
            WikiOutput output = WikiOutput.ForFormat(OutputFormat.HTML, null);
            QualifiedTopicRevision top = new QualifiedTopicRevision("InlineTestTopic", _namespaceManager.Namespace);
            Formatter.Format(top, Federation.NamespaceManagerForTopic(top).Read(top.LocalName), output,
                _namespaceManager, _lm, _externals, 0);
            string result = output.ToString();
            Assert.IsTrue(result.IndexOf("aaa foo zzz") >= 0);
        }
        [Test]
        public void LinkAfterBangTests()
        {
            FormatTest(
              @"!HelloWorld",
              @"<h1><a name=""HelloWorld"" class=""Anchor""></a><a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("HelloWorld")) + @""">HelloWorld</a></h1>

");
        }
        [Test]
        public void ListAfterPreTest()
        {
            FormatTest(
                @" pre
	* hello
	* goodbye",
                @"<pre>
 pre
</pre>
<ul>
<li> hello</li>

<li> goodbye</li>

</ul>
");

            IMockWikiApplication testApp = (IMockWikiApplication)Federation.Application;
            testApp.SetApplicationProperty("RemoveListItemWhitespace", true);
            FormatTest(
    @" pre
	* hello
	* goodbye",
    @"<pre>
 pre
</pre>
<ul>
<li>hello</li>

<li>goodbye</li>

</ul>
");

            testApp.SetApplicationProperty("RemoveListItemWhitespace", false);
            FormatTest(
    @" pre
	* hello
	* goodbye",
    @"<pre>
 pre
</pre>
<ul>
<li> hello</li>

<li> goodbye</li>

</ul>
");

        }
        [Test]
        public void ListTests()
        {
            FormatTest(
              @"        1. item 1
        1. item 2",
              @"<ol>
<li> item 1</li>

<li> item 2</li>

</ol>
");

            FormatTest(
              @"	* level 1
		* level 2
		* level 2
	* level 1
	* level 1 (again!)",
              @"<ul>
<li> level 1</li>

<ul>
<li> level 2</li>

<li> level 2</li>

</ul>
<li> level 1</li>

<li> level 1 (again!)</li>

</ul>
");

            IMockWikiApplication testApp = (IMockWikiApplication)Federation.Application;
            testApp.SetApplicationProperty("RemoveListItemWhitespace", true);
            FormatTest(
              @"        1. item 1
        1. item 2",
              @"<ol>
<li>item 1</li>

<li>item 2</li>

</ol>
");

            FormatTest(
              @"	* level 1
		* level 2
		* level 2
	* level 1
	* level 1 (again!)",
              @"<ul>
<li>level 1</li>

<ul>
<li>level 2</li>

<li>level 2</li>

</ul>
<li>level 1</li>

<li>level 1 (again!)</li>

</ul>
");

            testApp.SetApplicationProperty("RemoveListItemWhitespace", false);
            FormatTest(
  @"        1. item 1
        1. item 2",
  @"<ol>
<li> item 1</li>

<li> item 2</li>

</ol>
");

            FormatTest(
              @"	* level 1
		* level 2
		* level 2
	* level 1
	* level 1 (again!)",
              @"<ul>
<li> level 1</li>

<ul>
<li> level 2</li>

<li> level 2</li>

</ul>
<li> level 1</li>

<li> level 1 (again!)</li>

</ul>
");

        }
        [Test]
        public void MailToLink()
        {
            FormatTest(
              @"Please send mailto:person@domain.com some email!",
              @"<p>Please send <a class=""externalLink"" href=""mailto:person@domain.com"">mailto:person@domain.com</a> some email!</p>
");

            FormatTest(
              @"Please send ""person"":mailto:person@domain.com some email!",
              @"<p>Please send <a class=""externalLink"" href=""mailto:person@domain.com"">person</a> some email!</p>
");
            FormatTest(
              @"Please send ""person"":[mailto:person@domain.com] some email!",
              @"<p>Please send <a class=""externalLink"" href=""mailto:person@domain.com"">person</a> some email!</p>
");
        }
        [Test]
        public void MultipleParametersHyperLinks()
        {
            // This test verifies the & sign can work in a URL
            FormatTest(
                @"http://www.google.com/search?sourceid=navclient&ie=UTF-8&oe=UTF-8",
                @"<p><a class=""externalLink"" href=""http://www.google.com/search?sourceid=navclient&ie=UTF-8&oe=UTF-8"">http://www.google.com/search?sourceid=navclient&ie=UTF-8&oe=UTF-8</a></p>
");

        }
        [Test]
        public void NamespaceAsTopicPreceedsQualifiedNames()
        {
            string s = FormattedTestText(@"FlexWiki bad FlexWiki.OneMinuteWiki");
            AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("FlexWiki")) + @""">FlexWiki</a> bad");
            AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("OneMinuteWiki")) + @""">OneMinuteWiki</a>");
        }
        [Test]
        public void NestedWikiSpec()
        {
            FormatTest(@"		{{IncludeNest}}", @"<h5><a name=""hey there"" class=""Anchor""></a>hey there</h5>
<h6><a name=""black dog"" class=""Anchor""></a>black dog</h6>
");
        }
        [Test]
        public void NonAsciiTopicNameRegexTest()
        {
            ShouldBeTopicName("DistribuciónTécnica");
            ShouldBeTopicName("ProblemasProgramación");
            ShouldBeTopicName("SmørgåsBord");
            ShouldBeTopicName("ØrneRede");
            ShouldBeTopicName("HöchstMaß");
            ShouldBeTopicName("FaçadePattern");
            ShouldBeTopicName("ReykjavíkCity");

            // string russian = "\u1044\u1086\u1093\u1083\u1072\u1103\u1056\u1099\u1073\u1072";
            // ShouldBeTopicName(russian);

            ShouldNotBeTopicName("æøå");
            ShouldNotBeTopicName("ÆØÅ");
            ShouldNotBeTopicName("Ølle");
            ShouldNotBeTopicName("ølle");
        }
        [Test]
        public void NonAsciiTopicNameWithAnchorRegexTest()
        {
            ShouldBeTopicName("DistribuciónTécnica#Distribución");
            ShouldBeTopicName("DistribuciónTécnica#distribución");
            ShouldBeTopicName("DistribuciónTécnica#DistribuciónTécnica");
            ShouldBeTopicName("DistribuciónTécnica#distribuciónTécnica");
            ShouldBeTopicName("ProblemasProgramación#Problemas");
            ShouldBeTopicName("ProblemasProgramación#problemas");
            ShouldBeTopicName("ProblemasProgramación#ProblemasProgramación");
            ShouldBeTopicName("ProblemasProgramación#problemasProgramación");
            ShouldBeTopicName("SmørgåsBord#Smørgås");
            ShouldBeTopicName("SmørgåsBord#smørgås");
            ShouldBeTopicName("SmørgåsBord#SmørgåsBord");
            ShouldBeTopicName("SmørgåsBord#smørgåsBord");
            ShouldBeTopicName("ØrneRede#Ørne");
            ShouldBeTopicName("ØrneRede#ØrneRede");
            ShouldBeTopicName("HöchstMaß#Höchst");
            ShouldBeTopicName("HöchstMaß#höchst");
            ShouldBeTopicName("HöchstMaß#HöchstMaß");
            ShouldBeTopicName("HöchstMaß#höchstMaß");
            ShouldBeTopicName("FaçadePattern#FaçadePattern");
            ShouldBeTopicName("FaçadePattern#façadePattern");
            ShouldBeTopicName("FaçadePattern#Façade");
            ShouldBeTopicName("FaçadePattern#façade");
            ShouldBeTopicName("ReykjavíkCity#Reykjavík");
            ShouldBeTopicName("ReykjavíkCity#reykjavík");
            ShouldBeTopicName("ReykjavíkCity#ReykjavíkCity");
            ShouldBeTopicName("ReykjavíkCity#reykjavíkCity");

            // string russian = "\u1044\u1086\u1093\u1083\u1072\u1103\u1056\u1099\u1073\u1072";
            // ShouldBeTopicName(russian);

            ShouldNotBeTopicName("æøå#æøå");
            ShouldNotBeTopicName("ÆØÅ#ÆØÅ");
            ShouldNotBeTopicName("Ølle#Ølle");
            ShouldNotBeTopicName("ølle#ølle");
        }
        [Test]
        public void NumberInTopicName()
        {
            FormatTest(
              @"This is Some2Link to a topic.",
              @"<p>This is <a title=""Click here to create this topic"" class=""create"" href=""" +
              _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("Some2Link")) +
              @""">Some2Link</a> to a topic.</p>
");
        }
        [Test]
        public void PlainInOut()
        {
            FormatTest("Hello there", "<p>Hello there</p>\n");
        }
        [Test]
        public void PluralLinkTests()
        {
            FormatTestContains(
              @"BigDogs",
              @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("BigDog")) + @""">BigDogs</a>");
            FormatTestContains(
              @"BigPolicies",
              @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("BigPolicy")) + @""">BigPolicies</a>");
            FormatTestContains(
              @"BigAddresses",
              @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("BigAddress")) + @""">BigAddresses</a>");
            FormatTestContains(
              @"BigBoxes",
              @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("BigBox")) + @""">BigBoxes</a>");

            // Test for plural before singular
            string s = FormattedTestText(@"See what happens when I mention BigBoxes; the topic is called BigBox.");
            AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("BigBox")) + @""">BigBoxes</a>");
            AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("BigBox")) + @""">BigBox</a>");
        }
        [Test]
        public void PreFormattedBlockTests()
        {
            FormatTest(
                @"
{@KeyWord
Some pre
forma-
ted
 text
  goes
   here
*non bold* ''etc''
} is allowed
}@ is allowed
}@non-key is allowed
}@ KeyWord 
{@
No key case
}@WrongKey
}@
Normal text here
 Regular preformated text
Normal again
",
                @"

<pre>
Some pre
forma-
ted
 text
  goes
   here
*non bold* ''etc''
} is allowed
}@ is allowed
}@non-key is allowed
</pre>
<pre>
No key case
}@WrongKey
</pre>
<p>Normal text here</p>
<pre>
 Regular preformated text
</pre>
<p>Normal again</p>
");
        }
        [Test]
        public void PreNoformatting()
        {
            FormatTest(
              @" :-) bold '''not''' BigDog    _foo_",
              @"<pre>
 :-) bold '''not''' BigDog    _foo_
</pre>
");
        }
        [Test]
        public void PreTest()
        {
            FormatTest(@"{@
this is @@ some pre stuff
}@
this is not",
            @"<pre>
this is @@ some pre stuff
</pre>
<p>this is not</p>
");
            FormatTest(@" this is @@ some pre stuff
this is not",
            @"<pre>
 this is @@ some pre stuff
</pre>
<p>this is not</p>
");
            FormatTest(
                @" pre
 pre

",
                @"<pre>
 pre
 pre
</pre>
");
            FormatTest(
                @" pre
 pre",
                @"<pre>
 pre
 pre
</pre>
");
        }
        [Test]
        public void ProcessLineElementRefactor()
        {
            // make sure that input string that contain the parameter tokens are properly escaped
            FormatTest(@"${USER}=${1} """"# comment"""" is """"here""""",
                @"<p>${USER}=${1} # comment is here</p>
");
            // original impl expected < 10 escaped parameters
            FormatTest(@"""""One"""" """"two"""" """"three"""" """"four"""" """"five"""" """"six"""" """"seven"""" """"eight"""" """"nine"""" """"ten"""" """"eleven"""" """"twelve"""" ",
                @"<p>One two three four five six seven eight nine ten eleven twelve </p>
");
            FormatTest(@"The ${0} is @@ProductVersion@@",
                @"<p>The ${0} is " + new ClassicBehaviors().ProductVersion + @"</p>
");
            FormatTest(@"The """"ProductVersion"""" is @@ProductVersion@@",
                @"<p>The ProductVersion is " + new ClassicBehaviors().ProductVersion + @"</p>
");
            FormatTest(@"The """"@@"""" is @@ProductVersion@@ and that is all there is to it",
                @"<p>The @@ is " + new ClassicBehaviors().ProductVersion + @" and that is all there is to it</p>
");
            FormatTest(@"The """"ProductVersion"""" is @@ProductVersion@@ and that is all there is to it",
                @"<p>The ProductVersion is " + new ClassicBehaviors().ProductVersion + @" and that is all there is to it</p>
");
        }
        [Test]
        public void PropertyAnchors()
        {
            FormatTest("Foo:Bar",
            @"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Foo</legend><div class=""PropertyValue""><a name=""Foo"" class=""Anchor"">Bar</a></div>
</fieldset>

");
            FormatTest(":Foo:Bar", @"<a name=""Foo"" class=""Anchor""></a>
");
            FormatTest(@"Foo:[ Bar
Baz
]",
            @"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Foo</legend><div class=""PropertyValue""><a name=""Foo"" class=""Anchor"">Bar<p>Baz</p>
</a></div>
</fieldset>
");
            FormatTest(@":Foo:[ Bar
Baz
]",
            @"<a name=""Foo"" class=""Anchor""></a>
");

        }
        [Test]
        public void PropertyBehavior()
        {
            FormatTest(@"@@Property(""TopicWithColor"", ""Color"")@@", @"Yellow
");
        }
        [Test]
        public void RealTopicWithAnchorTest()
        {
            FormatTestContains(
              "HomePage#Anchor",
              "href=\"" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("HomePage")) + "#Anchor\">HomePage#Anchor</a>");
        }
        [Test]
        public void RealTopicWithAnchorRelabelTest()
        {
            FormatTestContains(
              "\"Relabel\":HomePage#Anchor",
              "href=\"" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("HomePage")) + "#Anchor\">Relabel</a>");
        }
        [Test]
        public void RelabelTests()
        {
            string s = FormattedTestText(@"""tell me about dogs"":BigDog");
            AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("BigDog")) + @""">tell me about dogs</a>");
        }
        [Test]
        public void Rule()
        {
            FormatTestContains("----", "<div class=\"Rule\"></div>");
            FormatTestContains("------------------------", "<div class=\"Rule\"></div>");
        }
        [Test]
        public void SimpleEscapeTest()
        {
            FormatTest(@"-deleted """"HelloWorld"""" text-",
                @"<p><del>deleted HelloWorld text</del></p>
");
            FormatTest(@"'''Hello """"MrWorld"""" World'''", @"<p><strong>Hello MrWorld World</strong></p>
");

            FormatTest(@"Hello """" World", @"<p>Hello """" World</p>
");
            FormatTest(@"Hello """"World""""", @"<p>Hello World</p>
");
            FormatTest(@"Hello """"WouldBeLink""""", @"<p>Hello WouldBeLink</p>
");
            FormatTest(@"""""WouldBeLink""""", @"<p>WouldBeLink</p>
");
            FormatTest(@"""""WouldBeLink"""" and more ''ital''", @"<p>WouldBeLink and more <em>ital</em></p>
");
            FormatTest(@"""""WouldBeLink and more ''ital''""""", @"<p>WouldBeLink and more ''ital''</p>
");
            FormatTest(@"""""WouldBeLink"""" and """"----"""" line", @"<p>WouldBeLink and ---- line</p>
");
            FormatTest(@"""""", @"<p>""""</p>
");
            FormatTest(@"("""""""""""")", @"<p>("""")</p>
");
            FormatTest(@"("""")", @"<p>("""")</p>
");
        }
        [Test]
        public void SimpleLinkTests()
        {
            FormatTest(
              @"Some VerySimpleLinksFatPig
StartOfLineShouldLink blah blah blah
blah blah EndOfLineShouldLink",
              @"<p>Some <a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("VerySimpleLinksFatPig")) + @""">VerySimpleLinksFatPig</a></p>
<p><a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("StartOfLineShouldLink")) + @""">StartOfLineShouldLink</a> blah blah blah</p>
<p>blah blah <a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("EndOfLineShouldLink")) + @""">EndOfLineShouldLink</a></p>
");
        }
        [Test]
        public void SimpleWikiSpec()
        {
            FormatTest(@"!Head1
	{{IncludeOne}}
	{{IncludeTwo}}
		{{IncludeThree}}
	{{IncludeFour}}
", @"<h1><a name=""Head1"" class=""Anchor""></a>Head1</h1>

<p>inc1</p>
<h2><a name=""inc2"" class=""Anchor""></a>inc2</h2>
<h4><a name=""inc3"" class=""Anchor""></a>inc3</h4>
<h2><a name=""inc4"" class=""Anchor""></a>inc4</h2>
");
        }
        [Test]
        public void SpaceInHyperlinkTextTest()
        {
            FormatTest(
              @"My ""xxx FooBar xxx"":[http://www.flexwiki.com/FooBar/default.aspx] link.",
              @"<p>My <a class=""externalLink"" href=""http://www.flexwiki.com/FooBar/default.aspx"">xxx FooBar xxx</a> link.</p>
");
            FormatTest(
              @"My ""xxx FooBar xxx"":http://www.flexwiki.com/FooBar/default.aspx link.",
              @"<p>My <a class=""externalLink"" href=""http://www.flexwiki.com/FooBar/default.aspx"">xxx FooBar xxx</a> link.</p>
");
        }
        [Test]
        public void SpaceInLinks()
        {
            FormatTestContains(
              @"A [test test] new topic",
              @"href=""" + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("test test")) + @""">test test</a> new topic");

            FormatTestContains(
              @"A ""NewTest"":[test test] new topic",
              @"href=""" + _lm.LinkToEditTopic(_namespaceManager.QualifiedTopicNameFor("test test")) + @""">NewTest</a> new topic");

            //			FormatTest(
            //				@"""Spacelink"":http://localhost/space in link/test.htm",
            //				@"<p><a class=""externalLink"" href=""http://localhost/space%20in%20link/test.htm"">Spacelink</a></p>
            //");

            FormatTest(
              @"""Correct link"":[http://localhost/link to a page/test.htm]",
              @"<p><a class=""externalLink"" href=""http://localhost/link%20to%20a%20page/test.htm"">Correct link</a></p>
");
            FormatTest(
              @"[http://localhost/link to a page/test2.htm]",
              @"<p><a class=""externalLink"" href=""http://localhost/link%20to%20a%20page/test2.htm"">http://localhost/link to a page/test2.htm</a></p>
");
            FormatTest(
              @"[http://localhost/link to a page/test3.htm] and [http://localhost/link to a page/test4.htm]",
              @"<p><a class=""externalLink"" href=""http://localhost/link%20to%20a%20page/test3.htm"">http://localhost/link to a page/test3.htm</a> and <a class=""externalLink"" href=""http://localhost/link%20to%20a%20page/test4.htm"">http://localhost/link to a page/test4.htm</a></p>
");

        }
        [Test]
        public void TextileFormat()
        {
            // _emphasis_
            FormatTest(
                @"_emphasis_",
                @"<p><em>emphasis</em></p>
");
            // *strong* 
            FormatTest(
                @"*strong*",
                @"<p><strong>strong</strong></p>
");
            // ??citation?? 
            FormatTest(
                @"??citation??",
                @"<p><cite>citation</cite></p>
");
            // -deleted text- 
            FormatTest(
                @"AB -CD- -EF- GH -IJ-",
                @"<p>AB <del>CD</del> <del>EF</del> GH <del>IJ</del></p>
");
            FormatTest(
                @"-deleted text- undeleted text -more deleted text-",
                @"<p><del>deleted text</del> undeleted text <del>more deleted text</del></p>
");

            FormatTest(
                @"-deleted text-",
                @"<p><del>deleted text</del></p>
");
            // +inserted text+ 
            FormatTest(
                @"+inserted text+",
                @"<p><ins>inserted text</ins></p>
");
            // ^superscript^ 
            FormatTest(
                @"^superscript^",
                @"<p><sup>superscript</sup></p>
");
            // ~subscript~ 
            FormatTest(
                @"~1/2~",
                @"<p><sub>1/2</sub></p>
");

        }
        [Test]
        public void TextileHyphensInLinkTest()
        {
            FormatTest(
              @"some leading text ""textile"":http://www.amazon.com/exec/obidos/tg/detail/-/0735617228/qid=1080277573/sr=8-1/ref=pd_ka_1/102-9825269-6457731?v=glance&s=books&n=50784 some trailing text",
              @"<p>some leading text <a class=""externalLink"" href=""http://www.amazon.com/exec/obidos/tg/detail/-/0735617228/qid=1080277573/sr=8-1/ref=pd_ka_1/102-9825269-6457731?v=glance&s=books&n=50784"">textile</a> some trailing text</p>
");
        }
        [Test]
        public void TextileLinkTest()
        {
            FormatTest(@"RowDataGateway http://www.google.com", @"<p><a title=""Click here to create this topic"" class=""create"" href=""/formattingtestswiki/WikiEdit.aspx?topic=FlexWiki.RowDataGateway"">RowDataGateway</a> <a class=""externalLink"" href=""http://www.google.com"">http://www.google.com</a></p>
");
            FormatTest(@"RowDataGateway ""Google"":http://www.google.com", @"<p><a title=""Click here to create this topic"" class=""create"" href=""/formattingtestswiki/WikiEdit.aspx?topic=FlexWiki.RowDataGateway"">RowDataGateway</a> <a class=""externalLink"" href=""http://www.google.com"">Google</a></p>
");
        }
        [Test]
        public void TildeInLinkTest()
        {
            FormatTest(
              @"""Main directory"":file://servername/umuff~/folder%20name/file.txt
""Main directory"":[file://servername/umuff~/folder%20name/file.txt]
file://servername/umuff~/folder%20name/file.txt",
              @"<p><a class=""externalLink"" href=""file://servername/umuff~/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff~/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff~/folder%20name/file.txt"">file://servername/umuff~/folder%20name/file.txt</a></p>
");
        }
        [Test]
        public void TopicBehaviorProperty()
        {
            FormattedTopicContainsTest(new QualifiedTopicRevision("TestTopicWithBehaviorProperties", _namespaceManager.Namespace), "len=5");
            FormattedTopicContainsTest(new QualifiedTopicRevision("TestTopicWithBehaviorProperties", _namespaceManager.Namespace), "lenWith=6");
            FormattedTopicContainsTest(new QualifiedTopicRevision("TestTopicWithBehaviorProperties", _namespaceManager.Namespace), "lenSpanning=20");
        }
        [Test]
        public void TopicBehaviorPropertyBug()
        {
            // A simplified version of TopicBehaviorProperty to help track down a bug
            // introduced during the refactor. Looks like the WikiTalk interpreter got
            // screwed up by something in the NamespaceManager reorganization. 
            FormattedTopicContainsTest(new QualifiedTopicRevision("TestTopicWithBehaviorProperty", _namespaceManager.Namespace), "lenSpanning=20");
        }
        [Test]
        public void TopicNameRegexTests()
        {
            ShouldBeTopicName("AaA");
            ShouldBeTopicName("AAa");
            ShouldBeTopicName("AaAA");
            ShouldBeTopicName("AaAa");
            ShouldBeTopicName("ZAAbAA");
            ShouldBeTopicName("ZAAb");
            ShouldBeTopicName("ZaAA");
            ShouldBeTopicName("CSharp");
            ShouldBeTopicName("Meeting25Dec");
            ShouldBeTopicName("KeyOfficeIRMFunctionality");
            ShouldBeTopicName("IBMMainframe");

            ShouldNotBeTopicName("AAA");
            ShouldNotBeTopicName("AA");
            ShouldNotBeTopicName("A42");
            ShouldNotBeTopicName("A");
            ShouldNotBeTopicName("a");
            ShouldNotBeTopicName("about");
            ShouldNotBeTopicName("Hello");
        }
        [Test]
        public void TopicNameWithAnchorRegexTests()
        {
            ShouldBeTopicName("AaA#Anchor");
            ShouldBeTopicName("AaA#anchor");
            ShouldBeTopicName("AaA#TestAnchor");
            ShouldBeTopicName("AaA#testAnchor");
            ShouldBeTopicName("AAa#Anchor");
            ShouldBeTopicName("AAa#anchor");
            ShouldBeTopicName("AAa#TestAnchor");
            ShouldBeTopicName("AAa#testAnchor");
            ShouldBeTopicName("AaAA#Anchor");
            ShouldBeTopicName("AaAA#anchor");
            ShouldBeTopicName("AaAA#TestAnchor");
            ShouldBeTopicName("AaAA#aestAnchor");
            ShouldBeTopicName("AaAa#Anchor");
            ShouldBeTopicName("AaAa#anchor");
            ShouldBeTopicName("AaAa#TestAnchor");
            ShouldBeTopicName("AaAa#testAnchor");
            ShouldBeTopicName("ZAAbAA#Anchor");
            ShouldBeTopicName("ZAAbAA#anchor");
            ShouldBeTopicName("ZAAbAA#TestAnchor");
            ShouldBeTopicName("ZAAbAA#testAnchor");
            ShouldBeTopicName("ZAAb#Anchor");
            ShouldBeTopicName("ZAAb#anchor");
            ShouldBeTopicName("ZAAb#TestAnchor");
            ShouldBeTopicName("ZAAb#testAnchor");
            ShouldBeTopicName("ZaAA#Anchor");
            ShouldBeTopicName("ZaAA#anchor");
            ShouldBeTopicName("ZaAA#TestAnchor");
            ShouldBeTopicName("ZaAA#testAnchor");
            ShouldBeTopicName("CSharp#Anchor");
            ShouldBeTopicName("CSharp#anchor");
            ShouldBeTopicName("CSharp#TestAnchor");
            ShouldBeTopicName("CSharp#testAnchor");
            ShouldBeTopicName("Meeting25Dec#Anchor");
            ShouldBeTopicName("Meeting25Dec#anchor");
            ShouldBeTopicName("Meeting25Dec#TestAnchor");
            ShouldBeTopicName("Meeting25Dec#testAnchor");
            ShouldBeTopicName("KeyOfficeIRMFunctionality#Anchor");
            ShouldBeTopicName("KeyOfficeIRMFunctionality#anchor");
            ShouldBeTopicName("KeyOfficeIRMFunctionality#TestAnchor");
            ShouldBeTopicName("KeyOfficeIRMFunctionality#testAnchor");
            ShouldBeTopicName("IBMMainframe#Anchor");
            ShouldBeTopicName("IBMMainframe#anchor");
            ShouldBeTopicName("IBMMainframe#TestAnchor");
            ShouldBeTopicName("IBMMainframe#testAnchor");

            ShouldNotBeTopicName("AAA#Anchor");
            ShouldNotBeTopicName("AAA#anchor");
            ShouldNotBeTopicName("AAA#TestAnchor");
            ShouldNotBeTopicName("AAA#testAnchor");
            ShouldNotBeTopicName("AA#Anchor");
            ShouldNotBeTopicName("AA#anchor");
            ShouldNotBeTopicName("AA#TestAnchor");
            ShouldNotBeTopicName("AA#testAnchor");
            ShouldNotBeTopicName("A42#Anchor");
            ShouldNotBeTopicName("A42#anchor");
            ShouldNotBeTopicName("A42#TestAnchor");
            ShouldNotBeTopicName("A42#testAnchor");
            ShouldNotBeTopicName("A#Anchor");
            ShouldNotBeTopicName("A#anchor");
            ShouldNotBeTopicName("A#TestAnchor");
            ShouldNotBeTopicName("A#testAnchor");
            ShouldNotBeTopicName("a#Anchor");
            ShouldNotBeTopicName("a#anchor");
            ShouldNotBeTopicName("a#TestAnchor");
            ShouldNotBeTopicName("a#testAnchor");
            ShouldNotBeTopicName("about#Anchor");
            ShouldNotBeTopicName("about#anchor");
            ShouldNotBeTopicName("about#TestAnchor");
            ShouldNotBeTopicName("about#testAnchor");
            ShouldNotBeTopicName("Hello#Anchor");
            ShouldNotBeTopicName("Hello#anchor");
            ShouldNotBeTopicName("Hello#TestAnchor");
            ShouldNotBeTopicName("Hello#testAnchor");
        }
        [Test]
        public void UnderEmphasis()
        {
            FormatTest(
              @"This should be _emphasised_ however, id_Foo and id_Bar should not be.",
              @"<p>This should be <em>emphasised</em> however, id_Foo and id_Bar should not be.</p>
");
        }
        [Test]
        public void UnusualCharactersInLinkTest()
        {
            FormatTest(
              @"some leading text http://www.lemonde.fr/web/article/0,1-0@2-3212,36-358279,0.htm some trailing text",
              @"<p>some leading text <a class=""externalLink"" href=""http://www.lemonde.fr/web/article/0,1-0@2-3212,36-358279,0.htm"">http://www.lemonde.fr/web/article/0,1-0@2-3212,36-358279,0.htm</a> some trailing text</p>
");
        }
        [Test]
        public void WikiSignatureWithHyphenDates()
        {
            FormatTest(
                @"Test comment. -- Derek Lakin [28-Jan-2005]",
                @"<p>Test comment. -- Derek Lakin [28-Jan-2005]</p>
");
        }
        [Test]
        public void WikiURIForImageResource()
        {
            FormatTest(@"wiki://ResourceReference/Cookies.gif", @"<p><img src=""http://www.google.com/Cookies.gif""/></p>
");
        }
        [Test]
        public void WikiURIForResource()
        {
            FormatTest(@"wiki://ResourceReference/Cookies", @"<p><a class=""externalLink"" href=""http://www.google.com/Cookies"">http://www.google.com/Cookies</a></p>
");
        }
        [Test]
        public void WikiURIForTopic()
        {
            string s = FormattedTestText("wiki://IncludeNestURI");
            AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("IncludeNestURI")) + @""">IncludeNestURI</a>");
        }
        [Test]
        public void WikiURIForTopicProperty()
        {
            FormatTest(@"wiki://TopicWithColor/#Color", @"<p>Yellow</p>
");
        }
        

    }
}
