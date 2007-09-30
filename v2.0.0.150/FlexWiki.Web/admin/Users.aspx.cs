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
using System.Web;
using System.Web.Security; 
using System.Web.UI; 
using System.Web.UI.WebControls; 


namespace FlexWiki.Web.Admin
{
    public class Users : Page
    {
        protected GridView UserList; 

        public void Page_Load(object sender, EventArgs a)
        {
            UserList.DataSource = Membership.GetAllUsers();
            DataBind(); 
        }
    }
}