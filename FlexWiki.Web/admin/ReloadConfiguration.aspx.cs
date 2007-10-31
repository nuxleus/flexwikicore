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
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FlexWiki.Web;

namespace FlexWiki.Web.Admin
{
	public class ReloadConfiguration : AdminPage
	{
		
		protected override void ShowMain()
		{
            string reload = Request.QueryString["reload"];
            if (reload == "1")
            {
                ((FlexWikiWebApplication)Federation.Application).ReloadConfiguration();
                UIResponse.WritePara("The configuration file has been reloaded.");
            }
            else
            {
                UIResponse.WritePara("Use the menu to the left to reload the configuration file from disk.");
                UIResponse.WritePara("This is necessary, for example, when you have modified the configuration file manually, " + 
                    "and have not restarted the web application. It should never be necessary when making changes via these " + 
                    "administrative web pages."); 
            }

		}
		
		protected override void ShowMenu()
		{

            UIResponse.WriteStartMenu("Confirm Reset");
            UIResponse.WriteMenuItem("ReloadConfiguration.aspx?reload=1", "Reload", "Reread the FlexWiki configuration file from disk.");
            UIResponse.WriteEndMenu();
            UIResponse.WritePara("&nbsp;");

            UIResponse.WriteStartMenu("ManageConfiguration");
            UIResponse.WriteMenuItem("Config.aspx", "Validate Configuration", "Validate that the configuration is correct.");
            UIResponse.WriteMenuItem("EditConfiguration.aspx", "Edit Configuration", "Edit the flexwiki.config file directly.");
            UIResponse.WriteMenuItem("ReloadConfiguration.aspx", "Reread Configuration", "Reread the FlexWiki configuration file from disk.");
            UIResponse.WriteMenuItem("ResetConfiguration.aspx", "Reset Configuration", "Reset the flexwiki.config file to its default.");
            UIResponse.WriteEndMenu();
            UIResponse.WritePara("&nbsp;");

			base.ShowMenu();
		}
		private void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
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
