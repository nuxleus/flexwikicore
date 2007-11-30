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

namespace FlexWiki.Web.Admin
{
    /// <summary>
    /// Summary description for Admin.
    /// </summary>
    public class ManageConfiguration : AdminPage
    {

        protected override void DefaultPageLoad()
        {
            MinimalPageLoad();
        }

        protected override void ShowMain()
        {
            UIResponse.WriteLine("Please use one of the menu items to the left."); 
        }

        protected override void ShowMenu()
        {
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
