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
    public static class ParserInstructionCode
    {
        // Fields

        public const int None = 0;
        public const int Success = 1;
        public const int Failure = 2;
        public const int MatchChar = 3;
        public const int MatchString = 4;
        public const int MatchCharSet = 5;
        public const int Choice = 6;
        public const int Goto = 7;
        public const int CallRule = 8;
        public const int RuleSuccess = 9;
        public const int RuleFailure = 10;
        public const int StartAttribute = 11;
        public const int EndAttribute = 12;
        public const int AttributeFailure = 13;
        public const int LineStart = 14;
        public const int MatchAny = 15;
        public const int LineEnd = 16;

        public const int Back = 512;

        // Methods

        public static string GetName(int code)
        {
            switch (code)
            {
                case None: return "None";
                case Success: return "Success";
                case Failure: return "Failure";
                case MatchChar: return "MatchChar";
                case MatchString: return "MatchString";
                case MatchCharSet: return "MatchCharSet";
                case Choice: return "Choice";
                case Goto: return "Goto";
                case CallRule: return "CallRule";
                case RuleSuccess: return "RuleSuccess";
                case RuleFailure: return "RuleFailure";
                case StartAttribute: return "StartAttribute";
                case EndAttribute: return "EndAttribute";
                case AttributeFailure: return "AttributeFailure";
                case LineStart: return "LineStart";
                case MatchAny: return "MatchAny";
                case LineEnd: return "LineEnd";
            }
            return "";
        }
    }
}
