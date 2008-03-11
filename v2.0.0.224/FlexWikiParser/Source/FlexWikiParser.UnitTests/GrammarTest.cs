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
using NUnit.Core;
using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    public class GrammarTest : NUnit.Core.TestCase
    {
        private string _expected;
        private string _source;

        public GrammarTest(string path, string name, XmlElement testElement)
			: base(path, name)
		{
			this.testFramework = TestFramework.FromAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            this.Description = GetElementText(testElement, "Description", true);
            _source = GetElementText(testElement, "SourceText", false);
            _expected = GetElementXml(testElement, "ExpectedWom", false);
		}

        public override void Run(TestCaseResult result)
        {
            if (ShouldRun)
            {
                DateTime start = DateTime.Now;
                try
                {
                    List<string> expectedLines = CreateStringList(WomDocument.Parse(_expected));

                    WomDocument parseResult = ParseWikiText(_source);
                    List<string> actualLines = CreateStringList(parseResult);

                    CompareStringLists(expectedLines, actualLines);
                    result.Success();
                }
                catch (Exception ex)
                {
                    if (ex is NunitException)
                    {
                        ex = ex.InnerException;
                    }
                    if (testFramework.IsIgnoreException(ex))
                    {
                        result.NotRun(BuildMessage(ex), BuildStackTrace(ex));
                    }
                    else
                    {
                        result.Failure(BuildMessage(ex), BuildStackTrace(ex));
                    }
                }
                finally
                {
                    DateTime stop = DateTime.Now;
                    TimeSpan span = stop.Subtract(start);
                    result.Time = (double)span.Ticks / (double)TimeSpan.TicksPerSecond;
                }
            }
            else
            {
                result.NotRun(this.IgnoreReason);
            }
        }

        private string BuildMessage(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            if (testFramework.IsAssertException(exception))
            {
                sb.Append(exception.Message);
            }
            else
            {
                sb.AppendFormat("{0} : {1}", exception.GetType().ToString(), exception.Message);
            }

            Exception inner = exception.InnerException;
            while (inner != null)
            {
                sb.Append(Environment.NewLine);
                sb.AppendFormat("  ----> {0} : {1}", inner.GetType().ToString(), inner.Message);
                inner = inner.InnerException;
            }

            return sb.ToString();
        }

        private string BuildStackTrace(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            for (; ; )
            {
                try
                {
                    sb.AppendLine(exception.StackTrace);
                }
                catch (Exception)
                {
                    sb.AppendLine("No stack trace available");
                }
                exception = exception.InnerException;
                if (exception == null)
                {
                    break;
                }
                else
                {
                    sb.AppendLine("--" + exception.GetType().Name);
                }
            }
            return sb.ToString();
        }

        private void CompareStringLists(List<string> expected, List<string> actual)
        {
            int i = 0;
            int j = 0;
            while (i < expected.Count && j < expected.Count)
            {
                string expectedLine = expected[i];
                if (expectedLine.Length == 0)
                {
                    i++;
                    continue;
                }
                string actualLine = actual[j];
                if (actualLine.Length == 0)
                {
                    j++;
                    continue;
                }
                if (expectedLine != actualLine)
                {
                    ReportError(expected, i, actual, j);
                }
                i++;
                j++;
            }
            while (i < expected.Count)
            {
                string expectedLine = expected[i];
                if (expectedLine.Length > 0)
                {
                    ReportError(expected, i, actual, j);
                }
                i++;
            }
            while (j < actual.Count)
            {
                string actualLine = actual[j];
                if (actualLine.Length > 0)
                {
                    ReportError(expected, i, actual, j);
                }
                j++;
            }
        }

        private static List<string> CreateStringList(WomDocument document)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter xmlWriter = new XmlTextWriter(stream, Encoding.Unicode);
            xmlWriter.Formatting = Formatting.Indented;
            document.Save(xmlWriter);
            xmlWriter.Flush();
            stream.Position = 0;
            List<string> result = new List<string>();
            StreamReader reader = new StreamReader(stream);
            for (; ; )
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                result.Add(line);
            }
            return result;
        }

        private static string GetElementText(XmlElement source, string name, bool allowNull)
        {
            XmlElement element = source[name];
            if (element != null)
            {
                return element.InnerText;
            }
            if (allowNull)
            {
                return "";
            }
            throw new ArgumentNullException(name);
        }

        private static string GetElementXml(XmlElement source, string name, bool allowNull)
        {
            XmlElement element = source[name];
            if (element != null)
            {
                return element.InnerXml;
            }
            if (allowNull)
            {
                return "";
            }
            throw new ArgumentNullException(name);
        }

        private static WomDocument ParseWikiText(string source)
        {
            
            return new WomDocument(new WomElement("Document"));
        }

        private void ReportError(List<string> expected, int expectedIndex,
            List<string> actual, int actualIndex)
        {
            Console.WriteLine("=== " + this.Parent.Name + "." + this.Name + " ===");
            Console.WriteLine("Expected:");
            foreach (string line in expected)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("Actual:");
            foreach (string line in actual)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine();

            string expectedLine = (expectedIndex < expected.Count) ? expected[expectedIndex] : "";
            string actualLine = (actualIndex < actual.Count) ? actual[actualIndex] : "";
            Assert.AreEqual(expectedLine, actualLine,
                String.Format("Expected Line #{0}, Actual line #{1}", expectedIndex, actualIndex));
        }
    }
}

