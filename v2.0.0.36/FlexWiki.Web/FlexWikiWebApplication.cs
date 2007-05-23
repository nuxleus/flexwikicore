using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using log4net;
using log4net.Config;
using log4net.Core;

namespace FlexWiki.Web
{
    public class FlexWikiWebApplication : IWikiApplication
    {

        // Fields 
        private FlexWikiWebApplicationConfiguration _applicationConfiguration;
        private readonly string _configPath;
        private readonly LinkMaker _linkMaker;
        private readonly OutputFormat _outputFormat;
        private readonly ITimeProvider _timeProvider = new DefaultTimeProvider();

        // Constructors

        public FlexWikiWebApplication(LinkMaker linkMaker)
            : this(GetFlexWikiConfigurationPath(), linkMaker)
        {
        }

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
            WatchConfiguration();

            _linkMaker.MakeAbsoluteUrls = _applicationConfiguration.MakeAbsoluteUrls;

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
            get
            {
                return _applicationConfiguration;
            }
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
        public LinkMaker LinkMaker
        {
            get { return _linkMaker; }
        }
        public OutputFormat OutputFormat
        {
            get { return _outputFormat; }
        }
        public ITimeProvider TimeProvider
        {
            get { return _timeProvider; }
        }


        // Methods

        //public void AppendToLog(string logfile, string message)
        //{
        //    string logpath = Path.Combine(Path.GetDirectoryName(_configPath), logfile);

        //    using (StreamWriter streamWriter = File.AppendText(logpath))
        //    {
        //        streamWriter.WriteLine(message);
        //    }
        //}
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
        public string ResolveRelativePath(string path)
        {
            string configDir = Path.GetDirectoryName(_configPath);

            path = Path.Combine(configDir, path);

            path = Path.GetFullPath(path);

            return path;
        }
        public void WriteFederationConfiguration()
        {
            throw new NotImplementedException("Not yet implemented.");
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
            using (FileStream fileStream = new FileStream(_configPath, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                _applicationConfiguration = (FlexWikiWebApplicationConfiguration)ser.Deserialize(fileStream);
            }
            LogInfo(this.GetType().ToString(), "Successfully loaded wiki configuration from: " + _configPath);
        }
        private void WatchConfiguration()
        {
            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(_configPath));
            watcher.Changed += new FileSystemEventHandler(WatcherChanged);
            watcher.Created += new FileSystemEventHandler(WatcherChanged);
            watcher.Deleted += new FileSystemEventHandler(WatcherChanged);
            watcher.Renamed += new RenamedEventHandler(WatcherRenamed);
            watcher.EnableRaisingEvents = true;
        }
        private void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            LogInfo(this.GetType().ToString(), "Wiki configuration file changed. Reloading.");
            LoadConfiguration();
        }
        private void WatcherRenamed(object sender, RenamedEventArgs e)
        {
            LogInfo(this.GetType().ToString(), "Wiki configuration file changed. Reloading.");
            LoadConfiguration();
        }



    }
}