using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using FlexWiki.Formatting;

namespace FlexWiki.UnitTests.Formatting
{
    public class TableFormattingTests : FormattingTestsBase
    {
        [Test]
        public void BasicTableTests()
        {
            FormatTest(
                @"||",
                @"<p>||</p>
");

            FormatTest(
              @"||t1||",
              @"<table cellpadding=""2"" cellspacing=""1"" class=""TableClass"">
<tr>
<td  class=""TableCell"">t1</td>
</tr>
</table>
");

            FormatTest(
              @"not a table||",
              @"<p>not a table||</p>
");

            FormatTest(
              @"||not a table",
              @"<p>||not a table</p>
");

            FormatTest(
              @"||''table''||'''more'''||columns||
||1||2||3||
",
              @"<table cellpadding=""2"" cellspacing=""1"" class=""TableClass"">
<tr>
<td  class=""TableCell""><em>table</em></td>
<td  class=""TableCell""><strong>more</strong></td>
<td  class=""TableCell"">columns</td>
</tr>
<tr>
<td  class=""TableCell"">1</td>
<td  class=""TableCell"">2</td>
<td  class=""TableCell"">3</td>
</tr>
</table>
");
        }

        [Test]
        public void EmoticonInTableTest()
        {
            FormatTest(
              @"||:-)||",
              @"<table cellpadding=""2"" cellspacing=""1"" class=""TableClass"">
<tr>
<td  class=""TableCell""><img src=""" + _lm.LinkToImage("emoticons/regular_smile.gif") + @"""/></td>
</tr>
</table>
");
        }

        [Test]
        public void WikinameInTableTest()
        {
            string s = FormattedTestText(@"||BigDog||");
            AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_namespaceManager.QualifiedTopicNameFor("BigDog")) + @""">BigDog</a>");
        }

        [Test]
        public void HyperlinkInTableTest()
        {
            FormatTest(
              @"||http://www.yahoo.com/foo.html||",
              @"<table cellpadding=""2"" cellspacing=""1"" class=""TableClass"">
<tr>
<td  class=""TableCell""><a class=""externalLink"" href=""http://www.yahoo.com/foo.html"">http://www.yahoo.com/foo.html</a></td>
</tr>
</table>
");
        }

        [Test]
        public void TableFormattingRulesTest1()
        {
            TableCellInfo info = new TableCellInfo();
            info.Parse("!+");
            Assert.IsTrue(info.IsHighlighted);
            Assert.IsFalse(info.AllowBreaks);
            Assert.AreEqual(info.CellAlignment, AlignOption.None);
            Assert.AreEqual(info.CellWidth, TableCellInfo.UnspecifiedWidth);
            Assert.AreEqual(info.ColSpan, 1);
            Assert.AreEqual(info.RowSpan, 1);
            Assert.AreEqual(info.BackgroundColor, null);
        }

        [Test]
        public void TableFormattingRulesCellWidthTest()
        {
            TableCellInfo info = new TableCellInfo();
            info.Parse("W10");
            Assert.AreEqual(info.CellWidth, 10);
        }

        [Test]
        public void TableFormattingRulesColor()
        {
            TableCellInfo info = new TableCellInfo();
            info.Parse("!*red*+");
            Assert.IsTrue(info.IsHighlighted);
            Assert.AreEqual(info.BackgroundColor, "red");
            Assert.IsFalse(info.AllowBreaks);
        }

        [Test]
        public void TableFormattingRulesSpanTest()
        {
            TableCellInfo info = new TableCellInfo();
            info.Parse("C10R3");
            Assert.AreEqual(info.ColSpan, 10);
            Assert.AreEqual(info.RowSpan, 3);
        }

        [Test]
        public void TableFormattingRulesTestLeftCell()
        {
            TableCellInfo info = new TableCellInfo();
            info.Parse("[");
            Assert.AreEqual(info.CellAlignment, AlignOption.Left);
        }

        [Test]
        public void TableFormattingRulesTestRightCell()
        {
            TableCellInfo info = new TableCellInfo();
            info.Parse("]");
            Assert.AreEqual(info.CellAlignment, AlignOption.Right);
        }

        [Test]
        public void TableFormattingRulesTestLeftCenter()
        {
            TableCellInfo info = new TableCellInfo();
            info.Parse("^");
            Assert.AreEqual(info.CellAlignment, AlignOption.Center);
        }

    }
}
