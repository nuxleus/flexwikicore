using System;
using System.Web;

using FlexWiki.Security; 

namespace FlexWiki.Web
{
    public static class PageUtilities
    {
        public static string RootUrl
        {
            get
            {
                string path = HttpContext.Current.Request.ApplicationPath;
                if (path.EndsWith("/"))
                {
                    return path;
                }
                else
                {
                    return path + "/";
                }
            }
        }

        public static NamespaceManager DefaultNamespaceManager(Federation federation)
        {
            return federation.NamespaceManagerForNamespace(federation.DefaultNamespace);
        }
        public static QualifiedTopicRevision GetTopicRevision(Federation federation)
        {
            string topic;

            topic = HttpContext.Current.Request.PathInfo;
            if (topic.StartsWith("/"))
            {
                topic = topic.Substring(1);
            }

            // See if we're dealign with old style references or new ones
            // OLD: My.Name.Space.Topic
            // NEW: My.Name.Space/Topic.html
            bool isNewStyle = topic.IndexOf("/") != -1;	// if we have a slash, it's new

            // OK, we've got the namespace and the name now
            QualifiedTopicRevision revision;
            if (topic == null || topic.Length == 0)
            {
                revision = new QualifiedTopicRevision(DefaultNamespaceManager(federation).HomePage,
                    DefaultNamespaceManager(federation).Namespace);
            }
            else
            {
                if (isNewStyle)
                {
                    string ns, top;
                    int slash = topic.IndexOf("/");
                    ns = topic.Substring(0, slash);
                    top = topic.Substring(slash + 1);

                    int tailDot = top.LastIndexOf(".");
                    if (tailDot != -1)
                        top = top.Substring(0, tailDot);	// trim of the extension (e.g., ".html")

                    revision = new QualifiedTopicRevision(ns + "." + top);
                }
                else
                {
                    revision = new QualifiedTopicRevision(topic);
                }
            }
            return revision;

        }
        public static string InsertStylesheetReferences(Federation federation, FlexWikiWebApplication wikiApplication)
        {
            string answer = MainStylesheetReference();
            QualifiedTopicRevision revision = GetTopicRevision(federation);
            string styleSheet = null;
            if (revision.IsQualified)
            {
                try
                {
                    styleSheet = federation.GetTopicPropertyValue(revision, "Stylesheet");
                }
                catch (FlexWikiAuthorizationException)
                {
                    // We don't want to blow up just because we can't read the topic. Continue as 
                    // if there was no stylesheet
                }
            }

            if (!string.IsNullOrEmpty(styleSheet))
            {
                answer += "\n<LINK href='" + styleSheet + "' type='text/css' rel='stylesheet'>";
            }
            else
            {
                string styleOverride = wikiApplication.ApplicationConfiguration.OverrideStylesheet;
                if (styleOverride != null && styleOverride.Length > 0)
                {
                    answer += "\n<LINK href='" + styleOverride + "' type='text/css' rel='stylesheet'>";
                }
            }

            return answer;
        }
        public static string MainStylesheetReference()
        {
            return "<LINK href='" + RootUrl + "wiki.css' type='text/css' rel='stylesheet'>";
        }
    }
}