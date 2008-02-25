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
                @"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Multiline</legend><div class=""PropertyValue""><a name=""Multiline"" class=""Anchor""><h1><a name=""Heading1"" class=""Anchor""></a>Heading1</h1>

<p><strong>bold</strong> <em>italics</em> <del>deleted</del></p>
</a></div>
</fieldset>
");
        }

    }
}
