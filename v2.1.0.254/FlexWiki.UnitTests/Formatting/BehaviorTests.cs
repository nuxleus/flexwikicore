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

        	// Test a clickable medium thumbnail with colours
            FormatTest("@@Presentations.Thumbnail(\"Namespaces/SampleNamespaceTwo/Playground2/images/cat-01.jpg\",federation.LinkMaker.LinkToImage(\"Namespaces/SampleNamespaceTwo/Playground2/images/cat-01.jpg\"),\"A Cat\",\"green\",\"medium\",\"red\",\"3px\",\"dashed\",\"true\")@@",
        	           "<form id=\"form1\" runat=\"server\">\n"+
                       "<table width=\"154\"  border=\"0\" cellspacing=\"0\" cellpadding=\"5\">\n"+
                       "<tr><td>\n<td>\n"+
                       "<a class=\"externalLink\" href='/formattingtestswiki/Namespaces/SampleNamespaceTwo/Playground2/images/cat-01.jpg' target=\"_blank\" >\n"+
                       "<img src=\"/flexwiki/Thumbnailer.ashx?img=Namespaces/SampleNamespaceTwo/Playground2/images/cat-01.jpg&amp;size=144\" \n"+
                       "style=\"border-color:red;border-width:3px;border-style:dashed;\"\n"+
                       " alt=\"A Cat\" />\n"+
                       "</a>\n</td></tr>\n<tr><td>\n"+
                       "<td align=\"center\"><font color=\"green\">A Cat</font>\n</td></tr>\n</table></form>\n\n") ;
			
        	// Test a non-clickable large thumbnail with colours
            FormatTest("@@Presentations.Thumbnail(\"Namespaces/SampleNamespaceTwo/Playground2/images/missile.jpg\",federation.LinkMaker.LinkToImage(\"Namespaces/SampleNamespaceTwo/Playground2/images/missile.jpg\"),\"A Missile\",\"red\",\"large\",\"blue\",\"6px\",\"inset\",\"false\")@@",
					   "<form id=\"form1\" runat=\"server\">\n"+
					   "<table width=\"298\"  border=\"0\" cellspacing=\"0\" cellpadding=\"5\">\n"+
					   "<tr><td>\n<td>\n"+
					   "<img src=\"/flexwiki/Thumbnailer.ashx?img=Namespaces/SampleNamespaceTwo/Playground2/images/missile.jpg&amp;size=288\" \n"+ 
					   "style=\"border-color:blue;border-width:6px;border-style:inset;\"\n"+
                       " alt=\"A Missile\" />\n"+
                       "</td></tr>\n<tr><td>\n"+
         	           "<td align=\"center\"><font color=\"red\">A Missile</font>\n</td></tr>\n</table></form>\n\n") ;        
        	// Test a non-clickable small thumbnail with no title and colours
            FormatTest("@@Presentations.Thumbnail(\"Namespaces/SampleNamespaceTwo/Playground2/images/missile.jpg\",federation.LinkMaker.LinkToImage(\"Namespaces/SampleNamespaceTwo/Playground2/images/missile.jpg\"),\"\",\"blue\",\"small\",\"blue\",\"3px\",\"outset\",\"false\")@@",
					   "<img src=\"/flexwiki/Thumbnailer.ashx?img=Namespaces/SampleNamespaceTwo/Playground2/images/missile.jpg&amp;size=72\" \n"+
					   "style=\"border-color:blue;border-width:3px;border-style:outset;\"\n"+
					   " alt=\"\" />\n\n") ;        
    
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

      <table class=""TableClass"">
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
        [Ignore("Need to upgrade the test configuration setup for this test to work -jwdavidson")]
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
