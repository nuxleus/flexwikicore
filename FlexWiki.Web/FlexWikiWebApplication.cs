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
using System.Collections.Generic;
using System.IO;
using System.Web; 
using System.Xml;
using System.Xml.Serialization;

using log4net;
using log4net.Config;
using log4net.Core;

using FlexWiki.Caching;
using FlexWiki.Collections; 

namespace FlexWiki.Web
{
    public class FlexWikiWebApplication : IWikiApplication
    {
        // Constants
        private const string c_outputCachePrefix = "outputCache://"; 

        // Fields 
        private FlexWikiWebApplicationConfiguration _applicationConfiguration;
        private AspNetWikiCache _cache = new AspNetWikiCache(); 
        private readonly object _configFileLock = new object();
        private readonly string _configPath;
        private readonly LinkMaker _linkMaker;
        private readonly OutputFormat _outputFormat;
        private readonly ITimeProvider _timeProvider = new DefaultTimeProvider();
		private Dictionary<string, object> _properties = new Dictionary<string,object>();

        // Constructors

        public FlexWikiWebApplication(LinkMaker linkMaker)
            : this(GetFlexWikiConfigurationPath(), linkMaker)
        {
        }

        // This one is only used by the BuildVerificationTests and should go away
        // at some point. Don't use it. 
        public FlexWikiWebApplication(string configPath, LinkMaker linkMaker)
            :
            this(configPath, linkMaker, OutputFormat.HTML)
        {
        }

        public FlexWikiWebApplication(string configPath, LinkMaker linkMaker,
            OutputFormat outputFormat)
        {
            _configPath = configPath;
            _linkMaker = linkMaker;
            _outputFormat = outputFormat;

            LoadConfiguration();
            // We no longer watch the config file for changes because it was causing too 
            // many problems. Now there's just a "reload configuration" button in 
            // the admin app (not yet implemented). 
            //WatchConfiguration();

            string log4NetConfigPath = "log4net.config";
            if (_applicationConfiguration.Log4NetConfigPath != null)
            {
                log4NetConfigPath = _applicationConfiguration.Log4NetConfigPath;
            }
            XmlConfigurator.ConfigureAndWatch(new FileInfo(ResolveRelativePath(log4NetConfigPath)));
        }

        // Properties 

        public FlexWikiWebApplicationConfiguration ApplicationConfiguration
        {
            get { return _applicationConfiguration; }
        }
        public string ApplicationConfigurationPath
        {
            get { return GetFlexWikiConfigurationPath(); }
        }
        public IWikiCache Cache
        {
            get { return _cache; }
        }
        public FederationConfiguration FederationConfiguration
        {
            get
            {
                if (_applicationConfiguration == null)
                {
                    return null;
                }

                return _applicationConfiguration.FederationConfiguration;
            }
        }
        public static string ForceWindowsAuthenticationCookieName
        {
            get { return "FlexWikiForceWindowsAuth"; }
        }
        public bool IsTransportSecure
        {
            get
            {
                HttpContext context = HttpContext.Current;

                // If there's been a thread switch or for whatever reason we can't 
                // find the current context, we have to assume the communications are
                // local and therefore secure. This is specifically the case with the
                // newsletter manager, which runs on a background thread, and should
                // have access to the content without having to require a secure
                // connection. 
                if (context == null)
                {
                    return true; 
                }

                return context.Request.IsSecureConnection; 
            }
        }
        public LinkMaker LinkMaker
        {
            get { return _linkMaker; }
        }
        public OutputFormat OutputFormat
        {
            get { return _outputFormat; }
        }
        public TimeSpan OutputCacheDuration
        {
            get
            {
                if (!ApplicationConfiguration.OutputCacheDurationSpecified)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    TimeSpan value;
                    if (TimeSpan.TryParse(ApplicationConfiguration.OutputCacheDuration, out value))
                    {
                        return value; 
                    }

                    return TimeSpan.Zero;
                }
            }
        }
		/// <summary>
		/// Gets or sets the properties available outside of this IWikiApplication. <see cref="IWikiApplication.this[string]"/>
		/// </summary>
		/// <param name="key">key of the value to get</param>
		/// <returns>the value associated with the key or null if the key is not found</returns>
		public object this[string key]
		{
			get { return _properties.ContainsKey(key) ? _properties[key] : null; }
			internal set { _properties[key] = value; } 
		}
        public ITimeProvider TimeProvider
        {
            get { return _timeProvider; }
        }

        internal void ReloadConfiguration()
        {
            LoadConfiguration(); 
        }

        // Methods
        public string CachedRender(string cacheKey, CachedRenderCallback renderCallback)
        {
            if (OutputCacheDuration.Equals(TimeSpan.Zero))
            {
                return renderCallback(); 
            }

            cacheKey = c_outputCachePrefix + cacheKey; 
            CachedRenderResult cachedRenderResult = Cache[cacheKey] as CachedRenderResult; 
            if (cachedRenderResult != null)
            {
                TimeSpan sinceCached = TimeProvider.Now - cachedRenderResult.WhenRendered; 
                // This next line would be great to have, but it fails under IIS7 due to not
                // running in integrated pipeline mode. 
                //HttpContext.Current.Response.Headers.Add("X-FlexWiki-RenderedFromCacheKey", cacheKey);
                if (sinceCached < OutputCacheDuration)
                {
                    LogDebug("FlexWiki.Web.FlexWikiWebApplication", "Rendering " + cacheKey + " from cache."); 
                    return FormatCachedContents(cachedRenderResult.Contents, cacheKey);
                }
            }

            using (RequestContext.Create())
            {
                string contents = renderCallback();
                DependencyCollection dependencies = RequestContext.Current.Dependencies;
                if (!RequestContext.Current.IsUncacheable)
                {
                    Cache[cacheKey] = new CachedRenderResult(contents, dependencies, TimeProvider.Now);
                }
                return contents; 
            }
        }
        public void Log(string source, LogLevel level, string message)
        {
            if (level == LogLevel.Debug)
            {
                LogDebug(source, message);
            }
            else if (level == LogLevel.Error)
            {
                LogError(source, message);
            }
            else if (level == LogLevel.Info)
            {
                LogInfo(source, message);
            }
            else if (level == LogLevel.Warn)
            {
                LogWarning(source, message);
            }

            // Otherwise we fail silently. While normally I would throw an argument exception here, 
            // that's not a good idea in your error handling code, which the logging code generally is. 
        }
        public void LogDebug(string source, string message)
        {
            LogManager.GetLogger(source).Debug(message);
        }
        public void LogError(string source, string message)
        {
            LogManager.GetLogger(source).Error(message);
        }
        public void LogInfo(string source, string message)
        {
            LogManager.GetLogger(source).Info(message);
        }
        public void LogWarning(string source, string message)
        {
            LogManager.GetLogger(source).Warn(message);
        }
        public void NoteModification(Modification modification)
        {
            foreach (string key in Cache.Keys)
            {
                if (key.StartsWith(c_outputCachePrefix))
                {
                    CachedRenderResult cachedRenderResult = Cache[key] as CachedRenderResult;
                    if (cachedRenderResult != null)
                    {
                        if (cachedRenderResult.Dependencies.IsInvalidatedBy(modification))
                        {
                            Cache[key] = null; 
                        }
                    }
                }
            }
        }
        public string ResolveRelativePath(string path)
        {
            string configDir = Path.GetDirectoryName(_configPath);

            path = Path.Combine(configDir, path);

            path = Path.GetFullPath(path);

            return path;
        }
        public void WriteFederationConfiguration()
        {
            LogInfo(this.GetType().ToString(), "Writing updated wiki configuration to: " + _configPath);
            XmlSerializer serializer = new XmlSerializer(typeof(FlexWikiWebApplicationConfiguration));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.IndentChars = "  ";
            settings.Indent = true;

            // In order to prevent the system from trying to read the file while 
            // we're still writing it, we take a lock. 
            lock (_configFileLock)
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(_configPath, settings))
                {
                    serializer.Serialize(xmlWriter, _applicationConfiguration);
                    xmlWriter.Close();
                }
            }
        }

        private string FormatCachedContents(string contents, string key)
        {
            return string.Format("<!-- Begin item ({0}) rendered from cache at {1} -->{2}<!-- End item ({0}) rendered from cache -->",
                key, TimeProvider.Now, contents); 
        }
        private static string GetFlexWikiConfigurationPath()
        {
            string configPath = System.Configuration.ConfigurationManager.AppSettings["FlexWikiConfigurationPath"];

            if (configPath == null)
            {
                configPath = "flexwiki.config";
            }

            string basedir = Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            configPath = Path.Combine(basedir, configPath);

            if (File.Exists(configPath))
            {
                return configPath;
            }

            throw new Exception("No configuration file was found. The configuration file should " +
                "either be at flexwiki.config in the application root or in the location specified by the " +
                "FlexWikiConfigurationPath app setting. All paths are relative to the directory " +
                "containing web.config (or app.config, as appropriate).");
        }
        private void LoadConfiguration()
        {
            LogInfo(this.GetType().ToString(), "Loading wiki configuration from: " + _configPath);
            XmlSerializer ser = new XmlSerializer(typeof(FlexWikiWebApplicationConfiguration));

            // In order to prevent the system from trying to write the file while 
            // we're still reading it, we take a lock. 
            lock (_configFileLock)
            {
                using (FileStream fileStream = new FileStream(_configPath, FileMode.Open,
                    FileAccess.Read, FileShare.Read))
                {
                    _applicationConfiguration = (FlexWikiWebApplicationConfiguration)ser.Deserialize(fileStream);
                    fileStream.Close();
                }
            }
            this["DisableEditServiceWrite"] = _applicationConfiguration.DisableEditServiceWrite;
            this["DisableXslTransform"] = _applicationConfiguration.DisableXslTransform;
            this["DisableThreadedMessaging"] = _applicationConfiguration.DisableThreadedMessaging;
			if (_applicationConfiguration.AlternateStylesheets.Length > 0)
			{
				string[] titles = new string[_applicationConfiguration.AlternateStylesheets.Length+1];
				titles[0] = PageUtilities.DefaultStylesheet;
				for (int i = 1; i < titles.Length; i++)
				{
					titles[i] = _applicationConfiguration.AlternateStylesheets[i-1].Title;
				}
				this["AlternateStyles"] = titles;
			}
            LogInfo(this.GetType().ToString(), "Successfully loaded wiki configuration from: " + _configPath);
        }
        //private void WatchConfiguration()
        //{
        //    // FileSystemWatcher introduces so many problems I decided to punt it. Instead, there's a 
        //    // "reload configuration" button in the admin app. 
        //    FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(_configPath),
        //        Path.GetFileName(_configPath));
        //    watcher.Changed += new FileSystemEventHandler(WatcherChanged);
        //    watcher.Created += new FileSystemEventHandler(WatcherChanged);
        //    watcher.Deleted += new FileSystemEventHandler(WatcherChanged);
        //    watcher.Renamed += new RenamedEventHandler(WatcherRenamed);
        //    watcher.EnableRaisingEvents = true;
        //}
        //private void WatcherChanged(object sender, FileSystemEventArgs e)
        //{
        //    LogInfo(this.GetType().ToString(), "Wiki configuration file changed. Reloading.");
        //    LoadConfiguration();
        //}
        //private void WatcherRenamed(object sender, RenamedEventArgs e)
        //{
        //    LogInfo(this.GetType().ToString(), "Wiki configuration file changed. Reloading.");
        //    LoadConfiguration();
        //}
    }
}
