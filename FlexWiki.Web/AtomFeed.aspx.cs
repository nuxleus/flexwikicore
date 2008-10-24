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
using System.Xml;

using FlexWiki.Formatting;

namespace FlexWiki.Web
{
    public class AtomFeed: BasePage
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

        public void BuildFeed()
        {
            QualifiedTopicRevision topic = GetTopicVersionKey();
            NamespaceManager storeManager = Federation.NamespaceManagerForTopic(topic);
            LinkMaker lm = TheLinkMaker;
            

            StringBuilder feed = new StringBuilder();
            bool feedInit = false;

            feed.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            feed.AppendLine(@"<feed xmlns=""http://www.w3.org/2005/Atom"">");

            string feedTitle = storeManager.GetTopicProperty(topic.LocalName, "Title").LastValue;
            string feedLink = BaseUrl + lm.LinkToTopic(topic);
            string feedUUID = storeManager.GetTopicProperty(topic.LocalName, "FeedUUID").LastValue;
            string feedLogo = storeManager.GetTopicProperty(topic.LocalName, "FeedLogo").LastValue;
            string feedIcon = storeManager.GetTopicProperty(topic.LocalName, "FeedIcon").LastValue;
            string blogCategory = storeManager.GetTopicProperty(topic.LocalName, "BlogCategory").LastValue;
            ArrayList entryList = storeManager.GetTopicInfo(topic.LocalName).GetListProperty("BlogTopics");

            ParserEngine _parser = storeManager.Federation.Parser;

            foreach (string entryTopicName in entryList)
            {
                string author = storeManager.GetTopicProperty(entryTopicName, "Creator").LastValue;
                string entryTitle = "";
                if (storeManager.GetTopicInfo(entryTopicName).HasProperty("Title"))
                {
                    entryTitle = storeManager.GetTopicProperty(entryTopicName, "Title").LastValue;
                }
                else
                {
                    entryTitle = storeManager.GetTopicInfo(entryTopicName).ExposedFormattedName;
                }
                System.DateTime lastModified = storeManager.GetTopicLastModificationTime(entryTopicName).ToUniversalTime();

                string entryAuthor = author;
                if (!feedInit)
                {
                    feed.AppendFormat(@"<title>{0}</title>", feedTitle);
                    feed.AppendFormat(@"<link href=""{0}"" rel=""self"" type=""application/atom+xml"" />", feedLink);
                    feed.AppendFormat(@"<updated>{0:s}Z</updated>", lastModified);
                    feed.AppendFormat(@"<icon>{0}</icon>", feedIcon);
                    feed.AppendFormat(@"<logo>{0}</logo>", feedLogo);
                    feed.AppendLine(@"<author>");
                    feed.AppendFormat(@"<name>{0}</name>", author);
                    feed.AppendLine(@"</author>");
                    feed.AppendFormat(@"<id>urn:uuid:{0}</id>", feedUUID);
                    if (!String.IsNullOrEmpty(blogCategory))
                    {
                        feed.AppendFormat(@"<category term=""{0}"" />", blogCategory);
                    }
                    feedInit = true;
                }
                string entryUUID = storeManager.GetTopicProperty(entryTopicName, "EntryUUID").LastValue;
                QualifiedTopicRevision entryTopicRev = new QualifiedTopicRevision(entryTopicName, storeManager.Namespace);
                string entryLink = BaseUrl + lm.LinkToTopic(entryTopicRev);

                string body = storeManager.GetTopicInfo(entryTopicName).GetProperty("_Body").ToString();
                WomDocument xmldoc = _parser.ProcessText(body, entryTopicRev, storeManager, true, 600);
                xmldoc.ParsedDocument = @"<div><div>" + xmldoc.ParsedDocument + "</div></div>";
                string entryContent = _parser.WikiToPresentation(xmldoc.XmlDoc);
                //string entryContent = Formatter.FormattedTopic(entryTopicRev, OutputFormat.HTML, null, storeManager.Federation, lm);
                feed.AppendLine(@"<entry>");
                feed.AppendFormat(@"<title>{0}</title>", entryTitle);
                feed.AppendFormat(@"<link href=""{0}"" />", entryLink);
                feed.AppendFormat(@"<id>urn:uuid:{0}</id>", entryUUID);
                feed.AppendFormat(@"<updated>{0:s}Z</updated>", lastModified);
                //feed.AppendLine(@"<content>");
                feed.AppendLine(@"<content type=""xhtml"">");
                feed.AppendLine(@"<div xmlns=""http://www.w3.org/1999/xhtml"">");
                feed.Append(entryContent);
                feed.AppendLine("</div>");
                feed.AppendLine("</content>");
                if (storeManager.GetTopicInfo(entryTopicName).HasProperty("Keywords"))
                {
                    ArrayList keywordsList = storeManager.GetTopicInfo(topic.LocalName).KeywordsList;
                    foreach (string keyword in keywordsList)
                    {
                        feed.AppendFormat(@"<category term=""{0}"" />", keyword);
                    }
                }
                feed.AppendLine("</entry>");

            }
            feed.AppendLine("</feed>");
            Response.Write(feed.ToString());

        }
    }
}
