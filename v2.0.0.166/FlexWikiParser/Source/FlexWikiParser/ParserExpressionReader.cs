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

namespace FlexWiki
{
    // Match expression syntax
    //
    // Expr         ::= (ws Choice)+ ws
    // Choice       ::= Factor (ws '|' ws Factor)*         // choice list
    // Factor       ::= Atom (('?' | '*' | '+') '?'?)?
    // Atom         ::= CharRange                          // char range
    //                | String                             // one or more chars
    //                | CharSetName                        // named charset
    //                | AnyChar                            // any char
    //                | LineStart                          // line start
    //                | LineEnd                            // line end
    //                | RuleCall                           // call another rule
    //                | '(' Expr ')'                       // parentheses
    // CharRange    ::= Char '..' Char
    // Char         ::= '\'' CharName '\''
    // String       ::= '\'' CharName+ '\''
    // CharSetName  ::= '{' Name '}'
    // AnyChar      ::= '_'
    // LineStart    ::= '^'
    // LineEnd      ::= '$'
    // RuleCall     ::= Name 
    // Name         ::= ('A'..'Z' | 'a'..'z') {NC}* 
    // CharName     ::= {SingleChar} | EscChar | HexChar | UnicodeChar
    // EscChar      ::= '\\\'' | '\\\"' | '\\\\' | '\\a' | '\\b' | '\\f' | '\\n' | '\\r' | '\\t' | '\\v'
    // HexChar      ::= '\\x' {HexDigit} ({HexDigit} ({HexDigit} ({HexDigit})?)?)?
    // UnicodeChar  ::= '\\u' {HexDigit} {HexDigit} {HexDigit} {HexDigit}
    // ws           ::= (' ' | '\t' | '\r\n' | '\n' | '\r')*

    // Character set definition
    //
    // CharSetDef   ::= ws CharSet ws (('+' | '-') ws CharSet ws)* 
    // CharSet      ::= CharSetName
    //                | '[' CharSetItems ']'    
    // CharSetItems ::= CharSetItem (',' ws CharSetItem)*
    // CharSetItem  ::= CharRange
    //                | Char  
    //                | AnyChar

    // Character sets
    //
    // {NC}           = ['A'..'Z', 'a'..'z', '0'..'9', '_']
    // {SingleChar}   = [_] - ['\'', '\\', '\n', '\r']
    // {HexDigit}     = ['0'..'9', 'A'..'F', 'a'..'f']

    /// <summary>
    /// Reads expression text and produces WOM for it
    /// </summary>
    public class ParserExpressionReader
    {
        // Fields

        private ParserEngine _engine;

        // Methods

        public ParserEngine ExpressionParserEngine
        {
            get
            {
                if (_engine == null)
                {
                    ParserEngineBuilder builder = new ParserEngineBuilder(CreateExpressionGrammar());
                    _engine = builder.CreateEngine();
                }
                return _engine;
            }
        }

        public bool Parse(string text, WomElement context)
        {
            return ExpressionParserEngine.Parse(text, context);
        }

        public void WriteInstructions(TextWriter writer)
        {
            ExpressionParserEngine.WriteInstructions(writer);
        }

        private static WomElement AnyChar()
        {
            return new WomElement("AnyChar");
        }

        private static WomElement Attribute(string name, WomElement expr)
        {
            return new WomElement("Attribute", new WomProperty("Name", name), expr);
        }

        private static WomElement CallRule(string name)
        {
            return new WomElement("CallRule", new WomProperty("Name", name));
        }

        private static WomElement CharSet(params char[] setChars)
        {
            return new WomElement("CharSetString",
                new WomProperty("Text", new String(setChars)));
        }

        private static WomElement CharSetRange(char start, char end)
        {
            return new WomElement("CharSetRange",
                new WomProperty("Start", start.ToString()),
                new WomProperty("End", end.ToString()));
        }

        private static WomElement Choice(params WomElement[] choices)
        {
            if (choices.Length < 2)
            {
                throw new ArgumentException(Resources.TwoChoicesExpected);
            }
            return new WomElement("Choice", choices);
        }

        private static WomElement CreateExpressionGrammar()
        {
            WomElement grammar = new WomElement("Grammar");
            // Match expression syntax

            // Expr         ::= (ws Choice)+ ws
            DefineRule(grammar, "Expr",
                OneOrMore(CallRule("ws"), CallRule("Choice")), CallRule("ws"));

            // Choice       ::= Factor (ws '|' ws Factor)*         // choice list
            DefineRule(grammar, "Choice", "Choice",
                CallRule("Factor"), ZeroOrMore(CallRule("ws"), Match('|'), CallRule("ws"), CallRule("Factor")));

            // Factor       ::= Optional 
            //                | ZeroOrMore
            //                | OneOrMore
            //                | Atom
            DefineRule(grammar, "Factor", Choice(
                CallRule("Optional"),
                CallRule("ZeroOrMore"),
                CallRule("OneOrMore"),
                CallRule("Atom")));

            // Optional     ::= Atom '?'
            DefineRule(grammar, "Optional", "Optional",
                CallRule("Atom"), Match('?'));

            // Optional     ::= Atom '*'
            DefineRule(grammar, "ZeroOrMore", "ZeroOrMore",
                CallRule("Atom"), Match('*'));

            // Optional     ::= Atom '+'
            DefineRule(grammar, "OneOrMore", "OneOrMore",
                CallRule("Atom"), Match('+'));

            // Atom         ::= MatchRange                          // char range
            //                | Match                             // one or more chars
            //                | MatchSet                           // named charset
            //                | AnyChar                            // any char
            //                | LineStart                          // line start
            //                | LineEnd                            // line end
            //                | CallRule                           // call another rule
            //                | '(' Expr ')'                       // parentheses
            DefineRule(grammar, "Atom", Choice(
                CallRule("MatchRange"),
                CallRule("Match"),
                CallRule("MatchSet"),
                //Call("AnyChar"),
                //Call("LineStart"),
                //Call("LineEnd"),
                CallRule("CallRule")));
            //Sequence(Match('('), Call("Expr"), Match(')'))));

            // MatchRange    ::= @Start=Char '..' @End=Char
            DefineRule(grammar, "MatchRange", "MatchRange",
                Match("'"), Attribute("Start", CallRule("CharName")), Match("'"),
                Match(".."),
                Match("'"), Attribute("End", CallRule("CharName")), Match("'"));

            //// Match         ::= '\'' CharName+ '\''
            DefineRule(grammar, "Match", "Match",
                Match("'"), Attribute("Text", OneOrMore(CallRule("CharName"))), Match("'"));

            //// MatchSet  ::= '{' @Name=Name '}'
            //DefineRule(grammar, "MatchSet", "MatchSet",
            //    Match('{'), Attribute("Name", CallRule("Name")), Match('}'));

            // CharSetName  ::= '{' @Name=Name '}'
            DefineRule(grammar, "CharSetName", "CharSetName",
                Match('{'), Attribute("Name", CallRule("Name")), Match('}'));

            // MatchSet   ::= CharSet (ws ('+' | '-') ws CharSet)* 
            DefineRule(grammar, "MatchSet", "MatchSet",
                CallRule("CharSet")); 
                //ZeroOrMore(CallRule("ws"), Choice(Match('+'), Match('-')), CallRule("ws"), CallRule("CharSet")));

            //// AnyChar      ::= '_'
            //DefineRule(grammar, "AnyChar",
            //    Match('_'));

            //// LineStart    ::= '^'
            //DefineRule(grammar, "LineStart",
            //    Match('^'));

            //// LineEnd      ::= '$'
            //DefineRule(grammar, "LineEnd",
            //    Match('$'));

            // CallRule     ::= Name 
            DefineRule(grammar, "CallRule", "CallRule",
                Attribute("Name", CallRule("Name")));

            // Name         ::= ('A'..'Z' | 'a'..'z') {NC}* 
            DefineRule(grammar, "Name", "Name",
                Choice(MatchRange('A', 'Z'), MatchRange('a', 'z')), ZeroOrMore(MatchSet("NC")));

            // CharName     ::= {SingleChar} | EscChar | HexChar | UnicodeChar
            DefineRule(grammar, "CharName",
                MatchSet("SingleChar"));
            //    MatchSet("SingleChar"), Call("EscChar"), Call("HexChar"), Call("UnicodeChar")));

            //// EscChar      ::= '\\\'' | '\\\"' | '\\\\' | '\\a' | '\\b' | '\\f' | '\\n' | '\\r' | '\\t' | '\\v'
            //DefineRule(grammar, "EscChar", Choice(
            //    Match("\\\'"), Match("\\\""), Match("\\\\"), Match("\\a"), Match("\\b"),
            //    Match("\\f"), Match("\\n"), Match("\\r"), Match("\\t"), Match("\\v")));

            //// HexChar      ::= '\\x' {HexDigit} ({HexDigit} ({HexDigit} ({HexDigit})?)?)?
            //DefineRule(grammar, "HexChar",
            //    Match("\\x"), Optional(MatchSet("HexDigit"),
            //    Optional(MatchSet("HexDigit"), Optional(MatchSet("HexDigit")))));

            //// UnicodeChar  ::= '\\u' {HexDigit} {HexDigit} {HexDigit} {HexDigit}
            //DefineRule(grammar, "UnicodeChar",
            //    Match("\\u"), MatchSet("HexDigit"), MatchSet("HexDigit"), MatchSet("HexDigit"), MatchSet("HexDigit"));

            // ws           ::= (' ' | '\t' | '\r\n' | '\n' | '\r')*
            DefineRule(grammar, "ws",
                ZeroOrMore(Choice(Match(' '), Match('\t'), Match("\r\n"), Match('\n'), Match('\r'))));

            //// Character set definition

            //// CharSetDef   ::= ws CharSet ws (('+' | '-') ws CharSet ws)* 
            //DefineRule(grammar, "CharSetDef",
            //    Call("ws"), Call("CharSet"), Call("ws"),
            //    ZeroOrMore(Choice(Match('+'), Match('-')), Call("ws"), Call("CharSet"), Call("ws")));

            // CharSet      ::= CharSetName
            //                | CharSetDef    
            DefineRule(grammar, "CharSet", Choice(
                CallRule("CharSetName"),
                CallRule("CharSetDef")));

            // CharSetDef      ::= '[' CharSetItem (',' ws CharSetItem)* ']'    
            DefineRule(grammar, "CharSetDef", "CharSetDef",
                Match('['), CallRule("CharSetItem"), 
                ZeroOrMore(Match(','), CallRule("ws"), CallRule("CharSetItem")),
                Match(']'));

            //// CharSetItems ::= CharSetItem (',' ws CharSetItem)*
            //DefineRule(grammar, "CharSetItems", Choice(
            //    Call("CharSetItem"), ZeroOrMore(Match(','), Call("ws"), Call("CharSetItem"))));

            // CharSetItem  ::= CharRange
            //                | Char  
            //                | AnyChar
            DefineRule(grammar, "CharSetItem", Choice(
                CallRule("MatchRange"),
                CallRule("Match")));
            //DefineRule(grammar, "CharSetItem", Choice(
                //CallRule("CharRange"),
                //CallRule("Char"),
                //CallRule("AnyChar")));

            //// Character sets 

            // {NC}          = ['A'..'Z', 'a'..'z', '0'..'9', '_']
            DefineCharSet(grammar, "NC",
                Union(CharSetRange('A', 'Z'), CharSetRange('a', 'z'), CharSetRange('0', '9'), CharSet('_')));

            // {SingleChar}  = [_] - ['\'', '\\', '\n', '\r']
            DefineCharSet(grammar, "SingleChar",
                Subtraction(AnyChar(), CharSet('\'', '\\', '\n', '\r')));

            //// {HexDigit}    = ['0'..'9', 'A'..'F', 'a'..'f']
            //DefineCharSet(grammar, "HexDigit",
            //    Union(CharSetRange('0', '9'), CharSetRange('A', 'F'), CharSetRange('a', 'f')));

            return grammar;
        }

        private static void DefineCharSet(WomElement grammar, string name, WomElement value)
        {
            WomElement charSet = new WomElement("CharSetDefinition", new WomProperty("Name", name), value);
            grammar.Add(charSet);
        }

        private static void DefineRule(WomElement grammar, string name, WomElement expr)
        {
            DefineRule(grammar, name, "", expr);
        }

        private static void DefineRule(WomElement grammar, string name, string elementName, WomElement expr)
        {
            WomElement rule = new WomElement("Rule");
            rule.Properties["Name"] = name;
            if (!string.IsNullOrEmpty(elementName))
            {
                rule.Properties["ElementName"] = elementName;
            }
            rule.Add(expr);
            grammar.Add(rule);
        }

        private static void DefineRule(WomElement grammar, string name, params WomElement[] exprList)
        {
            DefineRule(grammar, name, Sequence(exprList));
        }

        private static void DefineRule(WomElement grammar, string name, string elementName, params WomElement[] exprList)
        {
            DefineRule(grammar, name, elementName, Sequence(exprList));
        }

        private static WomElement Match(char value)
        {
            return new WomElement("Match", new WomProperty("Text", value.ToString()));
        }

        private static WomElement Match(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException("String cannot be empty");
            }
            return new WomElement("Match", new WomProperty("Text", value));
        }

        private static WomElement MatchRange(char startChar, char endChar)
        {
            WomElement result = new WomElement("MatchRange");
            result.Properties["Start"] = startChar.ToString();
            result.Properties["End"] = endChar.ToString();
            return result;
        }

        private static WomElement MatchSet(string name)
        {
            return new WomElement("MatchSet", new WomProperty("Name", name));
        }

        private static WomElement OneOrMore(WomElement expr)
        {
            return new WomElement("OneOrMore", expr);
        }

        private static WomElement OneOrMore(params WomElement[] sequence)
        {
            return OneOrMore(Sequence(sequence));
        }

        //private WomElement Optional(WomElement expr)
        //{
        //    WomElement result = new WomElement("Optional");
        //    result.Add(expr);
        //    return result;
        //}

        //private WomElement Optional(params WomElement[] sequence)
        //{
        //    return Optional(Sequence(sequence));
        //}

        private static WomElement Sequence(params WomElement[] sequence)
        {
            if (sequence.Length < 1)
            {
                throw new ArgumentException(Resources.OneOrMoreSequenceElementsExpected);
            }

            WomElement result = new WomElement("Sequence");
            result.Add(sequence);
            return result;
        }

        private static WomElement Subtraction(WomElement element1, WomElement element2)
        {
            return new WomElement("SetSubtraction", element1, element2);
        }

        private static WomElement Union(params WomElement[] elements)
        {
            return new WomElement("SetUnion", elements);
        }

        private static WomElement ZeroOrMore(WomElement expr)
        {
            return new WomElement("ZeroOrMore", expr);
        }

        private static WomElement ZeroOrMore(params WomElement[] sequence)
        {
            return ZeroOrMore(Sequence(sequence));
        }
    }
}
