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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using FlexWiki.Collections;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for QuickLink.
    /// </summary>
    public class QuickLink : BasePage
    {
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

        protected void DoSearch()
        {
            string defaultNamespace = Request.QueryString["QuickLinkNamespace"];

            LinkMaker lm = TheLinkMaker;

            string topic = Request.Form["QuickLink"];
            QualifiedTopicNameCollection hits = Federation.AllQualifiedTopicNamesThatExist(new TopicName(topic), defaultNamespace);

            string target = null;
            if (hits.Count == 0)
            {
                // No hits, create it in the default namespace, or the namespace that was specified
                TopicName topicName = new TopicName(topic);
                QualifiedTopicName qualifiedTopicName = topicName.ResolveRelativeTo(defaultNamespace);
                target = lm.LinkToEditTopic(qualifiedTopicName);
            }
            else if (hits.Count == 1)
            {
                // 1 hit; take it!
                NameValueCollection extras = new NameValueCollection();
                extras.Add("DelayRedirect", "1");
                target = lm.LinkToTopic(new QualifiedTopicRevision(hits[0]), false, extras);
            }

            // If we have a target, go there
            if (target != null)
            {
                // Response.Write(target);
                Response.Redirect(target);
                return;
            }

            // Uh, oh -- we're here because the name is ambiguous
            Response.Write(@"
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"" >
<HTML>
    <HEAD>
        <title id='title'>Topic is ambiguous</title>
        <LINK href='<%= RootUrl(Request) %>wiki.css' type='text/css' rel='stylesheet'>
    </HEAD>
	<p>The topic name you selected is ambiguous because it already exists in multiple namespaces.  Please select the one you want:
<ul>");
            foreach (TopicName each in hits)
            {
                Response.Write("<li><a href='" + lm.LinkToTopic(each) + "'>" + each.DottedName + "</a></li>");
            }
            Response.Write(@"
</ul>
    </body>
</HTML>
");
        }
    }
}
