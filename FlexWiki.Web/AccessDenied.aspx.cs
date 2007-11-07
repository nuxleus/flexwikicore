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
using System.Web.SessionState;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;

using FlexWiki.Formatting;
using FlexWiki.Security;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for WebForm1.
    /// </summary>
    public class AccessDenied : BasePage
    {
        protected HyperLink LoginLink;
        protected Label Msg;
        protected HyperLink ReturnLink;

        private void Page_Load(object sender, System.EventArgs e)
        {
            FlexWikiAuthorizationException ex = Context.Items["LastError"] as FlexWikiAuthorizationException;

            if (ex != null)
            {
                Msg.Text = "Access was denied. The error message was: " + ex.Message;
            }
            else
            {
                Msg.Text = "Access was denied. No further information is available.";
            }

            LoginLink.NavigateUrl = TheLinkMaker.LinkToLogin("");
            ReturnLink.NavigateUrl = TheLinkMaker.LinkToTopic("");
            try
            {
                QualifiedTopicRevision revision = PageUtilities.GetTopicRevision(Federation);
                if (revision != null)
                {
                    LoginLink.NavigateUrl = TheLinkMaker.LinkToLogin(revision.DottedNameWithVersion);
                }
            }
            catch (Exception x)
            {
                // Swallow any exceptions: this is error handling code so we're not 
                // interested in blowing up while we try to publish a helpful page. 
                FlexWikiWebApplication.LogError(typeof(AccessDenied).ToString(),
                    "Error trying to figure out what page the user was on: " + x.ToString());
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
