using System;

using FlexWiki.Caching; 

namespace FlexWiki
{
    public interface IWikiApplication
    {
        IWikiCache Cache { get; }
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
