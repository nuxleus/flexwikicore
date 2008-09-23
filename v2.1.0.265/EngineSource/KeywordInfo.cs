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

namespace FlexWiki
{
    /// <summary>
    /// Summary description for KeywordInfo.
    /// </summary>
    [ExposedClass("KeywordInfo", "Provides an instance of keyword info for the namespace")]
    public class KeywordInfo : BELArray, IComparable
    {
        private string _name;
        private int _count = 0;

        public void KeywordName(string name)
        {
            _name = name;
        }
        public void IncrementKeywordCount()
        {
            _count++;
        }

        [ExposedMethod("Name", ExposedMethodFlags.Default, "The name of the keyword")]
        public string ExposedName
        {
            get { return _name; }
        }
        [ExposedMethod("References", ExposedMethodFlags.Default, "The count of references for the keyword")]
        public int ExposedCount
        {
            get { return _count; }
        }

        public int CompareTo(object obj)
        {
            return this._name.CompareTo(((KeywordInfo)obj).ExposedName);
        }
    }
}
