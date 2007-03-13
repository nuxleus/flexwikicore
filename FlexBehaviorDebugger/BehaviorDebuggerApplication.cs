using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using FlexWiki;

namespace FlexWiki.BeL.Debugger
{
    class BehaviorDebuggerApplication : IWikiApplication
    {
        private readonly string _configPath;
        private FederationConfiguration _federationConfiguration;
        private readonly ITimeProvider _timeProvider = new DefaultTimeProvider();


        public BehaviorDebuggerApplication(string configPath)
        {
            _configPath = configPath;
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

                        _federationConfiguration = (FederationConfiguration)ser.Deserialize(fileStream);
                    }
                }

                return _federationConfiguration;
            }
        }

        public LinkMaker LinkMaker
        {
            get { return null;  }
        }

        public OutputFormat OutputFormat
        {
            get { return OutputFormat.Testing; }
        }

        public ITimeProvider TimeProvider
        {
            get { return _timeProvider; }
        }

        public void AppendToLog(string logfile, string message)
        {
            throw new NotImplementedException();
        }

        public string ResolveRelativePath(string path)
        {
            throw new NotImplementedException(); 
        }

        public void WriteFederationConfiguration()
        {
            throw new NotImplementedException(); 
        }




        #region IWikiApplication Members


        public void Log(string source, LogLevel level, string message)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void LogDebug(string source, string message)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void LogError(string source, string message)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void LogInfo(string source, string message)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void LogWarning(string source, string message)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
