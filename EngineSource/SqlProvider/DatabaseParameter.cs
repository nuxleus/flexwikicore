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
using System.Data; 

namespace FlexWiki.SqlProvider
{
    public class DatabaseParameter
    {
        private ParameterDirection _direction; 
        private string _name;
        private object _value; 

        public DatabaseParameter(string name, object value)
        {
            _name = name;
            _value = value; 
        }

        public ParameterDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public string Name
        {
            get { return _name; }
        }

        public object Value
        {
            get { return _value; }
            set
            {
                if (Direction == ParameterDirection.Input)
                {
                    throw new ReadOnlyException("Cannot change the value of an input parameter."); 
                }

                _value = value; 
            }
        }
    }
}
