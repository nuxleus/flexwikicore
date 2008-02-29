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
using System.Text;

namespace FlexWiki.Caching
{
    internal class TopicCacheKey
    {
        private string _path; 
        private QualifiedTopicRevision _revision; 

        internal TopicCacheKey(QualifiedTopicRevision revision, string path)
        {
            _revision = revision;
            _path = path; 
        }

        internal string Path
        {
            get { return _path; }
        }

        internal QualifiedTopicRevision Revision
        {
            get { return _revision; }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_path))
            {
                return string.Format("cache://{0}/{1}", _revision.Namespace,
                    _revision.AsUnqualifiedTopicRevision().DottedNameWithVersion);
            }
            else
            {
                return string.Format("cache://{0}/{1}/{2}", _revision.Namespace,
                    _revision.AsUnqualifiedTopicRevision().DottedNameWithVersion,
                    _path); 
            }
        }

        internal static bool TryParse(string input, out TopicCacheKey key)
        {
            key = null; 
           
            if (!input.StartsWith("cache://"))
            {
                return false; 
            }

            input = input.Substring("cache://".Length);

            int slashIndex = input.IndexOf("/");

            if (slashIndex == -1)
            {
                return false; 
            }

            string ns = input.Substring(0, slashIndex);

            if (string.IsNullOrEmpty(ns))
            {
                return false; 
            }

            input = input.Substring(ns.Length + 1);

            slashIndex = input.IndexOf("/");

            string topicRevision = input; 
            string path = null; 
            if (slashIndex != -1)
            {
                topicRevision = input.Substring(0, slashIndex);
                path = input.Substring(slashIndex + 1); 
            }

            QualifiedTopicRevision revision; 
            try
            {
                 revision = new QualifiedTopicRevision(topicRevision, ns);
            }
            catch (Exception)
            {
                return false; 
            }

            key = new TopicCacheKey(revision, path);

            return true; 
        }
    }
}
