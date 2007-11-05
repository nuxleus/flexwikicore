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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FlexWiki.Web.Newsletters;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for RssFace.
	/// </summary>
	public class RssFace : BasePage
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

		protected void ShowPage()
		{
            using (RequestContext.Create())
            {
                StringBuilder strbldr = new StringBuilder();
                string ns = Request.QueryString["namespace"];
                strbldr.AppendLine("<fieldset><legend class=\"DialogTitle\">Subscriptions</legend>");
                strbldr.AppendLine("<p>Listed below are the available <b>newsletters</b> and <b>RSS</b> subscriptions.");
                if (ns == null)
                {
                    strbldr.AppendLine(" All subscriptions available in this Wiki federation are listed.");
                }
                else
                {
                    strbldr.AppendLine(" Only the subscriptions for <i>" + EscapeHTML(ns) + "</i> are listed.");
                    strbldr.AppendLine("	You can also view <a href=\"" + Request.Path + "\">all subscriptions on this site</a>.");
                }

                strbldr.AppendLine("</p>");

                strbldr.AppendLine("<p>There are three ways to subscribe to change notifications for this site:</p>");

                strbldr.AppendLine("<ul>");
                strbldr.AppendLine("<li><b>Newsletters (email and RSS).</b>  Newsletters provide notification of changes to a set of related topics.");
                strbldr.AppendLine("To recieve newsletter updates by email, visit the newsletter topic and add your name to the Subscribers property.");
                strbldr.AppendLine("To subscribe the the RSS feed for the newsletter directly, use the listed link below.</li>");
                strbldr.AppendLine("<li><b>Namespace feeds (RSS).</b>  You can subscribe to RSS feeds for all of the topics in this namespace.</li>");
                strbldr.AppendLine("</ul>");

                strbldr.AppendLine(ShowNewsletters(ns));
                strbldr.AppendLine(ShowNamespaceFeeds(ns));

                strbldr.AppendLine("</fieldset>");
                Response.Write(strbldr.ToString());
            }
		}


		string ShowNewsletters(string ns)
		{
            StringBuilder strbldr = new StringBuilder();
            strbldr.AppendLine("<h1>Newsletters</h1>");
			
			NewsletterManager nm = new NewsletterManager(Federation, TheLinkMaker);

            Dictionary<string, List<QualifiedTopicName>> newsletterNamespaceMap = new Dictionary<string, List<QualifiedTopicName>>();
			foreach (QualifiedTopicName newsletterName in nm.GetAllNewsletterNames(ns))
			{
                List<QualifiedTopicName> topicsInThisNamespace = null;
                if (newsletterNamespaceMap.ContainsKey(newsletterName.Namespace))
                {
                    topicsInThisNamespace = newsletterNamespaceMap[newsletterName.Namespace];
                }
				else
				{
					topicsInThisNamespace = new List<QualifiedTopicName>();
					newsletterNamespaceMap[newsletterName.Namespace] = topicsInThisNamespace;
				}
				topicsInThisNamespace.Add(newsletterName);
			}

			List<string> bases = new List<string>();
			bases.AddRange(newsletterNamespaceMap.Keys);
			bases.Sort();

            if (bases.Count > 0)
            {
                strbldr.AppendLine("<table border=\"0\" cellspacing=\"6\" cellpadding=\"0\">");
            }
			foreach (string each in bases)
			{
				NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(each);
                if (ns == null)
                {
                    strbldr.AppendLine("<tr><td colspan=\"2\"><div class=\"SubscriptionNamespace\">" + EscapeHTML(storeManager.FriendlyTitle) + "</div></td></tr>");
                }
				foreach (QualifiedTopicName abs in newsletterNamespaceMap[each])
				{
					TopicVersionInfo info = Federation.GetTopicInfo(abs.ToString());
					string desc = info.GetProperty("Description");
					strbldr.AppendLine("<tr>");
                    strbldr.AppendLine("<td><a class=\"standardsButton\" href=\"" + RootUrl + "Rss.aspx?newsletter=" + abs.ToString() + "\">rss</a></td>");
                    strbldr.AppendLine("<td><a href=\"" + TheLinkMaker.LinkToTopic(abs) + "\">" + abs.LocalName + "</a></td>");
                    strbldr.AppendLine("</tr>");
                    strbldr.AppendLine("<tr>");
                    strbldr.AppendLine("<td></td>");
                    strbldr.AppendLine("<td><span style=\"font-size: x-small; color: gray\">" + (desc == null ? "" : desc) + "</span></td>");
                    strbldr.AppendLine("</tr>");
                    strbldr.AppendLine("<tr>");
				}
			}
            if (bases.Count > 0)
            {
                strbldr.AppendLine("</table>");
            }
            return strbldr.ToString();
		}

		string ShowNamespaceFeeds(string ns)
		{
            StringBuilder strbldr = new StringBuilder();

            strbldr.AppendLine("<h1>Namespace Feeds</h1>");
			List<NamespaceManager> bases = new List<NamespaceManager>();
			foreach (string each in Federation.Namespaces)
			{
                if (ns != null && ns != each)
                {
                    continue;
                }
				NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(each);
				bases.Add(storeManager);
			}
			bases.Sort();

            strbldr.AppendLine("<table border=\"0\" cellspacing=\"6\" cellpadding=\"0\">");
            foreach (NamespaceManager storeManager in bases)
			{
                if (ns == null)
                {
                    strbldr.AppendLine("<tr><td colspan=\"2\"><div class=\"SubscriptionNamespace\">" + EscapeHTML(storeManager.FriendlyTitle) + "</div></td></tr>");
                }
				strbldr.AppendLine("<tr>");
                strbldr.AppendLine("<td><a class=\"standardsButton\" href=\"" + RootUrl + "Rss.aspx?namespace=" + storeManager.Namespace + "\">rss</a></td>");
                strbldr.AppendLine("<td>Only this namespace (<a href=\"" + TheLinkMaker.LinkToTopic(new QualifiedTopicRevision(storeManager.Namespace + "." + storeManager.HomePage)) + "\">" + storeManager.FriendlyTitle + "</a>)</td>");
                strbldr.AppendLine("</tr>");
                strbldr.AppendLine("<tr>");
                strbldr.AppendLine("<td><a class=\"standardsButton\" href=\"" + RootUrl + "Rss.aspx?namespace=" + storeManager.Namespace + "&amp;inherited=y\">rss</a></td>");
                strbldr.AppendLine("<td>This namespace and related namespaces (<a href=\"" + TheLinkMaker.LinkToTopic(new QualifiedTopicRevision(storeManager.Namespace + "." + storeManager.HomePage)) + "\">" + storeManager.FriendlyTitle + "</a>");
                foreach (NamespaceManager import in storeManager.ImportedNamespaceManagers)
                {
                    strbldr.AppendLine(", <a href=\"" + TheLinkMaker.LinkToTopic(new QualifiedTopicRevision(import.Namespace + "." + import.HomePage)) + "\">" + import.FriendlyTitle + "</a>");
                }
                strbldr.AppendLine(")</td></tr>");
			}
            strbldr.AppendLine("</table>");
            return strbldr.ToString();
        }
	}
}
