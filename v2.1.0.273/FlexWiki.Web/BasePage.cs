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
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using FlexWiki;
using FlexWiki.Web.Newsletters;
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for Page.
    /// </summary>   
    public class BasePage : System.Web.UI.Page
    {
        private static string s_newsletterDaemonCacheKey = "---NEWSLETTERDAEMON---";

        private LinkMaker _linkMaker;
        // TODO: Does this really need to be ThreadStatic?
        [ThreadStatic]
        private static HttpResponse _response;
        private UIResponse _uiResponse;
        private string _userPrefix;

        protected bool _metaTags = false;
        protected bool _javaScript = false;
        protected bool _incTopBorder = false;
        protected bool _incLeftBorder = false;
        protected bool _incRightBorder = false;
        protected bool _incBottomBorder = false;
        protected bool isBlacklistedRestore = false;
        protected string formattedBody;

        protected string urlForDiffs;
        protected string urlForNoDiffs;

        protected Regex dirInclude = new Regex("{{([a-zA-Z]+)}}");
        protected StringBuilder leftBorder = new StringBuilder();
        protected StringBuilder rightBorder = new StringBuilder();
        protected StringBuilder topBorder = new StringBuilder();
        protected StringBuilder bottomBorder = new StringBuilder();
        protected string tempbottom;
        protected string templeft;
        protected string tempright;
        protected string temptop;

        public BasePage()
        {
            Load += new EventHandler(Page_Load);
        }

        protected string CreateCacheKey(string id)
        {
            return string.Format("{0}-{1}-{2}",
                HttpContext.Current.Request.Url.ToString(),
                UserId(),
                id);
        }
        /// <summary>
        /// Returns the URL suitable for composition with FlexWiki web pages to create
        /// valid FlexWiki links. This method does *not* return the scheme, servername, 
        /// or port. 
        /// </summary>
        /// <param name="req">An <see cref="HttpRequest"/> object to use to determine
        /// the root URL.</param>
        /// <returns>A string representing the root URL for the application.</returns>
        protected string RootUrl
        {
            get
            {
                return PageUtilities.RootUrl;
            }
        }

        protected NamespaceManager manager
        {
            get { return Federation.NamespaceManagerForTopic(topic); }
        }
        protected QualifiedTopicRevision topic
        {
            get
            {
                return GetTopicVersionKey();
            }
        }

        /// <summary>
        /// Answer a string to identify the current visitor.  Is authentication is up and the user is authenticated, answer the
        /// authenticated user's name (e.g., a Windows account name).  Otherwise answer the IP address of the visitor (possibly 
        /// with a user specified prefix).
        /// </summary>
        public string VisitorIdentityString
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    return User.Identity.Name;
                }
                string prefix = _userPrefix;
                if (prefix == null)
                {
                    return Request.UserHostAddress;
                }
                return prefix + "-" + Request.UserHostAddress;
            }
        }

        protected string DefaultNamespace
        {
            get
            {
                return Federation.DefaultNamespace;
            }
        }
        protected NamespaceManager DefaultNamespaceManager
        {
            get
            {
                return Federation.NamespaceManagerForNamespace(DefaultNamespace);
            }
        }
        protected Federation Federation
        {
            get
            {
                return (Federation)(Application[Constants.FederationCacheKey]);
            }
        }
        protected ParserEngine Parser
        {
            get { return (ParserEngine)(Application[Constants.EngineCacheKey]); }
        }
        protected FlexWikiWebApplication FlexWikiWebApplication
        {
            get
            {
                return (FlexWikiWebApplication)Federation.Application;
            }
        }
        protected bool IsPost
        {
            get
            {
                return Request.RequestType == "POST";
            }
        }
        protected LinkMaker TheLinkMaker
        {
            get
            {
                return _linkMaker;
            }
        }
        protected NewsletterDaemon TheNewsletterDaemon
        {
            get
            {
                return (NewsletterDaemon)(Application[s_newsletterDaemonCacheKey]);
            }
            set
            {
                Application[s_newsletterDaemonCacheKey] = value;
            }
        }
        protected UIResponse UIResponse
        {
            get
            {
                if (_uiResponse != null)
                    return _uiResponse;
                _uiResponse = new UIResponse(_response, RelativeBase);
                return _uiResponse;
            }
        }
        private string UserId()
        {
            if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                return "authenticated:" + Thread.CurrentPrincipal.Identity.Name;
            }
            else
            {
                return "anonymous";
            }
        }
        protected string UserPrefix
        {
            get
            {
                return _userPrefix;
            }
        }
        protected FlexWikiWebApplication WikiApplication
        {
            get { return Federation.Application as FlexWikiWebApplication; }
        }


        protected string InsertScripts()
        {
            StringBuilder headbldr = new StringBuilder();
            headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}WikiDefault.js\" type=\"text/javascript\"></script>\r\n", RootUrl);
            headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}WikiTopicBar.js\" type=\"text/javascript\"></script>\r\n", RootUrl);
            headbldr.AppendFormat("<script language=\"javascript\" src=\"{0}WikiMenu.js\" type=\"text/javascript\"></script>\r\n", RootUrl);
            return headbldr.ToString();
        }
        protected void InitBorders()
        {
            bool doBorders = WikiApplication.ApplicationConfiguration.EnableBordersAllPages;
            InitBorders(doBorders);

        }
        protected void InitBorders(bool doBorders)
        {

            if (doBorders)
            {
                Federation.Parser = Parser;
                tempbottom = WikiApplication.CachedRender(
                    CreateCacheKey("BottomBorder"),
                    delegate
                    {
                        return Federation.GetTopicFormattedBorder(topic, Border.Bottom);
                    });

                Federation.Parser = Parser;
                templeft = WikiApplication.CachedRender(
                    CreateCacheKey("LeftBorder"),
                    delegate
                    {
                        return Federation.GetTopicFormattedBorder(topic, Border.Left);
                    });

                Federation.Parser = Parser;
                tempright = WikiApplication.CachedRender(
                        CreateCacheKey("RightBorder"),
                        delegate
                        {
                            return Federation.GetTopicFormattedBorder(topic, Border.Right);
                        });

                Federation.Parser = Parser;
                temptop = WikiApplication.CachedRender(
                        CreateCacheKey("TopBorder"),
                        delegate
                        {
                            return Federation.GetTopicFormattedBorder(topic, Border.Top);
                        });

                if (!String.IsNullOrEmpty(templeft))
                {
                    leftBorder.AppendLine("<div class=\"Border\" id=\"LeftBorder\">");
                    leftBorder.AppendLine(templeft);
                    leftBorder.AppendLine("</div>");
                }

                if (!String.IsNullOrEmpty(tempright))
                {
                    rightBorder.AppendLine("<div class=\"Border\" id=\"RightBorder\">");
                    rightBorder.AppendLine(tempright);
                    rightBorder.AppendLine("</div>");
                }

                if (!String.IsNullOrEmpty(temptop))
                {
                    topBorder.AppendLine("<div class=\"Border\" id=\"TopBorder\">");
                    topBorder.AppendLine(temptop);
                    topBorder.AppendLine("</div>");
                }

                if (!String.IsNullOrEmpty(tempbottom))
                {
                    bottomBorder.AppendLine("<div class=\"Border\" id=\"BottomBorder\">");
                    bottomBorder.AppendLine(tempbottom);
                    bottomBorder.AppendLine("</div>");
                }
            }
        }
        protected string InsertLeftTopBorders()
        {
            StringBuilder strbldr = new StringBuilder();


            // Insert the TopBorder if it is required.
            if (!String.IsNullOrEmpty(temptop))
            {
                strbldr.AppendLine(topBorder.ToString());
            }

            // Insert the LeftBorder if it is required.
            if (!String.IsNullOrEmpty(templeft))
            {
                strbldr.AppendLine(leftBorder.ToString());
            }
            topBorder = null;
            leftBorder = null;
            return strbldr.ToString();
        }
        protected string InsertRightBottomBorders()
        {
            StringBuilder strbldr = new StringBuilder();

            if (!String.IsNullOrEmpty(templeft + temptop))
            {
                //strbldr.AppendLine("</div>");
            }

            // Insert the RightBorder if it is required.
            if (!String.IsNullOrEmpty(tempright))
            {
                strbldr.AppendLine(rightBorder.ToString());
            }

            // Insert the BottomBorder if it is required.
            if (!String.IsNullOrEmpty(tempbottom))
            {
                strbldr.AppendLine(bottomBorder.ToString());
            }
            rightBorder = null;
            bottomBorder = null;
            return strbldr.ToString();
        }

        public static void Spit(string s)
        {
            //			if (_Response != null)
            //				_Response.Write(s + "<br>");
        }
        public static void WriteNicely(TextWriter output, object obj)
        {
            IHTMLRenderable renderable = obj as IHTMLRenderable;
            if (renderable == null)
                output.Write(EscapeHTML(obj.ToString()) + "<br />");
            else
                renderable.RenderToHTML(output);
        }
        public static void WriteLineNicely(TextWriter output, object obj)
        {
            IHTMLRenderable renderable = obj as IHTMLRenderable;
            if (renderable == null)
                output.WriteLine(obj.ToString());
            else
                renderable.RenderToHTML(output);
        }

        /// <summary>
        /// Check to see if the format of the configuration file needs to be updated to a new version.
        /// If yes, do a redirect to the upgrader page...
        /// </summary>
        /// <returns>true if upgrade required</returns>
        protected virtual bool CheckForConfigurationFormatUpgrade()
        {
            // TODO: figure out how to make this work with the 2.0 bits
            return false;
            //throw new NotImplementedException("Not sure yet if we need this.");
            //FederationConfiguration config = FederationConfigurationFromFile;
            //if (config == null)
            //    return false;

            //// As the format of the file evolves, this logic below will evolve:

            //// Check to see if there are old-style deprecated <namespace> elements.
            //bool needed = false;
            //if (config.DeprecatedNamespaceDefinitions != null && config.DeprecatedNamespaceDefinitions.Count > 0)
            //    needed = true;

            //foreach(NamespaceProviderDefinition providerDefinition in config.NamespaceMappings)
            //{
            //    // If we find atleast one namespace provider definition with no Id
            //    // or Id that is empty we force an upgrade.
            //    if(providerDefinition.Id == null || (providerDefinition.Id != null && providerDefinition.Id.Length == 0))
            //    {
            //        needed = true;
            //        break;
            //    }
            //}

            //// OK we've figured it out
            //if (!needed)
            //    return false;

            //Response.Redirect(RelativeBase + "UpgradeConfigurationFile.aspx");
            //return true;
        }
        protected static void ClosePane(TextWriter w)
        {
            w.Write("</td></tr>");
            w.Write("</table>");
        }
        protected virtual void DefaultPageLoad()
        {
            // Setup the federation -- either find the existing one or create a new one
            _linkMaker = new LinkMaker(RootUrl);
            EstablishFederation();
            EstablishParser();
            

            MinimalPageLoad();
            PageLoad();
        }
        protected virtual void EnsurePluginsLoaded()
        {
            string loaded = (string)(Application["PluginsLoaded"]);
            if (loaded == "yes")
                return;
            LoadPlugins();
            Application["PluginsLoaded"] = "yes";
        }
        /// <summary>
        /// Escape the given string for special HTML characters (greater-than, etc.).
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The new string</returns>
        protected static string EscapeHTML(string input)
        {
            if (input == null)
                return "";
            // replace HTML special characters with character entities
            // this has the side-effect of stripping all markup from text
            string str = input;
            str = Regex.Replace(str, "&", "&amp;");
            str = Regex.Replace(str, "\"", "&quot;");
            str = Regex.Replace(str, "<", "&lt;");
            str = Regex.Replace(str, ">", "&gt;");
            return str;
        }
        protected void EstablishParser()
        {
            if (Parser != null)
            {
                return;
            }
            ParserEngine parser = new ParserEngine(Federation);
            SetParserEngine(parser);
        }
        //protected FederationConfiguration FederationConfigurationFromFile
        //{
        //    get
        //    {
        //        string config = ConfigurationManager.AppSettings["FederationNamespaceMapFile"];
        //        if (config == null)
        //            return null;
        //        string mappedConfig = MapPath(config);
        //        if (mappedConfig == null)
        //            return null;
        //        return FederationConfiguration.FromFile(mappedConfig);
        //    }
        //}

        /// <summary>
        /// Returns the URL suitable for composition with FlexWiki web pages to create
        /// valid FlexWiki links. This method *does* return the scheme, servername, 
        /// and port. 
        /// </summary>
        /// <param name="req">An <see cref="HttpRequest"/> object to use to determine
        /// the root URL.</param>
        /// <returns>A string representing the root URL for the application.</returns>
        protected string FullRootUrl(HttpRequest req)
        {
            UriBuilder builder = new UriBuilder(req.Url.Scheme, req.Url.Host, req.Url.Port,
              req.ApplicationPath);
            string path = builder.ToString();
            if (path.EndsWith("/"))
            {
                return path;
            }
            else
            {
                return path + "/";
            }
        }
        protected QualifiedTopicRevision GetTopicVersionKey()
        {
            return PageUtilities.GetTopicRevision(Federation);
        }
        //		protected AbsoluteTopicName GetTopicName()
        //		{
        //			string topic;
        //			
        //			string topicFromQueryString = Request.QueryString["topic"];
        //			if (topicFromQueryString != null)
        //				topic = topicFromQueryString;
        //			else
        //			{
        //				topic = Request.PathInfo;
        //				if (topic.StartsWith("/"))
        //					topic = topic.Substring(1);
        //			}
        //
        //			RelativeTopicName rel;
        //			if (topic == null || topic.Length == 0)
        //				rel = new RelativeTopicName(DefaultContentBase.HomePage, DefaultContentBase.Namespace);
        //			else
        //				rel = new RelativeTopicName(topic);
        //			IList topics = DefaultContentBase.AllAbsoluteTopicNamesThatExist(rel);
        //			if (topics.Count == 0)
        //				return rel.AsAbsoluteTopicName(DefaultContentBase.Namespace);		// topic doesn't exist, assume in the wiki's home content base
        //			if (topics.Count > 1)
        //			{
        //				throw TopicIsAmbiguousException.ForTopic(rel);
        //			}
        //			AbsoluteTopicName answer = (AbsoluteTopicName)topics[0];
        //			answer.Version = rel.Version;
        //			return answer;
        //		}


        /// <summary>
        /// Add a visitor event to the session state VisitorEvents
        /// </summary>
        /// <param name="e"></param>
        protected void LogVisitorEvent(VisitorEvent e)
        {
            ArrayList list = (ArrayList)(System.Web.HttpContext.Current.Session["VisitorEvents"]);
            if (list == null)
            {
                list = new ArrayList();
                System.Web.HttpContext.Current.Session["VisitorEvents"] = list;
            }
            list.Insert(0, e);	// Adding to the front means we're keeping it sorted with latest first    
        }
        protected string InsertFavicon()
        {
            return PageUtilities.InsertFavicon(FlexWikiWebApplication);
        }
        protected string InsertStylesheetReferences()
        {
            return PageUtilities.InsertStylesheetReferences(Federation, FlexWikiWebApplication);
        }
        protected string MainStylesheetReference()
        {
            return PageUtilities.MainStylesheetReference();
        }
        //protected void InsertStylesheetReferences(HtmlHead head)
        //{
        //    PageUtilities.InsertStylesheetReferences(Federation, FlexWikiWebApplication, head);
        //}
        //protected void MainStylesheetReference(HtmlHead head)
        //{
        //    PageUtilities.MainStylesheetReference(head);
        //}
        protected string MessagePostFix(string body)
        {
            string wikiTalkMultiline = @"(?<text>(?<!"")@@\[.*\]@@(?!""))";
            string wikiTalkString = @"(?<text>(?<!"")@@[a-zA-Z]+[a-zA-Z0-9,\(\)\s]*@@(?!""))";
            string includeFile = @"(?<text>(?<!""){{[A-Z]+[a-zA-Z0-9\._]*}}(?!""))";
            string wikiTalkEscape = "\"\"${text}\"\"";

            Regex wikiTalkGetter = new Regex(wikiTalkString, RegexOptions.Singleline|RegexOptions.CultureInvariant);
            //Regex wikiTalkReplace = new Regex(wikiTalkEscape);
            //MatchCollection matches = wikiTalkGetter.Matches(body);
            string result = wikiTalkGetter.Replace(body,wikiTalkEscape);
            wikiTalkGetter = new Regex(wikiTalkMultiline, RegexOptions.Singleline|RegexOptions.CultureInvariant);
            result = wikiTalkGetter.Replace(result, wikiTalkEscape);
            wikiTalkGetter = new Regex(includeFile, RegexOptions.Singleline|RegexOptions.CultureInvariant);
            result = wikiTalkGetter.Replace(result, wikiTalkEscape);

            return result;
        }
        protected virtual void MinimalPageLoad()
        {
            _response = Response;
            EnsurePluginsLoaded();
        }
        protected static void OpenPane(TextWriter w, string title)
        {
            OpenPane(w, title, null, null, null);
        }
        protected static void OpenPane(TextWriter w, string title, string imageURL, string imageLink, string imageTip)
        {
            w.Write("<table cellspacing='0' class='SidebarTile' cellpadding='2' border='0'>");
            w.Write("<tr><td valign='middle' class='SidebarTileTitle' >");
            if (imageURL != null)
            {
                if (imageLink != null)
                    w.Write("<a href='" + imageLink + "'>");
                w.Write("<img align='absmiddle' border='0' style='margin-left: 1px; margin-right: 5px'  src='" + imageURL + @"' ");
                if (imageTip != null)
                    w.Write("title='" + imageTip + "'");
                w.Write(">");
                if (imageLink != null)
                    w.Write("</a>");
            }
            if (imageLink == null)
                w.Write(title);
            else
                w.Write("<a href=\"" + imageLink + "\">" + title + "</a>");
            w.Write("</td></tr>");
            w.Write("<tr class='SidebarTileBody'><td>");
        }
        protected virtual void PageLoad()
        {
            if (CheckForConfigurationFormatUpgrade())
            {
                return;
            }

            // Make sure we've setup a LogEventFactory for the federation
            string logFile = FlexWikiWebApplication.ApplicationConfiguration.LogPath;
            string relativeTo = Server.MapPath("~");
            if (logFile != null)
            {
                logFile = System.IO.Path.Combine(relativeTo, logFile);
            }
            ILogEventFactory factory = Federation.LogEventFactory;
            if (factory == null)
            {
                Federation.LogEventFactory = new WebApplicationLogEventFactory(Application, logFile);
            }

            string ns = Federation.DefaultNamespace;
            if (ns == null)
            {
                throw new Exception("No default namespace defined.");
            }

            NamespaceManager namespaceManager = Federation.NamespaceManagerForNamespace(ns);
            if (namespaceManager == null)
            {
                throw new Exception("Default namespace (" + ns + ") doesn't exist.");
            }

            EstablishNewsletterDaemon();

            ProcessUnauthenticatedUserName();
        }
        protected virtual string RelativeBase
        {
            get
            {
                return "";
            }
        }
        protected void ResetFederation()
        {
            SetFederation(null);
        }
        /// <summary>
        /// Restores the passed in Topic version as the current version
        /// </summary>
        /// <param name="topic">Topic Version to Restore</param>
        /// <returns></returns>
        protected TopicRevision RestorePreviousVersion(QualifiedTopicRevision topic)
        {
            LogEvent e = Federation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, topic.ToString(), LogEventType.WriteTopic);
            try
            {
                QualifiedTopicRevision newVersionName = new QualifiedTopicRevision(topic.LocalName, topic.Namespace);
                newVersionName.Version = TopicRevision.NewVersionStringForUser(VisitorIdentityString, Federation.TimeProvider);
                NamespaceManager namespaceManager = Federation.NamespaceManagerForNamespace(topic.Namespace);
                namespaceManager.WriteTopicAndNewVersion(newVersionName.LocalName,
                    Federation.Read(topic), VisitorIdentityString);
            }
            finally
            {
                e.Record();
            }
            return new QualifiedTopicRevision(topic.LocalName, topic.Namespace);
        }
        protected string SendMail(MailMessage message)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                client.Send(message);
            }
            catch (Exception e)
            {
                FlexWikiWebApplication.LogError(this.GetType().ToString(), "An error occurred trying to send mail: " +
                    e.ToString());
                return e.ToString();
            }
            // Bizarre semantics, but null indicates success. 
            return null;
        }
        protected string SendRequestsTo
        {
            get
            {
                return FlexWikiWebApplication.ApplicationConfiguration.SendNamespaceCreationRequestsTo;
            }
        }
        protected UpdateMonitor UpdateMonitor
        {
            get
            {
                UpdateMonitor answer = (UpdateMonitor)(Application["UpdateMonitor"]);
                if (answer != null)
                    return answer;
                answer = new UpdateMonitor(Federation);
                Application["UpdateMonitor"] = answer;
                return answer;
            }
        }

        protected void EstablishFederation()
        {
            if (Federation != null)
            {
                return;
                // If we have one, just make sure it's valid
                /*
                 * Federation.Validate();
                 * return;
                 */
                // throw new NotImplementedException("Do we need the validate call? What bad thing happens if it's gone?");
            }

            // nope - need a new one
            FlexWikiWebApplication application = new FlexWikiWebApplication(TheLinkMaker);
            Federation fed = new Federation(application);
            SetFederation(fed);

            // Setup event monitoring
            SetupUpdateMonitoring();
        }
        private void EstablishNewsletterDaemon()
        {
            if (!FlexWikiWebApplication.ApplicationConfiguration.NewsletterConfiguration.Enabled)
            {
                return;
            }

            if (TheNewsletterDaemon != null)
            {
                // If we have one, just make sure it's alive
                TheNewsletterDaemon.EnsureRunning();
                return;
            }

            // Collect up the styles that need to be inserted into the newsletters.  Do this by reading the wiki.css file and override css file (if there is one)
            string styles = "";
            try
            {
                string mainStylesFile = MapPath(RelativeBase + "wiki.css");
                using (TextReader s = new StreamReader(mainStylesFile))
                {
                    styles += s.ReadToEnd();
                }
                string styleOverride = FlexWikiWebApplication.ApplicationConfiguration.OverrideStylesheet;
                if (styleOverride != null && styleOverride != "")
                {
                    string overrideCSSFile = MapPath(styleOverride);
                    using (TextReader s = new StreamReader(overrideCSSFile))
                    {
                        styles += s.ReadToEnd();
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                // This call will compile away under release builds, but still
                // gives us a place to set a breakpoint and a variable to examine
                // under debug builds
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            styles = @"<style>" + styles + @"
</style>";

            bool sendAsAttachments = FlexWikiWebApplication.ApplicationConfiguration.NewsletterConfiguration.SendAsAttachments;

            // Use the NewsletterRootUrl config entry as the base URL for newsletters if present, otherwise
            // default to whatever URL was used for this request. 
            string rootUrl = FlexWikiWebApplication.ApplicationConfiguration.NewsletterConfiguration.RootUrl;

            if (rootUrl == null || rootUrl.Length == 0)
            {
                rootUrl = FullRootUrl(Request);
            }

            // Make sure it ends with a trailing slash
            if (!rootUrl.EndsWith("/"))
            {
                rootUrl += "/";
            }

            NewsletterDaemon daemon = new NewsletterDaemon(
                Federation,
                rootUrl,
                FlexWikiWebApplication.ApplicationConfiguration.NewsletterConfiguration.NewslettersFrom,
                styles,
                sendAsAttachments,
                FlexWikiWebApplication.ApplicationConfiguration.NewsletterConfiguration.AuthenticateAs);
            TheNewsletterDaemon = daemon;
            daemon.EnsureRunning();
        }
        private void LoadPlugins()
        {
            FederationConfiguration configuration = Federation.Configuration;
            if (configuration == null)
                return;

            foreach (string plugin in configuration.Plugins)
            {
                System.Reflection.Assembly.Load(plugin);
            }
        }
        private void Page_Load(object sender, EventArgs e)
        {
            DefaultPageLoad();
        }
        /// <summary>
        /// Load the user's identity prefix from the incoming web request (and change it also if so requested).
        /// </summary>
        private void ProcessUnauthenticatedUserName()
        {
            // First set the local value to whatever is in the incoming cookies
            HttpCookie incoming = Request.Cookies["User"];
            if (incoming != null)
                _userPrefix = incoming.Value;

            // Now see if there's an incoming form propertyName that changes this (only if we're non-authenticated)
            if (!Request.IsAuthenticated)
            {
                string pre = null;
                try
                {
                    string up = Request.Form["UserSuppliedName"];
                    pre = up;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    ex.ToString();	// quiet, compiler!
                }
                if (pre != null)
                {
                    if (_userPrefix != pre)
                    {
                        _userPrefix = pre;
                        HttpCookie cookie = new HttpCookie("User", pre);
                        DateTime dt = DateTime.Now;
                        TimeSpan ts = new TimeSpan(100, 0, 0, 0, 0);		// make it stick for 100 days
                        cookie.Expires = dt.Add(ts);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);
                    }
                }
            }

            System.Web.HttpContext.Current.Items["VisitorIdentityString"] = VisitorIdentityString;
        }
        protected void SetBorderFlags(string template)
        {
            MatchCollection matches = dirInclude.Matches(template);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    switch (match.ToString())
                    {
                        case "{{FlexWikiTopBorder}}":
                            _incTopBorder = true;
                            break;

                        case "{{FlexWikiHeaderInfo}}":
                            _javaScript = true;
                            _metaTags = true;
                            break;

                        case "{{FlexWikiJavaScript}}":
                            _javaScript = true;
                            break;

                        case "{{FlexWikiMetaTags}}":
                            _metaTags = true;
                            break;

                        case "{{FlexWikiLeftBorder}}":
                            _incLeftBorder = true;
                            break;

                        case "{{FlexWikiRightBorder}}":
                            _incRightBorder = true;
                            break;

                        case "{{FlexWikiBottomBorder}}":
                            _incBottomBorder = true;
                            break;

                        default:
                            break;
                    }
                }
            }
            if (_incTopBorder || _incLeftBorder || _incRightBorder || _incBottomBorder)
            {
                InitBorders();
            }
        }
        private void SetFederation(Federation fed)
        {
            Application[Constants.FederationCacheKey] = fed;
        }
        private void SetParserEngine(ParserEngine parser)
        {
            Application[Constants.EngineCacheKey] = parser;
        }
        private void SetupUpdateMonitoring()
        {
            UpdateMonitor.Start();
        }

        public T FindControl<T>(string id) where T : Control
        {
            return FindControl<T>(Page, id);
        }

        public static T FindControl<T>(Control startingControl, string id) where T : Control
        {
            //Published Friday, April 13, 2007 3:05 PM by Palermo4 
            // this is null by default
            T found = default(T);

            int controlCount = startingControl.Controls.Count;

            if (controlCount > 0)
            {
                for (int i = 0; i < controlCount; i++)
                {
                    Control activeControl = startingControl.Controls[i];
                    if (activeControl is T)
                    {
                        found = startingControl.Controls[i] as T;
                        if (string.Compare(id, found.ID, true) == 0) break;
                        else found = null;
                    }
                    else
                    {
                        found = FindControl<T>(activeControl, id);
                        if (found != null) break;
                    }
                }
            }
            return found;
        }


    }
}
