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

namespace FlexWiki.Security
{
    public class FlexWikiAuthorizationException : Exception
    {
        private SecurableAction _action;
        private string _name; 
        private AuthorizationRuleScope _scope; 

        public FlexWikiAuthorizationException(SecurableAction action, AuthorizationRuleScope scope, string name)
        {
            _action = action;
            _scope = scope;
            _name = name; 
        }

        public SecurableAction Action
        {
            get { return _action; }
        }
        
        public override string Message
        {
            get
            {
                return string.Format("Permission to {0} {1} {2} is denied.",
                    _action, _scope, _name); 
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public AuthorizationRuleScope Scope
        {
            get { return _scope; }
        }
    }
}
