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
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;

using FlexWiki.Formatting;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for Blade.
    /// </summary>
    public class Blade : BasePage
    {
        private void Page_Load(object sender, System.EventArgs e)
        {
            // Put user code to initialize the page here
        }

        private static Regex fieldAndClass = new Regex("(?<name>([_A-Z][a-zA-Z0-9]+))\\((?<class>[^\\)]*)\\)");

        protected void DoPage()
        {
            LinkMaker lm = TheLinkMaker;

            string[] topics = ((string)(Request.QueryString["topics"])).Split(new char[] { ',' });
            string[] fields = ((string)(Request.QueryString["properties"])).Split(new char[] { ',' });
            NamespaceManager storeManager;

            foreach (string topic in topics)
            {
                QualifiedTopicRevision abs;
                IList tops;
                if (topic.IndexOf('.') > 0)
                {
                    string[] names = topic.Split(new char[] { '.' });
                    storeManager = Federation.NamespaceManagerForNamespace(names[0]);
                    tops = storeManager.AllQualifiedTopicNamesThatExist(names[1]);

                }
                else
                {
                    storeManager = Federation.NamespaceManagerForNamespace(DefaultNamespace);
                    tops = DefaultNamespaceManager.AllQualifiedTopicNamesThatExist(topic);
                }

                if (tops.Count == 0)
                {
                    // topic doesn't exist, assume in the wiki's home content base
                    abs = new QualifiedTopicRevision(topic, DefaultNamespaceManager.Namespace);
                }
                else if (tops.Count > 1)
                {
                    throw TopicIsAmbiguousException.ForTopic(new TopicRevision(topic));
                }
                else	// we got just one!
                {
                    QualifiedTopicName extract = (QualifiedTopicName)tops[0];
                    abs = extract.AsQualifiedTopicRevision();
                }

                foreach (string field in fields)
                {
                    string fieldName;
                    string fieldClass;

                    fieldName = field;
                    fieldClass = null;

                    if (fieldAndClass.IsMatch(field))
                    {
                        Match fieldAndClassMatch = fieldAndClass.Match(field);
                        fieldName = fieldAndClassMatch.Groups["name"].Value;
                        fieldClass = fieldAndClassMatch.Groups["class"].Value;
                    }

                    string fieldValue = Federation.GetTopicPropertyValue(abs, fieldName);
                    string s1;
                    if (fieldName == "_Body")
                    {
                        s1 = Formatter.FormattedString(abs, fieldValue, OutputFormat.HTML, storeManager, TheLinkMaker);
                    }
                    else
                    {
                        s1 = fieldValue;
                    }
                    // YUCK!  We need to wrap the enclosing <p> (if present) and replace it with the <div>
                    s1 = s1.Trim();
                    if (s1.StartsWith("<p>"))
                        s1 = s1.Substring(3);
                    if (s1.EndsWith("</p>"))
                        s1 = s1.Substring(0, s1.Length - 4);

                    Response.Write("<div");
                    if (fieldClass != null)
                        Response.Write(" class='" + fieldClass + "'");
                    Response.Write(">" + s1 + "</div>");
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
    }
}
