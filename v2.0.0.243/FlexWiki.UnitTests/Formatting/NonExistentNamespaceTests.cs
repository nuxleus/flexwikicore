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
