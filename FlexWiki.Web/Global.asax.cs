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
using System.Configuration;
using System.Web;
using System.Web.SessionState;
using System.Web.Security;

using log4net;

using FlexWiki.Security; 

namespace FlexWiki.Web 
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : System.Web.HttpApplication
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		public Global()
		{
			InitializeComponent();
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
		{

		}
 
		protected void Session_Start(Object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(Object sender, EventArgs e)
		{
            // Because browsers don't automatically authenticate every request, we need
            // to force Windows authentication if the login page has told us that the 
            // user has logged in via Windows auth. 
            HttpCookie cookie = Request.Cookies[FlexWikiWebApplication.ForceWindowsAuthenticationCookieName]; 
            if (cookie != null && cookie.Value == "true")
            {
                if (string.IsNullOrEmpty(Request.ServerVariables["LOGON_USER"]))
                {
                    Response.Clear();
                    Response.StatusCode = 401;
                    Response.StatusDescription = "Unauthorized";
                    Response.Flush();
                    Response.End();
                }
            }
		}

		protected void Application_EndRequest(Object sender, EventArgs e)
		{
		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{
            
		}

		protected void Application_Error(Object sender, EventArgs e)
		{
            Exception ex = Server.GetLastError();

            if ((ex is HttpUnhandledException) && (ex.InnerException is FlexWikiAuthorizationException))
            {
                try
                {
                    LogManager.GetLogger("FlexWiki.Web").Warn("Access was denied " + ex.ToString());
                    Context.Items["LastError"] = ex.InnerException;
                    Server.Transfer("AccessDenied.aspx");
                }
                finally
                {
                    Server.ClearError(); 
                }
            }
            else if ((ex is HttpUnhandledException) && (ex.InnerException is TransportSecurityRequirementException))
            {
                try
                {
                    Context.Items["LastError"] = ex.InnerException;
                    Server.Transfer("HttpsRequired.aspx");
                }
                finally
                {
                    Server.ClearError(); 
                }
            }
            else
            {
                // We don't call ClearError because we still want the error page to appear
                LogManager.GetLogger("FlexWiki.Web").Error("Uncaught exception in the web application: " + ex.ToString());
            }

		}

		protected void Session_End(Object sender, EventArgs e)
		{

		}

		protected void Application_End(Object sender, EventArgs e)
		{

		}
			
		#region Web Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.components = new System.ComponentModel.Container();
		}
		#endregion
	}
}

