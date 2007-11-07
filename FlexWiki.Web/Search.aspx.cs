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

        protected void DoSearch()
        {
            string search = Request.QueryString["search"];
            bool regexSearch = false;
            if (null != Request.QueryString["regex"])
            {
                if (true == string.Equals(Request.QueryString["regex"], "on", StringComparison.CurrentCultureIgnoreCase))
                {
                    regexSearch = true;
                }
            }
            Response.Write(@"
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

            Response.Write("<p>Namespace:<br /><select name=\"namespace\" class=\"SearchColumnFilterBox\" id=\"NamespaceFilter\">");
            Response.Write("<option value=\"" + All + "\">" + All + "</option>");
            foreach (string ns in uniqueNamespaces)
            {
                string sel = (ns == preferredNamespace) ? " selected=\"selected\" " : "";
                Response.Write("<option " + sel + " value=\"" + ns + "\">" + ns + "</option>");
            }
            Response.Write("</select></p></form>");

            if (search != null)
            {
                Response.Write("<fieldset><legend>Search Result</legend>");
                Response.Write("<div class=\"SearchMain\">");

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
                    Response.Write(@"<div class=""ErrorMessage"">
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
                                        Response.Write("<h1>" + ns + "</h1>");
                                    header = true;

                                    Response.Write("<div class=\"searchHitHead\">");
                                    Response.Write("<a title=\"" + topic.DottedName + "\"  href=\"" + lm.LinkToTopic(topic) + "\">");
                                    Response.Write(topic.LocalName);
                                    Response.Write("</a>");
                                    Response.Write("</div>");

                                    string[] lines = s.Split(new char[] { '\n' });
                                    Response.Write("<div class=\"searchHitBody\">");
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
                                            Response.Write(Formatter.FormattedString(topic.AsQualifiedTopicRevision(), each, OutputFormat.HTML, storeManager, TheLinkMaker));
                                        }
                                    }
                                    Response.Write("</div>");
                                }
                            }
                        }
                    }
                    Response.Write("</div></fieldset></fieldset>");
                }
            }
        }
    }
}
