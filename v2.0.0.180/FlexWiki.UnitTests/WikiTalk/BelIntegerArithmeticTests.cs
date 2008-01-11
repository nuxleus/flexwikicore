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
    public class BelIntegerArithmeticTests
    {
        [Test]
        public void BELIntegerAddTest()
        {
            BELInteger source = new BELInteger(4);
            int expectedResult = 10;
            int result = source.Add(6);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Add(6) = 10");
        }
        [Test]
        public void BELIntegerAddNegativeTest()
        {
            BELInteger source = new BELInteger(4);
            int expectedResult = 2;
            int result = source.Add(-2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Add(-2) = 2");
        }
        [Test]
        public void BELIntegerAddZeroTest()
        {
            BELInteger source = new BELInteger(4);
            int result = source.Add(0);
            Assert.AreEqual(source.Value, result, "Checking that BELInteger(4).Add(0) = 4");
        }
        [Test]
        public void BELIntegerAddNegativeSourceTest()
        {
            BELInteger source = new BELInteger(-4);
            int expectedResult = -2;
            int result = source.Add(2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-4).Add(2) = -2");
        }
        [Test]
        public void BELIntegerAddZeroResultTest()
        {
            BELInteger source = new BELInteger(-10);
            int expectedResult = 0;
            int result = source.Add(10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-10).Add(10) = 0");
        }
        [Test]
        public void BELIntegerAddMaxValueTest()
        {
            BELInteger source = new BELInteger(int.MaxValue);
            int max = int.MaxValue;
            int expectedResult = max + 10;
            int result = source.Add(10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Add(10) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerAddMaxValueNegativeTest()
        {
            BELInteger source = new BELInteger(int.MaxValue);
            int expectedResult = int.MaxValue - 10;
            int result = source.Add(-10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Add(-10) works");
        }
        [Test]
        public void BELIntegerAddMinValueTest()
        {
            BELInteger source = new BELInteger(int.MinValue);
            int expectedResult = int.MinValue + 10;
            int result = source.Add(10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Add(10) works");
        }
        [Test]
        public void BELIntegerAddMinValueNegativeTest()
        {
            BELInteger source = new BELInteger(int.MinValue);
            int min = int.MinValue;
            int expectedResult = min - 10;
            int result = source.Add(-10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Add(-10) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerAddStringTest()
        {
            BELInteger source = new BELInteger(4);
            int expectedResult = 10;
            int result = source.Add("6");
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Add(\"6\") = 10");
        }
        [Test]
        public void BELIntegerSubtractTest()
        {
            BELInteger source = new BELInteger(10);
            int expectedResult = 4;
            int result = source.Subtract(6);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Subtract(6) = 4");
        }
        [Test]
        public void BELIntegerSubtractNegativeTest()
        {
            BELInteger source = new BELInteger(4);
            int expectedResult = 6;
            int result = source.Subtract(-2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Subtract(-2) = 6");
        }
        [Test]
        public void BELIntegerSubtractZeroTest()
        {
            BELInteger source = new BELInteger(4);
            int result = source.Subtract(0);
            Assert.AreEqual(source.Value, result, "Checking that BELInteger(4).Subtract(0) = 4");
        }
        [Test]
        public void BELIntegerSubtractNegativeSourceTest()
        {
            BELInteger source = new BELInteger(-4);
            int expectedResult = -6;
            int result = source.Subtract(2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-4).Subtract(2) = -6");
        }
        [Test]
        public void BELIntegerSubtractZeroResultTest()
        {
            BELInteger source = new BELInteger(10);
            int expectedResult = 0;
            int result = source.Subtract(10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Subtract(10) = 0");
        }
        [Test]
        public void BELIntegerSubtractMaxValueTest()
        {
            BELInteger source = new BELInteger(int.MaxValue);
            int max = int.MaxValue;
            int expectedResult = max - 10;
            int result = source.Subtract(10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Subtract(10) works");
        }
        [Test]
        public void BELIntegerSubtractMaxValueNegativeTest()
        {
            BELInteger source = new BELInteger(int.MaxValue);
            int max = int.MaxValue;
            int expectedResult = max + 10;
            int result = source.Subtract(-10);
            Assert.AreEqual(expectedResult, result, "Checkng that BELInteger(int.MaxValue).Subtract(-10) wraps correctly");
        }
        [Test]
        public void BELIntegerSubtractMinValueTest()
        {
            BELInteger source = new BELInteger(int.MinValue);
            int expectedResult = int.MinValue + 10;
            int result = source.Subtract(-10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Subtract(-10) works");
        }
        [Test]
        public void BELIntegerSubtractMinValueNegativeTest()
        {
            BELInteger source = new BELInteger(int.MinValue);
            int min = int.MinValue;
            int expectedResult = min - 10;
            int result = source.Subtract(10);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Subtract(10) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerStringSubtractTest()
        {
            BELInteger source = new BELInteger(10);
            int expectedResult = 4;
            int result = source.Subtract("6");
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Subtract(\"6\") = 4");
        }
        [Test]
        public void BELIntegerMultiplyTest()
        {
            BELInteger source = new BELInteger(10);
            int expectedResult = 60;
            int result = source.Multiply(6);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Multiply(6) = 60");
        }
        [Test]
        public void BELIntegerMultiplyNegativeTest()
        {
            BELInteger source = new BELInteger(4);
            int expectedResult = -8;
            int result = source.Multiply(-2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Multiply(-2) = -8");
        }
        [Test]
        public void BELIntegerMultiplyZeroTest()
        {
            BELInteger source = new BELInteger(4);
            int result = source.Multiply(0);
            Assert.AreEqual(0, result, "Checking that BELInteger(4).Multiply(0) = 0");
        }
        [Test]
        public void BELIntegerMultiplyNegativeSourceTest()
        {
            BELInteger source = new BELInteger(-4);
            int expectedResult = -8;
            int result = source.Multiply(2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-4).Multiply(2) = -8");
        }
        [Test]
        public void BELIntegerMultiplyZeroResultTest()
        {
            BELInteger source = new BELInteger(10);
            int expectedResult = 0;
            int result = source.Multiply(0);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Multiply(0) = 0");
        }
        [Test]
        public void BELIntegerMultiplyMaxValueTest()
        {
            BELInteger source = new BELInteger(int.MaxValue);
            int max = int.MaxValue;
            int expectedResult = max * 2;
            int result = source.Multiply(2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Multiply(2) works");
        }
        [Test]
        public void BELIntegerMultiplyMaxValueNegativeTest()
        {
            BELInteger source = new BELInteger(int.MaxValue);
            int max = int.MaxValue;
            int expectedResult = max * -2;
            int result = source.Multiply(-2);
            Assert.AreEqual(expectedResult, result, "Checkng that BELInteger(int.MaxValue).Multiply(-2) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerMultiplyMinValueTest()
        {
            BELInteger source = new BELInteger(int.MinValue);
            int min = int.MinValue;
            int expectedResult = min * 2;
            int result = source.Multiply(2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Multiply(2) works");
        }
        [Test]
        public void BELIntegerMultiplyMinValueNegativeTest()
        {
            BELInteger source = new BELInteger(int.MinValue);
            int min = int.MinValue;
            int expectedResult = min * -2;
            int result = source.Multiply(-2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Multiply(-2) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerMultiplyByOneTest()
        {
            BELInteger source = new BELInteger(8);
            int result = source.Multiply(1);
            Assert.AreEqual(source.Value, result, "Checking that BELInteger(8).Multiply(1) = 8");
        }
        [Test]
        public void BELIntegerStringMultiplyTest()
        {
            BELInteger source = new BELInteger(10);
            int expectedResult = 60;
            int result = source.Multiply("6");
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Multiply(\"6\") = 60");
        }
        [Test]
        public void BELIntegerDivideTest()
        {
            BELInteger source = new BELInteger(10);
            int expectedResult = 5;
            int result = source.Divide(2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Divide(2) = 5");
        }
        [Test]
        public void BELIntegerDivideNegativeTest()
        {
            BELInteger source = new BELInteger(4);
            int expectedResult = -2;
            int result = source.Divide(-2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Divide(-2) = -2");
        }
        [Test]
        [ExpectedException(typeof(DivideByZeroException))]
        public void BELIntegerDivideZeroTest()
        {
            BELInteger source = new BELInteger(4);
            int result = source.Divide(0);
        }
        [Test]
        public void BELIntegerDivideNegativeSourceTest()
        {
            BELInteger source = new BELInteger(-4);
            int expectedResult = -2;
            int result = source.Divide(2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-4).Divide(2) = -8");
        }
        [Test]
        public void BELIntegerDivideMaxValueTest()
        {
            BELInteger source = new BELInteger(int.MaxValue);
            int max = int.MaxValue;
            int expectedResult = max / 2;
            int result = source.Divide(2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Divide(2) works");
        }
        [Test]
        public void BELIntegerDivideMaxValueNegativeTest()
        {
            BELInteger source = new BELInteger(int.MaxValue);
            int max = int.MaxValue;
            int expectedResult = max / -2;
            int result = source.Divide(-2);
            Assert.AreEqual(expectedResult, result, "Checkng that BELInteger(int.MaxValue).Divide(-2) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerDivideMinValueTest()
        {
            BELInteger source = new BELInteger(int.MinValue);
            int min = int.MinValue;
            int expectedResult = min / 2;
            int result = source.Divide(2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Divide(2) works");
        }
        [Test]
        public void BELIntegerDivideMinValueNegativeTest()
        {
            BELInteger source = new BELInteger(int.MinValue);
            int min = int.MinValue;
            int expectedResult = min / -2;
            int result = source.Divide(-2);
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Divide(-2) 'wraps' correctly");
        }
        [Test]
        public void BELIntegerDivideByOneTest()
        {
            BELInteger source = new BELInteger(8);
            int result = source.Divide(1);
            Assert.AreEqual(source.Value, result, "Checking that BELInteger(8).Divide(1) = 8");
        }
        [Test]
        public void BELIntegerStringDivideTest()
        {
            BELInteger source = new BELInteger(10);
            int expectedResult = 5;
            int result = source.Divide("2");
            Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Divide(\"2\") = 5");
        }

    }
}
