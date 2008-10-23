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
using System.IO;
using System.Web;
using System.Web.UI.HtmlControls;

using FlexWiki.Security;
using System.Text; 

namespace FlexWiki.Web
{
    public static class PageUtilities
    {
		public const string DefaultStylesheet = "Site Default";

        public static string BaseUrl
        {
            //a link string inlcuding the web protocol and server information
            get
            {
                string url = HttpContext.Current.Request.Url.AbsoluteUri;
                return url.Substring(0, url.IndexOf(RootUrl));
            }
        }
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
                if ((!String.IsNullOrEmpty(HttpContext.Current.Request.Form["Topic"])) && ((bool)federation.Application["EnableBordersAllPages"]) 
                            && (!String.IsNullOrEmpty(HttpContext.Current.Request.Form["Namespace"])))
                {
                    revision = new QualifiedTopicRevision(HttpContext.Current.Request.Form["Topic"], HttpContext.Current.Request.Form["Namespace"]);
                }
                else if ((!String.IsNullOrEmpty(HttpContext.Current.Request.QueryString["namespace"])) && ((bool)federation.Application["EnableBordersAllPages"]))
                {
                    if (HttpContext.Current.Request.QueryString["namespace"] != "[All]") //search All namespaces
                    {
                        NamespaceManager mgr = federation.NamespaceManagerForNamespace(HttpContext.Current.Request.QueryString["namespace"]);
                        revision = new QualifiedTopicRevision(mgr.HomePage, mgr.Namespace);
                    }
                    else
                    {
                        revision = new QualifiedTopicRevision(DefaultNamespaceManager(federation).HomePage,
                            DefaultNamespaceManager(federation).Namespace);
                    }

                }
                else if ((!String.IsNullOrEmpty(HttpContext.Current.Request.QueryString["topic"])) && ((bool)federation.Application["EnableBordersAllPages"]))
                {
                    string reqTopic = HttpContext.Current.Request.QueryString["topic"];
                    //NamespaceManager mgr = federation.NamespaceManagerForNamespace(reqTopic.Substring(0, reqTopic.IndexOf(".") - 1));
                    revision = new QualifiedTopicRevision(reqTopic.Substring(reqTopic.IndexOf(".") + 1), reqTopic.Substring(0, reqTopic.IndexOf(".")));
                }
                else
                {
                    revision = new QualifiedTopicRevision(DefaultNamespaceManager(federation).HomePage,
                        DefaultNamespaceManager(federation).Namespace);
                }
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
        public static string InsertFavicon(FlexWikiWebApplication wikiApplication)
        {
            string answer = "\n<link rel=\"shortcut icon\" href=\"{0}{1}\" />";
            string faviconFilePath = HttpContext.Current.Server.MapPath("favicon.ico");
            if (!wikiApplication.ApplicationConfiguration.DisableFavicon)
            {
                try
                {
                    if (File.Exists(faviconFilePath))
                    {
        				answer = string.Format(answer, RootUrl, "favicon.ico");
                        return answer;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
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
				answer += GetStylesheetLink(styleSheet, DefaultStylesheet, false);
            }
            else
            {
                string styleOverride = wikiApplication.ApplicationConfiguration.OverrideStylesheet;
                if (styleOverride != null && styleOverride.Length > 0)
                {
					answer += GetStylesheetLink(styleOverride, DefaultStylesheet, false);
                }
            }

			foreach(AlternateStylesheetConfiguration altStyle in wikiApplication.ApplicationConfiguration.AlternateStylesheets)
			{
				answer += GetStylesheetLink(altStyle.Href, altStyle.Title, true);
			}
            return answer;
        }
        public static string GetOverrideBordersContent(NamespaceManager manager, string scope)
        {
            string temp = "";

            switch (scope.ToLower())
            {
                case "none":
                   temp = "";
                   break;

                case "namespace":
                   temp = "";
                   if (manager.GetTopicProperty(manager.DefinitionTopicName.LocalName, "OverrideBorders") != null)
                      if (manager.GetTopicProperty(manager.DefinitionTopicName.LocalName, "OverrideBorders").LastValue != null)
                      {
                          temp = manager.GetTopicProperty(manager.DefinitionTopicName.LocalName, "OverrideBorders").LastValue;
                      }
                   break;

                case "federation":
                   NamespaceManager mgr = DefaultNamespaceManager(manager.Federation);
                   if (mgr.GetTopicProperty(mgr.DefinitionTopicName.LocalName, "OverrideBorders") != null)
                      if (mgr.GetTopicProperty(mgr.DefinitionTopicName.LocalName, "OverrideBorders").LastValue != null)
                      {
                          temp = mgr.GetTopicProperty(mgr.DefinitionTopicName.LocalName, "OverrideBorders").LastValue;
                      }
                   break;

                default:
                   temp = "";
                   break;
            }
            return temp;
        }
		public static string GetStylesheetLink(string styleOverride, string title, bool isAlternate)
		{
			string answer = "\n<link href=\"{0}{1}\" type=\"text/css\" rel=\"{2}\"{3}/>";
			string rel = isAlternate ? "alternate stylesheet" : "stylesheet";
			title = (title == null) ? "" : String.Format(" title=\"{0}\"", title);
			if (true == styleOverride.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
			{
				answer = string.Format(answer, "", styleOverride, rel, title);
			}
			else
			{
				answer = string.Format(answer, RootUrl, styleOverride, rel, title);
			}
			return answer;
		}

        public static string MainStylesheetReference()
        {
			return GetStylesheetLink("wiki.css", null, false);
        }

	}
}
