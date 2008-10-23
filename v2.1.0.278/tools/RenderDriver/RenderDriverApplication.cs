using System;
using System.IO; 

namespace FlexWiki.Tools.RenderDriver
{
    public class RenderDriverApplication : IWikiApplication
    {
        private readonly InMemoryCache _cache = new InMemoryCache();
        private readonly FederationConfiguration _federationConfiguration;
        private readonly LinkMaker _linkMaker;
        private readonly DefaultTimeProvider _timeProvider = new DefaultTimeProvider(); 

        public RenderDriverApplication(FederationConfiguration federationConfiguration, LinkMaker linkMaker)
        {
            _federationConfiguration = federationConfiguration;
            _linkMaker = linkMaker; 
        }

        public FlexWiki.Caching.IWikiCache Cache
        {
            get { return _cache; }
        }

        public FederationConfiguration FederationConfiguration
        {
            get { return _federationConfiguration; }
        }

        public bool IsTransportSecure
        {
            get { return false; }
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
            get { return _timeProvider;  }
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
            else
            {
                throw new ArgumentException("Unrecognized log level " + level.ToString()); 
            }
        }

        public void LogDebug(string source, string message)
        {
            Console.WriteLine("DEBUG: {0} {1}", source, message); 
        }

        public void LogError(string source, string message)
        {
            Console.WriteLine("ERROR: {0} {1}", source, message); 
        }

        public void LogInfo(string source, string message)
        {
            Console.WriteLine("INFO : {0} {1}", source, message); 
        }

        public void LogWarning(string source, string message)
        {
            Console.WriteLine("WARN : {0} {1}", source, message); 
        }

        public string ResolveRelativePath(string path)
        {
            return Path.Combine(Environment.CurrentDirectory, path); 
        }

        public void WriteFederationConfiguration()
        {
            throw new NotImplementedException();
        }

        public object this[string key]
        {
            get { return null; }
        }
    }
}
