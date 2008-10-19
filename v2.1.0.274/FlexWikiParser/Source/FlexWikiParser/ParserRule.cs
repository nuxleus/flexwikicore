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
    public class ParserRule
    {
        // Fields

        private string _name;
        private int _start;
        private string _elementName;
        private bool _reducible;

        // Constructors

        public ParserRule(string name)
        {
            _name = name;
        }

        // Properties

        public string ElementName
        {
            get { return this._elementName; }
            set { this._elementName = value; }
        }

        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        public bool Reducible
        {
            get { return this._reducible; }
            set { this._reducible = value; }
        }

        public int Start
        {
            get { return _start; }
            set { _start = value; }
        }

        // Methods

        public override string ToString()
        {
            return this._name;
        }
    }
}
