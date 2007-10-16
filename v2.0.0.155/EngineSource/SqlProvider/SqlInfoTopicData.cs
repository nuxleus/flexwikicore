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
using System.Text.RegularExpressions;

using FlexWiki;

namespace FlexWiki.SqlProvider
{
    internal class SqlInfoTopicData : TopicData
    {
        private string _namespace;
        private SqlInfoForTopic _info;
        private UnqualifiedTopicRevision _revision; 

        public SqlInfoTopicData(SqlInfoForTopic info, string ns)
        {
            _info = info;
            _namespace = ns;
            _revision = new UnqualifiedTopicRevision(info.Name); 
        }

        public override string Author
        {
            get
            {
                return TopicRevision.ParseVersion(_revision.Version).Author; 
            }
        }
        public string DottedName
        {
            get
            {
                return _revision.ResolveRelativeTo(_namespace).DottedName; 
            }
        }
        public override DateTime LastModificationTime
        {
            get
            {
                return _info.LastWriteTime;
            }
        }
        public override string Name
        {
            get
            {
                return _info.Name; 
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
                return _revision.Version; 
            }
        }


    }
}
