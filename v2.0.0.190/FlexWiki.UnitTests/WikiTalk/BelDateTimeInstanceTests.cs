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

namespace FlexWiki.UnitTests.WikiTalk
{
    [TestFixture]
    public class BelDateTimeInstanceTests
    {
        [Test]
        public void DateTimeInstanceNormalTest()
        {
            DateTime expectedDateTime = new DateTime(2005, 1, 13, 13, 32, 45, 416);
            DateTime resultDateTime = BELDateTime.Instance2(2005, 1, 13, 13, 32, 45, 416);
            Assert.AreEqual(expectedDateTime, resultDateTime, "Checking that the resulting DateTime is 13-Jan-2005 13:32:45:416.");
        }
        [Test]
        public void DateTimeInstanceMinTest()
        {
            DateTime expectedDateTime = DateTime.MinValue;
            DateTime resultDateTime = BELDateTime.Instance2(expectedDateTime.Year, expectedDateTime.Month, expectedDateTime.Day,
                expectedDateTime.Hour, expectedDateTime.Minute, expectedDateTime.Second, expectedDateTime.Millisecond);
            Assert.AreEqual(expectedDateTime, resultDateTime, "Checking that the resulting DateTime is the same as DateTime.MinValue.");
        }
        [Test]
        public void DateTimeInstanceMaxTest()
        {
            DateTime expectedDateTime = DateTime.MaxValue - new TimeSpan(TimeSpan.TicksPerMillisecond - 1);
            DateTime resultDateTime = BELDateTime.Instance2(expectedDateTime.Year, expectedDateTime.Month, expectedDateTime.Day,
                expectedDateTime.Hour, expectedDateTime.Minute, expectedDateTime.Second, expectedDateTime.Millisecond);
            Assert.AreEqual(expectedDateTime, resultDateTime, "Checking that the resulting DateTime is the same as DateTime.MaxValue.");
        }
    }
}
