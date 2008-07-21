using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FlexWiki.Formatting;
using FlexWiki.Collections;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class WomDocumentTests
    {
        //private IParserApplication application;
        private Federation federation;
        private NamespaceManager manager;
        private ParserEngine parser;

        private MockFileSystem _fileSystem;
        private FileSystemStore _provider;

        private Federation Federation
        {
            get { return federation; }
        }

        private string Root
        {
            get
            {
                return @"C:\flexwiki\namespaces\namespaceone";
            }
        }

        [SetUp]
        public void SetUp()
        {
            MockWikiApplication application = new MockWikiApplication(new FederationConfiguration(),
                new LinkMaker("test://FileSystemStoreTests/"), OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            federation = new Federation(application);
            _fileSystem = new MockFileSystem(
                new MockDirectory(@"C:\",
                    new MockDirectory("flexwiki",
                        new MockDirectory("namespaces",
                            new MockDirectory("namespaceone",
                                new MockFile(@"TopicOne.wiki", new DateTime(2004, 10, 28), @"This is some content"),
                                new MockFile(@"TopicTwo.wiki", new DateTime(2004, 10, 29), @"This is some other content"),
                                new MockFile(@"MULTIcapsGoodTopic.wiki",
                                    new DateTime(2004, 11, 05), new DateTime(2007, 10, 22), @"", MockTopicStorePermissions.ReadOnly, true),

                                new MockFile(@"HomePage.wiki", new DateTime(2004, 10, 30), @"Home page."),
                                new MockFile(@"HomePage(2003-11-24-20-31-20-WINGROUP-davidorn).awiki",
                                    new DateTime(2004, 10, 31), @"Old home page."),

                                new MockFile(@"CodeImprovementIdeas.wiki", new DateTime(2004, 11, 10), @"Latest"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-03-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 09), @"Latest"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-04.8890-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 08), @"Older"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-05.1000-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 07), @"Still older"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-06.1-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 06), @"Even older"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-07-Name).awiki",
                                    new DateTime(2004, 11, 05), @"Really old"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-08.123-Name).awiki",
                                    new DateTime(2004, 11, 04), @"Oldest"),

                                new MockFile(@"TestDeleteHistory.wiki", new DateTime(2004, 11, 10), @"Latest"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-03-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 09), @"Latest"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-04.8890-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 08), @"Older"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-05.1000-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 07), @"Still older"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-06.1-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 06), @"Even older"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-07-Name).awiki",
                                    new DateTime(2004, 11, 05), @"Really old"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-08.123-Name).awiki",
                                    new DateTime(2004, 11, 04), @"Oldest"),

                                new MockFile(@"ReadOnlyTopic.wiki",
                                    new DateTime(2004, 11, 05), @"", MockTopicStorePermissions.ReadOnly),
                                new MockFile(@"ReadOnlyTopic(2004-11-05-00-00-00-Name).awiki",
                                    new DateTime(2004, 11, 05), @""),
                                new MockFile(@"ReadOnlyTopic2.wiki",
                                    new DateTime(2004, 11, 05), new DateTime(2007, 10, 22), @"", MockTopicStorePermissions.ReadOnly, true),
                                new MockFile(@"ReadWriteTopic.wiki",
                                    new DateTime(2004, 11, 05), new DateTime(2007, 10, 22), @"", MockTopicStorePermissions.ReadWrite, false),

                                new MockFile(@"DeletedTopic(2004-11-11-00-00-00-Name).awiki",
                                    new DateTime(2004, 11, 11), @"This topic was deleted.")
                             )
                         )
                     )
                 )
             );

            federation.RegisterNamespace(new FileSystemStore(_fileSystem), "NamespaceOne",
                new NamespaceProviderParameterCollection(
                    new NamespaceProviderParameter("Root", Root)));

            // Necessary to bypass security because a non-existent manager can't be
            // retrieved directly from the federation
            manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");

            _provider = (FileSystemStore)manager.GetProvider(typeof(FileSystemStore));

            parser = new ParserEngine(federation);
            //Necessary to init WikiInputDocument for all WomDocument tests
            //inputDoc = parser.InitWikiDocument(Path.Combine(mockapp.WebPath, @"InputDocs/AstralisLux.wiki"));
        }

        [Test]
        public void WomDocUninitializedTest()
        {
            Assert.IsNull(parser.WomDocument);
        }
        [Test]
        public void WomDocInitializedTest()
        {
            string test = "This is some text.";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsNotNull(parser.WomDocument);
        }
        [Test]
        public void WomDocParaTest()
        {
            string test = "This is some text.";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains("<paraText>" + test + "</paraText>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // The WomDocument tests take a snippet of wikitext and convert it into abstract
        //   wiki object model notation ready for xsl transform
        //   These tests serve to validate the various rules and the parsing process
        //
        [Test]
        public void WomDocSingleLinePropertyNoTopicTest()
        {
            string test = "Summary: this is some text.\r\n";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains("<SinglelineProperty><Name>Summary</Name>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains("<womPropertyText>this is some text.</womPropertyText></SinglelineProperty>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocSingleLinePropertyGoodTopicTest()
        {
            string test = "HomePage: this is some text.\r\n";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"
<SinglelineProperty><Name>HomePage</Name><TopicExists><Namespace>NamespaceOne</Namespace><Topic>HomePage</Topic><TipId>id1</TipId><TipData><TipIdData>id1</TipIdData><TipText></TipText><TipStat>1/1/0001 12:00:00 AM</TipStat></TipData></TopicExists><womPropertyText>this is some text.</womPropertyText></SinglelineProperty>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocSingleLinePropertyBadTopicTest()
        {
            string test = "BadTopic: this is some text.\r\n";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains("<SinglelineProperty><Name>BadTopic</Name><CreateNewTopic><SinglelineProperty>BadTopic</SinglelineProperty><Namespace>NamespaceOne</Namespace></CreateNewTopic><womPropertyText>this is some text.</womPropertyText></SinglelineProperty>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocHiddenSingleLinePropertyTest()
        {
            string test = ":Summary: this is some text.\r\n";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains("<HiddenSinglelineProperty><Name>Summary</Name>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains("<womPropertyText> this is some text.</womPropertyText></HiddenSinglelineProperty>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocMultiLinePropertyTest()
        {
            string test = @"Summary:[
this is some text.
It is on multiple lines.
]
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains("<MultilineProperty><Name>Summary</Name>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"
<MultilineProperty><Name>Summary</Name><womPropertyText>this is some text.
It is on multiple lines.</womPropertyText></MultilineProperty>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocHiddenMultiLinePropertyTest()
        {
            string test = @":Summary:[
this is some text.
It is on multiple lines.
]
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<HiddenMultilineProperty>:Summary:[
this is some text.
It is on multiple lines.
]</HiddenMultilineProperty>
"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocSimpleUnorderedListTest()
        {
            string test = @"This is some text.

        * List item 1
        * List item 2
\r\n";
            test = test.Replace("        ", "\t");
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<list type=""unordered""><item>
<womListText> List item 1</womListText></item>
<item>
<womListText> List item 2</womListText></item></list>
<Para><paraText>\r\n</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocComplexUnorderedListTest()
        {
            string test = @"This is some text.

        * List item 1
        * List item 2
                * List 2 item 1
                * List 2 item 2
                        * List 3 item 1
                * List 2 item 3
                * List 2 item 4
        * List item 3
                * New List 2 item 1
This is some other text after the list.
\r\n";
            test = test.Replace("        ", "\t");
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
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
<Para><paraText>This is some other text after the list.</paraText></Para>
<Para><paraText>\r\n</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocSimpleOrderedListTest()
        {
            string test = @"This is some text.

        1. List item 1
        1. List item 2
\r\n";
            test = test.Replace("        ", "\t");
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<list type=""ordered""><item>
<womListText> List item 1</womListText></item>
<item>
<womListText> List item 2</womListText></item></list>
<Para><paraText>\r\n</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocComplexOrderedListTest()
        {
            string test = @"This is some text.

        1. List item 1
        1. List item 2
                1. List 2 item 1
                1. List 2 item 2
                        1. List 3 item 1
                1. List 2 item 3
                1. List 2 item 4
        1. List item 3
                1. New List 2 item 1
This is some other text after the list.
\r\n";
            test = test.Replace("        ", "\t");
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
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
<Para><paraText>This is some other text after the list.</paraText></Para>
<Para><paraText>\r\n</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocComplexMixedListTest()
        {
            string test = @"This is some text.

        1. List item 1
        1. List item 2
                * List 2 item 1
                * List 2 item 2
                        1. List 3 item 1
                * List 2 item 3
                * List 2 item 4
        1. List item 3
                1. New List 2 item 1
This is some other text after the list.
\r\n";
            test = test.Replace("        ", "\t");
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
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
<Para><paraText>This is some other text after the list.</paraText></Para>
<Para><paraText>\r\n</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocSimpleHeaderTest()
        {
            string test = @"This is some text.
! Header One text

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Header level=""1"">
<womHeaderText> Header One text</womHeaderText><AnchorText>_1__Header_One_text</AnchorText></Header><Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFirstMultipleHeaderTest()
        {
            string test = @"This is some text.
!! Header Two text

Here is some more text.
!!! Header Three text

Here is some more text.
!!!!! Header Five text

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Header level=""2"">
<womHeaderText> Header Two text</womHeaderText><AnchorText>_1__Header_Two_text</AnchorText></Header><Para><paraText>Here is some more text.</paraText></Para>
<Header level=""3"">
<womHeaderText> Header Three text</womHeaderText><AnchorText>_2__Header_Three_text</AnchorText></Header><Para><paraText>Here is some more text.</paraText></Para>
<Header level=""5"">
<womHeaderText> Header Five text</womHeaderText><AnchorText>_3__Header_Five_text</AnchorText></Header><Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocSecondMultipleHeaderTest()
        {
            string test = @"This is some text.
!!!! Header Four text

Here is some more text.
!!!!!! Header Six text

Here is some more text.
!!!!!!! Header Seven text

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Header level=""4"">
<womHeaderText> Header Four text</womHeaderText><AnchorText>_1__Header_Four_text</AnchorText></Header><Para><paraText>Here is some more text.</paraText></Para>
<Header level=""6"">
<womHeaderText> Header Six text</womHeaderText><AnchorText>_2__Header_Six_text</AnchorText></Header><Para><paraText>Here is some more text.</paraText></Para>
<Header level=""7"">
<womHeaderText> Header Seven text</womHeaderText><AnchorText>_3__Header_Seven_text</AnchorText></Header><Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocImageDisplayLinksTest()
        {
            string test = @"This is some text.
http://www.flexwiki.com/fwlogo.jpg

Here is some more text.
http://io9.com/test.gif

Here is some more text.
http://127.0.0.1/mypic.jpeg

Here is some more text.
http://www.example.com:8080/my_graphic.png

Here is some more text.
https://www.flexwiki.com/somedir/fwlogo.jpg

Here is some more text.
https://io9.com/first/second/third/fourth/fifth/sixth/test.gif

Here is some more text.
https://127.0.0.1/mypic.jpeg

Here is some more text.
https://www.example.com/mygraphic.png

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><HttpImageDisplayJpg>http://www.flexwiki.com/fwlogo.jpg</HttpImageDisplayJpg></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpImageDisplayGif>http://io9.com/test.gif</HttpImageDisplayGif></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpImageDisplayJpeg>http://127.0.0.1/mypic.jpeg</HttpImageDisplayJpeg></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpImageDisplayPng>http://www.example.com:8080/my_graphic.png</HttpImageDisplayPng></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpsImageDisplayJpg>https://www.flexwiki.com/somedir/fwlogo.jpg</HttpsImageDisplayJpg></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpsImageDisplayGif>https://io9.com/first/second/third/fourth/fifth/sixth/test.gif</HttpsImageDisplayGif></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpsImageDisplayJpeg>https://127.0.0.1/mypic.jpeg</HttpsImageDisplayJpeg></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><HttpsImageDisplayPng>https://www.example.com/mygraphic.png</HttpsImageDisplayPng></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkImageDisplayLinksTest()
        {
            string test = @"This is some text.
""http://www.flexwiki.com/fwlogo.jpg"":https://www.flexwiki.com

Here is some more text.
""http://io9.com/test.gif"":http://io9.com

Here is some more text.
""http://127.0.0.1/mypic.jpeg"":http://localhost/webpage.html

Here is some more text.
""http://www.example.com:8080/my_graphic.png"":http://www.example.com:8080/

Here is some more text.
""https://www.flexwiki.com/somedir/fwlogo.jpg"":https://www.flexwiki.com

Here is some more text.
""https://io9.com/first/second/third/fourth/fifth/sixth/test.gif"":https://io9.com/first/second/third/fourth/fifth/sixth/test.gif

Here is some more text.
""https://127.0.0.1/mypic.jpeg"":http://localhost/mypic.png

Here is some more text.
""https://www.example.com/mygraphic.png"":http://www.example.com/text.htm

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><FreeLinkToHttpImageDisplayJpg><HttpImageDisplayJpg>http://www.flexwiki.com/fwlogo.jpg</HttpImageDisplayJpg><WebLink>https://www.flexwiki.com</WebLink></FreeLinkToHttpImageDisplayJpg></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpImageDisplayGif><HttpImageDisplayGif>http://io9.com/test.gif</HttpImageDisplayGif><WebLink>http://io9.com</WebLink></FreeLinkToHttpImageDisplayGif></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpImageDisplayJpeg><HttpImageDisplayJpeg>http://127.0.0.1/mypic.jpeg</HttpImageDisplayJpeg><WebLink>http://localhost/webpage.html</WebLink></FreeLinkToHttpImageDisplayJpeg></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpImageDisplayPng><HttpImageDisplayPng>http://www.example.com:8080/my_graphic.png</HttpImageDisplayPng><WebLink>http://www.example.com:8080/</WebLink></FreeLinkToHttpImageDisplayPng></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpsImageDisplayJpg><HttpsImageDisplayJpg>https://www.flexwiki.com/somedir/fwlogo.jpg</HttpsImageDisplayJpg><WebLink>https://www.flexwiki.com</WebLink></FreeLinkToHttpsImageDisplayJpg></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpsImageDisplayGif><HttpsImageDisplayGif>https://io9.com/first/second/third/fourth/fifth/sixth/test.gif</HttpsImageDisplayGif><WebLink>https://io9.com/first/second/third/fourth/fifth/sixth/test.gif</WebLink></FreeLinkToHttpsImageDisplayGif></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpsImageDisplayJpeg><HttpsImageDisplayJpeg>https://127.0.0.1/mypic.jpeg</HttpsImageDisplayJpeg><WebLink>http://localhost/mypic.png</WebLink></FreeLinkToHttpsImageDisplayJpeg></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><FreeLinkToHttpsImageDisplayPng><HttpsImageDisplayPng>https://www.example.com/mygraphic.png</HttpsImageDisplayPng><WebLink>http://www.example.com/text.htm</WebLink></FreeLinkToHttpsImageDisplayPng></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocAnchorLinksTest()
        {
            string test = @"This is some text.
GoodTopic#Anchor

Here is some more text.
""Anchor Display Text"":AnotherGoodTopic#Placeholder

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><CreateNewTopic><Topic>GoodTopic</Topic><Namespace>NamespaceOne</Namespace></CreateNewTopic></Para>
<Para><paraText>Here is some more text.</paraText></Para>
<Para><CreateNewTopic><Topic>AnotherGoodTopic</Topic><Namespace>NamespaceOne</Namespace><DisplayText>Anchor Display Text</DisplayText></CreateNewTopic></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocGoodTopicLinksTest()
        {
            string test = @"This is some text.

This line contains a link to a HomePage in it


Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line contains a link to a </paraText><TopicExists><Namespace>NamespaceOne</Namespace><Topic>HomePage</Topic><TipId>id1</TipId><DisplayText>HomePage</DisplayText><TipData><TipIdData>id1</TipIdData><TipText></TipText><TipStat>1/1/0001 12:00:00 AM</TipStat></TipData></TopicExists><paraText> in it</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocBadTopicLinksTest()
        {
            string test = @"This is some text.

This line contains a link to a BadTopic in it

Here is some more text with a link MULTIcapsBadTopic in the line

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line contains a link to a </paraText><CreateNewTopic><StartsWithOneCap>BadTopic</StartsWithOneCap><Namespace>NamespaceOne</Namespace></CreateNewTopic><paraText> in it</paraText></Para>
<Para><paraText>Here is some more text with a link </paraText><CreateNewTopic><StartsWithMulticaps>MULTIcapsBadTopic</StartsWithMulticaps><Namespace>NamespaceOne</Namespace></CreateNewTopic><paraText> in the line</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Ignore("Need to upgrade the test configuration setup to include NamespaceManager for MockFileSystem for this test to work -jwdavidson")]
        [Test]
        public void WomDocGoodNamespaceTopicLinksTest()
        {
            string test = @"This is some text.

This line contains a link to a OdsWiki.GoodTopic in it

Here is some more text with a link OdsWiki.MULTIcapsGoodTopic in the line

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line contains a link to a </paraText><NamespaceTopicExists><Namespace>OdsWiki</Namespace><Topic>GoodTopic</Topic><TipId>id1</TipId><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></NamespaceTopicExists><paraText> in it</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text with a link </paraText><NamespaceTopicExists><Namespace>OdsWiki</Namespace><Topic>MULTIcapsGoodTopic</Topic><TipId>id2</TipId><TipData><TipIdData>id2</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></NamespaceTopicExists><paraText> in the line</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Ignore("Need to upgrade the test configuration setup to include NamespaceManager for MockFileSystem for this test to work -jwdavidson")]
        [Test]
        public void WomDocGoodNamespaceBadTopicLinksTest()
        {
            string test = @"This is some text.

This line contains a link to a OdsWiki.BadTopic in it

Here is some more text with a link OdsWiki.MULTIcapsBadTopic in the line

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line contains a link to a </paraText><CreateNamespaceTopic><NamespaceTopic>OdsWiki.BadTopic</NamespaceTopic><CreateTopic>BadTopic</CreateTopic></CreateNamespaceTopic><paraText> in it</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text with a link </paraText><CreateNamespaceTopic><NamespaceMulticapsTopic>OdsWiki.MULTIcapsBadTopic</NamespaceMulticapsTopic><CreateTopic>MULTIcapsBadTopic</CreateTopic></CreateNamespaceTopic><paraText> in the line</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocBadNamespaceTopicLinksTest()
        {
            string test = @"This is some text.

This line contains a link to a BadWiki.GoodTopic in it

Here is some more text with a link BadWiki.MULTIcapsGoodTopic in the line

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line contains a link to a </paraText><paraText>BadWiki.GoodTopic</paraText><paraText> in it</paraText></Para>
<Para><paraText>Here is some more text with a link </paraText><paraText>BadWiki.MULTIcapsGoodTopic</paraText><paraText> in the line</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTextileInLineTest()
        {
            string test = @"This is some text.

This line (1) contains ''italic text'' and a '''strong text''' in it
This line (2) contains ''italic text with embedded *strong text* and some ^superscript^ in'' it
This line (3) has *strong text* by itself
This line (4) has ^superscript text with *strong text* embedded^ in it
This line (5) contains a ??citation text?? in it
This line ( 6 ) has a section of -deleted terxt- and a section of +inserted text+ in it
This line (7) has _emphasized text_ in it
While this ( 8 ) line has a mix of ^superscript^ and ~subscript~ in it
This (9) is a @section of code@ in the text

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            //TODO: Break this into individual tests after fixing formats by adding end pattern and jump references:
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This line (1) contains </paraText><Italics>italic text</Italics><paraText> and a </paraText><Strong>strong text</Strong><paraText> in it</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This line (2) contains </paraText><Italics>italic text with embedded <Strong>strong text</Strong> and some <TextileSuperscriptInLine>superscript</TextileSuperscriptInLine> in</Italics><paraText> it</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This line (3) has </paraText><TextileStrong><womStrongText>strong text</womStrongText></TextileStrong><paraText> by itself</paraText></Para>"));

            // The remaining *strong* part will be picked up by an additional processor at the end of the WomDocument, just before Emoticon processing
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This line (4) has</paraText><TextileSuperscriptInLine>superscript text with <Strong>strong text</Strong> embedded</TextileSuperscriptInLine><paraText> in it</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This line (5) contains a</paraText><TextileCitationInLine>citation text</TextileCitationInLine><paraText> in it</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This line ( 6 ) has a section of</paraText><TextileDeletionInLine>deleted terxt</TextileDeletionInLine><paraText> and a section of</paraText><TextileInsertedInLine>inserted text</TextileInsertedInLine><paraText> in it</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This line (7) has</paraText><TextileEmphasisInLine>emphasized text</TextileEmphasisInLine><paraText> in it</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>While this ( 8 ) line has a mix of</paraText><TextileSuperscriptInLine>superscript</TextileSuperscriptInLine><paraText> and</paraText><TextileSubscriptInLine>subscript</TextileSubscriptInLine><paraText> in it</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This (9) is a</paraText><TextileCodeLineInLine>section of code</TextileCodeLineInLine><paraText> in the text</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTextileStartLineTest()
        {
            string test = @"This is some text.

*strong text* by itself on line (1)
^superscript text with *strong text* embedded^ in it on line (2)
??citation text?? on line (3)
-deleted terxt- at the start of line (4)
+inserted text+ in line (5)
_emphasized text_ in line ( 6 )
~subscript~ in line (7)
@section of code@ in the text of line ( 8 )

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            //TODO: Break this into individual tests after fixing formats by adding end pattern and jump references:
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><TextileStrong><womStrongText>strong text</womStrongText></TextileStrong><paraText> by itself on line (1)</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><TextileSuperscriptLineStart>superscript text with <Strong>strong text</Strong> embedded</TextileSuperscriptLineStart><paraText> in it on line (2)</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><TextileCitationLineStart>citation text</TextileCitationLineStart><paraText> on line (3)</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><TextileDeletionLineStart>deleted terxt</TextileDeletionLineStart><paraText> at the start of line (4)</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><TextileInsertedLineStart>inserted text</TextileInsertedLineStart><paraText> in line (5)</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><TextileEmphasisLineStart>emphasized text</TextileEmphasisLineStart><paraText> in line ( 6 )</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><TextileSubscriptLineStart>subscript</TextileSubscriptLineStart><paraText> in line (7)</paraText></Para>"));
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><TextileCodeLineStart>@section of code@</TextileCodeLineStart><paraText> in the text of line ( 8 )</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocPageRuleTest()
        {
            string test = @"This is some text.

This line contains a link to a BadTopic in it

Here is some more text with a page rule (hr) below
----
Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line contains a link to a </paraText><CreateNewTopic><StartsWithOneCap>BadTopic</StartsWithOneCap><Namespace>NamespaceOne</Namespace></CreateNewTopic><paraText> in it</paraText></Para>
<Para><paraText>Here is some more text with a page rule (hr) below</paraText></Para>
<PageRule/>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocWikiTalkMethodCreateTopicTest()
        {
            string test = @"This is some text.

TopicForMethodCreate:{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }
}
Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>

<WikiTalkMethod><Name>TopicForMethodCreate</Name><CreateNewTopic><WikiTalkMethod>TopicForMethodCreate</WikiTalkMethod><Namespace>NamespaceOne</Namespace></CreateNewTopic><wikiTalkMultiline>{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }
}</wikiTalkMultiline></WikiTalkMethod><Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocWikiTalkMethodTopicExistsTest()
        {
            string test = @"This is some text.

HomePage:{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }
}
Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>

<WikiTalkMethod><Name>HomePage</Name><TopicExists><Namespace>NamespaceOne</Namespace><Topic>HomePage</Topic><TipId>id1</TipId><TipData><TipIdData>id1</TipIdData><TipText></TipText><TipStat>1/1/0001 12:00:00 AM</TipStat></TipData></TopicExists><wikiTalkMultiline>{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }
}</wikiTalkMultiline></WikiTalkMethod><Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocHiddenWikiTalkMethodTest()
        {
            string test = @"This is some text.

:ShowNamespaceSelect:{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }
}
Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<HiddenWikiTalkMethod><Name>ShowNamespaceSelect</Name>
<wikiTalkMultiline>{ selected |
	selected.IfNull{ ShowNamespaceSelectHelper(namespace.Name) }
	Else{ ShowNamespaceSelectHelper(selected) }
}</wikiTalkMultiline></HiddenWikiTalkMethod>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocHttpLinkTest()
        {
            string test = @"This is some text.

This line has a link to http://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a link to </paraText><HttpLink>http://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html</HttpLink></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocHttpsLinkTest()
        {
            string test = @"This is some text.

This line has a link to https://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a link to </paraText><HttpsLink>https://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html</HttpsLink></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocGoodMalformedTopicLinksTest()
        {
            string test = @"This is some text.

This line contains a link to a [HomePage] in it

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line contains a link to a </paraText><TopicExists><Namespace>NamespaceOne</Namespace><Topic>HomePage</Topic><TipId>id1</TipId><DisplayText>HomePage</DisplayText><TipData><TipIdData>id1</TipIdData><TipText></TipText><TipStat>1/1/0001 12:00:00 AM</TipStat></TipData></TopicExists><paraText> in it</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocBadMalformedTopicLinksTest()
        {
            string test = @"This is some text.

This line contains a link to a [BadTopic] in it

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line contains a link to a </paraText><CreateNewTopic><MalformedTopic>BadTopic</MalformedTopic><Namespace>NamespaceOne</Namespace></CreateNewTopic><paraText> in it</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkToHttpTest()
        {
            string test = @"This is some text.

This line has a link to ""FlexWiki HomePage"":http://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a link to </paraText><FreeLinkToHttpLink><FreeLink>FlexWiki HomePage</FreeLink><HttpLink>http://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html</HttpLink></FreeLinkToHttpLink></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkToHttpsTest()
        {
            string test = @"This is some text.

This line has a link to ""Secure FlexWiki HomePage"":https://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a link to </paraText><FreeLinkToHttpsLink><FreeLink>Secure FlexWiki HomePage</FreeLink><HttpsLink>https://www.flexwiki.com/default.aspx/FlexWiki/HomePage.html</HttpsLink></FreeLinkToHttpsLink></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkToTopicExistsTest()
        {
            string test = @"This is some text.

This line has a link to ""FlexWiki TopicExists"":HomePage

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a link to </paraText><TopicExists><Namespace>NamespaceOne</Namespace><Topic>HomePage</Topic><TipId>id1</TipId><DisplayText>FlexWiki TopicExists</DisplayText><TipData><TipIdData>id1</TipIdData><TipText></TipText><TipStat>1/1/0001 12:00:00 AM</TipStat></TipData></TopicExists></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkToTopicCreateTest()
        {
            string test = @"This is some text.

This line has a link to ""FlexWiki CreateTopic"":BadTopic

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a link to </paraText><CreateNewTopic><Topic>BadTopic</Topic><Namespace>NamespaceOne</Namespace><DisplayText>FlexWiki CreateTopic</DisplayText></CreateNewTopic></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkToMULTIcapsTopicExistsTest()
        {
            string test = @"This is some text.

This line has a link to ""FlexWiki TopicExists"":MULTIcapsGoodTopic

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a link to </paraText><TopicExists><Namespace>NamespaceOne</Namespace><Topic>MULTIcapsGoodTopic</Topic><TipId>id1</TipId><DisplayText>FlexWiki TopicExists</DisplayText><TipData><TipIdData>id1</TipIdData><TipText></TipText><TipStat>1/1/0001 12:00:00 AM</TipStat></TipData></TopicExists></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkToMULTIcapsTopicCreateTest()
        {
            string test = @"This is some text.

This line has a link to ""FlexWiki CreateTopic"":MULTIcapsBadTopic

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a link to </paraText><CreateNewTopic><Topic>MULTIcapsBadTopic</Topic><Namespace>NamespaceOne</Namespace><DisplayText>FlexWiki CreateTopic</DisplayText></CreateNewTopic></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkToMalformedTopicExistsTest()
        {
            string test = @"This is some text.

This line has a link to ""FlexWiki TopicExists"":[HomePage]

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a link to </paraText><TopicExists><Namespace>NamespaceOne</Namespace><Topic>HomePage</Topic><TipId>id1</TipId><DisplayText>FlexWiki TopicExists</DisplayText><TipData><TipIdData>id1</TipIdData><TipText></TipText><TipStat>1/1/0001 12:00:00 AM</TipStat></TipData></TopicExists></Para>
<Para><paraText>Here is some more text.</paraText></Para>
"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkToMalformedTopicCreateTest()
        {
            string test = @"This is some text.

This line has a link to ""FlexWiki CreateTopic"":[BadTopic]

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a link to </paraText><CreateNewTopic><Topic>BadTopic</Topic><Namespace>NamespaceOne</Namespace><DisplayText>FlexWiki CreateTopic</DisplayText></CreateNewTopic></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocEscapedNoFormatTextTest()
        {
            string test = @"This is some text.

This line has a """"NoFormatText"""" in it.

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a </paraText><EscapedNoFormatText>NoFormatText</EscapedNoFormatText><paraText> in it.</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocLinkToAnchorTopicExistsTest()
        {
            string test = @"This is some text.

This line has an anchor to HomePage#Summary in it.

Here is some more text.
";
            WomDocument.ResetUniqueIdentifier();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has an anchor to </paraText><TopicExistsAnchor><Namespace>NamespaceOne</Namespace><Topic>HomePage</Topic><Anchor>Summary</Anchor><TipId>id1</TipId><DisplayText>HomePage</DisplayText><TipData><TipIdData>id1</TipIdData><TipText></TipText><TipStat>1/1/0001 12:00:00 AM</TipStat></TipData></TopicExistsAnchor><paraText> in it.</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocLinkToAnchorCreateTopicTest()
        {
            string test = @"This is some text.

This line has an anchor to BadTopic#Summary in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has an anchor to </paraText><CreateNewTopic><Topic>BadTopic</Topic><Namespace>NamespaceOne</Namespace></CreateNewTopic><paraText> in it.</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Ignore("Need to upgrade the test configuration setup to include NamespaceManager for MockFileSystem for this test to work -jwdavidson")]
        [Test]
        public void WomDocFreeLinkToNamespaceTopicExistsTest()
        {
            string test = @"This is some text.

This line has an freelink to ""My FreeLink"":NamespaceOne.HomePage in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has an freelink to </paraText><TopicExists><Namespace>OdsWiki</Namespace><Topic>GoodTopic</Topic><TipId>id1</TipId><DisplayText>My FreeLink</DisplayText><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExists><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Ignore("Need to upgrade the test configuration setup to include NamespaceManager for MockFileSystem for this test to work -jwdavidson")]
        [Test]
        public void WomDocFreeLinkToNamespaceTopicCreateTopicTest()
        {
            string test = @"This is some text.

This line has an freelink to ""My FreeLink"":NamespaceOne.BadTopic in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has an freelink to </paraText><CreateNewTopic><Topic>BadTopic</Topic><Namespace>OdsWiki</Namespace><DisplayText>My FreeLink</DisplayText></CreateNewTopic><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Ignore("Need to upgrade the test configuration setup to include NamespaceManager for MockFileSystem for this test to work -jwdavidson")]
        [Test]
        public void WomDocFreeLinkToNamespaceTopicBadNamespaceTest()
        {
            string test = @"This is some text.

This line has an freelink to ""My FreeLink"":FlexWiki.HomePage in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has an freelink to </paraText><FreeLinkToNamespaceTopic>""My FreeLink"":FlexWiki.GoodTopic</FreeLinkToNamespaceTopic><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Ignore("Need to upgrade the test configuration setup to include NamespaceManager for MockFileSystem for this test to work -jwdavidson")]
        [Test]
        public void WomDocFreeLinkToNamespaceMalformedTopicExistsTest()
        {
            string test = @"This is some text.

This line has an freelink to ""My FreeLink"":NamespaceOne.[HomePage] in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has an freelink to </paraText><TopicExists><Namespace>OdsWiki</Namespace><Topic>goodtopic</Topic><TipId>id1</TipId><DisplayText>My FreeLink</DisplayText><TipData><TipIdData>id1</TipIdData><TipText>This is just some text</TipText><TipStat>5/22/2008 12:06:58 PM - -76.70.99.195</TipStat></TipData></TopicExists><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Ignore("Need to upgrade the test configuration setup to include NamespaceManager for MockFileSystem for this test to work -jwdavidson")]
        [Test]
        public void WomDocFreeLinkToNamespaceMalformedTopicCreateTopicTest()
        {
            string test = @"This is some text.

This line has an freelink to ""My FreeLink"":NamespaceOne.[badtopic] in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<EmptyLine />
<Para><paraText>This line has an freelink to </paraText><CreateNewTopic><Topic>badtopic</Topic><Namespace>OdsWiki</Namespace><DisplayText>My FreeLink</DisplayText></CreateNewTopic><paraText> in it.</paraText></Para>
<EmptyLine />
<Para><paraText>Here is some more text.</paraText></Para>
"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkToNamespaceMalformedTopicBadNamespaceTest()
        {
            string test = @"This is some text.

This line has an freelink to ""My FreeLink"":FlexWiki.[goodtopic] in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has an freelink to </paraText><paraText>""My FreeLink"":FlexWiki.[goodtopic]</paraText><paraText> in it.</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocMailtoLinkTest()
        {
            string test = @"This is some text.

This line has a mailto to mailto:jwdavidson@gmail.com in it.
This line has a mailto to mailto:jwdavidson@gmail.com;somebodyelse@example.com in it.
This line has a mailto to mailto:jwdavidson@gmail.com?subject=FlexWiki%20Development in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a mailto to </paraText><MailtoLink>mailto:jwdavidson@gmail.com</MailtoLink><paraText> in it.</paraText></Para>
<Para><paraText>This line has a mailto to </paraText><MailtoLink>mailto:jwdavidson@gmail.com;somebodyelse@example.com</MailtoLink><paraText> in it.</paraText></Para>
<Para><paraText>This line has a mailto to </paraText><MailtoLink>mailto:jwdavidson@gmail.com?subject=FlexWiki%20Development</MailtoLink><paraText> in it.</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFreeLinkToMailtoTest()
        {
            string test = @"This is some text.

This line has a mailto to ""JW Davidson"":mailto:jwdavidson@gmail.com in it.
This line has a mailto to ""JW Davidson + Somebodyelse"":mailto:jwdavidson@gmail.com;somebodyelse@example.com in it.
This line has a mailto to ""FlexWiki Development"":mailto:jwdavidson@gmail.com?subject=FlexWiki%20Development in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a mailto to </paraText><FreeLinkToMailto><FreeLinkMail>JW Davidson</FreeLinkMail><Mailto>mailto:jwdavidson@gmail.com</Mailto></FreeLinkToMailto><paraText> in it.</paraText></Para>
<Para><paraText>This line has a mailto to </paraText><FreeLinkToMailto><FreeLinkMail>JW Davidson + Somebodyelse</FreeLinkMail><Mailto>mailto:jwdavidson@gmail.com;somebodyelse@example.com</Mailto></FreeLinkToMailto><paraText> in it.</paraText></Para>
<Para><paraText>This line has a mailto to </paraText><FreeLinkToMailto><FreeLinkMail>FlexWiki Development</FreeLinkMail><Mailto>mailto:jwdavidson@gmail.com?subject=FlexWiki%20Development</Mailto></FreeLinkToMailto><paraText> in it.</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocFileLinkTest()
        {
            string test = @"This is some text.

This line has a file link to file:\\someserver\share in it.
This line has a file link to file:\\someserver\share\test.gif in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a file link to </paraText><FileLink>file:\\someserver\share</FileLink><paraText> in it.</paraText></Para>
<Para><paraText>This line has a file link to </paraText><FileLink>file:\\someserver\share\test.gif</FileLink><paraText> in it.</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocAltFileLinkTest()
        {
            string test = @"This is some text.

This line has a file link to [file:\\someserver\share] in it.
This line has a file link to [file:\\someserver\share\test.gif] in it.

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a file link to </paraText><AltFileLink>file:\\someserver\share</AltFileLink><paraText> in it.</paraText></Para>
<Para><paraText>This line has a file link to </paraText><AltFileLink>file:\\someserver\share\test.gif</AltFileLink><paraText> in it.</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocWikiTalkStringTest()
        {
            string test = @"This is some text.

@@[
	""||{!}*Topic*||{!}*Date of Last Change*||{!}*Summary*||"", Newline,
	namespace.Topics.Select{ each | 
		each.HasProperty(""Owner"") 
		}.SortBy { each | 
			DateTime.Now.SpanBetween(each.LastModified) 
		}.Collect{ each |
			[
				""        * "", each.Name,
				"" %gray%("", each.LastModified.ToShortDateString(), "" "", each.LastModified.ToLongTimeString(), 
				"")"", Newline,
				""                * "",each.Summary,Newline,
			]
	}
]
@@
Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<WikiTalkString>@@[
	""||{!}*Topic*||{!}*Date of Last Change*||{!}*Summary*||"", Newline,
	namespace.Topics.Select{ each | 
		each.HasProperty(""Owner"") 
		}.SortBy { each | 
			DateTime.Now.SpanBetween(each.LastModified) 
		}.Collect{ each |
			[
				""        * "", each.Name,
				"" %gray%("", each.LastModified.ToShortDateString(), "" "", each.LastModified.ToLongTimeString(), 
				"")"", Newline,
				""                * "",each.Summary,Newline,
			]
	}
]
@@</WikiTalkString><Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocWikiStylingTest()
        {
            string test = @"This is some text.

Some text %red% some text in red

Some text %blue% some text in blue%% now it is normal
Some text %big blue% some text in blue%% now it is normal
Some text %blue small% some text in blue%% now it is normal
Some text %big blue big% some text in blue%% now it is normal
Some %blue% blue text%% with some %red% red text.
Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>Some text </paraText><WikiStyling><StyleColor>red</StyleColor><womWikiStyledText> some text in red</womWikiStyledText></WikiStyling></Para>
<Para><paraText>Some text </paraText><WikiStyling><StyleColor>blue</StyleColor><womWikiStyledText> some text in blue</womWikiStyledText></WikiStyling><paraText> now it is normal</paraText></Para>
<Para><paraText>Some text </paraText><WikiStyling><StyleSizeBig/><StyleColor>blue</StyleColor><womWikiStyledText> some text in blue</womWikiStyledText></WikiStyling><paraText> now it is normal</paraText></Para>
<Para><paraText>Some text </paraText><WikiStyling><StyleColor>blue</StyleColor><StyleSizeSmall/><womWikiStyledText> some text in blue</womWikiStyledText></WikiStyling><paraText> now it is normal</paraText></Para>
<Para><paraText>Some text </paraText><WikiStyling><StyleSizeBig/><StyleColor>blue</StyleColor><StyleSizeBig/><womWikiStyledText> some text in blue</womWikiStyledText></WikiStyling><paraText> now it is normal</paraText></Para>
<Para><paraText>Some </paraText><WikiStyling><StyleColor>blue</StyleColor><womWikiStyledText> blue text</womWikiStyledText></WikiStyling><paraText> with some </paraText><WikiStyling><StyleColor>red</StyleColor><womWikiStyledText> red text.</womWikiStyledText></WikiStyling></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocSimpleTableRowTest()
        {
            string test = @"This is some text.

||Region||Sales||
||East||$100||
||West||$500||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
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
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowCenterTableTest()
        {
            string test = @"This is some text.

||{T^}Region||Sales||
||East||$100||
||West||$500||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><TableCenter>T^</TableCenter></TableStyle><womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowTableFloatLeftTest()
        {
            string test = @"This is some text.

||{T[}Region||Sales||
||East||$100||
||West||$500||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><TableFloatLeft>T[</TableFloatLeft></TableStyle><womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowTableFloatRightTest()
        {
            string test = @"This is some text.

||{T]}Region||Sales||
||East||$100||
||West||$500||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><TableFloatRight>T]</TableFloatRight></TableStyle><womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowTableNoBorderTest()
        {
            string test = @"This is some text.

||{T-}Region||Sales||
||East||$100||
||West||$500||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><BorderlessTable>T-</BorderlessTable></TableStyle><womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowTableWidthTest()
        {
            string test = @"This is some text.

||{TW25}Region||Sales||
||East||$100||
||West||$500||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><TableWidth>TW25</TableWidth></TableStyle><womCell>Region</womCell></womCellText><womCellText>
<womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>East</womCell></womCellText><womCellText>
<womCell>$100</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>West</womCell></womCellText><womCellText>
<womCell>$500</womCell></womCellText></TableRow>
</Table>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowSpanColumnRowTest()
        {
            string test = @"This is some text.

||{R2}Region||{C2}Sales||
||Q1||Q2||
||East||$100||$800||
||West||$500||$9000||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><RowSpan>R2</RowSpan></TableStyle><womCell>Region</womCell></womCellText><womCellText>
<TableStyle><ColumnSpan>C2</ColumnSpan></TableStyle><womCell>Sales</womCell></womCellText></TableRow>
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
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableColumnSpanColumnLeftTest()
        {
            string test = @"This is some text.

||{[C2}Sales||
||Q1||Q2||
||$100||$800||
||$500||$9000||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><ColumnLeft>[</ColumnLeft><ColumnSpan>C2</ColumnSpan></TableStyle><womCell>Sales</womCell></womCellText></TableRow>
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
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowSpanColumnRightTest()
        {
            string test = @"This is some text.

||{]C2}Sales||
||Q1||Q2||
||$100||$800||
||$500||$9000||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><ColumnRight>]</ColumnRight><ColumnSpan>C2</ColumnSpan></TableStyle><womCell>Sales</womCell></womCellText></TableRow>
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
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowSpanColumnCenterTest()
        {
            string test = @"This is some text.

||{^C2}Sales||
||Q1||Q2||
||$100||$800||
||$500||$9000||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><ColumnCenter>^</ColumnCenter><ColumnSpan>C2</ColumnSpan></TableStyle><womCell>Sales</womCell></womCellText></TableRow>
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
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowSpanColumnCellHighlightTest()
        {
            string test = @"This is some text.

||{^!C2}Sales||
||Q1||Q2||
||$100||$800||
||$500||{!}$9000||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><ColumnCenter>^</ColumnCenter><CellHighlight>!</CellHighlight><ColumnSpan>C2</ColumnSpan></TableStyle><womCell>Sales</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>Q1</womCell></womCellText><womCellText>
<womCell>Q2</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$100</womCell></womCellText><womCellText>
<womCell>$800</womCell></womCellText></TableRow>
<TableRow><womCellText>
<womCell>$500</womCell></womCellText><womCellText>
<TableStyle><CellHighlight>!</CellHighlight></TableStyle><womCell>$9000</womCell></womCellText></TableRow>
</Table>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowColumnWidthTest()
        {
            string test = @"This is some text.

||{W75}Q1||Q2||Q3||Q4||
||$100||$800||$500||$900||
||$500||$9000||$500||$900||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><CellWidth>W75</CellWidth></TableStyle><womCell>Q1</womCell></womCellText><womCellText>
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
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowCellNoWrapTest()
        {
            string test = @"This is some text.

||{+} The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. || The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. ||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><CellNoWrap>+</CellNoWrap></TableStyle><womCell> The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. </womCell></womCellText><womCellText>
<womCell> The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. </womCell></womCellText></TableRow>
</Table>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocTableRowCellColorTest()
        {
            string test = @"This is some text.

||{*red*}RED RED RED||
||{*lightgreen*}LIGHT GREEN||
||{*#c0c0c0*}LIGHT GREY||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Table><TableRow><womCellText>
<TableStyle><CellStyleColor>red</CellStyleColor></TableStyle><womCell>RED RED RED</womCell></womCellText></TableRow>
<TableRow><womCellText>
<TableStyle><CellStyleColor>lightgreen</CellStyleColor></TableStyle><womCell>LIGHT GREEN</womCell></womCellText></TableRow>
<TableRow><womCellText>
<TableStyle><StyleHexColor>#c0c0c0</StyleHexColor></TableStyle><womCell>LIGHT GREY</womCell></womCellText></TableRow>
</Table>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocPreformattedSingleLineTest()
        {
            string test = @"This is some text.

 ||Region||Sales||
 ||East||$100||
 ||West||$500||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>

<PreformattedSingleLine> ||Region||Sales||
 ||East||$100||
 ||West||$500||</PreformattedSingleLine>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocAltPreformattedSingleLineTest()
        {
            string test = @"This is some text.

    ||Region||Sales||
    ||East||$100||
    ||West||$500||

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>

<PreformattedSingleLine>    ||Region||Sales||
    ||East||$100||
    ||West||$500||</PreformattedSingleLine>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocExtendedCodeTest()
        {
            string test = @"This is some text.

{+
   %blue%public void%% Foo()
   {
       %green%// comment here%%
       %blue%string%% s;
       *...*
   }
}+

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<ExtendedCode><womStyledCode>&nbsp;&nbsp;&nbsp;</womStyledCode><WikiStyling><StyleColor>blue</StyleColor><womWikiStyledText>public void</womWikiStyledText></WikiStyling><womStyledCode>&nbsp;Foo()<Break />&nbsp;&nbsp;&nbsp;{<Break />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</womStyledCode><WikiStyling><StyleColor>green</StyleColor><womWikiStyledText>// comment here</womWikiStyledText></WikiStyling><womStyledCode><Break />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</womStyledCode><WikiStyling><StyleColor>blue</StyleColor><womWikiStyledText>string</womWikiStyledText></WikiStyling><womStyledCode>&nbsp;s;<Break />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</womStyledCode><TextileStrongInLine>...</TextileStrongInLine><womStyledCode><Break />&nbsp;&nbsp;&nbsp;}</womStyledCode></ExtendedCode><Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocPreformattedMultilineTest()
        {
            string test = @"This is some text.

{@
your text goes
here and it does not have to start with space or tab
}@

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<PreformattedMultiline><womMultilineCode>
your text goes
here and it does not have to start with space or tab
</womMultilineCode>
</PreformattedMultiline><Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocPreformattedMultilineKeyTest()
        {
            string test = @"This is some text.

{@KeyID
your text goes
here and it does not have to start with space or tab
}@KeyID

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<PreformattedMultilineKeyed><womMultilineCode>
your text goes
here and it does not have to start with space or tab</womMultilineCode>
</PreformattedMultilineKeyed><Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocPreformattedMultilineMixedTest()
        {
            string test = @"This is some text.

{@KeyID
your text goes
here and it does not have to start with space or tab
{@
an embedded preformatted section
}@
this is some text
}@KeyID

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<PreformattedMultilineKeyed><womMultilineCode>
your text goes
here and it does not have to start with space or tab
{@
an embedded preformatted section
}@
this is some text</womMultilineCode>
</PreformattedMultilineKeyed><Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocSimpleEmoticonsTest()
        {
            string test = @"This is some text.

This line has a thumbs up at the end (y)
This line has (N) a thumbs down in it
This line has a :-S confused smile

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has a thumbs up at the end <Emoticon>emoticons/thumbs_up.gif</Emoticon></paraText></Para>
<Para><paraText>This line has <Emoticon>emoticons/thumbs_down.gif</Emoticon> a thumbs down in it</paraText></Para>
<Para><paraText>This line has a <Emoticon>emoticons/confused_smile.gif</Emoticon> confused smile</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocMoreEmoticonsTest()
        {
            string test = @"This is some text.

This line has (N) a thumbs down and a thumbs up (Y) in it

Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
<Para><paraText>This line has <Emoticon>emoticons/thumbs_down.gif</Emoticon> a thumbs down and a thumbs up <Emoticon>emoticons/thumbs_up.gif</Emoticon> in it</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [Test]
        public void WomDocComplexEmoticonsTest()
        {
            string test = @"This is some text.

|| (y)|| (n)|| (b)|| (d)|| (x)|| (z)|| (6)|| :-[|| (})|| ({)|| :-)|| ;)|| :(||
|| """"(y)""""|| """"(n)""""|| """"(b)""""|| """"(d)""""|| """"(x)""""|| """"(z)""""|| """"(6)""""|| """":-[""""|| """"(})""""|| """"({)""""|| """":-)""""|| """";)""""|| """":(""""||
||    ||    ||    ||    ||    ||    ||    ||    ||    ||    ||    ||   ||   ||
|| :| || :'(|| :-$|| (H)|| :-@|| (A)|| (L)|| (U)|| (k)|| (g)|| (f)|| (w)|| (p)||
|| """":|"""" || """":'(""""|| """":-$""""|| """"(H)""""|| """":-@""""|| """"(A)""""|| """"(L)""""|| """"(U)""""|| """"(k)""""|| """"(g)""""|| """"(f)""""|| """"(w)""""|| """"(p)""""||
||    ||    ||    ||    ||    ||    ||    ||    ||    ||    ||    ||   ||   ||
|| (~)|| (T)|| (t)|| (@)|| (c)|| (i)|| (S)|| (*)|| (8)|| (E)|| (^)|| (O)|| (M)||
|| """"(~)""""|| """"(T)""""|| """"(t)""""|| """"(@)""""|| """"(c)""""|| """"(i)""""|| """"(S)""""|| """"(*)"""" || """"(8)""""|| """"(E)""""|| """"(^)""""|| """"(O)""""|| """"(M)""""||
||    ||    ||    ||    ||    ||    ||    ||    ||    ||    ||    ||   ||   ||
|| :-P || (o) || :-D   ||    ||    ||    ||    ||    ||    ||    ||    ||   ||   ||
|| """":-P"""" || """"(o)""""   ||  """":-D""""  ||    ||    ||    ||    ||    ||    ||    ||    ||   ||   ||

I need a little flag (red maybe :-))

:)
:D
:-D
:/
:-/
:\
:-\
:p
:-p
:P
:-P

(`) (1) (2) (3) (4) (5) (6) (7) (8) (9) (0) (-) (=)
(q) (w) (e) (r) (t) (y) (u) (i) (o) (p) ([) (]) (\)
(a) (s) (d) (f) (g) (h) (j) (k) (l) (;) (')
(z) (x) (c) (v) (b) (n) (m) (,) (.) (/)
 
() () () () () () () () () () () () () 

(~) (!) (@) ($) (%) (^) (&) (*) (() ()) (_) (+)
(Q) (W) (E) (R) (T) (Y) (U) (I) (O) (P) ({) (}) (|)
(A) (S) (D) (F) (G) (H) (J) (K) (L) (:) ("")
(Z) (X) (C) (V) (B) (N) (M) (<) (>) (?)


Here is some more text.
";
            QualifiedTopicRevision topic = new QualifiedTopicRevision("NamespaceOne.TestDocument");
            WomDocument.ResetUniqueIdentifier();
            parser.ProcessText(test, topic, manager, true, 600);
            Assert.IsTrue(parser.WomDocument.ParsedDocument.Contains(@"<Para><paraText>This is some text.</paraText></Para>
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
<womCell> <Emoticon>emoticons/sad_smile.gif</Emoticon></womCell></womCellText></TableRow>
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
<Para><paraText>I need a little flag (red maybe <Emoticon>emoticons/regular_smile.gif</Emoticon>)</paraText></Para>
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
<Para><paraText>(`) (1) (2) (3) (4) (5) <Emoticon>emoticons/devil_smile.gif</Emoticon> (7) <Emoticon>emoticons/musical_note.gif</Emoticon> (9) (0) (-) (=)</paraText></Para>
<Para><paraText>(q) <Emoticon>emoticons/wilted_rose.gif</Emoticon> <Emoticon>emoticons/envelope.gif</Emoticon> (r) <Emoticon>emoticons/phone.gif</Emoticon> <Emoticon>emoticons/thumbs_up.gif</Emoticon> <Emoticon>emoticons/broken_heart.gif</Emoticon> <Emoticon>emoticons/lightbulb.gif</Emoticon> <Emoticon>emoticons/clock.gif</Emoticon> <Emoticon>emoticons/camera.gif</Emoticon> ([) (]) (\)</paraText></Para>
<Para><paraText><Emoticon>emoticons/angel_smile.gif</Emoticon> (s) <Emoticon>emoticons/martini_shaken.gif</Emoticon> <Emoticon>emoticons/rose.gif</Emoticon> <Emoticon>emoticons/present.gif</Emoticon> <Emoticon>emoticons/shades_smile.gif</Emoticon> (j) <Emoticon>emoticons/kiss.gif</Emoticon> <Emoticon>emoticons/heart.gif</Emoticon> (<Emoticon>emoticons/wink_smile.gif</Emoticon> (')</paraText></Para>
<Para><paraText><Emoticon>emoticons/guy_handsacrossamerica.gif</Emoticon> <Emoticon>emoticons/girl_handsacrossamerica.gif</Emoticon> <Emoticon>emoticons/coffee.gif</Emoticon> (v) <Emoticon>emoticons/beer_yum.gif</Emoticon> <Emoticon>emoticons/thumbs_down.gif</Emoticon> <Emoticon>emoticons/messenger.gif</Emoticon> (,) (.) (/)</paraText></Para>

<PreformattedSingleLine> </PreformattedSingleLine>
<Para><paraText>() () () () () () () () () () () () () </paraText></Para>
<Para><paraText><Emoticon>emoticons/film.gif</Emoticon> (!) <Emoticon>emoticons/kittykay.gif</Emoticon> ($) (%) <Emoticon>emoticons/cake.gif</Emoticon> (&) <Emoticon>emoticons/star.gif</Emoticon> (() ()) (_) (+)</paraText></Para>
<Para><paraText>(Q) <Emoticon>emoticons/wilted_rose.gif</Emoticon> <Emoticon>emoticons/envelope.gif</Emoticon> (R) <Emoticon>emoticons/phone.gif</Emoticon> <Emoticon>emoticons/thumbs_up.gif</Emoticon> <Emoticon>emoticons/broken_heart.gif</Emoticon> <Emoticon>emoticons/lightbulb.gif</Emoticon> <Emoticon>emoticons/clock.gif</Emoticon> <Emoticon>emoticons/camera.gif</Emoticon> <Emoticon>emoticons/dude_hug.gif</Emoticon> <Emoticon>emoticons/girl_hug.gif</Emoticon> (|)</paraText></Para>
<Para><paraText><Emoticon>emoticons/angel_smile.gif</Emoticon> <Emoticon>emoticons/moon.gif</Emoticon> <Emoticon>emoticons/martini_shaken.gif</Emoticon> <Emoticon>emoticons/rose.gif</Emoticon> <Emoticon>emoticons/present.gif</Emoticon> <Emoticon>emoticons/shades_smile.gif</Emoticon> (J) <Emoticon>emoticons/kiss.gif</Emoticon> <Emoticon>emoticons/heart.gif</Emoticon> (<Emoticon>emoticons/regular_smile.gif</Emoticon> ("")</paraText></Para>
<Para><paraText><Emoticon>emoticons/guy_handsacrossamerica.gif</Emoticon> <Emoticon>emoticons/girl_handsacrossamerica.gif</Emoticon> <Emoticon>emoticons/coffee.gif</Emoticon> (V) <Emoticon>emoticons/beer_yum.gif</Emoticon> <Emoticon>emoticons/thumbs_down.gif</Emoticon> <Emoticon>emoticons/messenger.gif</Emoticon> (<) (>) (?)</paraText></Para>
<Para><paraText>Here is some more text.</paraText></Para>"));
        }
    }
}
