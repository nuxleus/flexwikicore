using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using FlexWiki.Collections;
using FlexWiki.Formatting; 

namespace FlexWiki.UnitTests.Formatting
{
    [TestFixture] 
    public class NonExistentNamespaceTests : FormattingTestsBase
    {
        [Test]
        public void NonExistentNamespaceTest()
        {
            FormatTest(
              @"FooBar.BlahBlah",
              @"<p>FooBar.BlahBlah</p>
");
        }

        [Test]
        public void NonExistentNamespaceLeadingTextTest()
        {
            FormatTest(
              @"Leading text FooBar.BlahBlah",
              @"<p>Leading text FooBar.BlahBlah</p>
");
        }

        [Test]
        public void NonExistentNamespaceTrailingTextTest()
        {
            FormatTest(
              @"FooBar.BlahBlah trailing text",
              @"<p>FooBar.BlahBlah trailing text</p>
");
        }

        [Test]
        public void NonExistentNamespaceLeadingAndTrailingTextTest()
        {
            FormatTest(
              @"Leading text FooBar.BlahBlah trailing text",
              @"<p>Leading text FooBar.BlahBlah trailing text</p>
");
        }

        [Test]
        public void NonExistentMultipleNamespaceTest()
        {
            FormatTest(
              @"System.Diagnostics.Debug.WriteLine",
              @"<p>System.Diagnostics.Debug.WriteLine</p>
");
        }

    }
}
