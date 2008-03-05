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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;

using FlexWiki.Collections; 

namespace FlexWiki
{
    /// <summary>
    /// Summary description for Topic.
    /// </summary>
    // TODO: Should we update WikiTalk to reflect the new name of this object at some point?
    [ExposedClass("TopicInfo", "Provides information about a topic")]
    public class TopicVersionInfo : BELObject, IComparable
    {
        // Fields

        private bool _haveChanges;
        private bool _haveCreated;
        private bool _haveExists;
        private bool _haveKeywords;
        private bool _haveKeywordsList;
        private bool _haveLastModified;
        private bool _haveLastModifiedBy;
        private bool _haveProperties; 
        private bool _havePropertyNames;
        private bool _haveSummary;
        private bool _haveIsTopicLocked;

        private TopicChangeCollection _changes;
        private DateTime _created;
        private bool _exists;
        private Federation _federation;
        private bool _isTopicLocked;
        private string _keywords;
        private ArrayList _keywordsList; 
        private DateTime _lastModified;
        private string _lastModifiedBy; 
        private NamespaceManager _namespaceManager;
        private TopicPropertyCollection _properties; 
        private IList<string> _propertyNames;
        private string _summary; 
        private QualifiedTopicRevision _topicVersionKey;

        // Constructors

        public TopicVersionInfo(Federation aFed, QualifiedTopicRevision name)
        {
            _topicVersionKey = name;
            _federation = aFed;
        }

        // Properties

        [ExposedMethod(ExposedMethodFlags.Default, "Answer a list of TopicChanges describing this topic's history")]
        public TopicChangeCollection Changes
        {
            get
            {
                if (!_haveChanges)
                {
                    TopicChangeCollection answer = new TopicChangeCollection();
                    foreach (TopicChange each in Federation.GetTopicChanges(new TopicName(TopicRevision.DottedName)))
                    {
                        answer.Add(each);
                    }
                    _changes = answer;
                    _haveChanges = true; 
                }

                return _changes; 
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a DateTime indicating when the topic was created")]
        public DateTime Created
        {
            get
            {
                if (!_haveCreated)
                {
                    _created = Federation.GetTopicCreationTime(TopicRevision);
                    _haveCreated = true;
                }

                return _created; 
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer true if topic exists, otherwise false)")]
        public bool Exists
        {
            get
            {
                if (!_haveExists)
                {
                    _exists = NamespaceManager.TopicExists(TopicRevision.LocalName, ImportPolicy.DoNotIncludeImports);
                    _haveExists = true;
                }

                return _exists; 
            }
        }
        [ExposedMethod("Fullname", ExposedMethodFlags.Default, "Answer the complete name of the topic (including namespace and version, if present)")]
        public string ExposedFullname
        {
            get
            {
                return _topicVersionKey.ToString();
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the Federation containing the topic")]
        public Federation Federation
        {
            get
            {
                return _federation;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer true if topic is locked (topic is read only), otherwise false (topic is writable)")]
        public bool IsTopicLocked
        {
            get
            {
                if (!_haveIsTopicLocked)
                {
                    _isTopicLocked = NamespaceManager.TopicIsReadOnly(TopicRevision.LocalName);
                    _haveIsTopicLocked = true;
                }
                return _isTopicLocked;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the Keywords property's value")]
        public string Keywords
        {
            get
            {
                if (!_haveKeywords)
                {
                    _keywords = GetProperty("Keywords");
                    _haveKeywords = true; 
                }

                return _keywords; 
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer an array containing all the keywords listed in the Keywords property")]
        public ArrayList KeywordsList
        {
            get
            {
                if (!_haveKeywordsList)
                {
                    _keywordsList = GetListProperty("Keywords");
                    _haveKeywordsList = true; 
                }

                return _keywordsList; 
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a DateTime indicating when the topic was last modified")]
        public DateTime LastModified
        {
            get
            {
                if (!_haveLastModified)
                {
                    _lastModified = Federation.GetTopicModificationTime(TopicRevision);
                    _haveLastModified = true; 
                }

                return _lastModified; 
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a string indicating who last modified the topic")]
        public string LastModifiedBy
        {
            get
            {
                if (!_haveLastModifiedBy)
                {
                    _lastModifiedBy = Federation.GetTopicLastModifiedBy(TopicName);
                    _haveLastModifiedBy = true; 
                }

                return _lastModifiedBy; 
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a the (full)name of the topic")]
        public string Name
        {
            get
            {
                return TopicRevision.LocalName;
            }
        }
        [ExposedMethod("Namespace", ExposedMethodFlags.Default, "Answer the Namespace for this topic")]
        public NamespaceManager NamespaceManager
        {
            get
            {
                if (_namespaceManager != null)
                    return _namespaceManager;
                _namespaceManager = Federation.NamespaceManagerForTopic(TopicRevision);
                return _namespaceManager;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the list of property names for this topic")]
        public IList<string> PropertyNames
        {
            get
            {
                if (!_havePropertyNames)
                {
                    _propertyNames = Federation.GetTopicProperties(TopicRevision).Names;
                    _havePropertyNames = true; 
                }

                return _propertyNames; 
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the Summary property's value")]
        public string Summary
        {
            get
            {
                if (!_haveSummary)
                {
                    _summary = GetProperty("Summary");
                    _haveSummary = true;
                }

                return _summary; 
            }
        }
        public TopicName TopicName
        {
            get
            {
                return new TopicName(TopicRevision.LocalName, TopicRevision.Namespace); 
            }
        }
        public QualifiedTopicRevision TopicRevision
        {
            get
            {
                return _topicVersionKey;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the version stamp for this version of the topic")]
        public string Version
        {
            get
            {
                return TopicRevision.Version;
            }
        }

        // Methods

        public int CompareTo(object obj)
        {
            return this.TopicRevision.DottedName.CompareTo(((TopicVersionInfo)obj).TopicRevision.DottedName);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer an array of the values in the given list property")]
        public ArrayList GetListProperty(string propertyName)
        {
            return Federation.GetTopicListPropertyValue(TopicRevision, propertyName);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the given property from the topic")]
        public string GetProperty(string propertyName)
        {
            return Federation.GetTopicPropertyValue(TopicRevision, propertyName);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer true if the given topic has the given property; else false")]
        public bool HasProperty(string propertyName)
        {
            if (!_haveProperties)
            {
                _properties = Federation.GetTopicProperties(TopicRevision);
                _haveProperties = true; 
            }

            if (_properties == null)
            {
                return false;
            }
            return _properties.Contains(propertyName);
        }
        public override string ToString()
        {
            string answer = "TopicInfo for ";
            if (TopicRevision != null)
            {
                answer += TopicRevision;
            }
            return answer;
        }
        [ExposedMethod(ExposedMethodFlags.NeedContext, "Answer a formatted Table of Contents for the topic. Valid values for style are 'bullet' or 'numeric'")]
        public string TableOfContents(ExecutionContext ctx, string style, [ExposedParameter(true)] int maxDepth)
        {
            StringBuilder strbldr = new StringBuilder();
            string _spaces = "                                                                "; //64 spaces = (7 + 1) x 8 spaces
            string _style;
            int _maxDepth = 7;
            if (maxDepth >= 1 && maxDepth <= 7)
            {
                _maxDepth = maxDepth - 1;
            }

            string _headers = Federation.GetTopicHeaders(TopicRevision);
            if (style.ToLower().Equals("numeric"))
            {
                _style = "1.";
            }
            else
            {
                _style = "*";
            }

            string[] hdrLine = _headers.Split(new char[] { '\n' });

            for (int x = 0; x < hdrLine.Length - 1; x++)
            {
                string tempIn = hdrLine[x].Trim();
                int maxWidth = tempIn.Length > 6 ? 7 : tempIn.Length;
                int y = tempIn.Substring(0,maxWidth).LastIndexOf('!'); //ensure y has max value of 7 and only uses chars at the start of the Header
                if (y <= _maxDepth)
                {
                    string temp = tempIn.Substring(y + 1, tempIn.Length - y - 1);
                    while (temp.Contains("\"\""))
                    {
                        int z = temp.IndexOf("\"\"");
                        if (z == 0)
                        {
                            temp = temp.Substring(2);
                        }
                        else
                        {
                            temp = temp.Substring(0, z) + temp.Substring(z + 2);
                        }

                    }


                    temp = @"@@Presentations.Link(federation.LinkMaker.SimpleLinkTo([""Default.aspx/"",""" + TopicRevision.Namespace + @""",""/"",""" + TopicRevision.LocalName + @""",""#"",""" + HttpUtility.HtmlEncode(temp) + @"""].ToOneString),""" + temp + @""")@@";
                    strbldr.AppendLine(_spaces.Substring(1, (y + 1) * 8) + _style + temp);
                }
            }
            return strbldr.ToString();
        }
    }
}
