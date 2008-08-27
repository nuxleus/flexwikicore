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
    public class BelDateTimeWeeksInMontTests
    {
        [Test]
        public void DateTimeWeeksInMonthNormalTest()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(2005, 1, (int)DayOfWeek.Monday);
            Assert.AreEqual(6, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 6 weeks for January 2005");
        }
        [Test]
        public void DateTimeWeeksInMonthNotLeapYearTest()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(1998, 2, (int)DayOfWeek.Monday);
            Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1998, which was not a leap year");
        }
        [Test]
        public void DateTimeWeeksInMonthLeapYearTest()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(1996, 2, (int)DayOfWeek.Monday);
            Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1996, which was a leap year");
        }
        [Test]
        public void DateTimeWeeksInMonthStartsOnSundayTest()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(2004, 8, (int)DayOfWeek.Monday);
            Assert.AreEqual(6, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 6 weeks for August 2004, which starts on a Sunday");
        }
        [Test]
        public void DateTimeWeeksInMonthStartsOnMondayTest()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(2004, 11, (int)DayOfWeek.Monday);
            Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for November 2004, which starts on a Monday");
        }
        [Test]
        public void DateTimeWeeksInMonthCalendarMonday()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Monday);
            Assert.AreEqual(4, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 4 weeks for February 1999 for a calendar that starts on a Monday");
        }
        [Test]
        public void DateTimeWeeksInMonthCalendarTuesday()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Tuesday);
            Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Tuesday");
        }
        [Test]
        public void DateTimeWeeksInMonthCalendarWednesday()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Wednesday);
            Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Wednesday");
        }
        [Test]
        public void DateTimeWeeksInMonthCalendarThursday()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Thursday);
            Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Thursday");
        }
        [Test]
        public void DateTimeWeeksInMonthCalendarFriday()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Friday);
            Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Friday");
        }
        [Test]
        public void DateTimeWeeksInMonthCalendarSaturday()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Saturday);
            Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Saturday");
        }
        [Test]
        public void DateTimeWeeksInMonthCalendarSunday()
        {
            int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Sunday);
            Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Sunday");
        }
    }
}
