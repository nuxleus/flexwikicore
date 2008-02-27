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
using System.Globalization;

namespace FlexWiki
{
    /// <summary>
    /// Takes parser WOM and produces instance of ParserEngine
    /// </summary>
    public class ParserEngineBuilder
    {
        // Fields

        private Dictionary<string, int> _charSetIndex = new Dictionary<string, int>();
        private List<ParserCharSet> _charSets = new List<ParserCharSet>();
        private List<ParserInstruction> _instructions = new List<ParserInstruction>();
        private Dictionary<string, int> _ruleIndex = new Dictionary<string, int>();
        private List<ParserRule> _rules = new List<ParserRule>();
        private int _start;
        private List<string> _strings = new List<string>();

        private const int c_nextInstruction = -2;
        private const int c_unused = -1;

        // Constructors 

        public ParserEngineBuilder(WomElement element)
        {
            Initialize(element);
        }

        // Methods

        public ParserEngine CreateEngine()
        {
            return new ParserEngine(_instructions.ToArray(), _rules.ToArray(),
                _strings.ToArray(), _charSets.ToArray(), _start);
        }

        private int AddCallRule(int ruleIndex)
        {
            int nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.CallRule, nextIndex, ruleIndex));
            return _instructions.Count - 1;
        }

        private int AddCharSet(ParserCharSet value)
        {
            _charSets.Add(value);
            return _charSets.Count - 1;
        }

        private int AddChoice(int choice1, int choice2)
        {
            if (choice1 == c_nextInstruction)
            {
                choice1 = _instructions.Count + 1;
            }
            if (choice2 == c_nextInstruction)
            {
                choice2 = _instructions.Count + 1;
            }
            _instructions.Add(new ParserInstruction(ParserInstructionCode.Choice, choice1, choice2));
            return _instructions.Count - 1;
        }

        private int AddEmpty()
        {
            int nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.Goto, nextIndex));
            return _instructions.Count - 1;
        }

        private void AddInstruction(ParserInstruction instruction)
        {
            _instructions.Add(instruction);
        }

        private int AddMatchChar(char value)
        {
            int nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.MatchChar, nextIndex, value));
            return _instructions.Count - 1;
        }

        private int AddMatchSet(string name)
        {
            int charSetIndex = _charSetIndex[name];
            int nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.MatchCharSet, nextIndex, charSetIndex));
            return _instructions.Count - 1;
        }

        private int AddMatchSet(ParserCharSet value)
        {
            int charSetIndex = AddCharSet(value);
            int nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.MatchCharSet, nextIndex, charSetIndex));
            return _instructions.Count - 1;
        }

        private int AddMatchString(string value)
        {
            int strIndex = AddString(value);
            int nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.MatchString, nextIndex, strIndex));
            return _instructions.Count - 1;
        }

        private int AddString(string value)
        {
            for (int i = 0; i < _strings.Count; i++)
            {
                if (value == _strings[i])
                {
                    return i;
                }
            }
            _strings.Add(value);
            return _strings.Count - 1;
        }

        private int AddSuccess()
        {
            _instructions.Add(new ParserInstruction(ParserInstructionCode.Success));
            return _instructions.Count - 1;
        }

        private int AddRule(string name)
        {
            ParserRule rule = new ParserRule(name);
            int index = _ruleIndex[name];
            _rules[index] = rule;
            return index;
        }

        public int AddRuleSuccess()
        {
            _instructions.Add(new ParserInstruction(ParserInstructionCode.RuleSuccess));
            return _instructions.Count - 1;
        }

        private void Initialize(WomElement element)
        {
            _start = ProcessElement(element);
            AddSuccess();
        }

        private int ProcessAnyChar()
        {
            _charSets.Add(ParserCharSet.AnyChar);
            return _charSets.Count - 1;
        }

        private int ProcessAttribute(WomElement element)
        {
            int nameIndex = AddString(element.Properties["Name"]);
            int result = _instructions.Count;
            int nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.StartAttribute, nextIndex, nameIndex));
            ProcessElementList(element.ElementList);
            nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.EndAttribute, nextIndex, nameIndex));
            return result;
        }

        private int ProcessCallRule(WomElement element)
        {
            string ruleName = element.Properties["Name"];
            int ruleIndex = _ruleIndex[ruleName];
            return AddCallRule(ruleIndex);
        }

        private int ProcessCharSetDefinition(WomElement element)
        {
            int index = _charSetIndex[element.Properties["Name"]];
            _charSets[index] = _charSets[ProcessElement(element.ElementList[0])];
            return index;
        }

        private int ProcessCharSetRange(WomElement element)
        {
            ParserCharSet set = ParserCharSet.FromRange(
                Convert.ToChar(element.Properties["Start"], CultureInfo.CurrentCulture),
                Convert.ToChar(element.Properties["End"], CultureInfo.CurrentCulture));
            _charSets.Add(set);
            return _charSets.Count - 1;
        }

        private int ProcessCharSetString(WomElement element)
        {
            char[] chars = element.Properties["Text"].ToCharArray();
            ParserCharSet charSet = new ParserCharSet(chars);
            return AddCharSet(charSet);
        }

        private int ProcessChoice(WomElement element)
        {
            WomElementCollection elementList = element.ElementList;
            int result = ProcessElement(elementList[0]);
            for (int i = 1; i < elementList.Count; i++)
            {
                result = ProcessChoice(result, elementList[i]);
            }
            return result;
        }

        private int ProcessChoice(int choice1, WomElement element)
        {
            // Create first choice last instruction
            int choice1End = AddEmpty();
            // Create a choice node to point to the two defined choices
            int result = AddChoice(c_unused, c_unused);
            // Create second choice
            int choice2 = ProcessElement(element);
            // Point the choice node to its alternatives
            SetArguments(result, choice1, choice2);
            // Point end of first choice to the end of second choice
            SetArguments(choice1End, _instructions.Count);
            return result;
        }

        private int ProcessElement(WomElement element)
        {
            switch (element.Name)
            {
                case "AnyChar":
                    return ProcessAnyChar();

                case "Attribute":
                    return ProcessAttribute(element);

                case "CallRule":
                    return ProcessCallRule(element);

                case "CharSetDefinition":
                    return ProcessCharSetDefinition(element);

                case "CharSetRange":
                    return ProcessCharSetRange(element);

                case "CharSetString":
                    return ProcessCharSetString(element);

                case "Choice":
                    return ProcessChoice(element);

                case "Grammar":
                    return ProcessGrammar(element);

                case "LineEnd":
                    return ProcessLineEnd(element);

                case "LineStart":
                    return ProcessLineStart(element);

                case "Match":
                    return ProcessMatch(element);

                case "MatchAny":
                    return ProcessMatchAny(element);

                case "MatchRange":
                    return ProcessMatchRange(element);

                case "MatchSet":
                    return ProcessMatchSet(element);

                case "OneOrMore":
                    return ProcessOneOrMore(element);

                case "Optional":
                    return ProcessOptional(element);

                case "Rule":
                    return ProcessRule(element);

                case "Sequence":
                    return ProcessSequence(element);

                case "SetSubtraction":
                    return ProcessSetSubtraction(element);

                case "SetUnion":
                    return ProcessSetUnion(element);

                case "ZeroOrMore":
                    return ProcessZeroOrMore(element);

                default:
                    throw new InvalidOperationException("Unknown element name - " + element.Name);
            }
        }

        private int ProcessElementList(WomElementCollection elementList)
        {
            int result = ProcessElement(elementList[0]);
            for (int i = 1; i < elementList.Count; i++)
            {
                int code = AddEmpty();
                int start = ProcessElement(elementList[i]);
                SetArguments(code, start);
            }
            return result;
        }

        private int ProcessGrammar(WomElement element)
        {
            // Create rule and char set index
            int ruleIndex = 0;
            int charSetIndex = 0;
            for (int i = 0; i < element.ElementList.Count; i++)
            {
                WomElement item = element.ElementList[i];
                if (item.Name == "Rule")
                {
                    _rules.Add(null);
                    _ruleIndex.Add(item.Properties["Name"], ruleIndex++);
                }
                else if (item.Name == "CharSetDefinition")
                {
                    _charSets.Add(null);
                    _charSetIndex.Add(item.Properties["Name"], charSetIndex++);
                }
            }

            int startRule = ProcessElementList(element.ElementList);
            _start = AddCallRule(startRule);
            return _start;
        }

        private int ProcessLineEnd(WomElement element)
        {
            int nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.LineEnd, nextIndex));
            return _instructions.Count - 1;
        }

        private int ProcessLineStart(WomElement element)
        {
            int nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.LineStart, nextIndex));
            return _instructions.Count - 1;
        }

        private int ProcessMatch(WomElement element)
        {
            string text = element.Properties["Text"];
            if (text.Length == 1)
            {
                return AddMatchChar(text[0]);
            }
            return AddMatchString(text);
        }

        private int ProcessMatchAny(WomElement element)
        {
            int nextIndex = _instructions.Count + 1;
            _instructions.Add(new ParserInstruction(ParserInstructionCode.MatchAny, nextIndex));
            return _instructions.Count - 1;
        }

        private int ProcessMatchRange(WomElement element)
        {
            return AddMatchSet(ParserCharSet.FromRange(
                Convert.ToChar(element.Properties["Start"], CultureInfo.CurrentCulture),
                Convert.ToChar(element.Properties["End"], CultureInfo.CurrentCulture)));
        }

        private int ProcessMatchSet(WomElement element)
        {
            return AddMatchSet(element.Properties["Name"]);
        }

        private int ProcessOneOrMore(WomElement element)
        {
            int contentStart = ProcessElementList(element.ElementList);
            AddChoice(contentStart, c_nextInstruction);
            return contentStart;
        }

        private int ProcessOptional(WomElement element)
        {
            int contentStart = ProcessElementList(element.ElementList);
            int contentEnd = AddEmpty();
            int result = AddChoice(contentStart, contentEnd);
            SetArguments(contentEnd, _instructions.Count);
            return result;
        }

        private int ProcessRule(WomElement element)
        {
            int rule = AddRule(element.Properties["Name"]);
            _rules[rule].ElementName = element.Properties["ElementName"];
            SetRuleStart(rule, ProcessElementList(element.ElementList));
            AddRuleSuccess();
            return rule;
        }

        private int ProcessSequence(WomElement element)
        {
            WomElementCollection sequence = element.ElementList;
            return ProcessElementList(sequence);
        }

        private int ProcessSetSubtraction(WomElement element)
        {
            ParserCharSet charSet1 = _charSets[ProcessElement(element.ElementList[0])];
            ParserCharSet charSet2 = _charSets[ProcessElement(element.ElementList[1])];
            return AddCharSet(charSet1 - charSet2);
        }

        private int ProcessSetUnion(WomElement element)
        {
            ParserCharSet set = new ParserCharSet();
            WomElementCollection list = element.ElementList;
            for (int i = 0; i < list.Count; i++)
            {
                set = set + _charSets[ProcessElement(list[i])];
            }
            _charSets.Add(set);
            return _charSets.Count - 1;
        }

        private int ProcessZeroOrMore(WomElement element)
        {
            int contentStart = ProcessElementList(element.ElementList);
            return AddChoice(contentStart, c_nextInstruction);
        }

        private void SetArguments(int instructionIndex, int argument1)
        {
            ParserInstruction instruction = _instructions[instructionIndex];
            instruction._argument1 = argument1;
            _instructions[instructionIndex] = instruction;
        }

        private void SetArguments(int instructionIndex, int argument1, int argument2)
        {
            ParserInstruction instruction = _instructions[instructionIndex];
            instruction._argument1 = argument1;
            instruction._argument2 = argument2;
            _instructions[instructionIndex] = instruction;
        }

        private void SetRuleStart(int ruleIndex, int ruleStart)
        {
            _rules[ruleIndex].Start = ruleStart;
        }
    }
}
