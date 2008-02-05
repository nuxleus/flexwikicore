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
using System.Xml;
using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class ParserExpressionReaderTests
    {
        private ParserExpressionReader exprReader;
        private WomElement context;

        [SetUp]
        public void SetUp()
        {
            exprReader = new ParserExpressionReader();
            context = new WomElement("Context");
        }

        [Test]
        public void TestMatchText()
        {
            bool result = exprReader.Parse("'a'", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><Match Text=\"a\" /></Context>",
                context.Xml);
        }

        [Test]
        public void TestMatchText2()
        {
            bool result = exprReader.Parse("'ab'", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><Match Text=\"ab\" /></Context>",
                context.Xml);
        }

        [Test]
        public void TestMatchRange()
        {
            bool result = exprReader.Parse("'a'..'z'", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><MatchRange Start=\"a\" End=\"z\" /></Context>",
                context.Xml);
        }

        [Test]
        public void TestMatchSet()
        {
            bool result = exprReader.Parse("{Letter}", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><MatchSet Name=\"Letter\" /></Context>",
                context.Xml);
        }

        [Test]
        public void TestSequence()
        {
            bool result = exprReader.Parse("'a''b'", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><Match Text=\"a\" /><Match Text=\"b\" /></Context>",
                context.Xml);
        }

        [Test]
        public void TestSequenceWithSpaces()
        {
            bool result = exprReader.Parse(" 'a' 'b' ", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><Match Text=\"a\" /><Match Text=\"b\" /></Context>",
                context.Xml);
        }

        [Test]
        public void TestChoice()
        {
            bool result = exprReader.Parse("'a'|'b'", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><Choice><Match Text=\"a\" /><Match Text=\"b\" /></Choice></Context>",
                context.Xml);
        }

        [Test]
        public void TestOptional()
        {
            bool result = exprReader.Parse("'a''b'?", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><Match Text=\"a\" /><Optional><Match Text=\"b\" /></Optional></Context>",
                context.Xml);
        }

        [Test]
        public void TestZeroOrMore()
        {
            bool result = exprReader.Parse("'a''b'*", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><Match Text=\"a\" /><ZeroOrMore><Match Text=\"b\" /></ZeroOrMore></Context>",
                context.Xml);
        }

        [Test]
        public void TestOneOrMore()
        {
            bool result = exprReader.Parse("'a''b'+", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><Match Text=\"a\" /><OneOrMore><Match Text=\"b\" /></OneOrMore></Context>",
                context.Xml);
        }

        [Test]
        public void TestCallRule()
        {
            bool result = exprReader.Parse("'a' Test 'b'", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><Match Text=\"a\" /><CallRule Name=\"Test\" /><Match Text=\"b\" /></Context>",
                context.Xml);
        }

        [Test]
        public void TestCharSetDefinition()
        {
            bool result = exprReader.Parse("['a', 'b', 'c']", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><CharSetDef>"
                + "<Match Text=\"a\" />"
                + "<Match Text=\"b\" />"
                + "<Match Text=\"c\" />"
                + "</CharSetDef></Context>",
                context.Xml);
        }

        [Test]
        public void TestCharSetDefinitionRange()
        {
            bool result = exprReader.Parse("['a'..'z', '_', '0'..'9']", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><CharSetDef>"
                + "<MatchRange Start=\"a\" End=\"z\" />"
                + "<Match Text=\"_\" />"
                + "<MatchRange Start=\"0\" End=\"9\" />"
                + "</CharSetDef></Context>",
                context.Xml);
        }

        [Test]
        public void TestCharSetDefinitionAny()
        {
            bool result = exprReader.Parse("[_]", context);
            Assert.IsTrue(result, "Parse failed");
            Assert.AreEqual("<Context><CharSetDef>"
                + "<MatchAny />"
                + "</CharSetDef></Context>",
                context.Xml);
        }
    }
}
