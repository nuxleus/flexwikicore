using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki.UnitTests.Formatting
{
    [TestFixture]
    public class BehaviorTests : FormattingTestsBase
    {
        [Test]
        public void BehaviorTest()
        {
            string s = FormattedTestText(@"@@ProductName@@");
            AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("FlexWiki")) + @""">FlexWiki</a>");
        }

        [Test]
        public void BehaviorWithLineBreak()
        {
            string s = FormattedTestText(@"@@[100, 200
, 
300]@@");
            AssertStringContains(s, @"100200300");
        }

        [Test]
        public void ImageBehaviorTwoParamTest()
        {
            FormatTest("@@Image(\"http://server/image.jpg\", \"Alternative text\")@@",
              "<img src=\"http://server/image.jpg\" alt=\"Alternative text\"/>\n");
        }

        [Test]
        public void ImageBehaviorFourParamTest()
        {
            FormatTest("@@Image(\"http://server/image.jpg\", \"Alternative text\", \"500\", \"400\")@@",
              "<img src=\"http://server/image.jpg\" alt=\"Alternative text\" " +
              "width=\"500\" height=\"400\"/>\n");
        }

        [Test]
        public void ImageBehaviorEmbeddedQuotationMarks()
        {
            FormatTest(@"@@Image(""http://server/image.jpg"", ""Alt \""text\"""")@@",
              "<img src=\"http://server/image.jpg\" alt=\"Alt &quot;text&quot;\"/>\n");
        }

        [Test]
        public void ImageBehaviorTwoPerLineTest()
        {
            FormatTest("@@Image(\"http://server/image.jpg\", \"alt\")@@ and @@Image(\"http://server/image2.jpg\", \"alt2\")@@",
              "<img src=\"http://server/image.jpg\" alt=\"alt\"/> and <img src=\"http://server/image2.jpg\" alt=\"alt2\"/>\n");
        }

        [Ignore("This test needs to be rewritten once the NamespaceManager rearchitecture firms up")]
        [Test]
        public void XmlTransformBehaviorTwoParamTest()
        {
            /*
                  // Need to escape all the backslashes in the path
                  string xmlPath = testRssXmlPath.Replace(@"\", @"\\");
                  string xslPath = testRssXslPath.Replace(@"\", @"\\");

                  FormatTest("@@XmlTransform(\"" + xmlPath + "\", \"" + xslPath + "\")@@",
                      @"<p><h1>Weblogs @ ASP.NET</h1>

      <table cellpadding=""2"" cellspacing=""1"" class=""TableClass"">
      <tr>
      <td  class=""TableCell""><strong>Published Date</strong></td>
      <td  class=""TableCell""><strong>Title</strong></td>
      </tr>
      <tr>
      <td  class=""TableCell"">Wed, 07 Jan 2004 05:45:00 GMT</td>
      <td  class=""TableCell""><a class=""externalLink"" href=""http://weblogs.asp.net/aconrad/archive/2004/01/06/48205.aspx"">Fast Chicken</a></td>
      </tr>
      <tr>
      <td  class=""TableCell"">Wed, 07 Jan 2004 03:36:00 GMT</td>
      <td  class=""TableCell""><a class=""externalLink"" href=""http://weblogs.asp.net/CSchittko/archive/2004/01/06/48178.aspx"">Are You Linked In?</a></td>
      </tr>
      <tr>
      <td  class=""TableCell"">Wed, 07 Jan 2004 03:27:00 GMT</td>
      <td  class=""TableCell""><a class=""externalLink"" href=""http://weblogs.asp.net/francip/archive/2004/01/06/48172.aspx"">Whidbey configuration APIs</a></td>
      </tr>
      </table></p>
      ");
          */
        }

        [Test]
        public void XmlTransformBehaviorXmlParamNotFoundTest()
        {
            FormatTestContains("@@XmlTransform(\"file://noWayThisExists\", \"Alternative text\")@@",
                "Failed to load XML parameter");
        }

        [Ignore("This test needs to be rewritten once the NamespaceManager rearchitecture firms up")]
        [Test]
        public void XmlTransformBehaviorXslParamNotFoundTest()
        {
            /*
                  // Go against just the filename: the full path screws up the build machine
                  string xmlPath = Path.GetFileName(meerkatRssPath);

                  FormatTestContains("@@XmlTransform(\"" + xmlPath + "\", \"file://noWayThisExists\")@@",
                      "Failed to load XSL parameter");
            */
        }


    }
}
