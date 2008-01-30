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
    public class BelIntegerComparisonTests
    {
        [Test]
        public void BELIntegerLessThanTest()
        {
            BELInteger left = new BELInteger(4);
            Assert.IsTrue(left.LessThan(5));
            Assert.IsFalse(left.LessThan(4));
            Assert.IsFalse(left.LessThan(3));
        }

        [Test]
        public void BELIntegerLessThanOrEqualToTest()
        {
            BELInteger left = new BELInteger(4);
            Assert.IsTrue(left.LessThanOrEqualTo(5));
            Assert.IsTrue(left.LessThanOrEqualTo(4));
            Assert.IsFalse(left.LessThanOrEqualTo(3));
        }

        [Test]
        public void BELIntegerGreaterThanTest()
        {
            BELInteger left = new BELInteger(4);
            Assert.IsFalse(left.GreaterThan(5));
            Assert.IsFalse(left.GreaterThan(4));
            Assert.IsTrue(left.GreaterThan(3));
        }

        [Test]
        public void BELIntegerGreaterThanOrEqualToTest()
        {
            BELInteger left = new BELInteger(4);
            Assert.IsFalse(left.GreaterThanOrEqualTo(5));
            Assert.IsTrue(left.GreaterThanOrEqualTo(4));
            Assert.IsTrue(left.GreaterThanOrEqualTo(3));
        }


    }
}
