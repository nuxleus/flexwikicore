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
using System.Diagnostics;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using FlexWiki.Formatting;


namespace FlexWiki
{
    internal class FileInfoTopicData : TopicData
    {
        private static Regex s_historicalFileNameRegex = new Regex("[^(]+\\((?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?\\)");

        private string _namespace;
        private IFileInformation _fileInformation;

        public FileInfoTopicData(IFileInformation info, string ns)
        {
            _fileInformation = info;
            _namespace = ns;
        }


        public override string Author
        {
            get
            {
                string filename = _fileInformation.Name;
                // remove the extension
                filename = filename.Substring(0, filename.Length - _fileInformation.Extension.Length);
                Match m = s_historicalFileNameRegex.Match(filename);
                if (!m.Success)
                {
                    return null;
                }
                if (m.Groups["name"].Captures.Count == 0)
                {
                    return null;
                }
                return m.Groups["name"].Value;

            }
        }
        public string FullName
        {
            get
            {
                return _fileInformation.FullName;
            }
        }
        public override DateTime LastModificationTime
        {
            get
            {
                return _fileInformation.LastWriteTime;
            }
        }
        public override string Name
        {
            get
            {
                return _fileInformation.NameWithoutExtension;
            }
        }
        public override string Namespace
        {
            get
            {
                return _namespace;
            }
        }
        public override string Version
        {
            get
            {
                return ExtractVersionFromHistoricalFilename(_fileInformation.NameWithoutExtension);
            }
        }

        private static string ExtractTopicFromHistoricalFilename(string filename)
        {
            int p = filename.IndexOf("(");
            if (p == -1)
            {
                return filename;
            }
            return filename.Substring(0, p);
        }
        private static string ExtractVersionFromHistoricalFilename(string filename)
        {
            // ab(xyz)
            // 
            int p = filename.IndexOf("(");
            if (p == -1)
            {
                return null;
            }
            int close = filename.LastIndexOf(")");
            return filename.Substring(p + 1, close - p - 1);
        }

    }
}
