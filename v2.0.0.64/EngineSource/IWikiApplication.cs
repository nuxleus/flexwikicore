using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public interface IWikiApplication
    {
        FederationConfiguration FederationConfiguration { get; }
        bool IsTransportSecure { get; }
        LinkMaker LinkMaker { get; }
        OutputFormat OutputFormat { get; }
        ITimeProvider TimeProvider { get; }

        //void AppendToLog(string logfile, string message);
        void Log(string source, LogLevel level, string message);
        void LogDebug(string source, string message);
        void LogError(string source, string message);
        void LogInfo(string source, string message);
        void LogWarning(string source, string message); 
        string ResolveRelativePath(string path); 
        void WriteFederationConfiguration();
    }
}
