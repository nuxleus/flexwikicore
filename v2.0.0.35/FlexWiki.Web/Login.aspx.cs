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
using System.Collections;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration; 
using System.Web.SessionState;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for WebForm1.
	/// </summary>
	public partial class Login : BasePage
	{
        protected System.Web.UI.WebControls.Login Login1;
        protected System.Web.UI.WebControls.HyperLink ReturnLink; 

        protected void Login1_LoggedIn(object sender, EventArgs a)
        {
            FlexWikiWebApplication.LogInfo(typeof(Login).ToString(), "Forms user logged in: " +
                Login1.UserName); 
        }
        protected void Login1_LoginError(object sender, EventArgs a)
        {
            // There doesn't seem to be an easy way to figure out what exactly went wrong
            FlexWikiWebApplication.LogWarning(typeof(Login).ToString(), "Forms user was unable to log in: " +
                Login1.UserName); 
        }
		private void Page_Load(object sender, System.EventArgs e)
		{
            AuthenticationSection section = 
                WebConfigurationManager.GetWebApplicationSection("system.web/authentication") as AuthenticationSection;

            if (section != null)
            {
                // If we're set up for Windows authentication, let the web server and the 
                // browser handle it. Otherwise we'll let page execution continue, which 
                // uses Forms authentication. 
                if (section.Mode == AuthenticationMode.Windows)
                {
                    string user = Request.ServerVariables["LOGON_USER"];
                    if (string.IsNullOrEmpty(user))
                    {
                        Response.Clear(); 
                        Response.StatusCode = 401;
                        Response.StatusDescription = "Unauthorized";
                        Response.End();
                    }
                    else
                    {
                        FlexWikiWebApplication.LogInfo(typeof(Login).ToString(), "Windows user logged in: " +
                            user);
                        Login1.Visible = false;
                        ReturnLink.Visible = true;
                        ReturnLink.NavigateUrl = Request.QueryString["ReturnURL"];
                        ReturnLink.Text = string.Format("You have been logged in as {0}. Click here to return to FlexWiki.",
                            user); 

                        //Response.Redirect(Request.QueryString["ReturnURL"]); 
                    }
                }
            }
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

	}
}
