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
    /// Summary description for TopicLocks.
    /// </summary>
    public class TopicLocks : BasePage
    {
        private string _preferredNamespace = string.Empty;
        private ArrayList _uniqueNamespaces;
        private string _topic;
        private string _action;

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
            if (IsPost)
            {
                _preferredNamespace = Request.Form["namespace"];
                if (!String.IsNullOrEmpty(Request.Form["topic"]))
                {
                    ProcessPost();
                }
                Response.Redirect("TopicLocks.aspx?namespace=" + _preferredNamespace);
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
            string result = "<select onchange='changeNamespace()' class='SearchColumnFilterBox' name='namespace' id='NamespaceFilter'>";
            foreach (string ns in _uniqueNamespaces)
            {
                string sel = (ns == _preferredNamespace) ? " selected " : "";
                result += "<option " + sel + " value='" + ns + "'>" + ns + "</option>";
            }
            result += "</select>";
            result += "<input type='button' id='ChangeNamespaceBtn' name='ChangeNamespaceBtn' value='Change Namespace' ";
            result += "onclick='javascript:ChangeNamespace_Click()' />";
            return result;
        }

        protected void DoSearch()
        {
            LinkMaker lm = TheLinkMaker;

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
                    permittedTopics.Add(topic); 
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
            StringBuilder strbldr = new StringBuilder();


            strbldr.AppendLine("<table cellspacing='0' cellpadding='2' border='0'>");
            strbldr.AppendLine("<thead>");
            strbldr.AppendLine("<td class=\"SearchColumnHeading\" width=\"300\">Topic</td>");
            strbldr.AppendLine("<td class=\"SearchColumnHeading\" width=\"100\">Modified</td>");
            strbldr.AppendLine("<td class=\"SearchColumnHeading\" width=\"100\">File Lock Status</td>");
            if (storeManager.HasNamespacePermission(NamespacePermission.Manage))
            {
                strbldr.AppendLine("<td class=\"SearchColumnHeading\" width=\"300\">Change File Lock</td>");
            }
            strbldr.AppendLine("<td class=\"SearchColumnHeading\" width=\"200\">Author: ");
            strbldr.AppendLine("<select  onchange='filter()' class='SearchColumnFilterBox' id='AuthorFilter'>");
            strbldr.AppendLine("<option value='" + All + "'>" + All + "</option>");
            foreach (string author in authors.Values)
            {
                strbldr.AppendLine("<option value='" + author + "'>" + author + "</option>");
            }
            strbldr.AppendLine(@"</select>");
            strbldr.AppendLine("</td>");

            strbldr.AppendLine("</thead><tbody id=\"MainTable\">");

            int row = 0;
            foreach (QualifiedTopicName topic in permittedTopics)
            {
                strbldr.AppendLine("<tr id=\"row" + row + "\" class=\"" + (((row & 1) == 0) ? "SearchOddRow" : "SearchEvenRow") + "\">");
                row++;

                strbldr.AppendLine("<td>");
                strbldr.AppendLine("<b><a title=\"" + topic.DottedName + "\"  href=\"" + lm.LinkToTopic(topic) + "\">");
                strbldr.AppendLine(topic.LocalName);
                strbldr.AppendLine("</a></b>");
                strbldr.AppendLine("</td>");

                strbldr.AppendLine("<td>");
                DateTime stamp = storeManager.GetTopicLastModificationTime(topic.LocalName);
                if (stamp.Date == DateTime.Now.Date)
                {
                    strbldr.AppendLine(stamp.ToString("h:mm tt"));
                }
                else
                {
                    strbldr.AppendLine(stamp.ToString("MM/dd/yyyy h:mm tt"));
                }
                strbldr.AppendLine("</td>");
                strbldr.AppendLine("<td>");
                strbldr.AppendLine(storeManager.GetTopicInfo(topic.LocalName).IsLocked ? "Is Locked" : "Is Unlocked");
                strbldr.AppendLine("</td>");
                if (storeManager.HasNamespacePermission(NamespacePermission.Manage))
                {
                    strbldr.AppendLine("<td>");
                    if (!storeManager.GetTopicInfo(topic.LocalName).IsLocked)
                    {
                        strbldr.AppendLine("<input type=\"button\" value=\"Create Lock\" id=\"" + topic.LocalName + "_Btn\" ");
                        strbldr.AppendLine("onclick=\"javascript:FileAction_Click('" + topic.DottedName + "','Lock')\" />");
                    }
                    else
                    {
                        strbldr.AppendLine("<input type=\"button\" value=\"Remove Lock\" id=\"" + topic.LocalName + "_Btn\" ");
                        strbldr.AppendLine("onclick=\"javascript:FileAction_Click('" + topic.DottedName + "','Unlock')\" />");
                    }
                    strbldr.AppendLine("</td>");
                }
                strbldr.AppendLine("<td>");
                strbldr.AppendLine(Escape((string)(authorMap[topic])));
                strbldr.AppendLine("</td>");
                strbldr.AppendLine("</tr>");
                strbldr.AppendLine();
            }
            strbldr.AppendLine("</tbody></table>");
            Response.Write(strbldr.ToString());

        }
        private void ProcessPost()
        {
            //NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(_preferredNamespace);
            _action = Request.Form["fileaction"];
            _topic = Request.Form["topic"];

            TopicName topic = new TopicName(_topic);
            UnqualifiedTopicName unqualifiedtopic = new UnqualifiedTopicName(topic.LocalName);

            NamespaceManager _namespacemgr = Federation.NamespaceManagerForTopic(topic);
            if (_namespacemgr.HasNamespacePermission(NamespacePermission.Manage))
            {
                if (_action == "Lock")
                {
                    _namespacemgr.LockTopic(unqualifiedtopic);
                }
                else if (_action == "Unlock")
                {
                    _namespacemgr.UnlockTopic(unqualifiedtopic);
                }
            }
        }
    }
}