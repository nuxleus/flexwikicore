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

namespace FlexWiki.Web
{
	public partial class Logoff : BasePage
	{
        protected Label LogOffMessage;
        protected HyperLink ReturnLink; 

		private void Page_Load(object sender, System.EventArgs e)
		{
            AuthenticationSection section =
                WebConfigurationManager.GetWebApplicationSection("system.web/authentication") as AuthenticationSection;

            if (section != null)
            {
                // While we can log off just fine if we're using forms authentication, there's not 
                // much we can do if we're using Windows authentication or something else. 
                if (section.Mode == AuthenticationMode.Forms)
                {
                    FormsAuthentication.SignOut();
                    LogOffMessage.Text = Context.User.Identity.Name + " has logged off";
                }
                else if (section.Mode == AuthenticationMode.Windows)
                {
                    HttpCookie cookie = Response.Cookies[FlexWikiWebApplication.ForceWindowsAuthenticationCookieName];
                    if (cookie != null)
                    {
                        cookie.Value = ""; 
                    }
                    
                    // Attempt to close at least one of the connections, as that should force logoff to occur on that
                    // connection. Other connections may remain logged in, however. 
                    Response.AppendHeader("Connection", "Close"); 

                    LogOffMessage.Text = @"It is not possible to completely log off when using Windows authentication, 
which this site is currently configured to use. 
When you return to the wiki, you might still show up as logged in, or you might intermittantly show up as logged in/out. 
Closing your browser will complete the logoff process."; 
                }
                else 
                {
                    LogOffMessage.Text = "Unable to explicitly log off when authentication mode is " + section.Mode.ToString();
                }
            }
            else
            {
                LogOffMessage.Text = "Unable to log off when no authentication mechanism is defined."; 
            }

            string returnUrl = Request.QueryString["ReturnURL"]; 

            if (string.IsNullOrEmpty(returnUrl))
            {
                ReturnLink.Visible = false; 
            }
            else
            {
                ReturnLink.NavigateUrl = returnUrl; 
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
