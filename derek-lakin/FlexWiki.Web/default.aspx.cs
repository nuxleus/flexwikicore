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
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using FlexWiki;
using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for WebForm1.
    /// </summary>
    public class Default2 : BasePage
    {
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

        protected string urlForDiffs;
        protected string urlForNoDiffs;

        private static bool IsAbsoluteURL(string pattern)
        {
            if (pattern.StartsWith(System.Uri.UriSchemeHttp + System.Uri.SchemeDelimiter))
                return true;
            if (pattern.StartsWith(System.Uri.UriSchemeHttps + System.Uri.SchemeDelimiter))
                return true;
            if (pattern.StartsWith(System.Uri.UriSchemeFile + System.Uri.UriSchemeFile))
                return true;
            return false;
        }

        private LogEvent MainEvent;
        protected void StartPage()
        {
            if (Federation.GetPerformanceCounter(PerformanceCounterNames.TopicReads) != null)
                Federation.GetPerformanceCounter(PerformanceCounterNames.TopicReads).Increment();

            MainEvent = Federation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, GetTopicVersionKey().ToString(), LogEventType.ReadTopic);
            VisitorEvent e = new VisitorEvent(GetTopicVersionKey(), VisitorEvent.Read, DateTime.Now);
            LogVisitorEvent(e);
        }

        protected void EndPage()
        {
            MainEvent.Record();
        }

        protected string GetTitle()
        {
            StringBuilder titlebldr = new StringBuilder();

            string title = Federation.GetTopicPropertyValue(GetTopicVersionKey(), "Title");
            if (String.IsNullOrEmpty(title))
            {
                title = GetTopicVersionKey().FormattedName;
            }

            return HtmlStringWriter.Escape(title);
        }

        protected string InitHead()
        {
            StringBuilder initbldr = new StringBuilder();

            initbldr.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            initbldr.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">");
            initbldr.AppendLine("<head>");
            string initHead = initbldr.ToString();

            return initHead;

        }

        protected string DoHead()
        {
            StringBuilder headbldr = new StringBuilder();
            QualifiedTopicRevision topic = GetTopicVersionKey();
            LinkMaker lm = TheLinkMaker;

            // Consider establishing a redirect if there's a redirect to a topic or an URL
            string redir = Federation.GetTopicPropertyValue(GetTopicVersionKey(), "Redirect");
            if (redir != "")
            {
                UriBuilder uri = null;
                if (IsAbsoluteURL(redir))
                {
                    uri = new UriBuilder(redir);
                }
                else
                {
                    // Must be a topic name
                    string trimmed = redir.Trim();
                    QualifiedTopicNameCollection all = Federation.NamespaceManagerForTopic(GetTopicVersionKey()).AllQualifiedTopicNamesThatExist(trimmed);

                    if (all.Count == 1)
                    {
                        uri = new UriBuilder(
                            new Uri(
                                new Uri(FullRootUrl(Request)),
                                lm.LinkToTopic(
                                    new QualifiedTopicRevision(all[0]),
                                    false,
                                    Request.QueryString)));
                    }
                    else
                    {
                        if (all.Count == 0)
                            headbldr.AppendLine("<!-- Redirect topic does not exist -->\n");
                        else
                            headbldr.AppendLine("<!-- Redirect topic is ambiguous -->\n");
                    }
                }
                if (uri != null)
                {
                    if (Request.QueryString["DelayRedirect"] == "1")
                    {
                        headbldr.AppendLine("<meta http-equiv=\"refresh\" content=\"10;URL=\"" + uri + "\" />");
                    }
                    else
                    {
                        Response.Redirect(uri.Uri.ToString());
                        return "";
                    }
                }
            }

            headbldr.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" />");

            string keywords = Federation.GetTopicPropertyValue(GetTopicVersionKey(), "Keywords");
            if (keywords != "")
            {
                headbldr.AppendLine("<meta name=\"keywords\" content=\"" + keywords + "\" />");
            }
            string description = Federation.GetTopicPropertyValue(GetTopicVersionKey(), "Summary");
            if (description != "")
            {
                headbldr.AppendLine("<meta name=\"description\" content=\"" + description + "\" />");
            }
            headbldr.AppendLine("<meta name=\"author\" content=\"" +
                Federation.GetTopicLastModifiedBy(GetTopicVersionKey().AsQualifiedTopicName()) +
                "\" />");

            if (GetTopicVersionKey().Version != null)
            {
                // Don't index the versions
                headbldr.AppendLine("<meta name=\"Robots\" content=\"NOINDEX, NOFOLLOW\" />");
            }

            headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}WikiDefault.js\" type=\"text/javascript\"></script>\r\n", RootUrl);
            headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}WikiTopicBar.js\" type=\"text/javascript\"></script>\r\n", RootUrl);
            headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}WikiMenu.js\" type=\"text/javascript\"></script>\r\n", RootUrl);
            headbldr.AppendLine("<script type=\"text/javascript\">");
            headbldr.AppendLine("function showChanges()");
            headbldr.AppendLine("{");
            headbldr.AppendLine("    nav(\"" + urlForDiffs + "\");");
            headbldr.AppendLine("}");
            headbldr.AppendLine();
            headbldr.AppendLine("function hideChanges()");
            headbldr.AppendLine("{");
            headbldr.AppendLine("	nav(\"" + urlForNoDiffs + "\");");
            headbldr.AppendLine("}");
            headbldr.AppendLine("function BodyClick()");
            headbldr.AppendLine("{");
            headbldr.AppendLine("   SetEditing(false);");
            headbldr.AppendLine("}");
            headbldr.AppendLine("function BodyDblClick()");
            headbldr.AppendLine("{");

            bool editOnDoubleClick = true;
            editOnDoubleClick = FlexWikiWebApplication.ApplicationConfiguration.EditOnDoubleClick;

            if (editOnDoubleClick)
            {
                headbldr.AppendLine("location.href=\"" + this.TheLinkMaker.LinkToEditTopic(topic.AsQualifiedTopicName()) + "\";");
            }
            headbldr.AppendLine("}");
            headbldr.AppendLine("</script>");
            string head = headbldr.ToString();

            return head;
        }


        protected string DoPage()
        {
            QualifiedTopicRevision topic = GetTopicVersionKey();
            NamespaceManager manager = Federation.NamespaceManagerForTopic(topic);
            LinkMaker lm = TheLinkMaker;
            bool diffs = Request.QueryString["diff"] == "y";
            QualifiedTopicRevision diffVersion = null;
            bool restore = (Request.RequestType == "POST" && Request.Form["RestoreTopic"] != null);
            bool isBlacklistedRestore = false;


            if (restore == true)
            {
                // Prevent restoring a topic with blacklisted content
                if (Federation.IsBlacklisted(Federation.Read(topic)))
                {
                    isBlacklistedRestore = true;
                }
                else
                {
                    Response.Redirect(lm.LinkToTopic(this.RestorePreviousVersion(new QualifiedTopicRevision(Request.Form["RestoreTopic"]))));
                    return "";
                }
            }

            // Go edit if we try to view it and it doesn't exist
            if (!manager.TopicExists(topic.LocalName, ImportPolicy.DoNotIncludeImports))
            {
                Response.Redirect(lm.LinkToEditTopic(topic.AsQualifiedTopicName()));
                return "";
            }

            StringBuilder strbldr = new StringBuilder();

            urlForDiffs = lm.LinkToTopic(topic, true);
            urlForNoDiffs = lm.LinkToTopic(topic, false);

            if (diffs)
            {
                diffVersion = manager.VersionPreviousTo(topic.LocalName, topic.Version);
            }


            strbldr.AppendLine("<span id=\"TopicTip\" class=\"TopicTip\" ></span>");

            //////////////////////////
            ///

            // Get the core data (the formatted topic and the list of changes) from the cache.  If it's not there, generate it!
            string formattedBody = Federation.GetTopicFormattedContent(topic, diffVersion);


            StringBuilder leftBorder = new StringBuilder();
            StringBuilder rightBorder = new StringBuilder();
            StringBuilder topBorder = new StringBuilder();
            StringBuilder bottomBorder = new StringBuilder();

            string templeft;
            string tempright;
            string temptop;
            string tempbottom;

            templeft = Federation.GetTopicFormattedBorder(topic, Border.Left);
            if (!String.IsNullOrEmpty(templeft))
            {
                leftBorder.AppendLine("<div class=\"Border\" id=\"LeftBorder\">");
                leftBorder.AppendLine(templeft);
                leftBorder.AppendLine("</div>");
            }
            tempright = Federation.GetTopicFormattedBorder(topic, Border.Right);
            if (!String.IsNullOrEmpty(tempright))
            {
                rightBorder.AppendLine("<div class=\"Border\" id=\"RightBorder\">");
                rightBorder.AppendLine(tempright);
                rightBorder.AppendLine("</div>");
            }
            temptop = Federation.GetTopicFormattedBorder(topic, Border.Top);
            if (!String.IsNullOrEmpty(temptop))
            {
                topBorder.AppendLine("<div class=\"Border\" id=\"TopBorder\">");
                topBorder.AppendLine(temptop);
                topBorder.AppendLine("</div>");
            }
            tempbottom = Federation.GetTopicFormattedBorder(topic, Border.Bottom);
            if (!String.IsNullOrEmpty(tempbottom))
            {
                bottomBorder.AppendLine("<div class=\"Border\" id=\"BottomBorder\">");
                bottomBorder.AppendLine(tempbottom);
                bottomBorder.AppendLine("</div>");
            }

            // Using a simple 5 box model for Top/Left/Right/BottomBorder and TopicBody.
            // Insert the TopBorder if it is required.
            if (!String.IsNullOrEmpty(temptop))
            {
                strbldr.AppendLine(topBorder.ToString());
            }

            // Insert the LeftBorder if it is required.
            if (!String.IsNullOrEmpty(templeft))
            {
                strbldr.AppendLine(leftBorder.ToString());
            }

            // Insert the TopicBody to hold the topic content
            strbldr.AppendLine("<div id=\"TopicBody\">");
            strbldr.AppendLine("<form method=\"post\" action=\"" + lm.LinkToQuicklink() + "?QuickLinkNamespace=" + topic.Namespace + "\" name=\"QuickLinkForm\">");
            strbldr.AppendLine("<div id=\"TopicBar\" title=\"Click here to quickly jump to or create a topic\" class=\"TopicBar\" onmouseover=\"TopicBarMouseOver()\"  onclick=\"TopicBarClick(event)\"  onmouseout=\"TopicBarMouseOut()\">");
            strbldr.AppendLine("<div  id=\"StaticTopicBar\"  class=\"StaticTopicBar\" style=\"display: block\">" + GetTitle() + "</div>");
            strbldr.AppendLine("<div id=\"DynamicTopicBar\" class=\"DynamicTopicBar\" style=\"display: none\">");
            //strbldr.AppendLine("<!-- <input id=\"TopicBarNamespace\" style=\"display: none\" type=\"text\"  name=\"QuickLinkNamespace\" /> -->");
            strbldr.AppendLine("<input id=\"TopicBarInputBox\" title=\"Enter a topic here to go to or create\" class=\"QuickLinkInput\" type=\"text\"  name=\"QuickLink\" />");
            strbldr.AppendLine("<div class=\"DynamicTopicBarHelp\">Enter a topic name to show or a new topic name to create; then press Enter</div>");
            strbldr.AppendLine("</div></div></form>");

            if (isBlacklistedRestore)
            {
                strbldr.AppendLine("<div class=\"BlacklistedRestore\"><font color=\"red\"><b>The version of the topic you are trying to restore contains content that has been banned by policy of this site.  Restore can not be completed.</b></font></div>");
            }

            strbldr.AppendLine(formattedBody);

            // Close the TopicBody.
            strbldr.AppendLine("</div>");

            // Insert the RightBorder if it is required.
            if (!String.IsNullOrEmpty(tempright))
            {
                strbldr.AppendLine(rightBorder.ToString());
            }

            // Insert the BottomBorder if it is required.
            if (!String.IsNullOrEmpty(tempbottom))
            {
                strbldr.AppendLine(bottomBorder.ToString());
            }

            string page = strbldr.ToString();

            return page;
        }

        private void WriteRecentPane()
        {
            OpenPane(Response.Output, "Recent Topics");
            Response.Write("    ");
            ClosePane(Response.Output);
        }
    }
}
