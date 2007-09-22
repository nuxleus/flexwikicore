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
    public class BelArraySortTests
    {
        [Test]
        public void Sort()
        {
            ArrayList arrayList = new ArrayList();
            arrayList.Add(new BELInteger(1));
            arrayList.Add(new BELInteger(2));
            arrayList.Add(new BELInteger(3)); 
            arrayList.Add(new BELInteger(0)); 

            BELArray unsorted = new BELArray(arrayList);

            BELArray sorted = unsorted.Sort();

            Assert.AreEqual(4, sorted.Count, "Checking that the length was preserved.");

            for (int i = 0; i < 4; ++i)
            {
                Assert.AreEqual(i, ((BELInteger)sorted.Item(i)).Value, "Checking that sort order was correct.");
            }
        }


        [Test]
        public void SortBy()
        {
            ArrayList arrayList = new ArrayList();
            arrayList.Add(new BELInteger(1));
            arrayList.Add(new BELInteger(2));
            arrayList.Add(new BELInteger(3));
            arrayList.Add(new BELInteger(0));

            BELArray unsorted = new BELArray(arrayList); 

            // Create a simple arithmetic mapping for the sort
            BehaviorParser parser = new BehaviorParser("");
            ExposableParseTreeNode block = parser.Parse("4.Subtract(e)");

            ArrayList parameters = new ArrayList();
            BlockParameter parameter = new BlockParameter(null, "e"); 
            parameters.Add(parameter); 

            BELArray sorted = unsorted.SortBy(new ExecutionContext(), new Block(block, parameters, null));

            Assert.AreEqual(4, sorted.Count, "Checking that the length was preserved.");

            for (int i = 0; i < 4; ++i)
            {
                Assert.AreEqual(3 - i, ((BELInteger)sorted.Item(i)).Value, "Checking that sort order was correct.");
            }
        }

        [Test]
        public void SortByEmpty()
        {
            BELArray unsorted = new BELArray(); 

                        // Create a simple arithmetic mapping for the sort
            BehaviorParser parser = new BehaviorParser("");
            ExposableParseTreeNode block = parser.Parse("4.Subtract(e)");

            ArrayList parameters = new ArrayList();
            BlockParameter parameter = new BlockParameter(null, "e"); 
            parameters.Add(parameter); 

            BELArray sorted = unsorted.SortBy(new ExecutionContext(), new Block(block, parameters, null));

            Assert.IsNotNull(sorted, "Checking that result was returned for an empty array."); 
            Assert.AreEqual(0, sorted.Count, "Checking that the result array was empty.");
        }

    }
}
