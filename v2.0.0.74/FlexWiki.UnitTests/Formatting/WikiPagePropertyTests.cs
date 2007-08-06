using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using FlexWiki.Formatting; 

namespace FlexWiki.UnitTests.Formatting
{
    [TestFixture]
    public class WikiPagePropertyTests : FormattingTestsBase
    {
        [Test]
        public void SinglineLinePropertyTest()
        {
            FormatTest(
                @"Singleline: single line property",
                @"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Singleline</legend><div class=""PropertyValue""><a name=""Singleline"" class=""Anchor"">single line property</a></div>
</fieldset>

");
        }

        [Test]
        public void FormattedSingleLinePropertyTest()
        {
            FormatTest(
                @"Singleline: '''bold''' ''italics'' -deleted-",
                @"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Singleline</legend><div class=""PropertyValue""><a name=""Singleline"" class=""Anchor""><strong>bold</strong> <em>italics</em> <del>deleted</del></a></div>
</fieldset>

");
        }

        [Test]
        public void MultilinePropertyTest()
        {
            FormatTest(
              @"Multiline:[
first line
second line
]",
                @"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Multiline</legend><div class=""PropertyValue""><a name=""Multiline"" class=""Anchor""><p>first line</p>
<p>second line</p>
</a></div>
</fieldset>
");
        }

        [Test]
        public void FormattedMultilinePropertyTest()
        {
            FormatTest(
              @"Multiline:[
!Heading1
'''bold''' ''italics'' -deleted-
]",
                @"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Multiline</legend><div class=""PropertyValue""><a name=""Multiline"" class=""Anchor""><h1>Heading1</h1>

<p><strong>bold</strong> <em>italics</em> <del>deleted</del></p>
</a></div>
</fieldset>
");
        }

    }
}
