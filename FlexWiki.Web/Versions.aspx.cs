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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Util;
using System.Text;
using System.Text.RegularExpressions;

using FlexWiki.Collections;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for Versions.
    /// </summary>
    public class Versions : BasePage
    {
        protected System.Web.UI.WebControls.PlaceHolder phResult = new PlaceHolder();

        private QualifiedTopicRevision _theTopic;
        private TopicChangeCollection _changeList;
        private bool endProcess = false;
        private bool templatedPage = false;
        private string template;
        private int lineCnt;

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

        private void Page_Load(object sender, System.EventArgs e)
        {
            _changeList = Federation.GetTopicChanges(new TopicName(TheTopic.LocalName, TheTopic.Namespace));
            ShowPage();
        }

        protected QualifiedTopicRevision TheTopic
        {
            get
            {
                if (_theTopic != null)
                {
                    return _theTopic;
                }
                string topic;
                if (IsPost)
                {
                    topic = Request.Form["Topic"];
                }
                else
                {
                    topic = Request.QueryString["topic"];
                }
                _theTopic = new QualifiedTopicRevision(topic);
                return _theTopic;
            }
        }

        protected string BuildPageOne()
        {

            StringBuilder strOutput = new StringBuilder();

            string overrideBordersScope = "None";
            template = "";

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

                    SetBorderFlags(template);
                    templatedPage = true;

                    bool startProcess = false;
                    foreach (string s in template.Split(new char[] { '\n' }))
                    {
                        //lineCnt++;
                        if (!startProcess)
                        {
                            if (s.Contains("</title>")) //ignore input until after tag </title>
                            {
                                startProcess = true;
                            }
                        }
                        if (!endProcess)
                        {
                            strOutput.Append(DoTemplatedPageOne(s.Trim()));
                        }
                    }
            }
            else    //page without template
            {
                strOutput.Append(DoNonTemplatePageOne());
            }
            return strOutput.ToString();
        }
        protected string BuildPageTwo()
        {

            StringBuilder strOutput = new StringBuilder();

            if (templatedPage)  // page built using template
            {
                if (!String.IsNullOrEmpty(template))
                {
                    int count = 0;

                    foreach (string s in template.Split(new char[] { '\n' }))
                    {
                        count++;
                        if (count >= lineCnt)
                        {
                            strOutput.Append(DoTemplatedPageTwo(s.Trim()));
                        }
                    }
                }
            }
            else    //page without template
            {
                strOutput.Append(DoNonTemplatePageTwo());
            }
            return strOutput.ToString();
        }
        protected string DoNonTemplatePageOne()
        {
            StringBuilder strOutput = new StringBuilder();
            _javaScript = true;
            _metaTags = true;

            InitBorders();
            strOutput.AppendLine(InsertStylesheetReferences());
            strOutput.AppendLine(InsertFavicon());
            strOutput.AppendLine("</head>");
            strOutput.AppendLine("<body onload=\"PageInit();\">");

            strOutput.AppendLine(InsertLeftTopBorders());
            strOutput.AppendLine(DoPageImplementationOne());

            return strOutput.ToString();

        }
        protected string DoNonTemplatePageTwo()
        {
            StringBuilder strOutput = new StringBuilder();

            strOutput.AppendLine(DoPageImplementationTwo());
            strOutput.AppendLine(InsertRightBottomBorders());

            strOutput.AppendLine("</body>");
            strOutput.AppendLine("</html>");
            return strOutput.ToString();

        }
        protected string DoPageImplementationOne()
        {
            StringBuilder strBldr = new StringBuilder();

            //    <body onload="PageInit();">

            strBldr.AppendLine("<div id=\"TopicBody\">");
            strBldr.AppendLine("<div class=\"Dialog\">");
            strBldr.AppendLine("<div id=\"StaticTopicBar\" class=\"StaticTopicBar\" style=\"display: block\">");
            strBldr.AppendLine(TheTopic.ToString());
            strBldr.AppendLine("</div>");
            strBldr.AppendLine("<h3>Previous Versions</h3>");
            strBldr.AppendLine("<p>Select differences: Select the radio boxes of any versions and press \"Enter\" or");
            strBldr.AppendLine("press the button below or press key [alt-v] to compare these versions.<br />");
            strBldr.AppendLine("Legend: (Current) = shows difference with current version, (Previous) = shows difference");
            strBldr.AppendLine("with preceding version</p>");
            strBldr.AppendLine("<form action=\"" + LinkToCompare() + "\" method=\"get\">");
            strBldr.AppendLine("<input type=\"hidden\" name=\"topic\" value=\"" + TheTopic.DottedName + "\" />");
            strBldr.AppendLine("<input type=\"submit\" accesskey=\"v\" class=\"standardsButton\" title=\"Show the differences between two selected versions of this topic. [alt-v]\"");
            strBldr.AppendLine("value=\"Compare selected versions\" />");
            strBldr.AppendLine("<ul id=\"topicversions\">");
            //strBldr.AppendLine("<asp:PlaceHolder ID=\"phResult\" runat=\"server\" />");

            return strBldr.ToString();
            
        }
        protected string DoPageImplementationTwo()
        {
            StringBuilder strBldr = new StringBuilder();

            strBldr.AppendLine("</ul>");
            strBldr.AppendLine("</form>");
            strBldr.AppendLine("</div>");

            //ShowPage();

            // Close the TopicBody.
            strBldr.AppendLine("</div>");
            strBldr.AppendLine("</div>");


            return strBldr.ToString();
        }
        protected string DoTemplatedPageOne(string s)
        {
            StringBuilder strOutput = new StringBuilder();

            MatchCollection lineMatches = dirInclude.Matches(s);
            string temp = s;
            lineCnt++;
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
                            strOutput.AppendLine(DoPageImplementationOne());
                            endProcess = true;
                            return strOutput.ToString();

                        case "{{FlexWikiHeaderInfo}}":
                            strOutput.AppendLine(InsertStylesheetReferences());
                            strOutput.AppendLine(InsertFavicon());
                            break;

                        case "{{FlexWikiMetaTags}}":
                            break;

                        case "{{FlexWikiJavaScript}}":
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

                        default:
                            break;
                    }
                    temp = temp.Substring(s.IndexOf("}}") + 2);
                }
                if (!String.IsNullOrEmpty(temp))
                {
                    if (!endProcess)
                    {
                        strOutput.AppendLine(temp);
                    }
                }
            }
            else
            {
                strOutput.AppendLine(s);
            }
            return strOutput.ToString();
        }
        protected string DoTemplatedPageTwo(string s)
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
                            strOutput.AppendLine(DoPageImplementationTwo());
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
        private string GenerateList()
        {
            bool first = true;
            StringBuilder output = new StringBuilder();
            int row = 0;
            int lastRow = _changeList.Count - 1;

            foreach (TopicChange change in _changeList)
            {
                output.Append("<li>");
                //logic for selecting two versions to compare
                if (change.Version == TheTopic.Version || (first && TheTopic.Version == null))
                {
                    output.Append(" (Current)");
                }
                else
                {
                    output.AppendFormat(" (<a href=\"{0}\" title=\"Show Difference with current version\">Current</a>)", TheLinkMaker.LinkToCompare(TheTopic.DottedName, 0, row));
                }


                if (row < lastRow)
                {
                    output.AppendFormat(" (<a href=\"{0}\" style=\"standardsButton\" title=\"Show Difference with preceding version\" tabindex=\"{1}\">Previous</a>)", TheLinkMaker.LinkToCompare(TheTopic.DottedName, row, row + 1), row + 1);
                }
                else
                {
                    output.Append(" (Previous)");
                }


                output.AppendFormat(" <input type=\"radio\" {1} name=\"oldid\" value=\"{0}\" title=\"Select an older version to compare\" {2} />&nbsp;", row, ((first) ? " style=\"visibility:hidden\" " : ""), ((row == 1) ? "  checked=\"checked\"" : ""));
                output.AppendFormat(" <input type=\"radio\" name=\"diff\" value=\"{0}\" title=\"Select a newer version to compare\" {1} />", row, ((first) ? "  checked=\"checked\"" : ""));

                if (change.Created == DateTime.MinValue)
                {
                    output.Append("???");
                }
                else
                {
                    output.Append("&nbsp;&nbsp;<span class=\"version\"><a href=\"" + TheLinkMaker.LinkToTopic(change.TopicRevision) + "\" title=\"Show this version\" >");
                    string timeFormat = WikiApplication.FederationConfiguration.LocalTimeFormat;
                    if (change.Created.Date == DateTime.Now.Date)
                    {
                        output.Append(" Today, " + change.Created.ToString(timeFormat));
                    }
                    else
                    {
                        string localFormat = WikiApplication.FederationConfiguration.LocalDateFormat + " - " + timeFormat;
                        //string localFormat = "yyyy-MM-dd - HH:mm";
                        //System.Globalization.DateTimeFormatInfo dtf = new System.Globalization.CultureInfo("", false).DateTimeFormat;
                        //dtf.FullDateTimePattern = localFormat;
                        output.AppendFormat(change.Created.ToString(localFormat));

                    }
                    output.Append("</a></span>");
                }


                output.Append(" <span class=\"user\">" + change.Author + "</span>");
                first = false;
                output.AppendLine("</li>");
                row++;
            }
            return output.ToString();
        }
        protected string GetTitle()
        {
            string title = Federation.GetTopicPropertyValue(TheTopic, "Title");
            if (title == null || title == "")
            {
                title = string.Format("{0} - {1}", GetTopicVersionKey().FormattedName,
                    GetTopicVersionKey().Namespace);
            }
            return HtmlStringWriter.Escape(title) + " Versions ";
        }



        protected string LinkToCompare()
        {
            return TheLinkMaker.LinkToCompare(TheTopic.DottedName, int.MinValue, int.MinValue);
        }

        protected void ShowPage()
        {

            // Now generate the page!
            string output = string.Empty;
            if (_changeList != null)
            {
                output += GenerateList();
            }

            phResult.Controls.Add(new LiteralControl(output + "\n"));
        }




    }
}
