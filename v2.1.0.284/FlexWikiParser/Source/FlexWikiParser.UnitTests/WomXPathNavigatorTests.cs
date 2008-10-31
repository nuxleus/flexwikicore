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
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class WomXPathNavigatorTests
    {
        private WomDocument doc;
        private WomElement node11;
        private WomElement node21;
        private WomElement node22;
        private WomElement node23;
        private WomElement node31;

        [SetUp]
        public void SetUp()
        {
            doc = new WomDocument(
                node11 = new WomElement("Node11", new WomProperty("Prop1", "value1"), new WomProperty("Prop2", "value2"),
                    node21 = new WomElement("Node21", new WomProperty("Prop1", "value1"), new WomProperty("Prop2", "value2")),
                    node22 = new WomElement("Node22", new WomProperty("Prop1", "value1"), new WomProperty("Prop2", "value2"),
                        node31 = new WomElement("Node31"), new WomElement("Node32")),
                    node23 = new WomElement("Node23", new WomProperty("Prop1", "value1"), new WomProperty("Prop2", "value2"))));
        }

        [Test]
        public void TestMoveToParent()
        {
            WomXPathNavigator navigator = new WomXPathNavigator(node31);
            Assert.IsTrue(navigator.MoveToParent(), "navigator.MoveToParent() != true");
            Assert.IsTrue(new WomXPathNavigator(node22).IsSamePosition(navigator),
                "new WomXPathNavigator(node22).IsSamePosition(navigator) != true");
            Assert.AreEqual(XPathNodeType.Element, navigator.NodeType, "XPathNodeType.Element != navigator.NodeType");

            Assert.IsTrue(navigator.MoveToParent(), "navigator.MoveToParent() != true");
            Assert.IsTrue(new WomXPathNavigator(node11).IsSamePosition(navigator),
                "new WomXPathNavigator(node11).IsSamePosition(navigator) != true");
            Assert.AreEqual(XPathNodeType.Element, navigator.NodeType, "XPathNodeType.Element != navigator.NodeType");

            Assert.IsTrue(navigator.MoveToParent(), "navigator.MoveToParent() != true");
            Assert.IsTrue(new WomXPathNavigator(doc).IsSamePosition(navigator),
                "new WomXPathNavigator(doc).IsSamePosition(navigator) != true");
            Assert.AreEqual(XPathNodeType.Root, navigator.NodeType, "XPathNodeType.Root != navigator.NodeType");

            Assert.IsFalse(navigator.MoveToParent(), "navigator.MoveToParent() != false");
            Assert.IsTrue(new WomXPathNavigator(doc).IsSamePosition(navigator),
                "new WomXPathNavigator(doc).IsSamePosition(navigator) != true");
            Assert.AreEqual(XPathNodeType.Root, navigator.NodeType, "XPathNodeType.Root != navigator.NodeType");
        }

        [Test]
        public void TestMoveToNext()
        {
            WomXPathNavigator navigator = new WomXPathNavigator(node22);
            Assert.IsTrue(navigator.MoveToNext(), "navigator.MoveToNext() != true");
            Assert.IsTrue(new WomXPathNavigator(node23).IsSamePosition(navigator),
                "new WomXPathNavigator(node23).IsSamePosition(navigator) != true");
            Assert.AreEqual(XPathNodeType.Element, navigator.NodeType, "XPathNodeType.Element != navigator.NodeType");

            Assert.IsFalse(navigator.MoveToNext(), "navigator.MoveToNext() != false");
            Assert.IsTrue(new WomXPathNavigator(node23).IsSamePosition(navigator),
                "new WomXPathNavigator(node23).IsSamePosition(navigator) != true");
            Assert.AreEqual(XPathNodeType.Element, navigator.NodeType, "XPathNodeType.Element != navigator.NodeType");
        }

        [Test]
        public void TestMoveToPrevious()
        {
            WomXPathNavigator navigator = new WomXPathNavigator(node22);
            Assert.IsTrue(navigator.MoveToPrevious(), "navigator.MoveToPrevious() != true");
            Assert.IsTrue(new WomXPathNavigator(node21).IsSamePosition(navigator),
                "new WomXPathNavigator(node21).IsSamePosition(navigator) != true");
            Assert.AreEqual(XPathNodeType.Element, navigator.NodeType, "XPathNodeType.Element != navigator.NodeType");

            Assert.IsFalse(navigator.MoveToPrevious(), "navigator.MoveToPrevious() != false");
            Assert.IsTrue(new WomXPathNavigator(node21).IsSamePosition(navigator),
                "new WomXPathNavigator(node21).IsSamePosition(navigator) != true");
            Assert.AreEqual(XPathNodeType.Element, navigator.NodeType, "XPathNodeType.Element != navigator.NodeType");
        }

        [Test]
        public void TestMoveToFirstChild()
        {
            WomXPathNavigator navigator = new WomXPathNavigator(node11);
            Assert.IsTrue(navigator.MoveToFirstChild(), "navigator.MoveToFirstChild() != true");
            Assert.IsTrue(new WomXPathNavigator(node21).IsSamePosition(navigator),
                "new WomXPathNavigator(node21).IsSamePosition(navigator) != true");
            Assert.AreEqual(XPathNodeType.Element, navigator.NodeType, "XPathNodeType.Element != navigator.NodeType");

            Assert.IsFalse(navigator.MoveToFirstChild(), "navigator.MoveToFirstChild() != false");
            Assert.IsTrue(new WomXPathNavigator(node21).IsSamePosition(navigator),
                "new WomXPathNavigator(node21).IsSamePosition(navigator) != true");
            Assert.AreEqual(XPathNodeType.Element, navigator.NodeType, "XPathNodeType.Element != navigator.NodeType");
        }

        [Test]
        public void TestMoveToFirstAttribute()
        {
            WomXPathNavigator navigator = new WomXPathNavigator(node11);
            Assert.IsTrue(navigator.MoveToFirstAttribute(), "navigator.MoveToFirstAttribute() != true");
            Assert.AreEqual(XPathNodeType.Attribute, navigator.NodeType, "XPathNodeType.Attribute != navigator.NodeType");
            Assert.AreEqual("Prop1", navigator.Name, "\"Prop1\" != navigator.Name");
            Assert.AreEqual("value1", navigator.Value, "\"value1\" != navigator.Value");
        }

        [Test]
        public void TestMoveToNextAttribute()
        {
            WomXPathNavigator navigator = new WomXPathNavigator(node11);
            Assert.IsTrue(navigator.MoveToFirstAttribute(), "navigator.MoveToFirstAttribute() != true");
            Assert.AreEqual(XPathNodeType.Attribute, navigator.NodeType, "XPathNodeType.Attribute != navigator.NodeType");
            Assert.AreEqual("Prop1", navigator.Name, "\"Prop1\" != navigator.Name");
            Assert.AreEqual("value1", navigator.Value, "\"value1\" != navigator.Value");

            Assert.IsTrue(navigator.MoveToNextAttribute(), "navigator.MoveToNextAttribute() != true");
            Assert.AreEqual(XPathNodeType.Attribute, navigator.NodeType, "XPathNodeType.Attribute != navigator.NodeType");
            Assert.AreEqual("Prop2", navigator.Name, "\"Prop2\" != navigator.Name");
            Assert.AreEqual("value2", navigator.Value, "\"value2\" != navigator.Value");

            Assert.IsFalse(navigator.MoveToNextAttribute(), "navigator.MoveToNextAttribute() != false");
            Assert.AreEqual(XPathNodeType.Attribute, navigator.NodeType, "XPathNodeType.Attribute != navigator.NodeType");
            Assert.AreEqual("Prop2", navigator.Name, "\"Prop2\" != navigator.Name");
            Assert.AreEqual("value2", navigator.Value, "\"value2\" != navigator.Value");
        }
    }
}
