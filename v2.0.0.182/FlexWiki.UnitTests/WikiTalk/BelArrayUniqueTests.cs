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
using System.Collections;
using System.Text;

using NUnit.Framework; 

namespace FlexWiki.UnitTests.WikiTalk
{
    [TestFixture]
    public class BelArrayUniqueTests
    {
        [Test]
        public void BELArrayUniqueTest()
        {
            BELArray sourceArray = new BELArray();
            sourceArray.Add("one");
            sourceArray.Add("one");
            sourceArray.Add("two");
            sourceArray.Add("three");
            sourceArray.Add("three");
            sourceArray.Add("four");
            sourceArray.Add("five");
            sourceArray.Add("five");
            ArrayList expectedArray = new ArrayList();
            expectedArray.Add("one");
            expectedArray.Add("two");
            expectedArray.Add("three");
            expectedArray.Add("four");
            expectedArray.Add("five");
            ArrayList resultArray = sourceArray.Unique();
            Assert.AreEqual(expectedArray.Count, resultArray.Count, "Checking that the resulting array is the correct size");
            for (int i = 0; i < resultArray.Count; i++)
            {
                Assert.AreEqual(expectedArray[i].GetHashCode(), resultArray[i].GetHashCode(), "Checking the element value hash codes are correct");
            }
        }
        [Test]
        public void BELArrayUniqueEmptyTest()
        {
            BELArray sourceArray = new BELArray();
            ArrayList resultArray = sourceArray.Unique();
            Assert.AreEqual(0, resultArray.Count, "Checking that the result array is empty");
        }
        [Test]
        public void BELArrayUniqueAllUniqueTest()
        {
            BELArray sourceArray = new BELArray();
            sourceArray.Add("one");
            sourceArray.Add("two");
            sourceArray.Add("three");
            sourceArray.Add("four");
            sourceArray.Add("five");
            ArrayList resultArray = sourceArray.Unique();
            Assert.AreEqual(sourceArray.Count, resultArray.Count, "Checking that the resulting array is the correct size");
            for (int i = 0; i < resultArray.Count; i++)
            {
                Assert.AreEqual(sourceArray.Array[i].GetHashCode(), resultArray[i].GetHashCode(), "Checking the element value hash codes are correct");
            }
        }
        [Test]
        public void BELArrayUniqueAllSameTest()
        {
            BELArray sourceArray = new BELArray();
            sourceArray.Add("one");
            sourceArray.Add("one");
            sourceArray.Add("one");
            sourceArray.Add("one");
            sourceArray.Add("one");
            ArrayList expectedArray = new ArrayList();
            expectedArray.Add("one");
            ArrayList resultArray = sourceArray.Unique();
            Assert.AreEqual(expectedArray.Count, resultArray.Count, "Checking that the resulting array has a single element");
            Assert.AreEqual(expectedArray[0].GetHashCode(), resultArray[0].GetHashCode(), "Checking that the result element has the correct hash code");
        }
        [Test]
        public void BELArrayUniqueMixedTypesTest()
        {
            BELArray sourceArray = new BELArray();
            sourceArray.Add(1);
            sourceArray.Add("one");
            sourceArray.Add("one");
            sourceArray.Add(1);
            sourceArray.Add(2);
            sourceArray.Add("two");
            ArrayList expectedArray = new ArrayList();
            expectedArray.Add(new BELInteger(1));
            expectedArray.Add(new BELString("one"));
            expectedArray.Add(new BELInteger(2));
            expectedArray.Add(new BELString("two"));
            ArrayList resultArray = sourceArray.Unique();
            Assert.AreEqual(expectedArray.Count, resultArray.Count, "Checking that the resulting array is the correct size");
            for (int i = 0; i < resultArray.Count; i++)
            {
                Assert.AreEqual(expectedArray[i].GetHashCode(), resultArray[i].GetHashCode(), "Checking the element value hash codes are correct");
            }
        }
        [Test]
        public void BELArrayUniqueArrayOfSingleElementArraysTest()
        {
            // This test reflects a common scenario under WikiTalk where an arrays members are
            // single element arrays in themselves.
            BELArray oneElement = new BELArray();
            oneElement.Add("test@test.com");
            BELArray anotherElement = new BELArray();
            anotherElement.Add("test2@test.com");
            BELArray sourceArray = new BELArray();
            sourceArray.Add(oneElement);
            sourceArray.Add(oneElement);
            sourceArray.Add(anotherElement);
            sourceArray.Add(anotherElement);
            ArrayList expectedArray = new ArrayList();
            expectedArray.Add(oneElement);
            expectedArray.Add(anotherElement);
            ArrayList resultArray = sourceArray.Unique();
            Assert.AreEqual(expectedArray.Count, resultArray.Count, "Checking that the resulting array is the correct size");
            for (int i = 0; i < resultArray.Count; i++)
            {
                Assert.AreEqual(((BELArray)expectedArray[i]).Array[0].GetHashCode(), ((BELArray)resultArray[i]).Array[0].GetHashCode(),
                    "Checking the element value hash codes are correct");
            }
        }

    }
}
