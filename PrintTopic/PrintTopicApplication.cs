using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using FlexWiki; 

namespace PrintTopic
{
    class PrintTopicApplication : IWikiApplication
    {
        private readonly string _configPath;
        private FederationConfiguration _federationConfiguration; 
        private readonly LinkMaker _linkMaker; 
        private readonly ITimeProvider _timeProvider = new DefaultTimeProvider();


        public PrintTopicApplication(string configPath, LinkMaker linkMaker)
        {
            _configPath = configPath;
            _linkMaker = linkMaker; 
        }

        public FederationConfiguration FederationConfiguration
        {
            get 
            {
                if (_federationConfiguration == null)
                {
                    XmlSerializer ser = new XmlSerializer(typeof(FederationConfiguration));
                    using (FileStream fileStream = new FileStream(_configPath, FileMode.Open,
                        FileAccess.Read, FileShare.Read))
                    {

                        _federationConfiguration = (FederationConfiguration) ser.Deserialize(fileStream);
                    }
                }

                return _federationConfiguration; 
            }
        }
        public LinkMaker LinkMaker
        {
            get { return _linkMaker; }
        }
        public OutputFormat OutputFormat
        {
            get { return OutputFormat.HTML; }
        }
        public ITimeProvider TimeProvider
        {
            get { return _timeProvider; }
        }

        public void Log(string source, LogLevel level, string message)
        {
            // no-op
        }
        public void LogDebug(string source, string message)
        {
            // no-op
        }
        public void LogError(string source, string message)
        {
            // no-op
        }
        public void LogInfo(string source, string message)
        {
            // no-op
        }
        public void LogWarning(string source, string message)
        {
            // no-op
        }
        public string ResolveRelativePath(string path)
        {
            throw new NotImplementedException(); 
        }
        public void WriteFederationConfiguration()
        {
            throw new NotImplementedException(); 
        }


    }
}
