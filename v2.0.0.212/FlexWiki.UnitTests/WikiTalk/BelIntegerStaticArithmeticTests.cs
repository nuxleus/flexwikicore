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
    public class BelIntegerStaticArithmeticTests
    {
        [Test]
        public void BELIntegerStaticAddTest()
        {
            int expectedResult = 10;
            int result = BELInteger.Add(4, 6);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Add(4, 6) = 10");
        }
        [Test]
        public void BELIntegerStaticAddNegativeTest()
        {
            int expectedResult = 2;
            int result = BELInteger.Add(4, -2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Add(4, -2) = 2");
        }
        [Test]
        public void BELIntegerStaticAddZeroTest()
        {
            int result = BELInteger.Add(4, 0);
            Assert.AreEqual(4, result, "Checking that BELInteger.Add(4, 0) = 4");
        }
        [Test]
        public void BELIntegerStaticAddNegativeSourceTest()
        {
            int expectedResult = -2;
            int result = BELInteger.Add(-4, 2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Add(-4, 2) = -2");
        }
        [Test]
        public void BELIntegerStaticAddZeroResultTest()
        {
            BELInteger source = new BELInteger(-10);
            int expectedResult = 0;
            int result = source.Add(10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-10).Add(10) = 0");
        }
        [Test]
        public void BELIntegerStaticAddMaxValueTest()
        {
            int max = int.MaxValue;
            int expectedResult = max + 10;
            int result = BELInteger.Add(int.MaxValue, 10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Add(int.MaxValue, 10) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerStaticAddMaxValueNegativeTest()
        {
            int expectedResult = int.MaxValue - 10;
            int result = BELInteger.Add(int.MaxValue, -10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Add(int.MaxValue, -10) works");
        }
        [Test]
        public void BELIntegerStaticAddMinValueTest()
        {
            int expectedResult = int.MinValue + 10;
            int result = BELInteger.Add(int.MinValue, 10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Add(int.MinValue, 10) works");
        }
        [Test]
        public void BELIntegerStaticAddMinValueNegativeTest()
        {
            int min = int.MinValue;
            int expectedResult = min - 10;
            int result = BELInteger.Add(int.MinValue, -10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Add(int.MinValue, -10) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerStaticAddStringTest()
        {
            int expectedResult = 10;
            int result = BELInteger.Add("4", "6");
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Add(\"4\", \"6\") = 10");
        }
        [Test]
        public void BELIntegerStaticSubtractTest()
        {
            int expectedResult = 4;
            int result = BELInteger.Subtract(10, 6);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Subtract(10, 6) = 4");
        }
        [Test]
        public void BELIntegerStaticSubtractNegativeTest()
        {
            int expectedResult = 6;
            int result = BELInteger.Subtract(4, -2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Subtract(4, -2) = 6");
        }
        [Test]
        public void BELIntegerStaticSubtractZeroTest()
        {
            int result = BELInteger.Subtract(4, 0);
            Assert.AreEqual(4, result, "Checking that BELInteger.Subtract(4, 0) = 4");
        }
        [Test]
        public void BELIntegerStaticSubtractNegativeSourceTest()
        {
            int expectedResult = -6;
            int result = BELInteger.Subtract(-4, 2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Subtract(-4, 2) = -6");
        }
        [Test]
        public void BELIntegerStaticSubtractZeroResultTest()
        {
            int expectedResult = 0;
            int result = BELInteger.Subtract(10, 10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Subtract(10, 10) = 0");
        }
        [Test]
        public void BELIntegerStaticSubtractMaxValueTest()
        {
            int max = int.MaxValue;
            int expectedResult = max - 10;
            int result = BELInteger.Subtract(int.MaxValue, 10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Subtract(int.MaxValue, 10) works");
        }
        [Test]
        public void BELIntegerStaticSubtractMaxValueNegativeTest()
        {
            int max = int.MaxValue;
            int expectedResult = max + 10;
            int result = BELInteger.Subtract(int.MaxValue, -10);
            Assert.AreEqual(expectedResult, result, "Checkng that BELInteger.Subtract(int.MaxValue, -10) wraps correctly");
        }
        [Test]
        public void BELIntegerStaticSubtractMinValueTest()
        {
            int expectedResult = int.MinValue + 10;
            int result = BELInteger.Subtract(int.MinValue, -10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Subtract(int.MinValue, -10) works");
        }
        [Test]
        public void BELIntegerStaticSubtractMinValueNegativeTest()
        {
            int min = int.MinValue;
            int expectedResult = min - 10;
            int result = BELInteger.Subtract(int.MinValue, 10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Subtract(int.MinValue, 10) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerStaticStringSubtractTest()
        {
            int expectedResult = 4;
            int result = BELInteger.Subtract("10", "6");
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Subtract(\"10\", \"6\") = 4");
        }
        [Test]
        public void BELIntegerStaticMultiplyTest()
        {
            int expectedResult = 60;
            int result = BELInteger.Multiply(10, 6);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Multiply(10, 6) = 60");
        }
        [Test]
        public void BELIntegerStaticMultiplyNegativeTest()
        {
            int expectedResult = -8;
            int result = BELInteger.Multiply(4, -2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Multiply(4, -2) = -8");
        }
        [Test]
        public void BELIntegerStaticMultiplyZeroTest()
        {
            int result = BELInteger.Multiply(4, 0);
            Assert.AreEqual(0, result, "Checking that BELInteger.Multiply(4, 0) = 0");
        }
        [Test]
        public void BELIntegerStaticMultiplyNegativeSourceTest()
        {
            int expectedResult = -8;
            int result = BELInteger.Multiply(-4, 2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Multiply(-4, 2) = -8");
        }
        [Test]
        public void BELIntegerStaticMultiplyZeroResultTest()
        {
            int expectedResult = 0;
            int result = BELInteger.Multiply(10, 0);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Multiply(10, 0) = 0");
        }
        [Test]
        public void BELIntegerStaticMultiplyMaxValueTest()
        {
            int max = int.MaxValue;
            int expectedResult = max * 2;
            int result = BELInteger.Multiply(int.MaxValue, 2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Multiply(int.MaxValue, 2) works");
        }
        [Test]
        public void BELIntegerStaticMultiplyMaxValueNegativeTest()
        {
            int max = int.MaxValue;
            int expectedResult = max * -2;
            int result = BELInteger.Multiply(int.MaxValue, -2);
            Assert.AreEqual(expectedResult, result, "Checkng that BELInteger.Multiply(int.MaxValue, -2) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerStaticMultiplyMinValueTest()
        {
            int min = int.MinValue;
            int expectedResult = min * 2;
            int result = BELInteger.Multiply(int.MinValue, 2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Multiply(int.MinValue, 2) works");
        }
        [Test]
        public void BELIntegerStaticMultiplyMinValueNegativeTest()
        {
            int min = int.MinValue;
            int expectedResult = min * -2;
            int result = BELInteger.Multiply(int.MinValue, -2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Multiply(int.MinValue, -2) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerStaticMultiplyByOneTest()
        {
            int result = BELInteger.Multiply(8, 1);
            Assert.AreEqual(8, result, "Checking that BELInteger.Multiply(8, 1) = 8");
        }
        [Test]
        public void BELIntegerStaticStringMultiplyTest()
        {
            int expectedResult = 60;
            int result = BELInteger.Multiply("10", "6");
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Multiply(\"10\", \"6\") = 60");
        }
        [Test]
        public void BELIntegerStaticDivideTest()
        {
            int expectedResult = 5;
            int result = BELInteger.Divide(10, 2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Divide(10, 2) = 5");
        }
        [Test]
        public void BELIntegerStaticDivideNegativeTest()
        {
            int expectedResult = -2;
            int result = BELInteger.Divide(4, -2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Divide(4, -2) = -2");
        }
        [Test]
        [ExpectedException(typeof(DivideByZeroException))]
        public void BELIntegerStaticDivideZeroTest()
        {
            int result = BELInteger.Divide(4, 0);
        }
        [Test]
        public void BELIntegerStaticDivideNegativeSourceTest()
        {
            int expectedResult = -2;
            int result = BELInteger.Divide(-4, 2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Divide(-4, 2) = -8");
        }
        [Test]
        public void BELIntegerStaticDivideMaxValueTest()
        {
            int max = int.MaxValue;
            int expectedResult = max / 2;
            int result = BELInteger.Divide(int.MaxValue, 2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Divide(int.MaxValue, 2) works");
        }
        [Test]
        public void BELIntegerStaticDivideMaxValueNegativeTest()
        {
            int max = int.MaxValue;
            int expectedResult = max / -2;
            int result = BELInteger.Divide(int.MaxValue, -2);
            Assert.AreEqual(expectedResult, result, "Checkng that BELInteger.Divide(int.MaxValue, -2) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerStaticDivideMinValueTest()
        {
            int min = int.MinValue;
            int expectedResult = min / 2;
            int result = BELInteger.Divide(int.MinValue, 2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Divide(int.MinValue, 2) works");
        }
        [Test]
        public void BELIntegerStaticDivideMinValueNegativeTest()
        {
            int min = int.MinValue;
            int expectedResult = min / -2;
            int result = BELInteger.Divide(int.MinValue, -2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Divide(int.MinValue, -2) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerStaticDivideByOneTest()
        {
            int result = BELInteger.Divide(8, 1);
            Assert.AreEqual(8, result, "Checking that BELInteger.Divide(8, 1) = 8");
        }
        [Test]
        public void BELIntegerStaticStringDivideTest()
        {
            int expectedResult = 5;
            int result = BELInteger.Divide("10", "2");
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger.Divide(\"10\", \"2\") = 5");
        }

    }
}
