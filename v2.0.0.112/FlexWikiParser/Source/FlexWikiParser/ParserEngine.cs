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
using System.Diagnostics;
using System.Globalization;

namespace FlexWiki
{
    public class ParserEngine
    {
        // Fields

        private int _charIndex;
        private ParserCharSet[] _charSets;
        private WomElement _context;
        private Stack<WomElement> _contextStack = new Stack<WomElement>();
        private int _code;
        private WomElement _currentElement;
        private char _currentChar;
        private ParserInstruction _instruction;
        private ParserInstruction[] _instructions;
        private int _ip;
        private ParserRule[] _rules;
        private int[] _stack = new int[32];
        private int _stackIndex = -1;
        private int _start;
        private string[] _strings;
        private string _text;
        private int[] _track = new int[32];
        private int _trackIndex = -1;

        // Constructors

        public ParserEngine(ParserInstruction[] instructions,
            ParserRule[] rules,
            string[] strings,
            ParserCharSet[] characterSets,
            int start)
        {
            this._rules = rules;
            this._instructions = instructions;
            this._strings = strings;
            this._charSets = characterSets;
            this._start = start;
        }

        private ParserEngine(ParserEngine engine)
        {
            this._rules = engine._rules;
            this._instructions = engine._instructions;
            this._strings = engine._strings;
            this._charSets = engine._charSets;
            this._start = engine._start;
        }

        // Methods

        public bool Parse(string text)
        {
            return Parse(text, null);
        }

        public bool Parse(string text, WomElement context)
        {
            ParserEngine engine = new ParserEngine(this);
            engine._text = text;
            engine._context = context;
            return engine.InternalParse();
        }

        public void WriteInstructions(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            for (int i = 0; i < _instructions.Length; i++)
            {
                string ruleStart = "";
                for (int j = 0; j < _rules.Length; j++)
                {
                    ParserRule rule = _rules[j];
                    if (rule.Start == i)
                    {
                        ruleStart = " <-- " + rule.Name;
                        break;
                    }
                }
                writer.WriteLine("{0} {1} - {2}: {3}{4}",
                    i,
                    _instructions[i]._code,
                    ParserInstructionCode.GetName(_instructions[i]._code),
                    GetParams(_instructions[i]),
                    ruleStart);
            }
        }

        private void Backtrack()
        {
            _charIndex = _track[_trackIndex--];
            _currentChar = (_charIndex < _text.Length) ? _text[_charIndex] : '\0';
            _code = _track[_trackIndex--];
        }

        private string GetParams(ParserInstruction instruction)
        {
            switch (instruction._code)
            {
                case ParserInstructionCode.None: return "";
                case ParserInstructionCode.Success: return "";
                case ParserInstructionCode.Failure: return "";
                case ParserInstructionCode.MatchChar:
                    return "'" + ((char)instruction._argument2).ToString(CultureInfo.InvariantCulture)
                        + "' " + instruction._argument1.ToString(CultureInfo.InvariantCulture);
                case ParserInstructionCode.MatchString:
                    return "\"" + _strings[instruction._argument2] + "\" " + instruction._argument1.ToString(CultureInfo.InvariantCulture);
                case ParserInstructionCode.MatchCharSet:
                    return instruction._argument2.ToString(CultureInfo.InvariantCulture) + " " + instruction._argument1.ToString(CultureInfo.InvariantCulture);
                case ParserInstructionCode.Choice:
                    return instruction._argument1.ToString(CultureInfo.InvariantCulture) + " " + instruction._argument2.ToString(CultureInfo.InvariantCulture);
                case ParserInstructionCode.Goto:
                    return instruction._argument1.ToString(CultureInfo.InvariantCulture);
                case ParserInstructionCode.CallRule:
                    return "\"" + _rules[instruction._argument2].Name + "\" " + instruction._argument1.ToString(CultureInfo.InvariantCulture);
                case ParserInstructionCode.RuleSuccess:
                    return "";
                case ParserInstructionCode.RuleFailure:
                    return "";
                case ParserInstructionCode.StartAttribute:
                    return "\"" + _strings[instruction._argument2] + "\" " + instruction._argument1.ToString(CultureInfo.InvariantCulture);
                case ParserInstructionCode.EndAttribute:
                    return "\"" + _strings[instruction._argument2] + "\" " + instruction._argument1.ToString(CultureInfo.InvariantCulture);
                case ParserInstructionCode.AttributeFailure:
                    return instruction._argument1.ToString(CultureInfo.InvariantCulture);
            }
            return "";
        }

        private bool InternalParse()
        {
            WriteTrack(ParserInstructionCode.Failure);
            ReadInstruction(_start);
            _charIndex = -1;
            ReadChar();
            for (; ; )
            {
                switch (_code)
                {
                    case ParserInstructionCode.Success:
                        return true;

                    case ParserInstructionCode.Failure | ParserInstructionCode.Back:
                        return false;

                    case ParserInstructionCode.MatchAny:
                        if (_currentChar != '\0')
                        {
                            ReadChar();
                            ReadInstruction(_instruction._argument1);
                            continue;
                        }
                        break;

                    case ParserInstructionCode.MatchChar:
                        if (_currentChar == _instruction._argument2)
                        {
                            ReadChar();
                            ReadInstruction(_instruction._argument1);
                            continue;
                        }
                        break;

                    case ParserInstructionCode.MatchString:
                        if (MatchString(_strings[_instruction._argument2]))
                        {
                            ReadInstruction(_instruction._argument1);
                            continue;
                        }
                        break;

                    case ParserInstructionCode.MatchCharSet:
                        if (_charSets[_instruction._argument2].Contains(_currentChar))
                        {
                            ReadChar();
                            ReadInstruction(_instruction._argument1);
                            continue;
                        }
                        break;

                    case ParserInstructionCode.Choice:
                        WriteTrack(ParserInstructionCode.Goto, _instruction._argument2);
                        ReadInstruction(_instruction._argument1);
                        continue;

                    case ParserInstructionCode.Goto:
                        ReadInstruction(_instruction._argument1);
                        continue;

                    case ParserInstructionCode.Goto | ParserInstructionCode.Back:
                        ReadInstruction(ReadTrack());
                        continue;

                    case ParserInstructionCode.CallRule:
                        //TODO: implement rule result cache
                        ParserRule rule = this._rules[_instruction._argument2];
                        string elementName = rule.ElementName;
                        if (!string.IsNullOrEmpty(elementName))
                        {
                            _currentElement = new WomElement(rule.Name);
                            _currentElement._start = _charIndex;
                            _contextStack.Push(_context);
                            _context = _currentElement;
                        }
                        PushStack();
                        WriteTrack(ParserInstructionCode.RuleFailure, _charIndex);
                        ReadInstruction(rule.Start);
                        continue;

                    case ParserInstructionCode.RuleSuccess:
                        PopStack();
                        ParserRule rule1 = this._rules[_instruction._argument2];
                        string elementName1 = rule1.ElementName;
                        if (!string.IsNullOrEmpty(elementName1))
                        {
                            _context = _contextStack.Pop();
                            if (_context != null && _currentElement != null)
                            {

                                _currentElement._length = _charIndex - _currentElement._start;
                                if (_currentElement.ElementList.Count == 1)
                                {
                                    //Reduce element
                                    WomElement child = _currentElement.ElementList[0];
                                    if (_currentElement._start == child._start && _currentElement._length == child._length)
                                    {
                                        _currentElement.ElementList.Clear();
                                        _currentElement = child;
                                    }
                                }
                                _context.ElementList.Add(_currentElement);
                                _currentElement = _context;
                                //TODO: add backtrack for WOM element
                            }
                        }
                        ReadInstruction(_instruction._argument1);
                        continue;

                    case ParserInstructionCode.RuleFailure | ParserInstructionCode.Back:
                        _charIndex = ReadTrack();
                        PopStack();
                        ParserRule rule2 = this._rules[_instruction._argument2];
                        string elementName2 = rule2.ElementName;
                        if (!string.IsNullOrEmpty(elementName2))
                        {
                            _context = _contextStack.Pop();
                            _currentElement = _context;
                        }
                        break;

                    case ParserInstructionCode.StartAttribute:
                        //TODO: implement rule result cache
                        string attrName = this._strings[_instruction._argument2];
                        _currentElement = new WomElement(attrName);
                        _currentElement._start = _charIndex;
                        _contextStack.Push(_context);
                        _context = _currentElement;
                        PushStack();
                        WriteTrack(ParserInstructionCode.AttributeFailure, _charIndex);
                        ReadInstruction(_instruction._argument1);
                        continue;

                    case ParserInstructionCode.EndAttribute:
                        int nextInstruction = _instruction._argument1;
                        PopStack();
                        _context = _contextStack.Pop();
                        if (_context != null && _currentElement != null)
                        {
                            //ParserRule rule1 = this.rules[instruction.argument2];
                            _currentElement._length = _charIndex - _currentElement._start;
                            //context.ElementList.Add(currentElement);
                            _context.Properties[_currentElement.Name] = _currentElement.GetText(_text);
                            _currentElement = _context;
                            //TODO: add backtrack for WOM element
                        }
                        ReadInstruction(nextInstruction);
                        continue;

                    case ParserInstructionCode.AttributeFailure | ParserInstructionCode.Back:
                        _charIndex = ReadTrack();
                        PopStack();
                        _context = _contextStack.Pop();
                        _currentElement = _context;
                        break;

                    case ParserInstructionCode.LineStart:
                        if (_charIndex == 0 || (_currentChar != '\n' && _currentChar != '\r' &&
                            (_text[_charIndex - 1] == '\r' || _text[_charIndex - 1] == '\n')))
                        {
                            ReadInstruction(_instruction._argument1);
                            continue;
                        }
                        break;

                    case ParserInstructionCode.LineEnd:
                        if (_currentChar == '\0' || _currentChar == '\n' || _currentChar == '\r')
                        {
                            ReadInstruction(_instruction._argument1);
                            continue;
                        }
                        break;

                    default:
                        throw new InvalidOperationException(
                            String.Format(CultureInfo.CurrentCulture, "Unknown instruction code - {0}", _code));
                }

                Backtrack();
            }
        }

        private bool MatchString(string matchText)
        {
            if (matchText.Length > _text.Length - _charIndex)
            {
                return false;
            }
            for (int i = 0, j = _charIndex; i < matchText.Length; i++, j++)
            {
                if (matchText[i] != _text[j])
                {
                    return false;
                }
            }
            ReadChar(matchText.Length);
            return true;
        }

        private void PopStack()
        {
            _trackIndex = _stack[_stackIndex--];
            _ip = _stack[_stackIndex--];
            ReadInstruction(_ip);
        }

        private void PushStack()
        {
            if (_stackIndex + 2 >= _stack.Length)
            {
                int[] newStack = new int[_stack.Length * 2];
                Array.Copy(_stack, newStack, _stack.Length);
                _stack = newStack;
            }
            _stack[++_stackIndex] = _ip;
            _stack[++_stackIndex] = _trackIndex;
        }

        private void ReadChar()
        {
            _charIndex++;
            _currentChar = (_charIndex < _text.Length) ? _text[_charIndex] : '\0';
        }

        private void ReadChar(int delta)
        {
            _charIndex += delta;
            _currentChar = (_charIndex < _text.Length) ? _text[_charIndex] : '\0';
        }

        private void ReadInstruction(int index)
        {
            _ip = index;
            _instruction = _instructions[index];
            _code = _instruction._code;
        }

        private int ReadTrack()
        {
            return _track[_trackIndex--];
        }

        private void WriteTrack(int instructionCode)
        {
            if (_trackIndex + 2 >= _track.Length)
            {
                int[] newTrack = new int[_track.Length * 2];
                Array.Copy(_track, newTrack, _track.Length);
                _track = newTrack;
            }
            _track[++_trackIndex] = instructionCode | ParserInstructionCode.Back;
            _track[++_trackIndex] = _charIndex;
        }

        private void WriteTrack(int instructionCode, int argument1)
        {
            if (_trackIndex + 3 >= _track.Length)
            {
                int[] newTrack = new int[_track.Length * 2];
                Array.Copy(_track, newTrack, _track.Length);
                _track = newTrack;
            }
            _track[++_trackIndex] = argument1;
            _track[++_trackIndex] = instructionCode | ParserInstructionCode.Back;
            _track[++_trackIndex] = _charIndex;
        }
    }
}
