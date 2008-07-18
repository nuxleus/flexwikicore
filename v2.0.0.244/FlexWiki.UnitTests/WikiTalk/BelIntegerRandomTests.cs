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
    public class BelIntegerRandomTests
    {
        [Test]
        public void BELIntegerRandomRangeTest()
        {
            BELInteger source = new BELInteger(0);
            int maxValue = 11;
            int minValue = 0;
            int result = source.Random(1, 10);
            Assert.Less(result, maxValue, "1 - Checking that BELInteger(0).Random(1, 10) < 10");
            Assert.Greater(result, minValue, "1 - Checking that BELInyeger(0).Random(1, 10) > 0");
            result = source.Random(1, 10);
            Assert.Less(result, maxValue, "2 - Checking that BELInteger(0).Random(1, 10) < 10");
            Assert.Greater(result, minValue, "2 - Checking that BELInyeger(0).Random(1, 10) > 0");
            result = source.Random(1, 10);
            Assert.Less(result, maxValue, "3 - Checking that BELInteger(0).Random(1, 10) < 10");
            Assert.Greater(result, minValue, "3 - Checking that BELInyeger(0).Random(1, 10) > 0");
            result = source.Random(1, 10);
            Assert.Less(result, maxValue, "4 - Checking that BELInteger(0).Random(1, 10) < 10");
            Assert.Greater(result, minValue, "4 - Checking that BELInyeger(0).Random(1, 10) > 0");
            result = source.Random(1, 10);
            Assert.Less(result, maxValue, "5 - Checking that BELInteger(0).Random(1, 10) < 10");
            Assert.Greater(result, minValue, "5 - Checking that BELInyeger(0).Random(1, 10) > 0");
        }

        [Test]
        public void DELIntegerRandomMaxValueTest()
        {
            BELInteger source = new BELInteger(0);
            int maxValue = int.MaxValue;
            int minValue = 0;
            int result = source.Random(1, maxValue - 1);
            Assert.Less(result, maxValue, "1 - Checking that BELInteger(0).Random(1, " + ((int)(maxValue - 1)).ToString() + ") < " + ((int)(maxValue - 1)).ToString());
            Assert.Greater(result, minValue, "1 - Checking that BELInyeger(0).Random(1, " + ((int)(maxValue - 1)).ToString() + ") > 0");
            result = source.Random(1, maxValue - 1);
            Assert.Less(result, maxValue, "2 - Checking that BELInteger(0).Random(1, " + ((int)(maxValue - 1)).ToString() + ") < " + ((int)(maxValue - 1)).ToString());
            Assert.Greater(result, minValue, "2 - Checking that BELInyeger(0).Random(1, " + ((int)(maxValue - 1)).ToString() + ") > 0");
            result = source.Random(1, maxValue - 1);
            Assert.Less(result, maxValue, "3 - Checking that BELInteger(0).Random(1, " + ((int)(maxValue - 1)).ToString() + ") < " + ((int)(maxValue - 1)).ToString());
            Assert.Greater(result, minValue, "3 - Checking that BELInyeger(0).Random(1, " + ((int)(maxValue - 1)).ToString() + ") > 0");
            result = source.Random(1, maxValue - 1);
            Assert.Less(result, maxValue, "4 - Checking that BELInteger(0).Random(1, " + ((int)(maxValue - 1)).ToString() + ") < " + ((int)(maxValue - 1)).ToString());
            Assert.Greater(result, minValue, "4 - Checking that BELInyeger(0).Random(1, " + ((int)(maxValue - 1)).ToString() + ") > 0");
            result = source.Random(1, maxValue - 1);
            Assert.Less(result, maxValue, "5 - Checking that BELInteger(0).Random(1, " + ((int)(maxValue - 1)).ToString() + ") < " + ((int)(maxValue - 1)).ToString());
            Assert.Greater(result, minValue, "5 - Checking that BELInyeger(0).Random(1, " + ((int)(maxValue - 1)).ToString() + ") > 0");

        }
    }
}
