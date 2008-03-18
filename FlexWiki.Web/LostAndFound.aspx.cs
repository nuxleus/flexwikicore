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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using FlexWiki.Web.Analysis;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for LostAndFound.
    /// </summary>
    public class LostAndFound : BasePage
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

        protected string ShowPage()
        {
            StringBuilder strbldr = new StringBuilder();

            strbldr.AppendLine("<div id=\"TopicBody\">");
            strbldr.AppendLine("<fieldset><legend class=\"DialogTitle\">Lost And Found</legend>");
            strbldr.AppendLine("<form id=\"Form\" action=\"\">");

            ArrayList uniqueNamespaces = new ArrayList();
            foreach (string ns in Federation.Namespaces)
            {
                uniqueNamespaces.Add(ns);
            }
            uniqueNamespaces.Sort();

            string preferredNamespace = Request.QueryString["namespace"];
            if (preferredNamespace == null)
                preferredNamespace = DefaultNamespace;

            strbldr.AppendLine("<p>Namespace:<br /><select title=\"to explore the list and found for another namespace, select it here\" name=\"namespace\" class=\"SearchColumnFilterBox\" id=\"NamespaceFilter\">");
            foreach (string ns in uniqueNamespaces)
            {
                string sel = (ns == preferredNamespace) ? " selected=\"selected\" " : "";
                strbldr.AppendLine("<option " + sel + " value=\"" + ns + "\">" + ns + "</option>");
            }
            strbldr.AppendLine("</select> <input title=\"click here to explore the lost and found for the selected namespace\" type=\"submit\" id=\"Go\" value=\"Change namespace\" /></p></form>");


            NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(preferredNamespace);
            LinkMaker lm = TheLinkMaker;

            if (storeManager == null)
            {

                strbldr.AppendLine("<h1>Inaccessible namespace</h1>");
                strbldr.AppendLine("<p>The namespace you have selected is not accessible.</p>");
            }
            else
            {
                strbldr.AppendLine("<h1>Lost and Found</h1>");
                strbldr.AppendLine("<p>Below are listed pages that are not reachable from the home page of this namespace.</p>");
                strbldr.AppendLine("<p>Related pages (ones that link to each other) are listed together.  Bold topics are completely unreferenced.  Other topics are referenced, but only from within the related topic group.</p>");

                ContentStoreAnalysis analysis = new ContentStoreAnalysis(storeManager);

                QualifiedTopicRevision home = new QualifiedTopicRevision(storeManager.HomePage, storeManager.Namespace);
                strbldr.AppendLine("<ul>");
                foreach (Island eachIsland in analysis.Islands)
                {
                    if (eachIsland.Contains(home))
                    {
                        continue;		// skip the mainland!
                    }
                    bool first = true;
                    strbldr.AppendLine("<li>");
                    foreach (QualifiedTopicRevision eachTopic in eachIsland)
                    {
                        TopicAnalysis tan = analysis.AnalysisFor(eachTopic);
                        if (!first)
                        {
                            strbldr.AppendLine(", ");
                        }
                        first = false;
                        int refs = tan.RefCount;
                        if (refs == 0)
                        {
                            strbldr.AppendLine("<b>");
                        }
                        strbldr.AppendLine("<a href=\"" + lm.LinkToTopic(eachTopic) + "\">" + eachTopic.LocalName + "</a>");
                        if (refs == 0)
                        {
                            strbldr.AppendLine("</b>");
                        }
                    }
                    strbldr.AppendLine("</li>");
                }
                strbldr.AppendLine("</ul>");
            }
            strbldr.AppendLine("</fieldset>");
            return strbldr.ToString();
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

            strBldr.AppendLine(ShowPage());

            // Close the TopicBody.
            strBldr.AppendLine("</div>");


            string page = strBldr.ToString();

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
