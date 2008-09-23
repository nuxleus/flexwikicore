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
using System.Web.Security;

namespace FlexWiki.Web
{
    public class FlexWikiWebMembership : IMembership
    {
        public int RegisteredUsers
        {
            get { return Membership.GetAllUsers().Count; }
        }
        public int RegisteredUsersOnline
        {
            get { return Membership.GetNumberOfUsersOnline(); }
        }
        public bool PasswordRetrievalEnabled
        {
            get { return Membership.EnablePasswordRetrieval; }
        }
        public bool PasswordResetEnabled
        {
            get { return Membership.EnablePasswordReset; }
        }
    }
}
