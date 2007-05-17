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

namespace FlexWiki.UnitTests
{
    /// <summary>
    /// Stores information about a topicName - content, name, and version. 
    /// </summary>
    internal class MockTopic
    {
        private readonly MockTopicRevisionCollection _history = new MockTopicRevisionCollection();
        private string _name;
        private MockTopicRevision _tip; 

        internal MockTopic(string name)
        {
            _name = name; 
        }

        internal MockTopic(string name, string contents, string author, DateTime timestamp)
        {
            _name = name;

            _history.Add(new MockTopicRevision(contents, author, timestamp)); 
        }

        internal MockTopicRevision this[string version]
        {
            get
            {
                if (version == null)
                {
                    return Latest; 
                }

                foreach (MockTopicRevision history in History)
                {
                    if (history.Version == version)
                    {
                        return history; 
                    }
                }

                return null; 
            }
        }
        
        internal MockTopicRevisionCollection History
        {
            get { return _history; }
        }

        internal bool IsDeleted
        {
            get { return _tip == null; }
        }

        internal MockTopicRevision Latest
        {
            get { return _tip; }
        }

        internal string Name
        {
            get { return _name; }
        }

        internal DateTime Timestamp
        {
            get { return Latest.Created; }
        }

        internal void DeleteLatest()
        {
            _tip = null; 
        }

        internal void ReplaceContents(string contents)
        {
            Latest.Contents = contents; 
        }

        internal void WriteLatest(MockTopicRevision revision)
        {
            _tip = revision;
        }

    }
}