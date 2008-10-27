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
    public class WomElementPropertiesTests
    {
        WomElement element;

        [SetUp]
        public void SetUp()
        {
            element = new WomElement("Element");
            element.Properties["Attr1"] = "value1";
            element.Properties["Attr2"] = "value2";
        }

        [Test]
        public void TestSetByName()
        {
            Assert.AreEqual(2, element.Properties.Count, "2 != element.Attributes.Count ");
            Assert.AreEqual("value1", element.Properties["Attr1"], "\"value1\" != element.Attributes[\"Attr1\"] ");
            Assert.AreEqual("value2", element.Properties["Attr2"], "\"value2\" != element.Attributes[\"Attr2\"] ");
            Assert.AreEqual("value1", element.Properties[0], "\"value1\" != element.Attributes[0] ");
            Assert.AreEqual("value2", element.Properties[1], "\"value2\" != element.Attributes[1] ");
        }

        [Test]
        public void TestGetByNameNotExisted()
        {
            Assert.AreSame(String.Empty, element.Properties["Attr3"], "String.Empty != element.Attributes[\"Attr3\"] ");
        }

        [Test]
        public void TestSetByIndex()
        {
            element.Properties[0] = "newvalue1";
            element.Properties[1] = "newvalue2";

            Assert.AreEqual(2, element.Properties.Count, "2 != element.Attributes.Count ");
            Assert.AreEqual("newvalue1", element.Properties["Attr1"], "\"newvalue1\" != element.Attributes[\"Attr1\"] ");
            Assert.AreEqual("newvalue2", element.Properties["Attr2"], "\"newvalue2\" != element.Attributes[\"Attr2\"] ");
            Assert.AreEqual("newvalue1", element.Properties[0], "\"newvalue1\" != element.Attributes[0] ");
            Assert.AreEqual("newvalue2", element.Properties[1], "\"newvalue2\" != element.Attributes[1] ");
        }

        [Test]
        public void TestIndexOf()
        {
            Assert.AreEqual(0, element.Properties.IndexOf("Attr1"), "0 != element.Attributes.IndexOf(\"Attr1\") ");
        }

        [Test]
        public void TestHasAttributes()
        {
            Assert.IsTrue(element.HasProperties, "element.HasAttributes != true ");

            WomElement element2 = new WomElement("Test");
            Assert.IsFalse(element2.HasProperties, "element2.HasAttributes != false ");
        }

        [Test]
        public void TestGetAttributeName()
        {
            Assert.AreEqual("Attr1", element.Properties.GetName(0), "\"Attr1\" != element.GetAttributeName(0) ");
            Assert.AreEqual("Attr2", element.Properties.GetName(1), "\"Attr2\" != element.GetAttributeName(1) ");
        }

        [Test]
        public void TestEnumerator()
        {
            int count = 0;
            foreach (WomProperty attribute in element.Properties)
            {
                count++;
            }
            Assert.AreEqual(2, count, "2 != count ");
        }
    }
}
