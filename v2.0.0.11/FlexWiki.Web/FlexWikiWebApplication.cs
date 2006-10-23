using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

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

        public void AppendToLog(string logfile, string message)
        {
            string logpath = Path.Combine(Path.GetDirectoryName(_configPath), logfile);

            using (StreamWriter streamWriter = File.AppendText(logpath))
            {
                streamWriter.WriteLine(message);
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
            throw new NotImplementedException("Not yet implemented.");
        }

        private static string GetFlexWikiConfigurationPath()
        {
            string configPath = System.Configuration.ConfigurationManager.AppSettings["FlexWikiConfigurationPath"];

            if (configPath == null)
            {
                configPath = Path.Combine("config", "flexwiki.config");
            }

            string basedir = Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            configPath = Path.Combine(basedir, configPath);

            if (File.Exists(configPath))
            {
                return configPath;
            }

            throw new Exception("No configuration file was found. The configuration file should " +
                "either be at config/flexwiki.config or in the location specified by the " +
                "FlexWikiConfigurationPath app setting. All paths are relative to the directory " +
                "containing web.config (or app.config, as appropriate.");
        }
        private void LoadConfiguration()
        {
            XmlSerializer ser = new XmlSerializer(typeof(FlexWikiWebApplicationConfiguration));
            using (FileStream fileStream = new FileStream(_configPath, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                _applicationConfiguration = (FlexWikiWebApplicationConfiguration) ser.Deserialize(fileStream);
            }
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
            LoadConfiguration();
        }
        private void WatcherRenamed(object sender, RenamedEventArgs e)
        {
            LoadConfiguration();
        }



    }
}
