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
using System.Collections.ObjectModel; 
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.IO;

using FlexWiki.Collections; 
using FlexWiki.Security; 

namespace FlexWiki
{
    /// <summary>
    /// A FederationConfiguration is a persistable representation of all the configuration information needed for a Federation
    /// </summary>
    public class FederationConfiguration
    {
        private static Regex s_namespaceDefinitionRegex = new Regex("^([a-zA-Z0-9\\.]+)=(.*)$");

        private string _aboutWikiString;
        private readonly WikiAuthorizationRuleCollection _authorizationRules = new WikiAuthorizationRuleCollection(); 
        private readonly ArrayList _blacklistedExternalLinks = new ArrayList();
        private string _borders;
        private string _defaultImportedNamespaces; 
        private string _defaultNamespace;
        private readonly ArrayList _deprecatedDefinitions = new ArrayList();
        private bool _enablePerformanceCounters; 
        private bool _displaySpacesInWikiLinks;
        private string _interWikisTopic; 
        private readonly ArrayList _namespaceMappings = new ArrayList();
        private bool _noFollowExternalHyperlinks;
        private readonly Collection<string> _plugins = new Collection<string>(); 
        private int _wikiTalkVersion;

        public FederationConfiguration()
        {
        }

        [XmlElement(ElementName = "About")]
        public string AboutWikiString
        {
            get { return _aboutWikiString; }
            set { _aboutWikiString = value; }
        }
        [XmlArray("AuthorizationRules")]
        [XmlArrayItem("Rule")]
        public WikiAuthorizationRuleCollection AuthorizationRules
        {
            get { return _authorizationRules; }
        }
        [XmlArray(ElementName = "BlacklistedExternalLinks")]
        [XmlArrayItem(ElementName = "Link", Type = typeof(string))]
        public ArrayList BlacklistedExternalLinks
        {
            get
            {
                return _blacklistedExternalLinks;
            }
        }
        [XmlElement(ElementName = "Borders")]
        public string Borders
        {
            get { return _borders; }
            set { _borders = value; }
        }
        public string DefaultImportedNamespaces
        {
            get { return _defaultImportedNamespaces; }
            set { _defaultImportedNamespaces = value; }
        }
        public string DefaultNamespace
        {
            get { return _defaultNamespace; }
            set { _defaultNamespace = value; }
        }
        // Support reading in the old-style <Namespaces> element -- just to help users convert
        [XmlArray(ElementName = "Namespaces")]
        [XmlArrayItem(ElementName = "Namespace", Type = typeof(DeprecatedNamespaceDefinition))]
        public ArrayList DeprecatedNamespaceDefinitions
        {
            get
            {
                return _deprecatedDefinitions;
            }
        }
        public bool EnablePerformanceCounters
        {
            get { return _enablePerformanceCounters; }
            set { _enablePerformanceCounters = value; }
        }
        public bool DisplaySpacesInWikiLinks
        {
            get { return _displaySpacesInWikiLinks; }
            set { _displaySpacesInWikiLinks = value; }
        }
        public string InterWikisTopic
        {
            get { return _interWikisTopic; }
            set { _interWikisTopic = value; }
        }
        [XmlArray(ElementName = "NamespaceProviders")]
        [XmlArrayItem(ElementName = "Provider", Type = typeof(NamespaceProviderDefinition))]
        public ArrayList NamespaceMappings
        {
            get
            {
                return _namespaceMappings;
            }
        }
        [XmlElement(ElementName = "NoFollowExternalHyperlinks")]
        public bool NoFollowExternalHyperlinks
        {
            get { return _noFollowExternalHyperlinks; }
            set { _noFollowExternalHyperlinks = value; }
        }
        [XmlArray(ElementName = "Plugins")]
        [XmlArrayItem(ElementName = "Plugin", Type=typeof(string))]
        public Collection<string> Plugins
        {
            get { return _plugins; }
        }
        [XmlElement(ElementName = "WikiTalkVersion")]
        public int WikiTalkVersion
        {
            get { return _wikiTalkVersion; }
            set { _wikiTalkVersion = value; }
        }
        public void WriteToFile(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FederationConfiguration));
            TextWriter writer = new StreamWriter(path);
            serializer.Serialize(writer, this);
            writer.Close();
        }

    }
}
