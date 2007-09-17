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
    public struct ParserInstruction
    {
        // Fields

        internal int _code;
        internal int _argument1;
        internal int _argument2;

        // Constructors

        public ParserInstruction(int code)
        {
            this._code = code;
            this._argument1 = 0;
            this._argument2 = 0;
        }

        public ParserInstruction(int code, int argument1)
        {
            this._code = code;
            this._argument1 = argument1;
            this._argument2 = 0;
        }

        public ParserInstruction(int code, int argument1, int argument2)
        {
            this._code = code;
            this._argument1 = argument1;
            this._argument2 = argument2;
        }

        // Methods

        public override bool Equals(object obj)
        {
            ParserInstruction other = (ParserInstruction)obj;
            return (this == other);
        }

        public override int GetHashCode()
        {
            return 1234567 ^ _code ^ _argument1 ^ _argument2;
        }

        public static bool operator ==(ParserInstruction i1, ParserInstruction i2)
        {
            return (i1._code == i2._code
                && i1._argument1 == i2._argument1
                && i1._argument2 == i2._argument2);
        }

        public static bool operator !=(ParserInstruction i1, ParserInstruction i2)
        {
            return (!(i1._code == i2._code));
        }
    }
}
