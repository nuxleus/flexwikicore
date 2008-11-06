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
    public class BelDateTimeDaysInMonthTests
    {
        [Test]
        public void DateTimeDaysInMonthNormalTest()
        {
            int daysInMonth = BELDateTime.DaysInMonth(2005, 1);
            Assert.AreEqual(31, daysInMonth, "Checking that BELDateTime.DaysInMonth gives 31 days for January 2005");
        }
        [Test]
        public void DateTimeDaysInMonthNotLeapYearTest()
        {
            int daysInMonth = BELDateTime.DaysInMonth(1998, 2);
            Assert.AreEqual(28, daysInMonth, "Checking that BELDateTime.DaysInMonth gives 28 days for February 1998, which was not a leap year");
        }
        [Test]
        public void DateTimeDaysInMonthLeapYearTest()
        {
            int daysInMonth = BELDateTime.DaysInMonth(1996, 2);
            Assert.AreEqual(29, daysInMonth, "Checking that BELDateTime.DaysInMonth gives 29 days for February 1996, which was a leap year");
        }

    }
}
