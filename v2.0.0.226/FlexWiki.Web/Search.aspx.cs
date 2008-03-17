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
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for Search.
    /// </summary>
    public class Search : BasePage
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
            strBldr.AppendLine("<div class=\"Dialog\">");

            strBldr.AppendLine(DoSearch());

            // Close the TopicBody.
            strBldr.AppendLine("</div>");
            strBldr.AppendLine("</div>");


            string page = strBldr.ToString();

            return page;
        }
        protected string DoSearch()
        {
            StringBuilder strBldr = new StringBuilder();
            string search = Request.QueryString["search"];
            bool regexSearch = false;
            if (null != Request.QueryString["regex"])
            {
                if (true == string.Equals(Request.QueryString["regex"], "on", StringComparison.CurrentCultureIgnoreCase))
                {
                    regexSearch = true;
                }
            }
            strBldr.AppendLine("<div id=\"TopicBody\">");
            strBldr.AppendLine(@"
<div id=""TopicTip"" class=""TopicTip"" ></div>
<fieldset><legend class=""DialogTitle"">Search</legend>
<form id=""SearchForm"" action="""">
<p>Search:<br /><input type=""text""  name=""search"" value=""" + (String.IsNullOrEmpty(search) ? "[enter search term]" : FlexWiki.Web.HtmlWriter.Escape(search)) + @"""/>
<input type=""submit"" id=""SearchButton"" value=""Go"" /><br />
<input type=""checkbox"" id=""regex"" name=""regex"" " + (regexSearch == true ? @"checked=""checked"" />" : "/>") + @"Use Regular Expressions for search
</p>");

            ArrayList uniqueNamespaces = new ArrayList();
            foreach (string ns in Federation.Namespaces)
            {
                uniqueNamespaces.Add(ns);
            }
            uniqueNamespaces.Sort();

            string preferredNamespace = Request.QueryString["namespace"];
            if (preferredNamespace == null)
            {
                preferredNamespace = DefaultNamespace;
            }

            strBldr.AppendLine("<p>Namespace:<br /><select name=\"namespace\" class=\"SearchColumnFilterBox\" id=\"NamespaceFilter\">");
            strBldr.AppendLine("<option value=\"" + All + "\">" + All + "</option>");
            foreach (string ns in uniqueNamespaces)
            {
                string sel = (ns == preferredNamespace) ? " selected=\"selected\" " : "";
                strBldr.AppendLine("<option " + sel + " value=\"" + ns + "\">" + ns + "</option>");
            }
            strBldr.AppendLine("</select></p></form>");

            if (search != null)
            {
                strBldr.AppendLine("<fieldset><legend>Search Result</legend>");
                strBldr.AppendLine("<div class=\"SearchMain\">");

                // Check to see if we've been given a valid regular expression.
                bool validRegex = true;
                if (true == regexSearch)
                {
                    try
                    {
                        Regex regex = new Regex(search, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        validRegex = false;
                    }
                }

                if (false == validRegex)
                {
                    strBldr.AppendLine(@"<div class=""ErrorMessage"">
    <div class=""ErrorMessageTitle"">Regular Expression Error</div>
    <div class=""ErrorMessageBody"">The regular expression that you entered is not valid. Please correct it and try again.</div>
</div>");
                }
                else
                {
                    LinkMaker lm = TheLinkMaker;

                    Dictionary<NamespaceManager, QualifiedTopicNameCollection> searchTopics =
                        new Dictionary<NamespaceManager, QualifiedTopicNameCollection>();
                    if (preferredNamespace == All)
                    {
                        foreach (string ns in uniqueNamespaces)
                        {
                            NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(ns);
                            if (storeManager == null)
                                continue;
                            searchTopics[storeManager] = storeManager.AllTopics(ImportPolicy.DoNotIncludeImports);
                        }
                    }
                    else
                    {
                        NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(preferredNamespace);
                        searchTopics[storeManager] = storeManager.AllTopics(ImportPolicy.DoNotIncludeImports);
                    }

                    foreach (NamespaceManager storeManager in searchTopics.Keys)
                    {
                        string ns = storeManager.Namespace;
                        bool header = false;
                        foreach (QualifiedTopicName topic in searchTopics[storeManager])
                        {
                            // Skip topics we don't have read permission for - they don't exist as far as search
                            // is concerned.
                            if (Federation.HasPermission(topic.AsQualifiedTopicRevision(), TopicPermission.Read))
                            {
                                string s = Federation.Read(topic);
                                string bodyWithTitle = topic.ToString() + s;

                                bool found = false;
                                if (true == regexSearch)
                                {
                                    found = Regex.IsMatch(bodyWithTitle, search, RegexOptions.IgnoreCase);
                                }
                                else
                                {
                                    found = bodyWithTitle.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) > -1;
                                }
                                if (true == found)
                                {
                                    if (!header && searchTopics.Count > 1)
                                        strBldr.AppendLine("<h1>" + ns + "</h1>");
                                    header = true;

                                    strBldr.AppendLine("<div class=\"searchHitHead\">");
                                    strBldr.AppendLine("<a title=\"" + topic.DottedName + "\"  href=\"" + lm.LinkToTopic(topic) + "\">");
                                    strBldr.AppendLine(topic.LocalName);
                                    strBldr.AppendLine("</a>");
                                    strBldr.AppendLine("</div>");

                                    string[] lines = s.Split(new char[] { '\n' });
                                    strBldr.AppendLine("<div class=\"searchHitBody\">");
                                    foreach (string each in lines)
                                    {
                                        bool foundInLine = false;
                                        if (true == regexSearch)
                                        {
                                            foundInLine = Regex.IsMatch(each, search, RegexOptions.IgnoreCase);
                                        }
                                        else
                                        {
                                            foundInLine = each.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) > -1;
                                        }
                                        if (true == foundInLine)
                                        {
                                            strBldr.AppendLine(Formatter.FormattedString(topic.AsQualifiedTopicRevision(), each, OutputFormat.HTML, storeManager, TheLinkMaker));
                                        }
                                    }
                                    strBldr.AppendLine("</div>");
                                }
                            }
                        }
                    }
                    strBldr.AppendLine("</div></fieldset></fieldset>");
                }
            }
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
    }
}
