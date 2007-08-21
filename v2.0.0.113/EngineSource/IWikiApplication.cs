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
		/// <summary>
		/// Gets the value associated with the specified key. 
		/// </summary>
		/// <remarks>
		/// This property bag is intended to provide a mechanism for the IWikiApplication to provide information and data to pieces
		/// of the Engine, (e.g. WikiTalk.) It is up to the implementing class what it wants to expose to the Engine. These values
		/// are exposed to WikiTalk via the <see cref="Foundation.Application(string key)"/> method.
		/// </remarks>
		/// <param name="key">The key of the value to get.</param>
		/// <returns>The value associated with the specified <paramref name="key"/>. If the specified key is not found, null is returned.</returns>
		object this[string key] { get; }
    }
}
