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
using System.IO;
using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class WomDocumentTests
    {
        [Test]
        public void TestConstructor1()
        {
            WomDocument doc = new WomDocument();
            doc.Add(new WomElement("Test"));
            Assert.AreEqual("<Test />", doc.Xml);
        }

        [Test]
        public void TestConstructor2()
        {
            WomDocument doc = new WomDocument(new WomElement("Test",
                new WomElement("Child1"),
                new WomElement("Child2")));
            Assert.AreEqual("<Test><Child1 /><Child2 /></Test>", doc.Xml);
        }

        [Test]
        public void TestConstructor3()
        {
            WomDocument doc1 = new WomDocument(new WomElement("Test",
                new WomElement("Child1"),
                new WomElement("Child2")));
            WomDocument doc2 = new WomDocument(doc1);
            Assert.AreEqual("<Test><Child1 /><Child2 /></Test>", doc2.Xml);
            Assert.AreNotSame(doc1.Root, doc2.Root, "doc1.Root == doc2.Root");
            Assert.AreEqual(doc1.Root.ElementList.Count, doc2.Root.ElementList.Count,
                "doc1.Root.ElementList.Count != doc2.Root.ElementList.Count");
        }

        [Test]
        public void TestParse()
        {
            WomDocument doc = WomDocument.Parse("<Test Attr1='a1' Attr2='a2'><Element1/><Element2 Attr='12'/></Test>");
            Assert.AreEqual("<Test Attr1=\"a1\" Attr2=\"a2\"><Element1 /><Element2 Attr=\"12\" /></Test>",
                doc.Xml);
        }

        [Test]
        public void TestLoad()
        {
            StringReader textReader = new StringReader("<Test Attr1='a1' Attr2='a2'><Element1/><Element2 Attr='12'/></Test>");
            WomDocument doc = WomDocument.Load(textReader);
            Assert.AreEqual("<Test Attr1=\"a1\" Attr2=\"a2\"><Element1 /><Element2 Attr=\"12\" /></Test>",
                doc.Xml);
        }

        [Test]
        public void TestSave()
        {
            StringReader textReader = new StringReader("<Test Attr1='a1' Attr2='a2'><Element1/><Element2 Attr='12'/></Test>");
            WomDocument doc = WomDocument.Load(textReader);
            StringWriter textWriter = new StringWriter();
            doc.Save(textWriter);
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?>"
                + "<Test Attr1=\"a1\" Attr2=\"a2\"><Element1 /><Element2 Attr=\"12\" /></Test>",
                textWriter.ToString());
        }
    }
}
