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

namespace FlexWiki
{
    /// <summary>
    /// Reads grammar Xml and produces parser WOM
    /// </summary>
    public class ParserXmlGrammarReader
    {
        // Fields

        private XmlReader _reader;
        private WomDocument _document;

        // Constructors

        public ParserXmlGrammarReader(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            this._reader = reader;
        }

        // Methods

        public WomDocument Read()
        {
            if (_document != null)
            {
                return _document;
            }
            _document = new WomDocument();
            ReadGrammar();
            return _document;
        }

        private void ReadGrammar()
        {
            while (_reader.Read())
            {
                switch (_reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (_reader.LocalName != "Grammar")
                        {
                            throw new InvalidOperationException();
                        }
                        _document.Add(new WomElement("Grammar"));
                        ReadRules();
                        break;

                    case XmlNodeType.EndElement:
                        return;
                }
            }
        }

        private void ReadRule()
        {
            WomElement rule = new WomElement("Rule");
            if (_reader.HasAttributes)
            {
                rule.Properties["Name"] = _reader.GetAttribute("Name");
                string expr = _reader.GetAttribute("Match");
                ParserExpressionReader exprReader = new ParserExpressionReader();
                exprReader.Parse(expr, rule);
                _reader.MoveToElement();
                _document.Root.Add(rule);
            }
        }

        private void ReadRules()
        {
            while (_reader.Read())
            {
                switch (_reader.NodeType)
                {
                    case XmlNodeType.Element:
                        ReadRule();
                        break;

                    case XmlNodeType.EndElement:
                        return;
                }
            }
        }
    }
}
