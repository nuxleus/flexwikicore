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

            ArrayList entryList = new ArrayList();
            StringBuilder feed = new StringBuilder();
            StringBuilder errFeed = new StringBuilder();
            StringBuilder errEntry = new StringBuilder();
            bool feedInit = false;
            bool feedError = false;
            bool entryError = false;
            string author = "";
            string feedTitle = "";
            string feedUUID = "";
            string feedLogo = "";
            string feedIcon= "";
            string blogCategory ="";


            feed.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            feed.AppendLine(@"<!DOCTYPE message [");
            feed.AppendLine("<!ENTITY nbsp \"&#160;\"> ]>");
            feed.AppendLine(@"<feed xmlns=""http://www.w3.org/2005/Atom"">");

            errFeed.AppendLine(@"<pre id=""Error"" class=""Error"" style=""background: #F00;"">");
            errFeed.AppendLine("The Feed errors described below need to be corrected");

            if (storeManager.GetTopicInfo(topic.LocalName).HasProperty("Title"))
            {
                feedTitle = storeManager.GetTopicProperty(topic.LocalName, "Title").LastValue;
            }
            else
            {
                feedError = true;
            }
            if (storeManager.GetTopicInfo(topic.LocalName).HasProperty("Creator"))
            {
                author = storeManager.GetTopicProperty(topic.LocalName, "Creator").LastValue;
            }
            string feedLink = BaseUrl + lm.LinkToTopic(topic);
            if (storeManager.GetTopicInfo(topic.LocalName).HasProperty("FeedUUID"))
            {
                feedUUID = storeManager.GetTopicProperty(topic.LocalName, "FeedUUID").LastValue;
            }
            else
            {
                feedError = true;
            }
            if (storeManager.GetTopicInfo(topic.LocalName).HasProperty("FeedLogo"))
            {
                feedLogo = storeManager.GetTopicProperty(topic.LocalName, "FeedLogo").LastValue;
            }
            if (storeManager.GetTopicInfo(topic.LocalName).HasProperty("FeedIcon"))
            {
                feedIcon = storeManager.GetTopicProperty(topic.LocalName, "FeedIcon").LastValue;
            }
            if (storeManager.GetTopicInfo(topic.LocalName).HasProperty("BlogCategory"))
            {
                blogCategory = storeManager.GetTopicProperty(topic.LocalName, "BlogCategory").LastValue;
            }
            if (storeManager.GetTopicInfo(topic.LocalName).HasProperty("BlogTopics"))
            {
                entryList = storeManager.GetTopicInfo(topic.LocalName).GetListProperty("BlogTopics");
            }

            ParserEngine _parser = new ParserEngine(storeManager.Federation);

            errEntry.AppendLine(@"<pre id=""Error"" class=""Error"" style=""background: #F00;"">");
            errEntry.AppendLine("The Entry errors described below need to be corrected");

            if (entryList.Count > 0)
            {
                foreach (string entryTopicName in entryList)
                {
                    string entryTitle = "";
                    string entryAuthor = "";
                    string entryLink = "";
                    string entryUUID = "";
                    string entryContent = "";

                    System.DateTime lastModified = new DateTime();
                    QualifiedTopicRevision entryTopicRev = new QualifiedTopicRevision(entryTopicName, storeManager.Namespace);
                    TopicVersionInfo topicVerInfo = new TopicVersionInfo(storeManager.Federation, entryTopicRev);
                    if (topicVerInfo.Exists)
                    {
                        if (storeManager.GetTopicInfo(entryTopicName).HasProperty("Creator"))
                        {
                            entryAuthor = storeManager.GetTopicProperty(entryTopicName, "Creator").LastValue;
                            if (String.IsNullOrEmpty(author))
                            {
                                author = entryAuthor;
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(author))
                            {
                                entryAuthor = author;
                            }
                            else
                            {
                                feedError = true;
                                errFeed.AppendLine(" Error: The feed does not have a property named 'Creator' or the property does not contain a value.");
                                author = "Author Unspecified";
                                entryAuthor = author;
                            }
                        }

                        if (storeManager.GetTopicInfo(entryTopicName).HasProperty("Title"))
                        {
                            entryTitle = storeManager.GetTopicProperty(entryTopicName, "Title").LastValue;
                        }
                        else
                        {
                            entryTitle = storeManager.GetTopicInfo(entryTopicName).ExposedFormattedName;
                        }
                        lastModified = storeManager.GetTopicLastModificationTime(entryTopicName).ToUniversalTime();


                        if (!feedInit)
                        {
                            if (feedError)
                            {
                                feedError = true;
                                if (entryList.Count == 0)
                                {
                                    errFeed.AppendLine("Error: Feed does not have a property 'BlogTopics' or that property value is empty");
                                }
                                if (!String.IsNullOrEmpty(feedTitle))
                                {
                                    feed.AppendFormat(@"<title>{0}</title>", feedTitle);
                                }
                                else
                                {
                                    feed.AppendLine("<title>Feed title Unspecified</title>");
                                    errFeed.AppendLine("Error: Feed does not have a property 'Title' or value is empty.");
                                }
                                feed.AppendLine(@"<author>");
                                feed.AppendFormat(@"<name>{0}</name>", author);
                                feed.AppendLine(@"</author>");
                                if (!String.IsNullOrEmpty(feedUUID))
                                {
                                    feed.AppendFormat(@"<id>urn:uuid:{0}</id>", feedUUID);
                                }
                                else
                                {
                                    feed.AppendFormat(@"<id>urn:uuid:{0}</id>", System.Guid.NewGuid().ToString());
                                    errFeed.AppendLine("Error: Feed does not have a property 'FeedUUID' or value is empty. A temporary value has been assigned");
                                }
                                feed.AppendFormat(@"<updated>{0:s}Z</updated>", lastModified);
                                if (!String.IsNullOrEmpty(feedLogo))
                                {
                                    feed.AppendFormat(@"<logo>{0}</logo>", feedLogo);
                                }
                                if (!String.IsNullOrEmpty(feedIcon))
                                {
                                    feed.AppendFormat(@"<icon>{0}</icon>", feedIcon);
                                }
                                if (!String.IsNullOrEmpty(blogCategory))
                                {
                                    feed.AppendFormat(@"<category term=""{0}"" />", blogCategory);
                                }
                                errFeed.AppendLine("</pre>");

                            }
                            else
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
                            }
                            feedInit = true;
                        }
                        if (storeManager.GetTopicInfo(entryTopicName).HasProperty("EntryUUID"))
                        {
                            entryUUID = storeManager.GetTopicProperty(entryTopicName, "EntryUUID").LastValue;
                        }
                        else
                        {
                            entryError = true;
                            entryUUID = System.Guid.NewGuid().ToString();
                            errEntry.AppendLine("Error: Entry does not have a property 'EntryUUID' or value is empty. A temporary value has been assigned");
                        }
                        entryLink = BaseUrl + lm.LinkToTopic(entryTopicRev);
                        errEntry.AppendLine("</pre>");

                        string body = storeManager.GetTopicInfo(entryTopicName).GetProperty("_Body").ToString();
                        WomDocument xmldoc = new WomDocument(null);
                        xmldoc = _parser.FormatTextFragment(body, entryTopicRev, storeManager, true, 600);
                        xmldoc.ParsedDocument = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE message [
<!ENTITY nbsp ""&#160;""> ]>
             <div id=""womDocRoot"">" + xmldoc.ParsedDocument + "</div>";
                        entryContent = _parser.WikiToPresentation(xmldoc.XmlDoc);
                        //string entryContent = Formatter.FormattedTopic(entryTopicRev, OutputFormat.HTML, null, storeManager.Federation, lm);
                        feed.AppendLine(@"<entry>");
                        feed.AppendFormat(@"<title>{0}</title>", entryTitle);
                        feed.AppendFormat(@"<link href=""{0}"" />", entryLink);
                        feed.AppendFormat(@"<id>urn:uuid:{0}</id>", entryUUID);
                        feed.AppendFormat(@"<updated>{0:s}Z</updated>", lastModified);
                        //feed.AppendLine(@"<content>");
                        feed.AppendLine(@"<content type=""xhtml"">");
                        feed.AppendLine(@"<div xmlns=""http://www.w3.org/1999/xhtml"">");
                        if (feedError)
                        {
                            feed.Append(errFeed.ToString());
                        }
                        if (entryError)
                        {
                            feed.Append(errEntry.ToString());
                        }
                        feed.Append(entryContent);
                        feed.AppendLine("</div>");
                        feed.AppendLine("</content>");
                        if (storeManager.GetTopicInfo(entryTopicName).HasProperty("Keywords"))
                        {
                            ArrayList keywordsList = storeManager.GetTopicInfo(entryTopicName).KeywordsList;
                            foreach (string keyword in keywordsList)
                            {
                                feed.AppendFormat(@"<category term=""{0}"" />", keyword);
                            }
                        }

                        feed.AppendLine("</entry>");
                    }
                    else
                    {
                        entryError = true;
                        errEntry.AppendLine("Error: The topic specified in the property 'BlogContents' does not exist.");
                        if (feedError)
                        {
                            feedError = true;
                            if (entryList.Count == 0)
                            {
                                errFeed.AppendLine("Error: Feed does not have a property 'BlogTopics' or that property value is empty");
                            }
                            if (!String.IsNullOrEmpty(feedTitle))
                            {
                                feed.AppendFormat(@"<title>{0}</title>", feedTitle);
                            }
                            else
                            {
                                feed.AppendLine("<title>Feed title Unspecified</title>");
                                errFeed.AppendLine("Error: Feed does not have a property 'Title' or value is empty.");
                            }
                            if (!String.IsNullOrEmpty(author))
                            {
                                feed.AppendLine(@"<author>");
                                feed.AppendFormat(@"<name>{0}</name>", author);
                                feed.AppendLine(@"</author>");
                            }
                            else
                            {
                                feed.AppendLine(@"<author>");
                                feed.AppendFormat(@"<name>{0}</name>", "Author Unspecified");
                                feed.AppendLine(@"</author>");
                                errFeed.AppendLine("Error: Feed does not have a property 'Creator' or value is empty. This may occur as there was no valid entry found to provide a 'Creator' property.");
                            }
                            if (!String.IsNullOrEmpty(feedUUID))
                            {
                                feed.AppendFormat(@"<id>urn:uuid:{0}</id>", feedUUID);
                            }
                            else
                            {
                                feed.AppendFormat(@"<id>urn:uuid:{0}</id>", System.Guid.NewGuid().ToString());
                                errFeed.AppendLine("Error: Feed does not have a property 'FeedUUID' or value is empty. A temporary value has been assigned");
                            }
                            feed.AppendFormat(@"<updated>{0:s}Z</updated>", lastModified);
                            if (!String.IsNullOrEmpty(feedLogo))
                            {
                                feed.AppendFormat(@"<logo>{0}</logo>", feedLogo);
                            }
                            if (!String.IsNullOrEmpty(feedIcon))
                            {
                                feed.AppendFormat(@"<icon>{0}</icon>", feedIcon);
                            }
                            if (!String.IsNullOrEmpty(blogCategory))
                            {
                                feed.AppendFormat(@"<category term=""{0}"" />", blogCategory);
                            }
                            errFeed.AppendLine("</pre>");

                        }
                        else
                        {
                            feed.AppendFormat(@"<title>{0}</title>", feedTitle);
                            feed.AppendFormat(@"<link href=""{0}"" rel=""self"" type=""application/atom+xml"" />", feedLink);
                            feed.AppendFormat(@"<updated>{0:s}Z</updated>", lastModified);
                            feed.AppendFormat(@"<icon>{0}</icon>", feedIcon);
                            feed.AppendFormat(@"<logo>{0}</logo>", feedLogo);
                            feed.AppendLine(@"<author>");
                            feed.AppendFormat(@"<name>{0}</name>", author);
                            feed.AppendLine(@"</author>");                 feed.AppendFormat(@"<id>urn:uuid:{0}</id>", feedUUID);
           
                            if (!String.IsNullOrEmpty(blogCategory))
                            {
                                feed.AppendFormat(@"<category term=""{0}"" />", blogCategory);
                            }
                        }
                        errEntry.AppendLine("</pre>");
                        feed.AppendLine(@"<entry>");
                        feed.AppendFormat(@"<title>{0}</title>", entryTitle);
                        feed.AppendFormat(@"<link href=""{0}"" />", entryLink);
                        feed.AppendFormat(@"<id>urn:uuid:{0}</id>", entryUUID);
                        feed.AppendFormat(@"<updated>{0:s}Z</updated>", lastModified);
                        //feed.AppendLine(@"<content>");
                        feed.AppendLine(@"<content type=""xhtml"">");
                        feed.AppendLine(@"<div xmlns=""http://www.w3.org/1999/xhtml"">");
                        if (feedError)
                        {
                            feed.Append(errFeed.ToString());
                        }
                        if (entryError)
                        {
                            feed.Append(errEntry.ToString());
                        }
                        feed.Append(entryContent);
                        feed.AppendLine("</div>");
                        feed.AppendLine("</content>");
                        if (storeManager.GetTopicInfo(entryTopicName).HasProperty("Keywords"))
                        {
                            ArrayList keywordsList = storeManager.GetTopicInfo(entryTopicName).KeywordsList;
                            foreach (string keyword in keywordsList)
                            {
                                feed.AppendFormat(@"<category term=""{0}"" />", keyword);
                            }
                        }

                        feed.AppendLine("</entry>");
                        

                          
                    }   //end topicVerInfo does not exist
                }
            }
            else
            {
                feedError = true;
                if (entryList.Count == 0)
                {
                    errFeed.AppendLine("Error: Feed does not have a property 'BlogTopics' or that property value is empty");
                }
                if (!String.IsNullOrEmpty(feedTitle))
                {
                    feed.AppendFormat(@"<title>{0}</title>", feedTitle);
                }
                else
                {
                    feed.AppendLine("<title>Feed title Unspecified</title>");
                    errFeed.AppendLine("Error: Feed does not have a property 'Title' or value is empty.");
                }
                if (!String.IsNullOrEmpty(author))
                {
                    feed.AppendLine(@"<author>");
                    feed.AppendFormat(@"<name>{0}</name>", author);
                    feed.AppendLine(@"</author>");
                }
                else
                {
                    feed.AppendLine(@"<author>");
                    feed.AppendFormat(@"<name>{0}</name>", "Author Unspecified");
                    feed.AppendLine(@"</author>");
                    errFeed.AppendLine("Error: Feed does not have a property 'Creator' or value is empty. This may occur as there was no valid entry found to provide a 'Creator' property.");
                }
                if (!String.IsNullOrEmpty(feedUUID))
                {
                    feed.AppendFormat(@"<id>urn:uuid:{0}</id>", feedUUID);
                }
                else
                {
                    feed.AppendFormat(@"<id>urn:uuid:{0}</id>", System.Guid.NewGuid().ToString());
                    errFeed.AppendLine("Error: Feed does not have a property 'FeedUUID' or value is empty. A temporary value has been assigned");
                }
                feed.AppendFormat(@"<updated>{0:s}Z</updated>", System.DateTime.Now.ToUniversalTime());
                if (!String.IsNullOrEmpty(feedLogo))
                {
                    feed.AppendFormat(@"<logo>{0}</logo>", feedLogo);
                }
                if (!String.IsNullOrEmpty(feedIcon))
                {
                    feed.AppendFormat(@"<icon>{0}</icon>", feedIcon);
                }
                if (!String.IsNullOrEmpty(blogCategory))
                {
                    feed.AppendFormat(@"<category term=""{0}"" />", blogCategory);
                }
                errFeed.AppendLine("</pre>");

                feed.AppendLine(@"<entry>");
                feed.AppendLine(@"<title>Error Information</title>");
                feed.AppendLine(@"<content type=""xhtml"">");
                feed.AppendLine(@"<div xmlns=""http://www.w3.org/1999/xhtml"">");
                feed.Append(errFeed.ToString());
                feed.AppendLine("</div>");
                feed.AppendLine("</content>");
                feed.AppendLine(@"</entry>");
            }
            feed.AppendLine("</feed>");
            Response.Write(feed.ToString());

        }
    }
}
