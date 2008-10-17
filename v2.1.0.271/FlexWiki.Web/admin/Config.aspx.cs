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
using System.Web.SessionState;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;
using FlexWiki.Web;
using FlexWiki;

namespace FlexWiki.Web.Admin
{
    /// <summary>
    /// Summary description for Config.
    /// </summary>
    public class Config : AdminPage
    {

        protected override void EnsurePluginsLoaded()
        {
            // Don't load plugins for the checker 
        }

        protected override void PageLoad()
        {
        }

        protected override void ShowMain()
        {
            if (CheckForConfigurationFormatUpgrade())
                return;

            ConfigurationChecker checker = new ConfigurationChecker();

            checker.Check();
            checker.WriteStoplightTo(UIResponse);
            checker.WriteTo(UIResponse);
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
