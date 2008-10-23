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

using log4net;
using log4net.Config;
using log4net.Core;
using FlexWiki;
using FlexWiki.Collections;
using FlexWiki.Formatting;
using System.Web;
//using System.Threading;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for WebForm1.
    /// </summary>
    public class Default2 : BasePage
    {
        private LogEvent MainEvent;

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

        private LinkMaker lm
        {
            get { return TheLinkMaker; }
        }
        //private NamespaceManager manager
        //{
        //    get { return Federation.NamespaceManagerForTopic(topic); }
        //}
        protected string BuildPage()
        {

            // Consider clearing the cache for this topic 
            string cache = Federation.GetTopicPropertyValue(GetTopicVersionKey(), "_ClearCache") ;
            // LogDebug(this.GetType().ToString(),"_ClearCache = "+cache);    
            if (cache != "" && cache.ToUpper().Equals("TRUE"))
            {
            	FlexWikiWebApplication.Cache.ClearTopic(topic.ToString()) ;
            }                
        	
            StringBuilder strOutput = new StringBuilder();
            bool diffs = Request.QueryString["diff"] == "y";
            QualifiedTopicRevision diffVersion = null;
            bool restore = (Request.RequestType == "POST" && Request.Form["RestoreTopic"] != null);


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

            urlForDiffs = lm.LinkToTopic(topic, true);
            urlForNoDiffs = lm.LinkToTopic(topic, false);

            if (diffs)
            {
                diffVersion = manager.VersionPreviousTo(topic.LocalName, topic.Version);
            }

            Federation.Parser = Parser;
            // Get the core data (the formatted topic and the list of changes) from the cache.  If it's not there, generate it!
            formattedBody = WikiApplication.CachedRender(
                CreateCacheKey("FormattedBody"),
                delegate
                {
                    return Federation.GetTopicFormattedContent(topic, diffVersion);
                });

            
            string overrideBordersScope = "None";
            string template = "";

            if (!String.IsNullOrEmpty(WikiApplication.ApplicationConfiguration.OverrideBordersScope))
            {
                overrideBordersScope = WikiApplication.ApplicationConfiguration.OverrideBordersScope;
            }
            if (!String.IsNullOrEmpty(overrideBordersScope))
            {
                template = PageUtilities.GetOverrideBordersContent(manager, overrideBordersScope);
            }
            if (!String.IsNullOrEmpty(template))  // page built using template
            {

                    template = Regex.Replace(template, "<body>", "<body onclick=\"javascript:BodyClick()\" ondblclick=\"javascript:BodyDblClick()\">");

                    SetBorderFlags(template);

                    bool startProcess = false;
                    //string sp;

                    //sp = template.TrimEnd(new char[] { '\n' });
                    //sp = sp.Replace("\r", "");
                    //sp = sp.TrimEnd(new char[] { '\n' });
                    foreach (string s in template.Split(new char[] { '\n' }))
                    {
                        if (!startProcess)
                        {
                            if (s.Contains("</title>")) //ignore input until after tag </title>
                            {
                                startProcess = true;
                            }
                        }
                        else
                        {
                            strOutput.Append(DoTemplatedPage(s.Trim()));
                        }
                    }
            }
            else    //page without template
            {
                strOutput.Append(DoNonTemplatePage());
            }
            return strOutput.ToString();
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

            if (_metaTags)
            {
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
            }

            if (_javaScript)
            {
                headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}WikiDefault.js\" type=\"text/javascript\"></script>\r\n", RootUrl);
                headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}WikiTopicBar.js\" type=\"text/javascript\"></script>\r\n", RootUrl);
                headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}WikiMenu.js\" type=\"text/javascript\"></script>\r\n", RootUrl);
                headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}WikiThreads.js\" type=\"text/javascript\"></script>\r\n", RootUrl);
                if (!String.IsNullOrEmpty(FlexWikiWebApplication.ApplicationConfiguration.LocalJavascript))
                {
                    headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}{1}\" type=\"text/javascript\"></script>\r\n", RootUrl,
                        FlexWikiWebApplication.ApplicationConfiguration.LocalJavascript);
                }
                if (manager.GetTopicInfo(topic.LocalName).HasProperty("CreateAtomFeedLink"))
                {
                    if (manager.GetTopicProperty(topic.LocalName, "CreateAtomFeedLink").LastValue.ToLower() == "true")
                    {
                        string atomLink = BaseUrl + RootUrl + "AtomFeed.aspx/" + topic.DottedName;
                        headbldr.AppendLine(@"<link rel=""alternate"" type=""application/atom+xml"" title=""" + topic.FormattedName + @""" href=""" + atomLink + @""" />");
                    }
                }
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
            }
            string head = headbldr.ToString();

            return head;
        }
        protected string DoNonTemplatePage()
        {
            StringBuilder strOutput = new StringBuilder();
            _javaScript = true;
            _metaTags = true;

            InitBorders(true);
            strOutput.AppendLine(InsertStylesheetReferences());
            strOutput.AppendLine(InsertFavicon());
            strOutput.AppendLine(DoHead());
            strOutput.AppendLine("</head>");
            strOutput.AppendLine("<body onclick=\"javascript:BodyClick()\" ondblclick=\"javascript:BodyDblClick()\">");

            strOutput.AppendLine(InsertLeftTopBorders());
            strOutput.AppendLine(DoPageImplementation(lm));
            strOutput.AppendLine(InsertRightBottomBorders());

            strOutput.AppendLine("</body>");
            strOutput.AppendLine("</html>");
            return strOutput.ToString();

        }
        protected string DoPageImplementation(LinkMaker lm)
        {
            StringBuilder strbldr = new StringBuilder();

            strbldr.AppendLine("<div id=\"TopicBody\">");
            strbldr.AppendLine("<span id=\"TopicTip\" class=\"TopicTip\" ></span>");

            // Insert the TopicBody to hold the topic content
            //strbldr.AppendLine("<div id=\"TopicBody\">");
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


            string page = strbldr.ToString();

            return page;
        }
        protected string DoTemplatedPage(string s)
        {
            StringBuilder strOutput = new StringBuilder();

            MatchCollection lineMatches = dirInclude.Matches(s);
            string temp = s;
            if (lineMatches.Count > 0)
            {
                int position;
                position = temp.IndexOf("{{");
                if (position > 0)
                {
                    strOutput.AppendLine(temp.Substring(0, position));
                }
                foreach (Match submatch in lineMatches)
                {
                    switch (submatch.ToString())
                    {
                        case "{{FlexWikiTopicBody}}":
                            strOutput.AppendLine(DoPageImplementation(lm));
                            break;

                        case "{{FlexWikiHeaderInfo}}":
                            strOutput.AppendLine(InsertStylesheetReferences());
                            strOutput.AppendLine(InsertFavicon());
                            strOutput.AppendLine(DoHead());
                            break;

                        case "{{FlexWikiMetaTags}}":
                            strOutput.AppendLine(DoHead());
                            break;

                        case "{{FlexWikiJavaScript}}":
                            if (!_metaTags)
                            {
                                strOutput.AppendLine(DoHead());
                            }
                            break;

                        case "{{FlexWikiCss}}":
                            strOutput.AppendLine(InsertStylesheetReferences());
                            break;

                        case "{{FlexWikiFavIcon}}":
                            strOutput.AppendLine(InsertFavicon());
                            break;

                        case "{{FlexWikiTopBorder}}":
                            if (!String.IsNullOrEmpty(temptop))
                            {
                                strOutput.AppendLine(temptop.ToString());
                            }
                            break;

                        case "{{FlexWikiLeftBorder}}":
                            if (!String.IsNullOrEmpty(templeft))
                            {
                                strOutput.AppendLine(templeft.ToString());
                            }
                            break;

                        case "{{FlexWikiRightBorder}}":
                            if (!String.IsNullOrEmpty(tempright))
                            {
                                strOutput.AppendLine(tempright.ToString());
                            }
                            break;

                        case "{{FlexWikiBottomBorder}}":
                            if (!String.IsNullOrEmpty(tempbottom))
                            {
                                strOutput.AppendLine(tempbottom.ToString());
                            }
                            break;


                        default:
                            break;
                    }
                    temp = temp.Substring(s.IndexOf("}}") + 2);
                }
                if (!String.IsNullOrEmpty(temp))
                {
                    strOutput.AppendLine(temp);
                }
            }
            else
            {
                strOutput.AppendLine(s);
            }
            return strOutput.ToString();
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


        protected void StartPage()
        {
            if (Federation.GetPerformanceCounter(PerformanceCounterNames.TopicReads) != null)
                Federation.GetPerformanceCounter(PerformanceCounterNames.TopicReads).Increment();

            MainEvent = Federation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, GetTopicVersionKey().ToString(), LogEventType.ReadTopic);
            VisitorEvent e = new VisitorEvent(GetTopicVersionKey(), VisitorEvent.Read, DateTime.Now);
            LogVisitorEvent(e);
        }
        

        private void WriteRecentPane()
        {
            OpenPane(Response.Output, "Recent Topics");
            Response.Write("    ");
            ClosePane(Response.Output);
        }
        public void LogDebug(string source, string message)
        {
            LogManager.GetLogger(source).Debug(message);
        }
        public void LogError(string source, string message)
        {
            LogManager.GetLogger(source).Error(message);
        }
        public void LogInfo(string source, string message)
        {
            LogManager.GetLogger(source).Info(message);
        }
        public void LogWarning(string source, string message)
        {
            LogManager.GetLogger(source).Warn(message);
        }		        
    }
}
