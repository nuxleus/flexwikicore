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
    public class ParserCharSet
    {
        // Fields

        public static readonly ParserCharSet AnyChar = CreateAnyCharSet();

        // All ASCII symbols are stored in bool array to provide fast solution for 80% cases
        private bool[] _ascii = new bool[c_asciiCount];

        private List<CharRange> _ranges = new List<CharRange>();

        // _categories is used to keep information about Unicode categories.
        // Sign bit is used to indicate that Unicode categories are used.
        // Each category is represented as one bit.
        private int _categories;

        private ParserCharSet _negative;

        private const int c_asciiCount = 128;
        private const int c_categoryNoSignMask = Int32.MaxValue; // 0x7FFFFFFF
        private const int c_categorySignMask = Int32.MinValue; // 0x80000000		
        private const char c_lastChar = '\uFFFF';
        private const int c_unicodeCategoryCount = 30;

        private static readonly Dictionary<string, int> s_categories = InitializeCategories();
        private static readonly bool[][] s_asciiCategories = InitializeAsciiCategories();

        // Constructors

        public ParserCharSet(params char[] entries)
        {
            List<char> rangeLetters = null;
            for (int i = 0; i < entries.Length; i++)
            {
                char ch = entries[i];
                if (ch < c_asciiCount)
                {
                    _ascii[ch] = true;
                }
                else
                {
                    if (rangeLetters == null)
                    {
                        rangeLetters = new List<char>();
                    }
                    rangeLetters.Add(ch);
                }
            }
            if (rangeLetters != null)
            {
                rangeLetters.Sort();
                for (int i = 0; i < rangeLetters.Count; i++)
                {
                    char ch = rangeLetters[i];
                    AppendRange(_ranges, new CharRange(ch, ch));
                }
            }
        }

        private ParserCharSet()
        {
        }

        private ParserCharSet(ParserCharSet other)
        {
            Array.Copy(other._ascii, _ascii, _ascii.Length);
            _categories = other._categories;
            _ranges.AddRange(other._ranges);
            if (other._negative != null)
            {
                _negative = new ParserCharSet(other._negative);
            }
        }

        // Methods

        public bool Contains(char value)
        {
            if (value < c_asciiCount)
            {
                return _ascii[value];
            }
            bool result = false;
            if (_categories < 0)
            {
                UnicodeCategory cat = Char.GetUnicodeCategory(value);
                result = ((1 << (int)cat) & _categories) > 0;
            }
            if (!result)
            {
                //TODO: do binary search if count > 16. 
                for (int i = 0; i < _ranges.Count; i++)
                {
                    CharRange range = _ranges[i];
                    if (value < range.First)
                    {
                        break;
                    }
                    if (value <= range.Last)
                    {
                        result = true;
                        break;
                    }
                }
            }
            if (result && _negative != null)
            {
                result = !_negative.Contains(value);
            }
            return result;
        }

        public override bool Equals(object obj)
        {
            ParserCharSet other = obj as ParserCharSet;
            if (other != null)
            {
                for (int i = 0; i < _ascii.Length; i++)
                {
                    if (_ascii[i] != other._ascii[i])
                    {
                        return false;
                    }
                }

                if (_ranges.Count != other._ranges.Count)
                {
                    return false;
                }
                for (int i = 0; i < _ranges.Count; i++)
                {
                    if (_ranges[i] != other._ranges[i])
                    {
                        return false;
                    }
                }

                if (_categories != other._categories)
                {
                    return false;
                }
                return (_negative == other._negative);
            }
            return false;
        }

        public static ParserCharSet FromRange(char first, char last)
        {
            if (last < first)
            {
                throw new ArgumentException("last character must be greater or equal to start character");
            }
            ParserCharSet charSet = new ParserCharSet();
            if (last < c_asciiCount)
            {
                for (int i = first; i <= last; i++)
                {
                    charSet._ascii[i] = true;
                }
            }
            else if (first < c_asciiCount)
            {
                for (int i = first; i < c_asciiCount; i++)
                {
                    charSet._ascii[i] = true;
                }
                if (last > first)
                {
                    charSet._ranges.Add(new CharRange((char)c_asciiCount, last));
                }
            }
            else
            {
                charSet._ranges.Add(new CharRange(first, last));
            }
            return charSet;
        }

        public static ParserCharSet FromUnicodeCategory(string name)
        {
            int categories = s_categories[name];
            if (categories == 0)
            {
                throw new ArgumentException("Unknown category");
            }
            ParserCharSet result = new ParserCharSet();
            // Set sign bit to indicate that categories are used.
            result._categories = categories | c_categorySignMask;
            result.AddAsciiCategories(categories);
            return result;
        }

        public override int GetHashCode()
        {
            int hashCode = _ascii.GetHashCode() ^ _ranges.GetHashCode() ^ _categories;
            if (_negative != null)
            {
                hashCode ^= _negative.GetHashCode();
            }
            return hashCode;
        }

        public static ParserCharSet operator +(ParserCharSet set1, ParserCharSet set2)
        {
            ParserCharSet result = new ParserCharSet(set1);
            result.Add(set2);
            return result;
        }

        public static ParserCharSet operator -(ParserCharSet set1, ParserCharSet set2)
        {
            ParserCharSet result = new ParserCharSet(set1);
            result.Remove(set2);
            return result;
        }

        public static bool operator ==(ParserCharSet set1, ParserCharSet set2)
        {
            if ((object)set1 != null)
            {
                return set1.Equals(set2);
            }
            else
            {
                return ((object)set2 == null);
            }
        }

        public static bool operator !=(ParserCharSet set1, ParserCharSet set2)
        {
            return (!(set1 == set2));
        }

        private void Add(ParserCharSet set)
        {
            AddAsciiArray(set._ascii);
            AddUnicode(set);
        }

        private void AddAsciiArray(bool[] ascii)
        {
            for (int i = 0; i < c_asciiCount; i++)
            {
                this._ascii[i] |= ascii[i];
            }
        }

        private void AddAsciiCategories(int categories)
        {
            // check each bit and add ASCII values if it is set. 
            for (int i = 0; i < s_asciiCategories.Length; i++)
            {
                if ((categories & 1) > 0)
                {
                    AddAsciiArray(s_asciiCategories[i]);
                }
                categories >>= 1;
            }
        }

        private void AddRanges(List<CharRange> ranges)
        {
            List<CharRange> result = new List<CharRange>(_ranges.Count + ranges.Count);
            int i = 0;
            int j = 0;
            while (i < _ranges.Count && j < ranges.Count)
            {
                if (_ranges[i].First <= ranges[j].First)
                {
                    AppendRange(result, _ranges[i++]);
                }
                else
                {
                    AppendRange(result, ranges[j++]);
                }
            }
            // Copy the rest of items
            for (int k = i; k < _ranges.Count; k++)
            {
                result.Add(_ranges[k]);
            }
            for (int k = j; k < ranges.Count; k++)
            {
                result.Add(ranges[k]);
            }
            _ranges = result;
        }

        private void AddUnicode(ParserCharSet set)
        {
            AddRanges(set._ranges);
            if (_negative != null)
            {
                _negative.RemoveUnicode(set);
            }
            _categories |= set._categories;
        }

        private static void AppendRange(List<CharRange> list, CharRange range)
        {
            if (list.Count == 0)
            {
                list.Add(range);
            }
            else
            {
                CharRange lastRange = list[list.Count - 1];
                char last = lastRange.Last;
                if (last != c_lastChar)
                {
                    if (range.First > last + 1)
                    {
                        list.Add(range);
                    }
                    else if (last < range.Last)
                    {
                        lastRange.Last = range.Last;
                    }
                }
            }
        }

        private static ParserCharSet CreateAnyCharSet()
        {
            // Any char is the one which has any character inside
            ParserCharSet anyChar = new ParserCharSet();
            for (int i = 0; i < c_asciiCount; i++)
            {
                anyChar._ascii[i] = true;
            }
            anyChar._categories = -1; // any category
            return anyChar;
        }

        private static bool[][] InitializeAsciiCategories()
        {
            // Categories of all ascii codes
            bool[][] asciiCategories = new bool[c_unicodeCategoryCount][];
            for (int i = 0; i < asciiCategories.Length; i++)
            {
                asciiCategories[i] = new bool[c_asciiCount];
            }
            for (int i = 0; i < c_asciiCount; i++)
            {
                UnicodeCategory category = Char.GetUnicodeCategory((char)i);
                asciiCategories[(int)category][i] = true;
            }
            return asciiCategories;
        }

        private static Dictionary<string, int> InitializeCategories()
        {
            Dictionary<string, int> categories = new Dictionary<string, int>();

            // Define standard Unicode categories
            // Reference form http://www.unicode.org/Public/UNIDATA/UCD.html is used.

            // Lu - Letter, Uppercase 
            categories["Lu"] = 1 << (int)UnicodeCategory.UppercaseLetter;
            // Ll - Letter, Lowercase 
            categories["Ll"] = 1 << (int)UnicodeCategory.LowercaseLetter;
            // Lt - Letter, Titlecase 
            categories["Lt"] = 1 << (int)UnicodeCategory.TitlecaseLetter;
            // Lm - Letter, Modifier 
            categories["Lm"] = 1 << (int)UnicodeCategory.ModifierLetter;
            // Lo - Letter, Other 
            categories["Lo"] = 1 << (int)UnicodeCategory.OtherLetter;
            // L - any letter
            categories["L"] = (1 << (int)UnicodeCategory.UppercaseLetter)
                            | (1 << (int)UnicodeCategory.LowercaseLetter)
                            | (1 << (int)UnicodeCategory.TitlecaseLetter)
                            | (1 << (int)UnicodeCategory.ModifierLetter)
                            | (1 << (int)UnicodeCategory.OtherLetter);

            // Mn - Mark, Nonspacing 
            categories["Mn"] = 1 << (int)UnicodeCategory.NonSpacingMark;
            // Mc - Mark, Spacing Combining 
            categories["Mc"] = 1 << (int)UnicodeCategory.SpacingCombiningMark;
            // Me - Mark, Enclosing 
            categories["Me"] = 1 << (int)UnicodeCategory.EnclosingMark;
            // M - any mark
            categories["M"] = (1 << (int)UnicodeCategory.NonSpacingMark)
                            | (1 << (int)UnicodeCategory.SpacingCombiningMark)
                            | (1 << (int)UnicodeCategory.EnclosingMark);

            // Nd - Number, Decimal Digit 
            categories["Nd"] = 1 << (int)UnicodeCategory.DecimalDigitNumber;
            // Nl - Number, Letter 
            categories["Nl"] = 1 << (int)UnicodeCategory.LetterNumber;
            // No - Number, Other 
            categories["No"] = 1 << (int)UnicodeCategory.OtherNumber;
            // N - any number
            categories["N"] = (1 << (int)UnicodeCategory.DecimalDigitNumber)
                            | (1 << (int)UnicodeCategory.LetterNumber)
                            | (1 << (int)UnicodeCategory.OtherNumber);

            // Pc - Punctuation, Connector 
            categories["Pc"] = 1 << (int)UnicodeCategory.ConnectorPunctuation;
            // Pd - Punctuation, Dash 
            categories["Pd"] = 1 << (int)UnicodeCategory.DashPunctuation;
            // Ps - Punctuation, Open 
            categories["Ps"] = 1 << (int)UnicodeCategory.OpenPunctuation;
            // Pe - Punctuation, Close 
            categories["Pe"] = 1 << (int)UnicodeCategory.ClosePunctuation;
            // Pi - Punctuation, Initial quote (may behave like Ps or Pe depending on usage) 
            categories["Pi"] = 1 << (int)UnicodeCategory.InitialQuotePunctuation;
            // Pf - Punctuation, Final quote (may behave like Ps or Pe depending on usage) 
            categories["Pf"] = 1 << (int)UnicodeCategory.FinalQuotePunctuation;
            // Po - Punctuation, Other 
            categories["Po"] = 1 << (int)UnicodeCategory.OtherPunctuation;
            // P - any punctuation
            categories["P"] = (1 << (int)UnicodeCategory.ConnectorPunctuation)
                            | (1 << (int)UnicodeCategory.DashPunctuation)
                            | (1 << (int)UnicodeCategory.OpenPunctuation)
                            | (1 << (int)UnicodeCategory.ClosePunctuation)
                            | (1 << (int)UnicodeCategory.InitialQuotePunctuation)
                            | (1 << (int)UnicodeCategory.FinalQuotePunctuation)
                            | (1 << (int)UnicodeCategory.OtherPunctuation);

            // Sm - Symbol, Math 
            categories["Sm"] = 1 << (int)UnicodeCategory.MathSymbol;
            // Sc - Symbol, Currency 
            categories["Sc"] = 1 << (int)UnicodeCategory.CurrencySymbol;
            // Sk - Symbol, Modifier 
            categories["Sk"] = 1 << (int)UnicodeCategory.ModifierSymbol;
            // So - Symbol, Other 
            categories["So"] = 1 << (int)UnicodeCategory.OtherSymbol;
            // S - any symbol
            categories["S"] = (1 << (int)UnicodeCategory.MathSymbol)
                            | (1 << (int)UnicodeCategory.CurrencySymbol)
                            | (1 << (int)UnicodeCategory.ModifierSymbol)
                            | (1 << (int)UnicodeCategory.OtherSymbol);

            // Zs - Separator, Space 
            categories["Zs"] = 1 << (int)UnicodeCategory.SpaceSeparator;
            // Zl - Separator, Line 
            categories["Zl"] = 1 << (int)UnicodeCategory.LineSeparator;
            // Zp - Separator, Paragraph 
            categories["Zp"] = 1 << (int)UnicodeCategory.ParagraphSeparator;
            // Z - any separator
            categories["Z"] = (1 << (int)UnicodeCategory.SpaceSeparator)
                            | (1 << (int)UnicodeCategory.LineSeparator)
                            | (1 << (int)UnicodeCategory.ParagraphSeparator);

            // Cc - Other, Control 
            categories["Cc"] = 1 << (int)UnicodeCategory.Control;
            // Cf - Other, Format 
            categories["Cf"] = 1 << (int)UnicodeCategory.Format;
            // Cs - Other, Surrogate 
            categories["Cs"] = 1 << (int)UnicodeCategory.Surrogate;
            // Co - Other, Private Use 
            categories["Co"] = 1 << (int)UnicodeCategory.PrivateUse;
            // Cn - Other, Not Assigned (no characters in the file have this property) 
            categories["Cn"] = 1 << (int)UnicodeCategory.OtherNotAssigned;
            // C - any other
            categories["C"] = (1 << (int)UnicodeCategory.Control)
                            | (1 << (int)UnicodeCategory.Format)
                            | (1 << (int)UnicodeCategory.Surrogate)
                            | (1 << (int)UnicodeCategory.PrivateUse)
                            | (1 << (int)UnicodeCategory.OtherNotAssigned);
            return categories;
        }

        private void Remove(ParserCharSet set)
        {
            RemoveAsciiArray(set._ascii);
            RemoveUnicode(set);
        }

        private void RemoveAsciiArray(bool[] ascii)
        {
            for (int i = 0; i < c_asciiCount; i++)
            {
                this._ascii[i] &= !ascii[i];
            }
        }

        private void RemoveRanges(List<CharRange> ranges)
        {
            //TODO: optimize
            if (ranges.Count == 0 || _ranges.Count == 0)
            {
                // nothing to remove
                return;
            }
            List<CharRange> result = new List<CharRange>(_ranges.Count);
            int j = 0;
            for (int i = 0; i < ranges.Count; i++)
            {
                CharRange range = ranges[i];
                while (j < _ranges.Count)
                {
                    CharRange current = _ranges[j];
                    if (current.First > range.Last)
                    {
                        break;
                    }
                    if (current.First >= range.First)
                    {
                        if (current.Last <= range.Last)
                        {
                            j++;
                        }
                        else
                        {
                            if (range.Last + 1 <= current.Last)
                            {
                                current.First = (char)(range.Last + 1);
                                break;
                            }
                            else
                            {
                                j++;
                            }
                        }
                    }
                    else
                    {
                        if (current.Last < range.First)
                        {
                            result.Add(current);
                            j++;
                        }
                        else
                        {
                            result.Add(new CharRange(current.First, (char)(range.Last - 1)));
                            if (range.Last + 1 <= current.Last)
                            {
                                current.First = (char)(range.Last + 1);
                                break;
                            }
                            else
                            {
                                j++;
                            }
                        }
                    }
                }
            }
            while (j < _ranges.Count)
            {
                result.Add(_ranges[j++]);
            }
            _ranges = result;
        }

        private void RemoveUnicode(ParserCharSet set)
        {
            if (_categories < 0)
            {
                // Remove category bits, but make sure that we preserve sign bit
                _categories &= ~(set._categories & c_categoryNoSignMask);
                if (_categories == c_categorySignMask)
                {
                    _categories = 0;
                }
            }
            //TODO: add m_negative
            if (_negative != null)
            {
                _negative.AddUnicode(set);
            }
            RemoveRanges(set._ranges);
        }

        private class CharRange
        {
            public char First;
            public char Last;

            public CharRange(char first, char last)
            {
                First = first;
                Last = last;
            }

            public override bool Equals(object obj)
            {
                CharRange other = obj as CharRange;
                if (other != null)
                {
                    return (this.First == other.First && this.Last == other.Last);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return First ^ Last;
            }
        }
    }
}
