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
    public class BelArrayTests
    {
        [Test]
        public void ArrayReverse()
        {
            ArrayList arrayList = new ArrayList();
            arrayList.Add(new BELInteger(0));
            arrayList.Add(new BELInteger(1));
            arrayList.Add(new BELInteger(2));
            arrayList.Add(new BELInteger(3));

            BELArray original = new BELArray(arrayList);

            BELArray reversed = original.Reverse();

            Assert.AreEqual(4, reversed.Count, "Checking that the length was preserved.");

            for (int i = 0; i < 4; ++i)
            {
                Assert.AreEqual(3 - i, ((BELInteger)reversed.Item(i)).Value, "Checking that order was reversed.");
            }

        }
        [Test]
        public void ArraySnip()
        {
            ArrayList arrayList = new ArrayList();
            arrayList.Add(new BELInteger(0));
            arrayList.Add(new BELInteger(1));
            arrayList.Add(new BELInteger(2));
            arrayList.Add(new BELInteger(3));

            BELArray original = new BELArray(arrayList);

            BELArray snipped = original.Snip(2);

            Assert.AreEqual(4, original.Count, "Checking that the length was preserved.");
            Assert.AreEqual(2, snipped.Count, "Checking that the snipped length was correct.");
            Assert.AreEqual(0, ((BELInteger) snipped.Item(0)).Value, "Checking that the contents of the snipped array are correct.");
            Assert.AreEqual(1, ((BELInteger) snipped.Item(1)).Value, "Checking that the contents of the snipped array are correct.");
        }
        [Test]
        public void ArrayToString()
        {
            ArrayList arrayList = new ArrayList();
            arrayList.Add(new BELInteger(0));
            arrayList.Add(new BELInteger(1));
            arrayList.Add(new BELInteger(2));
            arrayList.Add(new BELInteger(3));

            BELArray original = new BELArray(arrayList);

            string result = original.ToOneString;

            Assert.AreEqual(4, original.Count, "Checking that the length was preserved.");
            Assert.AreEqual("0123", result, "Checking that the string outputs are correct.");
        }
        [Test]
        public void ArrayItems()
        {
            ArrayList arrayList = new ArrayList();
            arrayList.Add(new BELInteger(0));
            arrayList.Add(new BELString("array"));
            arrayList.Add(BELBoolean.True);
            arrayList.Add(new BELInteger(3));

            BELArray original = new BELArray(arrayList);

            //int item0 = 0;
            string item1 = "array";
            bool item2 = true;
            int item3 = 3;

            Assert.AreEqual(4, original.Count, "Checking that the length was preserved.");

            Assert.AreEqual(0, ((BELInteger) original.Item(0)).Value, "Checking that the Item outputs are correct.");
            Assert.AreEqual(item1, ((BELString) original.Item(1)).Value, "Checking that the Item outputs are correct.");
            Assert.AreEqual(item2, ((BELBoolean) original.Item(2)).Value, "Checking that the Item outputs are correct.");
            Assert.AreEqual(item3, ((BELInteger) original.Item(3)).Value, "Checking that the Item outputs are correct.");
        }
    }
}
