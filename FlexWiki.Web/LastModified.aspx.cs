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
using System.Data;
using System.Drawing;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;

using FlexWiki.Collections;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for LastModified.
    /// </summary>
    public class LastModified : BasePage
    {
        private string _preferredNamespace = string.Empty;
        private string _numberRecords = string.Empty;
        private int _recordLimit = 25;
        private ArrayList _uniqueNamespaces;

        private void Page_Load(object sender, System.EventArgs e)
        {
            _uniqueNamespaces = new ArrayList();
            foreach (string ns in Federation.Namespaces)
            {
                _uniqueNamespaces.Add(ns);
            }
            _uniqueNamespaces.Sort();

            _preferredNamespace = Request.QueryString["namespace"];
            if (_preferredNamespace == null)
            {
                _preferredNamespace = DefaultNamespace;
            }
            _numberRecords = Request.QueryString["records"];
            if (!String.IsNullOrEmpty(_numberRecords))
            {
                try
                {
                    _recordLimit = Int32.Parse(_numberRecords);
                }
                catch
                {
                    _recordLimit = 25;
                }
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

        private static string All = "[All]";

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

                    SetBorderFlags(template);

                    bool startProcess = false;
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
            strOutput.AppendLine("</head>");
            strOutput.AppendLine("<body>");

            strOutput.AppendLine(InsertLeftTopBorders());
            strOutput.AppendLine(DoPageImplementation());
            strOutput.AppendLine(InsertRightBottomBorders());

            strOutput.AppendLine("</body>");
            strOutput.AppendLine("</html>");
            return strOutput.ToString();

        }
        protected string DoPageImplementation()
        {
            StringBuilder strBldr = new StringBuilder();

            //strBldr.AppendLine("<div id=\"TopicBody\">");
            strBldr.AppendLine("<fieldset class=\"Dialog\">");
            strBldr.AppendLine("<legend class=\"DialogTitle\">Recent Changes</legend>");
            strBldr.Append("<p>Namespace: ");
            strBldr.Append(NamespaceFilter());
            strBldr.Append("&nbsp;Records: ");
            strBldr.Append(NumberFilter());
            strBldr.AppendLine("</p>");

            strBldr.AppendLine(DoSearch());

            // Close the TopicBody.
            strBldr.AppendLine("</fieldset>");
            strBldr.AppendLine("</div>");


            string page = strBldr.ToString();

            return page;
        }
        protected string DoSearch()
        {
            LinkMaker lm = TheLinkMaker;
            StringBuilder strBldr = new StringBuilder();
            int counter = 0;

            NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(_preferredNamespace);

            // Get the complete list of topics and authors 
            QualifiedTopicNameCollection topics = storeManager.AllTopicsSortedLastModifiedDescending();

            // Omit topics for which we don't have read permission - we're not going to be able 
            // to access information about them anyway. 
            QualifiedTopicNameCollection permittedTopics = new QualifiedTopicNameCollection();
            foreach (QualifiedTopicName topic in topics)
            {
                if (storeManager.HasPermission(new UnqualifiedTopicName(topic.LocalName), TopicPermission.Read))
                {
                    if (_recordLimit > 0)
                    {
                        if (counter < _recordLimit)
                        {
                            permittedTopics.Add(topic);
                            counter++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        permittedTopics.Add(topic);
                    }
                }
            }

            Dictionary<QualifiedTopicName, string> authorMap = new Dictionary<QualifiedTopicName, string>();
            SortedList<string, string> authors = new SortedList<string, string>();
            foreach (QualifiedTopicName topic in permittedTopics)
            {
                string author = storeManager.GetTopicLastAuthor(topic.LocalName);
                authorMap[topic] = author;
                // Overwrites the entry for author if it already exists, thus giving
                // us only unique authors. 
                authors[author] = author;
            }

            strBldr.AppendLine("<table cellspacing='0' cellpadding='2' border='0'>");
            strBldr.AppendLine("<thead>");
            strBldr.AppendLine("<td class=\"SearchColumnHeading\" width=\"300\">Topic</td>");
            strBldr.AppendLine("<td class=\"SearchColumnHeading\" width=\"100\">Modified</td>");
            strBldr.AppendLine("<td class=\"SearchColumnHeading\" width=\"200\">Author: ");
            strBldr.AppendLine("<select  onchange='filter()' class='SearchColumnFilterBox' id='AuthorFilter'>");
            strBldr.AppendLine("<option value='" + All + "'>" + All + "</option>");
            foreach (string author in authors.Values)
            {
                strBldr.AppendLine("<option value='" + author + "'>" + author + "</option>");
            }
            strBldr.AppendLine("</select>");
            strBldr.AppendLine("</td>");

            strBldr.AppendLine("</thead><tbody id=\"MainTable\">");

            int row = 0;
            foreach (QualifiedTopicName topic in permittedTopics)
            {
                strBldr.AppendLine("<tr id=\"row" + row + "\" class=\"" + (((row & 1) == 0) ? "SearchOddRow" : "SearchEvenRow") + "\">");
                row++;

                strBldr.AppendLine("<td>");
                strBldr.Append("<b><a title=\"" + topic.DottedName + "\"  href=\"" + lm.LinkToTopic(topic) + "\">");
                strBldr.Append(topic.LocalName);
                strBldr.Append("</a></b>");
                strBldr.AppendLine("</td>");

                strBldr.AppendLine("<td>");
                DateTime stamp = storeManager.GetTopicLastModificationTime(topic.LocalName);
                string timeFormat = WikiApplication.FederationConfiguration.LocalTimeFormat;
                string dateFormat = WikiApplication.FederationConfiguration.LocalDateFormat;
                if (stamp.Date == DateTime.Now.Date)
                {
                    strBldr.Append(stamp.ToString(timeFormat));
                }
                else
                {
                    strBldr.Append(stamp.ToString(dateFormat + " - " + timeFormat));
                }
                strBldr.AppendLine("</td>");
                strBldr.AppendLine("<td>");
                strBldr.AppendLine(Escape((string)(authorMap[topic])));
                strBldr.AppendLine("</td>");
                strBldr.AppendLine("</tr>");
            }
            strBldr.AppendLine("</tbody></table>");
            return strBldr.ToString();
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
                            strOutput.AppendLine(DoPageImplementation());
                            break;

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
        private static string Escape(string input)
        {
            // replace HTML special characters with character entities
            // this has the side-effect of stripping all markup from text
            string str = input;
            str = Regex.Replace(str, "&", "&amp;");
            str = Regex.Replace(str, "<", "&lt;");
            str = Regex.Replace(str, ">", "&gt;");
            return str;
        }
        protected string NamespaceFilter()
        {
            string result = "<select onchange='changeNamespace()' class='SearchColumnFilterBox' id='NamespaceFilter'>";
            foreach (string ns in _uniqueNamespaces)
            {
                string sel = (ns == _preferredNamespace) ? " selected " : "";
                result += "<option " + sel + " value='" + ns + "'>" + ns + "</option>";
            }
            result += "</select>";
            return result;
        }
        protected string NumberFilter()
        {
            string result = "<select onchange='changeNamespace()' class='SearchColumnFilterBox' id='NumberFilter'>";
            // fix up selected index for different values

            result += "<option" + (_recordLimit == 25 ? " selected " : "") + " value='25'>25</option>";
            result += "<option" + (_recordLimit == 50 ? " selected " : "") + " value='50'>50</option>";
            result += "<option" + (_recordLimit == 100 ? " selected " : "") + " value='100'>100</option>";
            result += "<option" + (_recordLimit == 500 ? " selected " : "") + " value='500'>500</option>";
            result += "<option" + (_recordLimit == -1 ? " selected " : "") + " value='-1'>All</option>";
            result += "</select>";
            return result;
        }

    }
}
