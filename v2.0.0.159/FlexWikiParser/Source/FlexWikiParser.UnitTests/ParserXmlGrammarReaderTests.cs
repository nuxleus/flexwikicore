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
    public class ParserXmlGrammarReaderTests
    {
        [Test]
        public void TestRuleMatch()
        {
            StringReader textReader = new StringReader(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<wps:Grammar version=""1.0"" xmlns:wps=""urn:flexwiki-com:wom/2006/04"">
					<wps:Rule Name=""Test"" Match=""'abc'""/>
				</wps:Grammar>");
            XmlTextReader xmlReader = new XmlTextReader(textReader);
            ParserXmlGrammarReader grammarReader = new ParserXmlGrammarReader(xmlReader);
            WomDocument grammar = grammarReader.Read();
            Assert.AreEqual(@"<Grammar><Rule Name=""Test""><Match Text=""abc"" /></Rule></Grammar>",
                grammar.Xml);
        }
    }
}
