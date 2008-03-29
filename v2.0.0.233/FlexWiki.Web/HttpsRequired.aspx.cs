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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using FlexWiki.Formatting;
using FlexWiki.Security; 

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for WebForm1.
	/// </summary>
	public class HttpsRequired : BasePage
	{
        protected HtmlGenericControl MetaRefresh; 
        protected Label Msg; 
        protected HyperLink ReturnLink; 

		private void Page_Load(object sender, System.EventArgs e)
		{
            try
            {
                string originalUrl = Request.Url.ToString(); 
                string secureUrl = originalUrl.Substring("http:".Length).Insert(0, "https:");
                MetaRefresh.Attributes["content"] = "5; url=" + secureUrl; 
                ReturnLink.NavigateUrl = secureUrl;
            }
            catch (Exception x)
            {
                // Swallow any exceptions: this is error handling code so we're not 
                // interested in blowing up while we try to publish a helpful page. 
                FlexWikiWebApplication.LogError(typeof(HttpsRequired).ToString(),
                    "Error trying to build URL for redirection: " + x.ToString()); 
                
                Msg.Text = "There was an error trying to construct the link to the secure portion of the website. Please retry your request, using 'https://' at the beginning of the address rather than 'http://'. The error has been logged for administrative staff.";
                ReturnLink.Visible = false;                 
            }

		}

		#region Web Form Designer generated code
		protected override void OnInit(EventArgs e)
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
