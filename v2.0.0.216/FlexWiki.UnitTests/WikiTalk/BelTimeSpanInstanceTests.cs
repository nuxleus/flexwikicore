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
    public class BelTimeSpanInstanceTests
    {
        [Test]
        public void TimeSpanInstanceNormalTest()
        {
            TimeSpan expectedTimeSpan = new TimeSpan(1, 2, 3, 4, 5);
            TimeSpan resultTimeSpan = BELTimeSpan.Instance2(1, 2, 3, 4, 5);
            Assert.AreEqual(expectedTimeSpan, resultTimeSpan, "Checking that the resulting TimeSpan is 1 day, 2 hours, 3 minutes, 4 seconds a 5 milliseconds.");
        }
        [Test]
        public void TimeSpanInstanceMinTest()
        {
            TimeSpan expectedTimeSpan = TimeSpan.MinValue + new TimeSpan(5808);
            TimeSpan resultTimeSpan = BELTimeSpan.Instance2(expectedTimeSpan.Days, expectedTimeSpan.Hours, expectedTimeSpan.Minutes,
                expectedTimeSpan.Seconds, expectedTimeSpan.Milliseconds);
            Assert.AreEqual(expectedTimeSpan, resultTimeSpan, "Checking that the resulting TimeSpan is the same as TimeSpan.MinValue.");
        }
        [Test]
        public void TimeSpanInstanceMaxTest()
        {
            TimeSpan expectedTimeSpan = TimeSpan.MaxValue - new TimeSpan(5807);
            TimeSpan resultTimeSpan = BELTimeSpan.Instance2(expectedTimeSpan.Days, expectedTimeSpan.Hours, expectedTimeSpan.Minutes,
                expectedTimeSpan.Seconds, expectedTimeSpan.Milliseconds);
            Assert.AreEqual(expectedTimeSpan, resultTimeSpan, "Checking that the resulting TimeSpan is the same as TimeSpan.MaxValue.");
        }

    }
}
