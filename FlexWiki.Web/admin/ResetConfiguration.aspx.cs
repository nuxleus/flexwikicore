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
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using FlexWiki;
using FlexWiki.Web;
using System.IO;

namespace FlexWiki.Web.Admin
{
    /// <summary>
    /// Summary description for Admin.
    /// </summary>
    public class ResetConfiguration : AdminPage
    {
        protected override void DefaultPageLoad()
        {
            MinimalPageLoad();
        }

        protected override void ShowMain()
        {
            if (Request.HttpMethod.Equals("POST"))
            {
                if ("1".Equals(Request.QueryString["reset"]))
                {
                    File.Copy(FlexWikiWebApplication.ApplicationConfigurationPath, 
                        FlexWikiWebApplication.ApplicationConfigurationPath 
                        + "." 
                        + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") 
                        + ".backup");
                    File.Copy(FlexWikiWebApplication.ApplicationConfigurationPath + ".template",
                        FlexWikiWebApplication.ApplicationConfigurationPath, true);

                    ((FlexWikiWebApplication)Federation.Application).ReloadConfiguration();

                    UIResponse.WritePara("The configuration has been reset."); 
                }
            }

            UIResponse.WritePara(@"Pressing the button below will copy your existing flexwiki.config file to 
flexwiki.config.datetime.backup, and will copy flexwiki.config.template to flexwiki.config. This will 
have the effect of resetting your configuration to the default, including REMOVING ANY NAMESPACE PROVIDERS
YOU HAVE SET UP. Only press this button if you are sure you want to reset to the default configuration.");

            UIResponse.Write("<form action='?reset=1' method='post'><input type='submit' value='Reset Configuration To Default' /></form>"); 
        }

        protected override void ShowMenu()
        {
            UIResponse.WriteStartMenu("Manage Configuration");
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
