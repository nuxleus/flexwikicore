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

namespace FlexWiki.UnitTests.Security
{
    internal class TestIdentity
    {
        private string _name;
        private readonly List<string> _roles = new List<string>(); 

        /// <summary>
        /// Creates an anonymous identity
        /// </summary>
        internal TestIdentity()
        {
        }

        /// <summary>
        /// Creates an authenticated identity.
        /// </summary>
        internal TestIdentity(string name, params string[] roles)
        {
            if (name == null)
            {
                throw new ArgumentException("name cannot be null", "name"); 
            }

            _name = name;
            _roles.AddRange(roles); 
        }

        internal bool IsAuthenticated
        {
            get { return _name != null; }
        }

        internal string Name
        {
            get { return _name; }
        }

        internal IList<string> Roles
        {
            get { return _roles; }
        }
    }
}
