using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using FlexWiki;
using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki.UnitTests.Formatting
{
    [TestFixture]
    public class HyperLineTests : FormattingTestsBase
    {

        [Test]
        public void BracketedHyperLinks()
        {
            FormatTest(
                @"(http://www.msn.com) (http://www.yahoo.com) (http://www.yahoo.com)",
                @"<p>(<a class=""externalLink"" href=""http://www.msn.com"">http://www.msn.com</a>) (<a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a>) (<a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a>)</p>
");
            FormatTest(
                @"[http://www.msn.com] [http://www.yahoo.com] [http://www.yahoo.com]",
                @"<p><a class=""externalLink"" href=""http://www.msn.com"">http://www.msn.com</a> <a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a> <a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a></p>
");
            FormatTest(
                @"{http://www.msn.com} {http://www.yahoo.com} {http://www.yahoo.com}",
                @"<p>{<a class=""externalLink"" href=""http://www.msn.com"">http://www.msn.com</a>} {<a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a>} {<a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a>}</p>
");
        }

        [Test]
        public void BasicHyperLinks()
        {
            FormatTest(
                @"http://www.msn.com http://www.yahoo.com",
                @"<p><a class=""externalLink"" href=""http://www.msn.com"">http://www.msn.com</a> <a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a></p>
");
            FormatTest(
                @"ftp://feeds.scripting.com",
                @"<p><a class=""externalLink"" href=""ftp://feeds.scripting.com"">ftp://feeds.scripting.com</a></p>
");
            FormatTest(
                @"gopher://feeds.scripting.com",
                @"<p><a class=""externalLink"" href=""gopher://feeds.scripting.com"">gopher://feeds.scripting.com</a></p>
");
            FormatTest(
                @"telnet://melvyl.ucop.edu/",
                @"<p><a class=""externalLink"" href=""telnet://melvyl.ucop.edu/"">telnet://melvyl.ucop.edu/</a></p>
");
            FormatTest(
                @"news:comp.infosystems.www.servers.unix",
                @"<p><a class=""externalLink"" href=""news:comp.infosystems.www.servers.unix"">news:comp.infosystems.www.servers.unix</a></p>
");
            FormatTest(
                @"https://server/directory",
                @"<p><a class=""externalLink"" href=""https://server/directory"">https://server/directory</a></p>
");
            FormatTest(
                @"http://www.msn:8080/ http://www.msn:8080",
                @"<p><a class=""externalLink"" href=""http://www.msn:8080/"">http://www.msn:8080/</a> <a class=""externalLink"" href=""http://www.msn:8080"">http://www.msn:8080</a></p>
");
            FormatTest(
                @"notes://server/directory",
                @"<p><a class=""externalLink"" href=""notes://server/directory"">notes://server/directory</a></p>
");
            FormatTest(
                @"ms-help://server/directory",
                @"<p><a class=""externalLink"" href=""ms-help://server/directory"">ms-help://server/directory</a></p>
");
        }

        [Test]
        public void NamedHyperLinks()
        {
            FormatTest(
                @"""msn"":http://www.msn.com ""yahoo"":http://www.yahoo.com",
                @"<p><a class=""externalLink"" href=""http://www.msn.com"">msn</a> <a class=""externalLink"" href=""http://www.yahoo.com"">yahoo</a></p>
");
            FormatTest(
                @"""ftp link"":ftp://feeds.scripting.com",
                @"<p><a class=""externalLink"" href=""ftp://feeds.scripting.com"">ftp link</a></p>
");
            FormatTest(
                @"""gopher link"":gopher://feeds.scripting.com",
                @"<p><a class=""externalLink"" href=""gopher://feeds.scripting.com"">gopher link</a></p>
");
            FormatTest(
                @"""telnet link"":telnet://melvyl.ucop.edu/",
                @"<p><a class=""externalLink"" href=""telnet://melvyl.ucop.edu/"">telnet link</a></p>
");
            FormatTest(
                @"""news group link"":news:comp.infosystems.www.servers.unix",
                @"<p><a class=""externalLink"" href=""news:comp.infosystems.www.servers.unix"">news group link</a></p>
");
            FormatTest(
                @"""secure link"":https://server/directory",
                @"<p><a class=""externalLink"" href=""https://server/directory"">secure link</a></p>
");
            FormatTest(
                @"""port link"":http://www.msn:8080/ ""port link"":http://www.msn:8080",
                @"<p><a class=""externalLink"" href=""http://www.msn:8080/"">port link</a> <a class=""externalLink"" href=""http://www.msn:8080"">port link</a></p>
");
            FormatTest(
                @"""notes link"":notes://server/directory",
                @"<p><a class=""externalLink"" href=""notes://server/directory"">notes link</a></p>
");
            FormatTest(
                @"""ms-help link"":ms-help://server/directory",
                @"<p><a class=""externalLink"" href=""ms-help://server/directory"">ms-help link</a></p>
");
        }

        [Test]
        public void NamedHyperLinksWithBrackets()
        {
            FormatTest(
                @"""msn"":[http://www.msn.com] ""yahoo"":[http://www.yahoo.com]",
                @"<p><a class=""externalLink"" href=""http://www.msn.com"">msn</a> <a class=""externalLink"" href=""http://www.yahoo.com"">yahoo</a></p>
");
            FormatTest(
                @"""ftp link"":[ftp://feeds.scripting.com]",
                @"<p><a class=""externalLink"" href=""ftp://feeds.scripting.com"">ftp link</a></p>
");
            FormatTest(
                @"""gopher link"":[gopher://feeds.scripting.com]",
                @"<p><a class=""externalLink"" href=""gopher://feeds.scripting.com"">gopher link</a></p>
");
            FormatTest(
                @"""telnet link"":[telnet://melvyl.ucop.edu/]",
                @"<p><a class=""externalLink"" href=""telnet://melvyl.ucop.edu/"">telnet link</a></p>
");
            FormatTest(
                @"""news group link"":[news:comp.infosystems.www.servers.unix]",
                @"<p><a class=""externalLink"" href=""news:comp.infosystems.www.servers.unix"">news group link</a></p>
");
            FormatTest(
                @"""secure link"":[https://server/directory]",
                @"<p><a class=""externalLink"" href=""https://server/directory"">secure link</a></p>
");
            FormatTest(
                @"""port link"":[http://www.msn:8080/] ""port link"":[http://www.msn:8080]",
                @"<p><a class=""externalLink"" href=""http://www.msn:8080/"">port link</a> <a class=""externalLink"" href=""http://www.msn:8080"">port link</a></p>
");
            FormatTest(
                @"""notes link"":[notes://server/directory]",
                @"<p><a class=""externalLink"" href=""notes://server/directory"">notes link</a></p>
");
            FormatTest(
                @"""ms-help link"":[ms-help://server/directory]",
                @"<p><a class=""externalLink"" href=""ms-help://server/directory"">ms-help link</a></p>
");
        }

        [Test]
        public void PoundHyperLinks()
        {
            FormatTest(
                @"http://www.msn.com#hello",
                @"<p><a class=""externalLink"" href=""http://www.msn.com#hello"">http://www.msn.com#hello</a></p>
");
            FormatTest(
                @"http://www.msn.com#hello",
                @"<p><a class=""externalLink"" href=""http://www.msn.com#hello"">http://www.msn.com#hello</a></p>
");
            FormatTest(
                @"ms-help://server/directory#hello",
                @"<p><a class=""externalLink"" href=""ms-help://server/directory#hello"">ms-help://server/directory#hello</a></p>
");
        }

        [Test]
        public void PlusSignHyperLinks()
        {
            FormatTest(
                @"http://www.google.com/search?q=wiki+url+specification",
                @"<p><a class=""externalLink"" href=""http://www.google.com/search?q=wiki+url+specification"">http://www.google.com/search?q=wiki+url+specification</a></p>
");
        }

        [Test]
        public void PercentSignHyperLinks()
        {
            FormatTest(
                @"file://server/directory/file%20GM%.doc",
                @"<p><a class=""externalLink"" href=""file://server/directory/file%20GM%.doc"">file://server/directory/file%20GM%.doc</a></p>
");
            FormatTest(
                @"""Sales 20% Markup"":file://server/directory/sales%2020%%20Markup.doc",
                @"<p><a class=""externalLink"" href=""file://server/directory/sales%2020%%20Markup.doc"">Sales 20% Markup</a></p>
");
        }

        [Test]
        public void DoNotConvertIntoLinks()
        {
            FormatTest(
                @":",
                @"<p>:</p>
");
            FormatTest(
                @"http",
                @"<p>http</p>
");
            FormatTest(
                @"http:",
                @"<p>http:</p>
");
            FormatTest(
                @"https",
                @"<p>https</p>
");
            FormatTest(
                @"https:",
                @"<p>https:</p>
");
            FormatTest(
                @"ftp",
                @"<p>ftp</p>
");
            FormatTest(
                @"ftp:",
                @"<p>ftp:</p>
");
            FormatTest(
                @"gopher",
                @"<p>gopher</p>
");
            FormatTest(
                @"gopher:",
                @"<p>gopher:</p>
");
            FormatTest(
                @"news",
                @"<p>news</p>
");
            FormatTest(
                @"news:",
                @"<p>news:</p>
");
            FormatTest(
                @"telnet",
                @"<p>telnet</p>
");
            FormatTest(
                @"telnet:",
                @"<p>telnet:</p>
");
            FormatTest(
                @"ms-help:",
                @"<p>ms-help:</p>
");
            FormatTest(
                @"ms-help",
                @"<p>ms-help</p>
");
            FormatTest(
                @"notes",
                @"<p>notes</p>
");
            FormatTest(
                @"notes:",
                @"<p>notes:</p>
");
        }

        [Test]
        public void ParensHyperLinks()
        {
            FormatTest(
                @"file://servername/directory/File%20(1420).txt",
                @"<p><a class=""externalLink"" href=""file://servername/directory/File%20(1420).txt"">file://servername/directory/File%20(1420).txt</a></p>
");
        }

        [Test]
        public void SemicolonHyperLinks()
        {
            FormatTest(
                @"http://servername/directory/File.html?test=1;test2=2",
                @"<p><a class=""externalLink"" href=""http://servername/directory/File.html?test=1;test2=2"">http://servername/directory/File.html?test=1;test2=2</a></p>
");
        }

        [Test]
        public void DollarSignHyperLinks()
        {
            FormatTest(
                @"http://feeds.scripting.com/discuss/msgReader$4",
                @"<p><a class=""externalLink"" href=""http://feeds.scripting.com/discuss/msgReader$4"">http://feeds.scripting.com/discuss/msgReader$4</a></p>
");
            FormatTest(
                @"file://machine/user$/folder/file",
                @"<p><a class=""externalLink"" href=""file://machine/user$/folder/file"">file://machine/user$/folder/file</a></p>
");
        }

        [Test]
        public void TildeHyperLinks()
        {
            // Collides with textile subscript markup
            FormatTest(
                @"""TildeLink"":http://servername/~mike",
                @"<p><a class=""externalLink"" href=""http://servername/~mike"">TildeLink</a></p>
");
            FormatTest(
                @"http://servername/~mike",
                @"<p><a class=""externalLink"" href=""http://servername/~mike"">http://servername/~mike</a></p>
");
        }

    }
}
