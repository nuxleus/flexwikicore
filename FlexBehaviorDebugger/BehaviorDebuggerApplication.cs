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
using System.Xml.Serialization;

using FlexWiki;
using FlexWiki.Caching; 

namespace FlexWiki.BeL.Debugger
{
    class BehaviorDebuggerApplication : IWikiApplication
    {
        private readonly NullCache _cache = new NullCache(); 
        private readonly string _configPath;
        private FederationConfiguration _federationConfiguration;
        private readonly ITimeProvider _timeProvider = new DefaultTimeProvider();


        public BehaviorDebuggerApplication(string configPath)
        {
            _configPath = configPath;
        }

        public IWikiCache Cache
        {
            get { return _cache; }
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

        public bool IsTransportSecure
        {
            get { return true; }
        }

        public LinkMaker LinkMaker
        {
            get { return null;  }
        }

        public OutputFormat OutputFormat
        {
            get { return OutputFormat.Testing; }
        }

		public object this[string key]
		{
			get { return null; }
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
