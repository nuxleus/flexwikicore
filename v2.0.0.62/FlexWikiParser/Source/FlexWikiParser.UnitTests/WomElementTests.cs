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
using System.IO;
using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class WomElementTests
    {
        WomElement element;
        WomElement child1;
        WomElement child2;
        WomElement child3;
        WomElement emptyElement;

        [SetUp]
        public void SetUp()
        {
            element = new WomElement("Element");
            element.ElementList.Add(child1 = new WomElement("Child1"));
            element.ElementList.Add(child2 = new WomElement("Child2"));
            element.ElementList.Add(child3 = new WomElement("Child3"));
            emptyElement = new WomElement("Element");
        }

        [Test]
        public void TestIndex()
        {
            Assert.AreEqual(0, child1.Index, "0 != child1.Index ");
            Assert.AreEqual(1, child2.Index, "1 != child2.Index ");
            Assert.AreEqual(2, child3.Index, "2 != child3.Index ");

            WomElement child = new WomElement("Child");
            Assert.AreEqual(-1, child.Index, "-1 != child.Index ");
        }

        [Test]
        public void TestFirstChild()
        {
            Assert.AreSame(child1, element.FirstChild, "child1 != element.FirstChild ");
            Assert.IsNull(emptyElement.FirstChild, "emptyElement.FirstChild != null ");
        }

        [Test]
        public void TestLastChild()
        {
            Assert.AreSame(child3, element.LastChild, "child3 != element.LastChild ");
            Assert.IsNull(emptyElement.LastChild, "emptyElement.LastChild != null ");
        }

        [Test]
        public void TestHasElements()
        {
            Assert.IsTrue(element.HasElements, "element.HasElements != true ");
            Assert.IsFalse(emptyElement.HasElements, "emptyElement.HasElements != false ");
        }

        [Test]
        public void TestClone()
        {
            element.Properties["Attr"] = "value";
            child1.Properties["Attr1"] = "value1";
            child2.Properties["Attr2"] = "value2";
            child3.Properties["Attr3"] = "value3";

            WomElement clonedElement = new WomElement(element);
            Assert.IsNotNull(clonedElement, "clonedElement == null ");
            Assert.AreNotSame(element, clonedElement, "element == clonedElement ");
            Assert.AreEqual(3, clonedElement.ElementList.Count, "3 != clonedElement.ElementList.Count ");
            Assert.AreNotSame(child1, clonedElement.ElementList[0], "child1 == clonedElement.ElementList[0] ");
            Assert.AreNotSame(child2, clonedElement.ElementList[1], "child2 == clonedElement.ElementList[1] ");
            Assert.AreNotSame(child3, clonedElement.ElementList[2], "child3 == clonedElement.ElementList[2] ");
            Assert.AreEqual("value", clonedElement.Properties["Attr"],
                "\"value\" != clonedElement.Attributes[\"Attr\"] ");
            Assert.AreEqual("value1", clonedElement.ElementList[0].Properties["Attr1"],
                "\"value1\" != clonedElement.ElementList[0].Attributes[\"Attr1\"] ");
            Assert.AreEqual("value2", clonedElement.ElementList[1].Properties["Attr2"],
                "\"value2\" != clonedElement.ElementList[1].Attributes[\"Attr2\"] ");
            Assert.AreEqual("value3", clonedElement.ElementList[2].Properties["Attr3"],
                "\"value3\" != clonedElement.ElementList[2].Attributes[\"Attr3\"] ");

            clonedElement.Properties["Attr"] = "newValue";
            clonedElement.ElementList[0].Properties["Attr1"] = "newValue";

            Assert.AreEqual("value", element.Properties["Attr"], "\"value\" != element.Attributes[\"Attr\"] ");
            Assert.AreEqual("value1", child1.Properties["Attr1"], "\"value1\" != child1.Attributes[\"Attr1\"] ");
        }

        [Test]
        public void TestAdd()
        {
            element.Add("text",
                new WomProperty("Attr1", "value1"),
                new WomProperty("Attr2", "value2"),
                new WomElement("Child5"));
            Assert.AreEqual(5, element.ElementList.Count, "5 != element.ElementList.Count ");
            Assert.AreEqual(2, element.Properties.Count, "2 != element.Attributes.Count ");
            Assert.AreEqual("value1", element.Properties["Attr1"], "\"value1\" != element.Attributes[\"Attr1\"] ");
            Assert.AreEqual("value2", element.Properties["Attr2"], "\"value2\" != element.Attributes[\"Attr2\"] ");
            Assert.AreEqual("_text", element.ElementList[3].Name, "\"_text\" != element.ElementList[3].Name ");
            Assert.AreEqual("Child5", element.ElementList[4].Name, "\"Child5\" != element.ElementList[4].Name ");
        }

        [Test]
        public void TestIsTextElement()
        {
            element.Add("text");
            Assert.IsTrue(element.ElementList[3].IsTextElement, "element.ElementList[3].IsTextElement != true ");
        }

        [Test]
        public void TestWriteXmlElement()
        {
            StringWriter textWriter = new StringWriter();
            XmlWriter xmlWriter = new XmlTextWriter(textWriter);
            emptyElement.WriteTo(xmlWriter);
            Assert.AreEqual("<Element />", textWriter.ToString());
        }

        [Test]
        public void TestWriteXmlElementAttributes()
        {
            StringWriter textWriter = new StringWriter();
            XmlWriter xmlWriter = new XmlTextWriter(textWriter);
            emptyElement.Properties["Attr1"] = "1";
            emptyElement.Properties["Attr2"] = "test";
            emptyElement.WriteTo(xmlWriter);
            Assert.AreEqual("<Element Attr1=\"1\" Attr2=\"test\" />", textWriter.ToString());
        }

        [Test]
        public void TestWriteXmlElementContent()
        {
            StringWriter textWriter = new StringWriter();
            XmlWriter xmlWriter = new XmlTextWriter(textWriter);
            child1.Properties["Attr"] = "Test";
            element.WriteTo(xmlWriter);
            Assert.AreEqual("<Element><Child1 Attr=\"Test\" /><Child2 /><Child3 /></Element>",
                textWriter.ToString());
        }

        [Test]
        public void TestWriteXmlElementText()
        {
            StringWriter textWriter = new StringWriter();
            XmlWriter xmlWriter = new XmlTextWriter(textWriter);
            child1.Add("Text");
            element.WriteTo(xmlWriter);
            Assert.AreEqual("<Element><Child1>Text</Child1><Child2 /><Child3 /></Element>",
                textWriter.ToString());
        }

        [Test]
        public void TestReadXmlElement()
        {
            StringReader textReader = new StringReader("<Element />");
            XmlReader xmlReader = new XmlTextReader(textReader);
            WomElement readElement = WomElement.Load(xmlReader);
            Assert.AreEqual(emptyElement, readElement, "emptyElement != readElement");
        }

        [Test]
        public void TestReadXmlElementAttributes()
        {
            emptyElement.Properties["Attr1"] = "1";
            emptyElement.Properties["Attr2"] = "test";

            StringReader textReader = new StringReader("<Element Attr1=\"1\" Attr2=\"test\" />");
            XmlReader xmlReader = new XmlTextReader(textReader);
            WomElement readElement = WomElement.Load(xmlReader);
            Assert.AreEqual(emptyElement, readElement, "emptyElement != readElement");
        }

        [Test]
        public void TestReadXmlElementContent()
        {
            child1.Properties["Attr"] = "Test";
            StringReader textReader = new StringReader("<Element><Child1 Attr=\"Test\" /><Child2 /><Child3 /></Element>");
            XmlReader xmlReader = new XmlTextReader(textReader);
            WomElement readElement = WomElement.Load(xmlReader);
            Assert.AreEqual(element, readElement, "element != readElement");
        }

        [Test]
        public void TestReadXmlElementText()
        {
            child1.Add("Text");
            StringReader textReader = new StringReader("<Element><Child1>Text</Child1><Child2 /><Child3 /></Element>");
            XmlReader xmlReader = new XmlTextReader(textReader);
            WomElement readElement = WomElement.Load(xmlReader);
            Assert.AreEqual(element, readElement, "element != readElement");
        }
    }
}
