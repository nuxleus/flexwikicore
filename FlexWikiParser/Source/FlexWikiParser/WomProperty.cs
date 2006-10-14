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
using System.Xml;

namespace FlexWiki
{
    public class WomProperty
    {
        // Fields

        internal string _name;
        internal string _value;

        // Constructors

        public WomProperty(WomProperty other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            this._name = other._name;
            this._value = other._value;
        }

        public WomProperty(string name, string value)
        {
            this._name = WomNameTable.Instance.Add(name);
            this._value = value;
        }

        // Properties

        public string Name
        {
            get { return this._name; }
        }

        public string Value
        {
            get { return this._value; }
        }

        // Methods

        public override bool Equals(object obj)
        {
            WomProperty other = obj as WomProperty;
            if (other == null)
            {
                return false;
            }
            return ((object)this._name == (object)other._name && this._value == other._value);
        }

        public override int GetHashCode()
        {
            int hashCode = _name.GetHashCode();
            if (_value != null)
            {
                hashCode ^= _value.GetHashCode();
            }
            return hashCode;

        }

        public override string ToString()
        {
            return _name + "=" + (_value ?? "<null>");
        }

        public void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteAttributeString(_name, _value);
        }


    }
}
