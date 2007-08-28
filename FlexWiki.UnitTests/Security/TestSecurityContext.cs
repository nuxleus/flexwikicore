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
using System.Security.Principal;
using System.Threading; 

namespace FlexWiki.UnitTests.Security
{
    internal class TestSecurityContext : IDisposable
    {
        private IPrincipal _oldPrincipal; 

        internal TestSecurityContext(string user, params string[] roles)
        {
            _oldPrincipal = Thread.CurrentPrincipal;
            IIdentity newIdentity;
            if (user == null)
            {
                newIdentity = new TestAnonymousIdentity(); 
            }
            else 
            {
                newIdentity = new GenericIdentity(user);
            }

            Thread.CurrentPrincipal = new GenericPrincipal(newIdentity, roles); 
        }

        void IDisposable.Dispose()
        {
            Thread.CurrentPrincipal = _oldPrincipal; 
        }
    }
}
