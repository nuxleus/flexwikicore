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
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Web;


namespace FlexWiki
{
    /// <summary> 
    /// LinkMaker understand how to make links to the various pages that make the wiki work
    /// </summary>
    [ExposedClass("LinkMaker", "Builds hyperlinks to various important pages")]
    public class LinkMaker : BELObject
    {
        // Fields

        private QualifiedTopicRevision _returnToTopicForEditLinks;
        private string _siteURL;

        // Constructors

        public LinkMaker(string siteURL)
        {
            _siteURL = siteURL;
        }

        // Properties

        public string SiteURL
        {
            get
            {
                return _siteURL;
            }
        }
        public QualifiedTopicRevision ReturnToTopicForEditLinks
        {
            get
            {
                return _returnToTopicForEditLinks;
            }
            set
            {
                _returnToTopicForEditLinks = value;
            }
        }


        // Methods

        public LinkMaker Clone()
        {
            return new LinkMaker(_siteURL);
        }
        /// <summary>
        /// Creates the Change User Profile link
        /// </summary>
        /// <param name="topic">TopicName for the link</param>
        /// <returns></returns>
        public string LinkToChangeUserProfile(int userID)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(SiteURL);
            builder.Append("UserProfile.aspx?UserID=");
            builder.Append(userID.ToString());
            builder.Append("&Mode=2");
            return builder.ToString();
        }
        /// <summary>
        /// Creates the Create User Profile link
        /// </summary>
        /// <param name="topic">TopicName for the link</param>
        /// <returns></returns>
        public string LinkToCreateUserProfile()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(SiteURL);
            builder.Append("UserProfile.aspx?UserID=0");
            builder.Append("&amp;Mode=1");
            return builder.ToString();
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to compare two versions for the given topic")]
        public string LinkToCompare(string fullTopicName, int diff, int oldid)
        {
            return SimpleLinkTo("Compare.aspx" + (fullTopicName != null ? "?topic=" + HttpUtility.UrlEncode(fullTopicName) : "") + ((diff >= 0) ? "&amp;diff=" + diff.ToString() : string.Empty) + ((oldid >= 0) ? "&amp;oldid=" + oldid.ToString() : string.Empty));
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the page that lets the user edit the given topic")]
        public string LinkToEditTopic(string topic)
        {
            return EditLink(topic);
        }
        public string LinkToEditTopic(TopicName topic)
        {
            return EditLink(topic.DottedName);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the given image")]
        public string LinkToImage(string s)
        {
            return SimpleLinkTo(s);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the page that allows a user to login")]
        public string LinkToLogin(string topic)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(SiteURL);
            builder.Append("Login.aspx?ReturnURL=" + HttpUtility.UrlEncode(TopicLink(topic, false, null)));
            return builder.ToString();
        }
        public string LinkToLogin(TopicRevision topic)
        {
            return LinkToLogin(topic.DottedNameWithVersion);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the page that logs off the current user")]
        public string LinkToLogoff(string topic)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(SiteURL);
            builder.Append("Logoff.aspx?ReturnURL=" + HttpUtility.UrlEncode(TopicLink(topic, false, null)));
            return builder.ToString();
        }
        public string LinkToLogoff(TopicRevision topic)
        {
            return LinkToLogoff(topic.DottedNameWithVersion);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the lost and found for the given namespace")]
        public string LinkToLostAndFound(string ns)
        {
            return SimpleLinkTo("LostAndFound.aspx" + (ns != null ? "?namespace=" + ns : ""));
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the 'print' view of the given topic")]
        public string LinkToPrintView(string topic)
        {
            return PrintLink(topic);
        }
        public string LinkToPrintView(TopicRevision topic)
        {
            return PrintLink(topic.DottedNameWithVersion);
        }
        public string LinkToQuicklink()
        {
            return SimpleLinkTo("QuickLink.aspx");
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the recent changes list for the given namespace")]
        public string LinkToRecentChanges(string ns)
        {
            return SimpleLinkTo("LastModified.aspx" + (ns != null ? "?namespace=" + ns : ""));
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the page that allows the given topic to be renamed")]
        public string LinkToRename(string fullyQualifiedTopicName)
        {
            return SimpleLinkTo("Rename.aspx" + (fullyQualifiedTopicName != null ? "?topic=" + fullyQualifiedTopicName : ""));
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the topic that processes a restore")]
        public string LinkToRestore(string topic)
        {
            return RestoreLink(topic);
        }
        public string LinkToRestore(TopicRevision topic)
        {
            return RestoreLink(topic.DottedNameWithVersion);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to search for the given string in the given namespace")]
        public string LinkToSearchFor(string Namespace, string searchExpression)
        {
            if (Namespace != null)
            {
                return SimpleLinkTo("Search.aspx" + "?namespace=" + HttpUtility.UrlEncode(Namespace) + (searchExpression != null ? "&amp;search=" + HttpUtility.UrlEncode(searchExpression) : ""));
            }
            else
            {
                return SimpleLinkTo("Search.aspx" + (searchExpression != null ? "?search=" + HttpUtility.UrlEncode(searchExpression) : ""));
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the search page for the given namespace")]
        public string LinkToSearchNamespace(string ns)
        {
            return SimpleLinkTo("Search.aspx" + (ns != null ? "?namespace=" + ns : ""));
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the list of subscriptions for the given namespace (or null for all)")]
        public string LinkToSubscriptions(string ns)
        {
            return SimpleLinkTo("RssFace.aspx" + (ns != null ? "?namespace=" + ns : ""));
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to the given topic")]
        public string LinkToTopic(string topic)
        {
            return TopicLink(topic, false, null);
        }
        public string LinkToTopic(TopicName topic)
        {
            return LinkToTopic(topic.DottedName);
        }
        public string LinkToTopic(TopicRevision topic)
        {
            return LinkToTopic(topic, false);
        }
        public string LinkToTopic(TopicRevision topic, bool showDiffs)
        {
            return TopicLink(topic.DottedNameWithVersion, showDiffs, null);
        }
        public string LinkToTopic(TopicRevision topic, bool showDiffs, NameValueCollection extraQueryParms)
        {
            return TopicLink(topic.DottedNameWithVersion, showDiffs, extraQueryParms);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to display the given topic with diffs highlighted")]
        public string LinkToTopicWithDiffs(string topic)
        {
            return TopicLink(topic, true, null);
        }
        public string LinkToUser(string user)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<a href=\"mailto:");
            builder.Append(user);
            builder.Append("\">");
            builder.Append(user);
            builder.Append("</a>");
            return builder.ToString();
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a link to a list of all the versions for the given topic")]
        public string LinkToVersions(string fullTopicName)
        {
            return SimpleLinkTo("Versions.aspx" + (fullTopicName != null ? "?topic=" + HttpUtility.UrlEncode(fullTopicName) : ""));
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer an absolute link on this site.")]
        public string SimpleLinkTo(string target)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(SiteURL);
            builder.Append(target);
            return builder.ToString();
        }

        private string EditLink(string top)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(SiteURL);
            builder.Append("WikiEdit.aspx?topic=");
            builder.Append(HttpUtility.UrlEncode(top));
            if (ReturnToTopicForEditLinks != null)
            {
                builder.Append("&amp;return=" + HttpUtility.UrlEncode(ReturnToTopicForEditLinks.DottedName));
            }

            return builder.ToString();
        }
        private string PrintLink(string top)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(SiteURL);
            builder.Append("Print.aspx/");
            builder.Append(top);
            return builder.ToString();
        }
        /// <summary>
        /// Creates the Restore link
        /// </summary>
        /// <param name="topic">TopicName for the link</param>
        /// <returns></returns>
        private string RestoreLink(string top)
        {
            NameValueCollection extras = new NameValueCollection();
            extras.Add("restore", "y");
            return TopicLink(top, false, extras);
        }
        private string TopicLink(string top, bool showDiffs, NameValueCollection extraQueryParms)
        {
            StringBuilder builder = new StringBuilder();
            TopicRevision topic = new TopicRevision(top);
            builder.Append(SiteURL);
            builder.Append("default.aspx");

            // A null or empty topic links us to the wiki homepage
            if (!string.IsNullOrEmpty(top))
            {
                builder.Append("/");
                if (topic.Namespace != null && topic.Namespace != "")
                {
                    builder.Append(HttpUtility.UrlEncode(topic.Namespace) + "/");
                }
                builder.Append(topic.LocalName);
                if (topic.Version != null)
                {
                    builder.Append("(" + HttpUtility.UrlEncode(topic.Version) + ")");
                }
                builder.Append(".html");		// hard coded for now -- later we'll be cooler! (maybe)
            }
            StringBuilder query = new StringBuilder();
            if (showDiffs)
            {
                query.Append("diff=y");
            }
            if (extraQueryParms != null)
            {
                foreach (string each in extraQueryParms)
                {
                    if (query.Length != 0)
                    {
                        query.Append("&amp;");
                    }
                    query.Append(each + "=" + HttpUtility.UrlEncode((string)(extraQueryParms[each])));
                }
            }
            if (query.Length != 0)
            {
                builder.Append("?" + query.ToString());
            }
            return builder.ToString();
        }
    }
}
