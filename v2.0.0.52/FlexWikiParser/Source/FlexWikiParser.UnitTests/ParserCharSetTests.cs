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

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class ParserCharSetTests
    {
        [Test]
        public void TestContains()
        {
            ParserCharSet ws = new ParserCharSet(' ', '\r', '\n', '\t');
            Assert.IsTrue(ws.Contains(' '), "' ' must be in set");
            Assert.IsTrue(ws.Contains('\t'), "'\\t' must be in set");
            Assert.IsTrue(ws.Contains('\n'), "'\\n' must be in set");
            Assert.IsTrue(ws.Contains('\r'), "'\\r' must be in set");
            Assert.IsFalse(ws.Contains('a'), "'a' must not be in set");
        }

        [Test]
        public void TestNegation()
        {
            ParserCharSet ws = new ParserCharSet(' ', '\r', '\n', '\t');
            ParserCharSet notWS = ParserCharSet.AnyChar - ws;
            Assert.IsFalse(notWS.Contains(' '), "' ' must not be in set");
            Assert.IsFalse(notWS.Contains('\t'), "'\\t' must not be in set");
            Assert.IsFalse(notWS.Contains('\n'), "'\\n' must not be in set");
            Assert.IsFalse(notWS.Contains('\r'), "'\\r' must not be in set");
            Assert.IsTrue(notWS.Contains('a'), "'a' must be in set");
        }

        [Test]
        public void TestRange()
        {
            ParserCharSet letter = ParserCharSet.FromRange('A', 'Z');
            Assert.IsTrue(letter.Contains('A'), "'A' must be in set");
            Assert.IsTrue(letter.Contains('Z'), "'Z' must be in set");
            Assert.IsTrue(letter.Contains('K'), "'K' must be in set");
            Assert.IsFalse(letter.Contains('a'), "'a' must not be in set");
        }

        [Test]
        public void TestUnion()
        {
            ParserCharSet upperCaseLetter = ParserCharSet.FromRange('A', 'Z');
            ParserCharSet lowerCaseLetter = ParserCharSet.FromRange('a', 'z');
            ParserCharSet letter = upperCaseLetter + lowerCaseLetter;
            Assert.IsTrue(letter.Contains('A'), "'A' must be in set");
            Assert.IsTrue(letter.Contains('Z'), "'Z' must be in set");
            Assert.IsTrue(letter.Contains('K'), "'K' must be in set");
            Assert.IsTrue(letter.Contains('a'), "'a' must be in set");
            Assert.IsTrue(letter.Contains('z'), "'z' must be in set");
            Assert.IsTrue(letter.Contains('m'), "'m' must be in set");
            Assert.IsFalse(letter.Contains('0'), "'0' must not be in set");
        }

        [Test]
        public void TestSubtraction()
        {
            ParserCharSet upperCaseLetter = ParserCharSet.FromRange('A', 'Z');
            ParserCharSet hexUpperLeter = ParserCharSet.FromRange('A', 'F');
            ParserCharSet notHex = upperCaseLetter - hexUpperLeter;
            Assert.IsFalse(notHex.Contains('A'), "'A' must not be in set");
            Assert.IsTrue(notHex.Contains('Z'), "'Z' must be in set");
            Assert.IsFalse(notHex.Contains('C'), "'C' must not be in set");
            Assert.IsFalse(notHex.Contains('F'), "'F' must not be in set");
            Assert.IsTrue(notHex.Contains('G'), "'G' must be in set");
            Assert.IsFalse(notHex.Contains('0'), "'0' must not be in set");
            Assert.IsTrue(notHex.Contains('M'), "'M' must be in set");
        }

        [Test]
        public void TestCategoryContains()
        {
            ParserCharSet upperCase = ParserCharSet.FromUnicodeCategory("Lu");
            Assert.IsTrue(upperCase.Contains('A'), "'A' must be in set");
            Assert.IsTrue(upperCase.Contains('Z'), "'Z' must be in set");
            Assert.IsTrue(upperCase.Contains('K'), "'K' must be in set");
            Assert.IsTrue(upperCase.Contains('\u0410'), "'\u0410' must be in set"); // Cyrillic A
            Assert.IsTrue(upperCase.Contains('\u042F'), "'\u042F' must be in set"); // Cyrillic YA
            Assert.IsFalse(upperCase.Contains('c'), "'c' must not be in set");
            Assert.IsFalse(upperCase.Contains('0'), "'0' must not be in set");
            Assert.IsFalse(upperCase.Contains('\u0430'), "'\u0430' must not be in set"); // Cyrillic a
            Assert.IsFalse(upperCase.Contains('\u044F'), "'\u043F' must not be in set"); // Cyrillic ya
        }

        [Test]
        public void TestCategoryContainsAnyLetter()
        {
            ParserCharSet letter = ParserCharSet.FromUnicodeCategory("L");
            Assert.IsTrue(letter.Contains('A'), "'A' must be in set");
            Assert.IsTrue(letter.Contains('Z'), "'Z' must be in set");
            Assert.IsTrue(letter.Contains('K'), "'K' must be in set");
            Assert.IsTrue(letter.Contains('\u0410'), "'\u0410' must be in set"); // Cyrillic A
            Assert.IsTrue(letter.Contains('\u042F'), "'\u042F' must be in set"); // Cyrillic YA
            Assert.IsTrue(letter.Contains('c'), "'c' must be in set");
            Assert.IsFalse(letter.Contains('0'), "'0' must not be in set");
            Assert.IsTrue(letter.Contains('\u0430'), "'\u0430' must be in set"); // Cyrillic a
            Assert.IsTrue(letter.Contains('\u044F'), "'\u043F' must be in set"); // Cyrillic ya
        }

        [Test]
        public void TestCategoryUnion()
        {
            ParserCharSet upper = ParserCharSet.FromUnicodeCategory("Lu");
            ParserCharSet digit = ParserCharSet.FromUnicodeCategory("Nd");
            ParserCharSet upperLetterOrDigit = upper + digit;
            Assert.IsTrue(upperLetterOrDigit.Contains('A'), "'A' must be in set");
            Assert.IsTrue(upperLetterOrDigit.Contains('Z'), "'Z' must be in set");
            Assert.IsTrue(upperLetterOrDigit.Contains('\u0410'), "'\u0410' must be in set"); // Cyrillic A
            Assert.IsTrue(upperLetterOrDigit.Contains('5'), "'5' must be in set");
            Assert.IsFalse(upperLetterOrDigit.Contains('c'), "'c' must not be in set");
            Assert.IsFalse(upperLetterOrDigit.Contains('\u044F'), "'\u043F' must not be in set"); // Cyrillic ya
        }

        [Test]
        public void TestCategoryAsciiUnion()
        {
            ParserCharSet upper = ParserCharSet.FromUnicodeCategory("Lu");
            ParserCharSet digit = new ParserCharSet('0', '1', '2');
            ParserCharSet upperLetterOrDigit = upper + digit;
            Assert.IsTrue(upperLetterOrDigit.Contains('A'), "'A' must be in set");
            Assert.IsTrue(upperLetterOrDigit.Contains('Z'), "'Z' must be in set");
            Assert.IsTrue(upperLetterOrDigit.Contains('\u0410'), "'\u0410' must be in set"); // Cyrillic A
            Assert.IsTrue(upperLetterOrDigit.Contains('1'), "'1' must be in set");
            Assert.IsFalse(upperLetterOrDigit.Contains('5'), "'5' must not be in set");
            Assert.IsFalse(upperLetterOrDigit.Contains('c'), "'c' must not be in set");
            Assert.IsFalse(upperLetterOrDigit.Contains('\u044F'), "'\u043F' must not be in set"); // Cyrillic ya
        }

        [Test]
        public void TestCategorySubtraction()
        {
            ParserCharSet anyLetter = ParserCharSet.FromUnicodeCategory("L");
            ParserCharSet upperLeter = ParserCharSet.FromUnicodeCategory("Lu");
            ParserCharSet anyButUpper = anyLetter - upperLeter;
            Assert.IsFalse(anyButUpper.Contains('A'), "'A' must not be in set");
            Assert.IsTrue(anyButUpper.Contains('a'), "'a' must be in set");
            Assert.IsFalse(anyButUpper.Contains('\u0410'), "'\u0410' must not be in set"); // Cyrillic A
            Assert.IsTrue(anyButUpper.Contains('\u044F'), "'\u043F' must be in set"); // Cyrillic ya
            Assert.IsFalse(anyButUpper.Contains('0'), "'0' must not be in set");
        }

        [Test]
        public void TestRangeContains()
        {
            ParserCharSet upperBasicRussian = ParserCharSet.FromRange('\u0410', '\u042F');
            Assert.IsTrue(upperBasicRussian.Contains('\u0410'), "'\u0410' must be in set"); // Cyrillic Capital Letter A
            Assert.IsTrue(upperBasicRussian.Contains('\u042F'), "'\u042F' must be in set"); // Cyrillic Capital Letter YA
            Assert.IsFalse(upperBasicRussian.Contains('\u0430'), "'\u0430' must not be in set"); // Cyrillic Small Letter a
            Assert.IsFalse(upperBasicRussian.Contains('A'), "'A' must not be in set");
            Assert.IsFalse(upperBasicRussian.Contains('z'), "'z' must not be in set");
        }

        [Test]
        public void TestRangeUnion()
        {
            ParserCharSet upperBasicRussian = ParserCharSet.FromRange('\u0410', '\u042F');
            ParserCharSet lowerBasicRussian = ParserCharSet.FromRange('\u0430', '\u044F');
            ParserCharSet basicRussian = upperBasicRussian + lowerBasicRussian;
            Assert.IsTrue(basicRussian.Contains('\u0410'), "'\u0410' must be in set"); // Cyrillic Capital Letter A
            Assert.IsTrue(basicRussian.Contains('\u042F'), "'\u042F' must be in set"); // Cyrillic Capital Letter YA
            Assert.IsTrue(basicRussian.Contains('\u0430'), "'\u0430' must be in set"); // Cyrillic Small Letter a
            Assert.IsTrue(basicRussian.Contains('\u044E'), "'\u044e' must be in set"); // Cyrillic Small Letter yu
            Assert.IsFalse(basicRussian.Contains('A'), "'A' must not be in set");
            Assert.IsFalse(basicRussian.Contains('z'), "'z' must not be in set");
        }

        [Test]
        public void TestRangeSubtraction()
        {
            ParserCharSet upperBasicRussian = ParserCharSet.FromRange('\u0410', '\u042F');
            // set of russian upper case vowels               A         E         I          O         U       YERU       E         YU        YA
            ParserCharSet upperRussianVowels = new ParserCharSet('\u0410', '\u0415', '\u0418', '\u041E', '\u0423', '\u042B', '\u042D', '\u042E', '\u042F');
            ParserCharSet upperRussianConsonats = upperBasicRussian - upperRussianVowels;
            Assert.IsFalse(upperRussianConsonats.Contains('\u0410'), "'\u0410' must not be in set"); // Cyrillic Capital Letter A
            Assert.IsTrue(upperRussianConsonats.Contains('\u0411'), "'\u0411' must be in set"); // Cyrillic Capital Letter BE
            Assert.IsTrue(upperRussianConsonats.Contains('\u041A'), "'\u041A' must be in set"); // Cyrillic Capital Letter KA
            Assert.IsFalse(upperRussianConsonats.Contains('\u042F'), "'\u042F' must not be in set"); // Cyrillic Capital Letter YA
            Assert.IsFalse(upperRussianConsonats.Contains('\u041E'), "'\u041E' must not be in set"); // Cyrillic Capital Letter O
            Assert.IsFalse(upperRussianConsonats.Contains('\u0430'), "'\u0430' must not be in set"); // Cyrillic Small Letter a
            Assert.IsFalse(upperRussianConsonats.Contains('\u044E'), "'\u044e' must not be in set"); // Cyrillic Small Letter yu
            Assert.IsFalse(upperRussianConsonats.Contains('A'), "'A' must not be in set");
            Assert.IsFalse(upperRussianConsonats.Contains('z'), "'z' must not be in set");
        }

        //TODO: create tests for complex cases with ranges + categories
    }
}
