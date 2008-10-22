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
        [Test]
        public void DateTimeInstanceFromString()
        {
            DateTime expectedDateTime = new DateTime(2008, 10, 25, 0, 0, 0);
            DateTime resultDateTime = BELDateTime.InstanceFromString2("2008-10-25");
            Assert.AreEqual(expectedDateTime, resultDateTime, "1. Checking that the resulting DateTime is 25-Oct-2008");

            resultDateTime = BELDateTime.InstanceFromString2("10/25/2008");
            Assert.AreEqual(expectedDateTime, resultDateTime, "2. Checking that the resulting DateTime is 25-Oct-2008");
            resultDateTime = BELDateTime.InstanceFromString2("25-Oct-2008");
            Assert.AreEqual(expectedDateTime, resultDateTime, "3. Checking that the resulting DateTime is 25-Oct-2008");
            resultDateTime = BELDateTime.InstanceFromString2("2008-Oct-25");
            Assert.AreEqual(expectedDateTime, resultDateTime, "4. Checking that the resulting DateTime is 25-Oct-2008");
            resultDateTime = BELDateTime.InstanceFromString2("Oct 25, 2008");
            Assert.AreEqual(expectedDateTime, resultDateTime, "5. Checking that the resulting DateTime is 25-Oct-2008");

            resultDateTime = BELDateTime.InstanceFromString2("25-10-2008");
            Assert.AreNotEqual(expectedDateTime, resultDateTime, "10. Checking that the resulting DateTime is not 25-Oct-2008");
            resultDateTime = BELDateTime.InstanceFromString2("25/10/2008");
            Assert.AreNotEqual(expectedDateTime, resultDateTime, "11. Checking that the resulting DateTime is 25-Oct-2008");

            expectedDateTime = new DateTime(2008, 10, 25, 13, 45, 55);
            resultDateTime = BELDateTime.InstanceFromString2("10/25/2008 1:45:55 PM");
            Assert.AreEqual(expectedDateTime, resultDateTime, "20. Checking that the resulting DateTime is 25-Oct-2008 13:45:55");

        }
    }
}
