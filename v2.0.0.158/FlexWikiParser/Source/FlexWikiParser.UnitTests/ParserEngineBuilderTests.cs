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

// WikiText --> WOM --> XHTML
//    WikiParserEngine 
// WikiParserEngine uses grammar definiton written in XML.
// XML grammar is read by WikiParserEngineBuilder
// WikiParserEngineBuilder creates WOM for Meta language and creates WikiParserEngine out of it.
// WikiParserEngineBuilder uses WikiXmlGrammarReader and ExpressionParser.
namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class ParserEngineBuilderTests
    {
        [Test]
        public void TestMatchCharA()
        {
            WomElement matchChar = new WomElement("Match", new WomProperty("Text", "a"));
            ParserEngineBuilder builder = new ParserEngineBuilder(matchChar);
            ParserEngine engine = builder.CreateEngine();
            Assert.IsTrue(engine.Parse("a"), "engine.Parse(\"a\") != true");
            Assert.IsFalse(engine.Parse("b"), "engine.Parse(\"b\") != false");
        }

        [Test]
        public void TestMatchCharB()
        {
            WomElement matchChar = new WomElement("Match", new WomProperty("Text", "b"));
            ParserEngineBuilder builder = new ParserEngineBuilder(matchChar);
            ParserEngine engine = builder.CreateEngine();
            Assert.IsFalse(engine.Parse("a"), "engine.Parse(\"a\") != false");
            Assert.IsTrue(engine.Parse("b"), "engine.Parse(\"b\") != true");
        }

        [Test]
        public void TestMatchString()
        {
            WomElement matchString = new WomElement("Match", new WomProperty("Text", "ab"));
            ParserEngineBuilder builder = new ParserEngineBuilder(matchString);
            ParserEngine engine = builder.CreateEngine();
            Assert.IsTrue(engine.Parse("ab"), "engine.Parse(\"ab\") != true");
            Assert.IsFalse(engine.Parse("a"), "engine.Parse(\"a\") != false");
            Assert.IsFalse(engine.Parse("b"), "engine.Parse(\"b\") != false");
            Assert.IsFalse(engine.Parse("bc"), "engine.Parse(\"bc\") != false");
        }

        [Test]
        public void TestMatchSet()
        {
            WomElement matchRange = new WomElement("MatchRange",
                new WomProperty("Start", "0"),
                new WomProperty("End", "9"));
            ParserEngineBuilder builder = new ParserEngineBuilder(matchRange);
            ParserEngine engine = builder.CreateEngine();
            Assert.IsTrue(engine.Parse("0"), "engine.Parse(\"0\") != true");
            Assert.IsTrue(engine.Parse("5"), "engine.Parse(\"5\") != true");
            Assert.IsTrue(engine.Parse("9"), "engine.Parse(\"9\") != true");
            Assert.IsFalse(engine.Parse("a"), "engine.Parse(\"a\") != false");
            Assert.IsFalse(engine.Parse("b"), "engine.Parse(\"b\") != false");
        }

        [Test]
        public void TestSequence()
        {
            WomElement sequence = new WomElement("Sequence",
                new WomElement("MatchRange",
                    new WomProperty("Start", "a"),
                    new WomProperty("End", "z")),
                new WomElement("MatchRange",
                    new WomProperty("Start", "0"),
                    new WomProperty("End", "9")));
            ParserEngineBuilder builder = new ParserEngineBuilder(sequence);
            ParserEngine engine = builder.CreateEngine();
            Assert.IsTrue(engine.Parse("a0"), "engine.Parse(\"a0\") != true");
            Assert.IsTrue(engine.Parse("a9"), "engine.Parse(\"ab\") != true");
            Assert.IsTrue(engine.Parse("b5"), "engine.Parse(\"b5\") != true");
            Assert.IsFalse(engine.Parse("a"), "engine.Parse(\"a\") != false");
            Assert.IsFalse(engine.Parse("4a"), "engine.Parse(\"4a\") != false");
        }

        [Test]
        public void TestChoice()
        {
            WomElement choice = new WomElement("Choice",
                new WomElement("Match", new WomProperty("Text", "a")),
                new WomElement("Match", new WomProperty("Text", "b")));
            ParserEngineBuilder builder = new ParserEngineBuilder(choice);
            ParserEngine engine = builder.CreateEngine();
            Assert.IsTrue(engine.Parse("a"), "engine.Parse(\"a\") != true");
            Assert.IsTrue(engine.Parse("b"), "engine.Parse(\"b\") != true");
            Assert.IsFalse(engine.Parse("c"), "engine.Parse(\"c\") != false");
        }

        [Test]
        public void TestOptional()
        {
            WomElement grammar = new WomElement("Grammar",
                new WomElement("Rule", new WomProperty("Name", "Test"), new WomProperty("ElementName", "Test"),
                    new WomElement("Optional",
                        new WomElement("Match", new WomProperty("Text", "a")))));

            ParserEngineBuilder builder = new ParserEngineBuilder(grammar);
            ParserEngine engine = builder.CreateEngine();
            WomElement context = new WomElement("TestContext");
            bool result = engine.Parse("aa", context);
            Assert.IsTrue(result, "engine.Parse(\"aa\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Should start at 0");
            Assert.AreEqual(1, context.FirstChild.Length, "Should have length 1");

            context.ElementList.Clear();
            result = engine.Parse("b", context);
            Assert.IsTrue(result, "engine.Parse(\"b\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Should start at 0");
            Assert.AreEqual(0, context.FirstChild.Length, "Should have length 0");
        }

        [Test]
        public void TestZeroOrMore()
        {
            WomElement grammar = new WomElement("Grammar",
                new WomElement("Rule", new WomProperty("Name", "Test"), new WomProperty("ElementName", "Test"),
                    new WomElement("ZeroOrMore",
                        new WomElement("Match", new WomProperty("Text", "a")))));

            ParserEngineBuilder builder = new ParserEngineBuilder(grammar);
            ParserEngine engine = builder.CreateEngine();
            WomElement context = new WomElement("TestContext");
            bool result = engine.Parse("aaab", context);
            Assert.IsTrue(result, "engine.Parse(\"aaab\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Should start at 0");
            Assert.AreEqual(3, context.FirstChild.Length, "Should have length 3");

            context.ElementList.Clear();
            result = engine.Parse("b", context);
            Assert.IsTrue(result, "engine.Parse(\"b\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Should start at 0");
            Assert.AreEqual(0, context.FirstChild.Length, "Should have length 0");
        }

        [Test]
        public void TestZeroOrMore2()
        {
            WomElement grammar = new WomElement("Grammar",
                new WomElement("Rule", new WomProperty("Name", "Test"), new WomProperty("ElementName", "Test"),
                    new WomElement("Match", new WomProperty("Text", "a")),
                    new WomElement("ZeroOrMore",
                        new WomElement("Match", new WomProperty("Text", "b")),
                        new WomElement("Match", new WomProperty("Text", "a")))));

            ParserEngineBuilder builder = new ParserEngineBuilder(grammar);
            ParserEngine engine = builder.CreateEngine();
            WomElement context = new WomElement("TestContext");
            bool result = engine.Parse("ab", context);
            Assert.IsTrue(result, "engine.Parse(\"ab\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Should start at 0");
            Assert.AreEqual(1, context.FirstChild.Length, "Should have length 1");
        }

        [Test]
        public void TestOneOrMore()
        {
            WomElement grammar = new WomElement("Grammar",
                new WomElement("Rule", new WomProperty("Name", "Test"), new WomProperty("ElementName", "Test"),
                    new WomElement("OneOrMore",
                        new WomElement("Match", new WomProperty("Text", "a")))));

            ParserEngineBuilder builder = new ParserEngineBuilder(grammar);
            ParserEngine engine = builder.CreateEngine();
            WomElement context = new WomElement("TestContext");
            bool result = engine.Parse("aaab", context);
            Assert.IsTrue(result, "engine.Parse(\"aaab\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Should start at 0");
            Assert.AreEqual(3, context.FirstChild.Length, "Should have length 3");

            context.ElementList.Clear();
            result = engine.Parse("b", context);
            Assert.IsFalse(result, "engine.Parse(\"b\") != false");
        }

        [Test]
        public void TestCallRule()
        {
            WomElement grammar = new WomElement("Grammar",
                new WomElement("Rule", new WomProperty("Name", "Test"), new WomProperty("ElementName", "Test"),
                    new WomElement("Match", new WomProperty("Text", "a")),
                    new WomElement("CallRule", new WomProperty("Name", "Item")),
                    new WomElement("Match", new WomProperty("Text", "b"))),
                new WomElement("Rule", new WomProperty("Name", "Item"), new WomProperty("ElementName", "Item"),
                    new WomElement("Match", new WomProperty("Text", "c"))));

            ParserEngineBuilder builder = new ParserEngineBuilder(grammar);
            ParserEngine engine = builder.CreateEngine();
            WomElement context = new WomElement("TestContext");
            bool result = engine.Parse("acb", context);
            Assert.IsTrue(result, "engine.Parse(\"acb\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Test should start at 0");
            Assert.AreEqual(3, context.FirstChild.Length, "Test should have length 3");
            Assert.AreEqual("Test", context.FirstChild.Name, "context.FirstChild.Name != \"Test\"");

            Assert.IsNotNull(context.FirstChild.FirstChild, "context.FirstChild.FirstChilde is null");
            Assert.AreEqual(1, context.FirstChild.FirstChild.Start, "Item should start at 1");
            Assert.AreEqual(1, context.FirstChild.FirstChild.Length, "Item should have length 1");
            Assert.AreEqual("Item", context.FirstChild.FirstChild.Name, "context.FirstChild.FirstChilde.Name != \"Test\"");

            context.ElementList.Clear();
            result = engine.Parse("ac", context);
            Assert.IsFalse(result, "engine.Parse(\"ac\") != false");
        }

        [Test]
        public void TestLineStart()
        {
            WomElement grammar = new WomElement("Grammar",
                new WomElement("Rule", new WomProperty("Name", "Test"), new WomProperty("ElementName", "Test"),
                    new WomElement("ZeroOrMore",
                        new WomElement("MatchAny")),
                    new WomElement("LineStart"),
                    new WomElement("Match", new WomProperty("Text", "a"))));

            ParserEngineBuilder builder = new ParserEngineBuilder(grammar);
            ParserEngine engine = builder.CreateEngine();
            WomElement context = new WomElement("TestContext");
            bool result = engine.Parse("aaa\r\naa", context);
            Assert.IsTrue(result, "engine.Parse(\"aaa\r\naa\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Test should start at 0");
            Assert.AreEqual(6, context.FirstChild.Length, "Test should have length 6");
            Assert.AreEqual("Test", context.FirstChild.Name, "context.FirstChild.Name != \"Test\"");

            context.ElementList.Clear();
            result = engine.Parse("aaa\naa", context);
            Assert.IsTrue(result, "engine.Parse(\"aaa\naa\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Test should start at 0");
            Assert.AreEqual(5, context.FirstChild.Length, "Test should have length 5");
            Assert.AreEqual("Test", context.FirstChild.Name, "context.FirstChild.Name != \"Test\"");
        }

        [Test]
        public void TestLineEnd()
        {
            WomElement grammar = new WomElement("Grammar",
                new WomElement("Rule", new WomProperty("Name", "Test"), new WomProperty("ElementName", "Test"),
                    new WomElement("ZeroOrMore",
                        new WomElement("Match", new WomProperty("Text", "a"))),
                    new WomElement("LineEnd")));

            ParserEngineBuilder builder = new ParserEngineBuilder(grammar);
            ParserEngine engine = builder.CreateEngine();
            WomElement context = new WomElement("TestContext");
            bool result = engine.Parse("aaa\r\nbb", context);
            Assert.IsTrue(result, "engine.Parse(\"aaa\r\nbb\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Test should start at 0");
            Assert.AreEqual(3, context.FirstChild.Length, "Test should have length 3");
            Assert.AreEqual("Test", context.FirstChild.Name, "context.FirstChild.Name != \"Test\"");

            context.ElementList.Clear();
            result = engine.Parse("aaaaa", context);
            Assert.IsTrue(result, "engine.Parse(\"aaaaa\") != true");
            Assert.IsNotNull(context.FirstChild, "context.FirstChild is null");
            Assert.AreEqual(0, context.FirstChild.Start, "Test should start at 0");
            Assert.AreEqual(5, context.FirstChild.Length, "Test should have length 5");
            Assert.AreEqual("Test", context.FirstChild.Name, "context.FirstChild.Name != \"Test\"");
        }
    }
}
