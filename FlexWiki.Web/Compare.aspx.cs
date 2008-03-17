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
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FlexWiki;
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary for Compare.
    /// </summary>
    public class Compare : BasePage
    {

        private LogEvent _mainEvent = null;
        private QualifiedTopicRevision _requestedTopic = null;

        private string _topicString = string.Empty;
        private int _diff = 0;
        private int _oldid = 0;

        protected QualifiedTopicRevision RequestedTopic
        {
            get
            {
                return _requestedTopic;
            }
        }

        #region Vom Web Form-Designer generierter Code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: Dieser Aufruf ist für den ASP.NET Web Form-Designer erforderlich.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);
        }
        #endregion

        private void Page_Load(object sender, System.EventArgs e)
        {
            _topicString = Request.QueryString["topic"];

            try
            {
                _diff = Convert.ToInt32(Request.QueryString["diff"]);
            }
            catch { }
            try
            {
                _oldid = Convert.ToInt32(Request.QueryString["oldid"]);
            }
            catch { }

            try
            {
                _requestedTopic = new QualifiedTopicRevision(_topicString);
            }
            catch { }

            if (_requestedTopic == null || _diff >= _oldid)
            {
                Response.Redirect("default.aspx");
            }
        }

        protected string GetTitle()
        {
            string title = Federation.GetTopicPropertyValue(RequestedTopic, "Title");
            if (title == null || title == "")
            {
                title = string.Format("{0} - {1}", GetTopicVersionKey().FormattedName, GetTopicVersionKey().Namespace);
            }
            return HtmlStringWriter.Escape(title);
        }

        protected void StartPage()
        {
            if (Federation.GetPerformanceCounter(PerformanceCounterNames.TopicsCompared) != null)
            {
                Federation.GetPerformanceCounter(PerformanceCounterNames.TopicsCompared).Increment();
            }

            _mainEvent = Federation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, RequestedTopic.LocalName, LogEventType.CompareTopic);
            VisitorEvent e = new VisitorEvent(RequestedTopic, VisitorEvent.Compare, DateTime.Now);
            LogVisitorEvent(e);
        }

        protected void EndPage()
        {
            _mainEvent.Record();
        }

        protected string DoPage()
        {
            StringBuilder strBldr = new StringBuilder();
            
            QualifiedTopicRevision newestTopicVersion = null;
            QualifiedTopicRevision oldTopicVersion = null;
            int counter = 0;
            IEnumerable changeList = Federation.GetTopicChanges(RequestedTopic.AsQualifiedTopicName());
            strBldr.AppendLine("<div id=\"TopicBody\">");
            strBldr.AppendLine("<div id=\"content\">");
            strBldr.AppendLine("<a name=\"top\" id=\"contentTop\"></a>");
            strBldr.AppendLine("<h1 class=\"firstHeading\">Compare two versions</h1>");
            strBldr.AppendLine("<div id=\"bodyContent\">");
            foreach (TopicChange change in changeList)
            {
                if (counter == _diff)
                {
                    string newLocalName = change.DottedName.Substring(change.DottedName.IndexOf(".") + 1);
                    string newNamespace = change.DottedName.Substring(0, change.DottedName.IndexOf("."));
                    newestTopicVersion = new QualifiedTopicRevision(newLocalName, newNamespace, change.Version);
                }
                else if (counter == _oldid)
                {
                    string oldLocalName = change.DottedName.Substring(change.DottedName.IndexOf(".") + 1);
                    string oldNamespace = change.DottedName.Substring(0, change.DottedName.IndexOf("."));
                    oldTopicVersion = new QualifiedTopicRevision(oldLocalName, oldNamespace, change.Version);
                    break;
                }
                counter++;
            }

            if (newestTopicVersion != null && oldTopicVersion != null)
            {
                strBldr.AppendLine(@"<div id=""MainRegion"" class=""TopicBody"">
<form method=""post"" action=""" + TheLinkMaker.LinkToQuicklink() + @"?QuickLinkNamespace=" + RequestedTopic.Namespace + @""" name=""QuickLinkForm"">
<div id=""TopicBar"" title=""Click here to quickly jump to or create a topic"" class=""TopicBar"" onmouseover=""javascript:TopicBarMouseOver()""  onclick=""javascript:TopicBarClick(event)""  onmouseout=""javascript:TopicBarMouseOut()"">
<div  id=""StaticTopicBar"" class=""StaticTopicBar"" style=""display: block"">" + GetTitle() + @"</div>
<div id=""DynamicTopicBar"" class=""DynamicTopicBar"" style=""display: none"">
<!-- <input id=""TopicBarNamespace"" style=""display: none"" type=""text""  name=""QuickLinkNamespace""/> -->
<input id=""TopicBarInputBox"" title=""Enter a topic here to go to or create"" class=""QuickLinkInput"" type=""text""  name=""QuickLink""/>
<div class=""DynamicTopicBarHelp"">Enter a topic name to show or a new topic name to create; then press Enter</div>
</div>
</div>
</form>
");
                string formattedBody = Federation.GetTopicFormattedContent(newestTopicVersion, oldTopicVersion);
                strBldr.AppendLine(formattedBody);
                strBldr.AppendLine("</div>");
            }
            strBldr.AppendLine("</div>");
            strBldr.AppendLine("</div>");

            return strBldr.ToString();
        }
        protected string BuildPage()
        {

            StringBuilder strOutput = new StringBuilder();

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

                template = Regex.Replace(template, "<body>", "<body onload=\"focus();\">");

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
                    strOutput.Append(DoTemplatedPage(s.Trim()));
                }
            }
            else    //page without template
            {
                strOutput.Append(DoNonTemplatePage());
            }
            return strOutput.ToString();
        }
        protected string DoNonTemplatePage()
        {
            StringBuilder strOutput = new StringBuilder();
            _javaScript = true;
            _metaTags = true;

            InitBorders();
            strOutput.AppendLine(InsertStylesheetReferences());
            strOutput.AppendLine(InsertFavicon());
            strOutput.AppendLine(InsertScripts());
            strOutput.AppendLine("</head>");
            strOutput.AppendLine("<body onload=\"focus();\">");

            strOutput.AppendLine(InsertLeftTopBorders());
            strOutput.AppendLine(DoPage());
            strOutput.AppendLine(InsertRightBottomBorders());

            strOutput.AppendLine("</body>");
            strOutput.AppendLine("</html>");
            return strOutput.ToString();

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
                            strOutput.AppendLine(DoPage());
                            break;

                        case "{{FlexWikiHeaderInfo}}":
                            strOutput.AppendLine(InsertStylesheetReferences());
                            strOutput.AppendLine(InsertFavicon());
                            strOutput.AppendLine(InsertScripts());
                            break;

                        case "{{FlexWikiMetaTags}}":
                            break;

                        case "{{FlexWikiJavaScript}}":
                            strOutput.AppendLine(InsertScripts());
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
    }
}
