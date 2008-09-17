using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FlexWiki.Formatting;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class ParserContextTests
    {
        //private IParserApplication application;
        private Federation federation;
        private ParserEngine parser;
        IMockWikiApplication testApp;


        [SetUp]
        public void SetUp()
        {
            federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleEmptyNamespace);
            testApp = (IMockWikiApplication)federation.Application;

            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            testApp.SetApplicationProperty("DisableNewParser", false);
            testApp.SetApplicationProperty("EnableNewParser", true);
            parser = new ParserEngine(federation);
        }

        [Test]
        public void ParserContextInitializedTest()
        {
            Assert.IsNotNull(parser.EngineContext);
            Assert.AreEqual("FlexWiki.Formatting.ParserContext", parser.EngineContext.GetType().ToString());
        }
        [Test]
        public void ParserContextCountMainRules()
        {
            ParserContext context = parser.EngineContext;
            Assert.AreEqual(37, context.RuleList.Count);
        }
        [Test]
        public void ParserContextAddRule()
        {
            int rulecount = parser.EngineContext.RuleList.Count;
            
            ParserContext context = parser.EngineContext;

            int len_regexstr = context.RegExpStr.Length;
            context.AddRule(new ParserRule("TestRule", "TestRule", "", "", "", null, context, context));
            Assert.AreEqual(rulecount + 1, context.RuleList.Count);
            Assert.AreEqual(len_regexstr + 3 + "TestRule".Length, context.RegExpStr.Length);
        }
        [Test]
        public void ParserContextGrammarDocument()
        {
            ParserContext context = parser.EngineContext;
            context.GrammarDocument.Read();
            Assert.IsTrue(context.GrammarDocument.Output.Contains("WikiText"));
        }
        [Test]
        public void ParserContextWomElement()
        {
            ParserContext context = parser.EngineContext;
            Assert.AreEqual("WikiText", context.WomElement);
        }
    }
}
