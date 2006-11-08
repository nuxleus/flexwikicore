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
using NUnit.Framework;
using FlexWiki;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class WomNameTableTests
    {
        [Test]
        public void TestSingleton()
        {
            WomNameTable nameTable = WomNameTable.Instance;
            Assert.IsNotNull(nameTable);
            Assert.AreSame(nameTable, WomNameTable.Instance);
        }

        [Test]
        public void TestAddString()
        {
            // Use dynamic string to avoid Intern string effect.
            string s01 = GetDynamicString("StringAdd", 1);
            Assert.AreNotSame("StringAdd01", s01);
            Assert.AreEqual("StringAdd01", s01);

            // null argument
            ExtendedAssert.Exception(typeof(ArgumentNullException), delegate()
            {
                WomNameTable.Instance.Add(null);
            });

            // empty string
            string s02 = WomNameTable.Instance.Add(String.Empty);
            Assert.AreSame(String.Empty, s02);

            // non-empty string
            string s03 = WomNameTable.Instance.Add(GetDynamicString("StringAdd", 3).Substring(2));
            Assert.AreEqual("ringAdd03", s03);
            string s031 = WomNameTable.Instance.Add(GetDynamicString("StringAdd", 3).Substring(2));
            Assert.AreSame(s03, s031);
            string s032 = WomNameTable.Instance.Add(GetDynamicString("StringAdd", 3).Substring(1));
            Assert.AreNotSame(s03, s032);
        }

        [Test]
        public void TestGetString()
        {
            // null argument
            ExtendedAssert.Exception(typeof(ArgumentNullException), delegate()
            {
                WomNameTable.Instance.Get(null);
            });

            // empty string
            string s04 = WomNameTable.Instance.Get(String.Empty);
            Assert.AreSame(String.Empty, s04);

            // non-empty string
            string s05 = WomNameTable.Instance.Get(GetDynamicString("StringGet", 5).Substring(2));
            Assert.IsNull(s05);

            string s06 = WomNameTable.Instance.Add(GetDynamicString("StringGet", 6).Substring(2));
            string s061 = WomNameTable.Instance.Get(GetDynamicString("StringGet", 6).Substring(2));
            Assert.AreEqual(s06, s061);
        }

        [Test]
        public void TestAddCharArray()
        {
            // test arguments. 
            // We do not do any special for the arguments checking. We rely on CLR array checks.
            ExtendedAssert.Exception(typeof(ArgumentNullException), delegate()
            {
                WomNameTable.Instance.Add(null, 0, 1);
            });
            ExtendedAssert.Exception(typeof(IndexOutOfRangeException), delegate()
            {
                WomNameTable.Instance.Add(new char[1] { 'a' }, -1, 1);
            });
            ExtendedAssert.Exception(typeof(IndexOutOfRangeException), delegate()
            {
                WomNameTable.Instance.Add(new char[1] { 'a' }, 2, 1);
            });
            ExtendedAssert.Exception(typeof(IndexOutOfRangeException), delegate()
            {
                WomNameTable.Instance.Add(new char[1] { 'a' }, 1, 1);
            });
            ExtendedAssert.Exception(typeof(ArgumentOutOfRangeException), delegate()
            {
                WomNameTable.Instance.Add(new char[1] { 'a' }, 1, -1);
            });

            // empty string
            string s01 = WomNameTable.Instance.Add(new char[1] { 'a' }, 0, 0);
            Assert.AreSame(String.Empty, s01);

            // non-empty string
            string s02 = WomNameTable.Instance.Add(GetDynamicString("ArrayAdd", 2).ToCharArray(), 2, "ArrayAdd".Length);
            Assert.AreEqual("rayAdd02", s02);
            string s021 = WomNameTable.Instance.Add(GetDynamicString("ArrayAdd", 2).ToCharArray(), 2, "ArrayAdd".Length);
            Assert.AreSame(s02, s021);
            string s022 = WomNameTable.Instance.Add(GetDynamicString("ArrayAdd", 2).Substring(2, "ArrayAdd".Length));
            Assert.AreSame(s02, s022);
            string s023 = WomNameTable.Instance.Add(GetDynamicString("ArrayAdd", 2).ToCharArray(), 1, "ArrayAdd".Length);
            Assert.AreNotSame(s02, s023);
        }

        [Test]
        public void TestGetCharArray()
        {
            // test arguments. 
            // We do not do any special for the arguments checking. We rely on CLR array checks.
            ExtendedAssert.Exception(typeof(ArgumentNullException), delegate()
            {
                WomNameTable.Instance.Get(null, 0, 1);
            });
            ExtendedAssert.Exception(typeof(IndexOutOfRangeException), delegate()
            {
                WomNameTable.Instance.Get(new char[1] { 'a' }, -1, 1);
            });
            ExtendedAssert.Exception(typeof(IndexOutOfRangeException), delegate()
            {
                WomNameTable.Instance.Get(new char[1] { 'a' }, 2, 1);
            });
            ExtendedAssert.Exception(typeof(IndexOutOfRangeException), delegate()
            {
                WomNameTable.Instance.Get(new char[1] { 'a' }, 1, 1);
            });

            // empty string
            string s01 = WomNameTable.Instance.Get(new char[1] { 'a' }, 0, 0);
            Assert.AreSame(String.Empty, s01);

            // non-empty string
            string s02 = WomNameTable.Instance.Get(GetDynamicString("ArrayGet", 2).ToCharArray(), 2, "ArrayGet".Length);
            Assert.IsNull(s02);

            // non-empty string
            string s03 = WomNameTable.Instance.Add(GetDynamicString("ArrayGet", 3).ToCharArray(), 2, "ArrayGet".Length);
            string s031 = WomNameTable.Instance.Get(GetDynamicString("ArrayGet", 3).ToCharArray(), 2, "ArrayGet".Length);
            Assert.AreSame(s03, s031);
            string s032 = WomNameTable.Instance.Add(GetDynamicString("ArrayGet", 3).Substring(2, "ArrayGet".Length));
            Assert.AreSame(s03, s032);
        }

        // This function is needed to avoid Intern string effect.
        private string GetDynamicString(string prefix, int suffix)
        {
            return prefix + String.Format("{0:D2}", suffix);
        }
    }
}
