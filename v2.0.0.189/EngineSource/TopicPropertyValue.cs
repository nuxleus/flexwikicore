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
    public class TopicPropertyValue
    {
        public TopicPropertyValue()
        {
        }

        public TopicPropertyValue(string value)
        {
            _value = value; 
        }

        private string _value;

        public string RawValue
        {
            get
            {
                return _value; 
            }
        }

        //CA Can't decide if this should be a method or a propertyName
        public IList<string> AsList()
        {
            return TopicParser.SplitTopicPropertyValue(_value); 
        }
    }
}
