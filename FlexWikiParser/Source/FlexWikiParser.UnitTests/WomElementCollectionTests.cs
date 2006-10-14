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

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class WomElementCollectionTests
    {
        WomElement test1;
        WomElement child11;
        WomElement child12;
        WomElement child13;
        WomElement test2;
        WomElement child21;
        WomElement child22;
        WomElement child23;

        [SetUp]
        public void SetUp()
        {
            test1 = new WomElement("Test1");
            test1.ElementList.Add(child11 = new WomElement("Child11"));
            test1.ElementList.Add(child12 = new WomElement("Child12"));
            test1.ElementList.Add(child13 = new WomElement("Child13"));

            test2 = new WomElement("Test2");
            test2.ElementList.Add(child21 = new WomElement("Child21"));
            test2.ElementList.Add(child22 = new WomElement("Child22"));
            test2.ElementList.Add(child23 = new WomElement("Child23"));
        }

        [Test]
        public void TestOwner()
        {
            Assert.AreSame(test1, test1.ElementList.Owner, "test1 != test1.ElementList1.Owner ");
        }

        #region IList<WomElement> members tests

        [Test]
        public void TestIndexOf()
        {
            Assert.AreEqual(0, test1.ElementList.IndexOf(child11), "0 != test1.ElementList.IndexOf(child11) ");
            Assert.AreEqual(1, test1.ElementList.IndexOf(child12), "1 != test1.ElementList.IndexOf(child12) ");
            Assert.AreEqual(2, test1.ElementList.IndexOf(child13), "2 != test1.ElementList.IndexOf(child13) ");
        }

        [Test]
        public void TestInsert()
        {
            WomElement newChilld = new WomElement("newChild");
            test1.ElementList.Insert(1, newChilld);
            Assert.AreEqual(4, test1.ElementList.Count, "4 != test1.ElementList.Count ");
            Assert.AreSame(newChilld, test1.ElementList[1], "newChilld != test1.ElementList[1] ");
            Assert.AreSame(test1, newChilld.Parent, "test1 != newChilld.Parent ");
            Assert.AreSame(child11, test1.ElementList[0], "child11 != test1.ElementList[0] ");
            Assert.AreSame(child12, test1.ElementList[2], "child12 != test1.ElementList[2] ");
            Assert.AreSame(child13, test1.ElementList[3], "child13 != test1.ElementList[3] ");
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInsertedParented()
        {
            test1.ElementList.Insert(1, child21);
        }

        [Test]
        public void TestRemoveAt()
        {
            WomElement oldElement = test1.ElementList[1];
            test1.ElementList.RemoveAt(1);
            Assert.AreEqual(2, test1.ElementList.Count, "2 != test1.ElementList.Count ");
            Assert.IsNull(oldElement.Parent, "oldElement.Parent != null ");
            Assert.AreSame(child11, test1.ElementList[0], "child11 != test1.ElementList[0] ");
            Assert.AreSame(child13, test1.ElementList[1], "child13 != test1.ElementList[1] ");
        }

        [Test]
        public void TestIndexerSet()
        {
            WomElement newChilld = new WomElement("newChild");
            test1.ElementList[1] = newChilld;
            Assert.AreEqual(3, test1.ElementList.Count, "3 != test1.ElementList.Count ");
            Assert.AreSame(newChilld, test1.ElementList[1], "newChilld != test1.ElementList[1] ");
            Assert.AreSame(test1, newChilld.Parent, "test1 != newChilld.Parent ");
            Assert.IsNull(child12.Parent, "child12.Parent != null ");
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestIndexerSetParented()
        {
            test1.ElementList[1] = child21;
        }

        #endregion

        #region ICollection<WomElement> members tests

        [Test]
        public void TestAdd()
        {
            Assert.AreEqual(3, test1.ElementList.Count, "3 != test1.ElementList.Count ");
            Assert.AreSame(child11, test1.ElementList[0], "child11 != test1.ElementList[0] ");
            Assert.AreSame(child12, test1.ElementList[1], "child12 != test1.ElementList[1] ");
            Assert.AreSame(child13, test1.ElementList[2], "child13 != test1.ElementList[1] ");
            Assert.AreSame(test1, child11.Parent, "test1 != child11.Parent ");
            Assert.AreSame(test1, child12.Parent, "test1 != child12.Parent ");
            Assert.AreSame(test1, child13.Parent, "test1 != child13.Parent ");
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestAddParented()
        {
            test2.ElementList.Add(child11);
        }

        [Test]
        public void TestClear()
        {
            test1.ElementList.Clear();
            Assert.AreEqual(0, test1.ElementList.Count, "0 != test1.ElementList.Count ");
            Assert.IsNull(child11.Parent, "child11.Parent != null ");
            Assert.IsNull(child12.Parent, "child12.Parent != null ");
            Assert.IsNull(child13.Parent, "child13.Parent != null ");
        }

        [Test]
        public void TestContains()
        {
            Assert.IsTrue(test1.ElementList.Contains(child11), "test1.ElementList.Contains(child11) != true ");
            Assert.IsFalse(test1.ElementList.Contains(child21), "test1.ElementList.Contains(child21) != false ");
        }

        [Test]
        public void TestCopyTo()
        {
            WomElement[] array = new WomElement[4];
            test1.ElementList.CopyTo(array, 1);
            Assert.IsNull(array[0], "array[0] != null ");
            Assert.AreSame(child11, array[1], "child11 != array[1] ");
            Assert.AreSame(child12, array[2], "child12 != array[2] ");
            Assert.AreSame(child13, array[3], "child13 != array[3] ");
        }

        [Test]
        public void TestIsReadonly()
        {
            ICollection<WomElement> collection = test1.ElementList as ICollection<WomElement>;
            Assert.IsFalse(collection.IsReadOnly, "collection.IsReadOnly != false ");
        }

        [Test]
        public void TestRemove()
        {
            bool removeResult = test1.ElementList.Remove(child12);
            Assert.IsTrue(removeResult, "removeResult != true ");
            Assert.IsNull(child12.Parent, "child12.Parent != null ");
            Assert.AreEqual(2, test1.ElementList.Count, "2 != test1.ElementList.Count ");
            Assert.AreSame(child11, test1.ElementList[0], "child11 != test1.ElementList[0] ");
            Assert.AreSame(child13, test1.ElementList[1], "child13 != test1.ElementList[1] ");
        }

        [Test]
        public void TestRemoveNotOwned()
        {
            bool removeResult = test1.ElementList.Remove(child21);
            Assert.IsFalse(removeResult, "removeResult != false ");
            Assert.AreSame(test2, child21.Parent, "test2 != child21.Parent ");
            Assert.AreEqual(3, test1.ElementList.Count, "3 != test1.ElementList.Count ");
        }

        #endregion

        #region IEnumerator<WomElement> members tests

        [Test]
        public void TestEnumerator()
        {
            int count = 0;
            foreach (WomElement child in test1.ElementList)
            {
                count++;
            }
            Assert.AreEqual(3, count, "3 != count ");
        }

        #endregion
    }
}
