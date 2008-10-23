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
using System.Reflection;
using System.IO;
using System.Xml;
using NUnit.Core;

namespace FlexWiki.UnitTests
{
    [SuiteBuilder]
    public class GrammarTestSuiteBuilder : ISuiteBuilder
    {
        public TestSuite BuildFrom(Type type, int assemblyKey)
        {
            TestSuite suite = new TestSuite("FlexWiki Grammar Tests");
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(this.GetType(), "FlexWikiGrammarTests.xml"))
            {
                XmlDocument suiteDoc = new XmlDocument();
                suiteDoc.Load(stream);
                CreateTestFixtures(suite, suiteDoc);
            }
            return suite;
        }

        public bool CanBuildFrom(Type type)
        {
            return type == this.GetType();
        }

        private static void CreateTestFixtures(TestSuite suite, XmlDocument suiteDoc)
        {
            foreach (XmlElement fixtureElement in suiteDoc.DocumentElement.SelectNodes("TestFixture"))
            {
                string name = fixtureElement.GetAttribute("Name");
                if (name.Length > 0)
                {
                    TestSuite fixture = new TestSuite(name);
                    suite.Add(fixture);
                    CreateTests(fixture, fixtureElement);
                }
            }
        }

        private static void CreateTests(TestSuite fixture, XmlElement fixtureElement)
        {
            foreach (XmlElement testElement in fixtureElement.SelectNodes("Test"))
            {
                string name = testElement.GetAttribute("Name");
                if (name.Length > 0)
                {
                    GrammarTest test = new GrammarTest(fixture.Name, name, testElement);
                    fixture.Add(test);
                }
            }
        }
    }
}
