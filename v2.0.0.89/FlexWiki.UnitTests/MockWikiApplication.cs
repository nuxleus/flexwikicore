using System;

using FlexWiki.Caching; 
using FlexWiki.UnitTests.Caching; 

namespace FlexWiki.UnitTests
{
    internal class MockWikiApplication : IWikiApplication
    {
        private MockCache _cache = new MockCache(); 
        private FederationConfiguration _configuration;
        private bool _isTransportSecure; 
        private LinkMaker _linkMaker;
        private OutputFormat _ouputFormat;
        private ITimeProvider _timeProvider; 


        public MockWikiApplication(FederationConfiguration configuration, LinkMaker linkMaker,
            OutputFormat outputFormat, ITimeProvider timeProvider)
        {
            _configuration = configuration;
            _linkMaker = linkMaker;
            _ouputFormat = outputFormat;
            _timeProvider = timeProvider; 

        }

        public IWikiCache Cache
        {
            get { return _cache; }
        }

        public FederationConfiguration FederationConfiguration
        {
            get { return _configuration; }
        }

        public bool IsTransportSecure
        {
            get { return _isTransportSecure; }
            set { _isTransportSecure = value; }
        }

        public LinkMaker LinkMaker
        {
            get { return _linkMaker; }
        }

        public OutputFormat OutputFormat
        {
            get { return _ouputFormat; }
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
            return System.IO.Path.Combine("FW:\\", path); 
        }
        public void WriteFederationConfiguration()
        {
            throw new NotImplementedException(); 
        }

    }
}
