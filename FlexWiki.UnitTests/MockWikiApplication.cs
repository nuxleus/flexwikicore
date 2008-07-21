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
using System.Xml;
using System.Xml.Serialization;


using FlexWiki.Caching; 
using FlexWiki.UnitTests.Caching;
using FlexWiki.Web;
using FlexWiki.Collections;

namespace FlexWiki.UnitTests
{
    internal class MockWikiApplication : IWikiApplication, IMockWikiApplication
    {
        private FlexWikiWebApplicationConfiguration _applicationConfiguration;
        private MockCache _cache = new MockCache(); 
        private FederationConfiguration _configuration;
        private bool _isTransportSecure; 
        private LinkMaker _linkMaker;
        private readonly ModificationCollection _modificationsReported = new ModificationCollection();
        private IMembership _membership = new FlexWikiWebMembership();
        private OutputFormat _ouputFormat;
        private ITimeProvider _timeProvider;
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        public MockWikiApplication(FederationConfiguration configuration, LinkMaker linkMaker,
            OutputFormat outputFormat, ITimeProvider timeProvider)
        {
            _configuration = configuration;
            _linkMaker = linkMaker;
            _ouputFormat = outputFormat;
            _timeProvider = timeProvider;

            LoadConfiguration();
        }

        public MockWikiApplication(LinkMaker linkMaker,
            OutputFormat outputFormat)
        {
            _linkMaker = linkMaker;
            _ouputFormat = outputFormat;

            LoadConfiguration();

        }

        public FlexWikiWebApplicationConfiguration ApplicationConfiguration
        {
            get { return _applicationConfiguration; }
        }
        
        public IWikiCache Cache
        {
            get { return _cache; }
        }
        public ExecutionEnvironment ExecutionEnvironment
        {
            get { return ExecutionEnvironment.Testing; }
        }
        public FederationConfiguration FederationConfiguration
        {
            get { return _configuration; }
        }

        public bool IsTransportSecure
        {
            get { return _isTransportSecure; }
            set { _isTransportSecure = value; }
        }

        public LinkMaker LinkMaker
        {
            get { return _linkMaker; }
        }

        public ModificationCollection ModificationsReported
        {
            get { return _modificationsReported; }
        }
        public IMembership Membership
        {
            get { return _membership; }
        }
        public OutputFormat OutputFormat
        {
            get { return _ouputFormat; }
        }

        public object this[string key]
        {
            get { return _properties.ContainsKey(key) ? _properties[key] : null; }
            internal set { _properties[key] = value; }
        }


        public ITimeProvider TimeProvider
        {
            get { return _timeProvider; }
        }

        public void Log(string source, LogLevel level, string message)
        {
            // no-op
        }
        public void LogDebug(string source, string message)
        {
            // no-op
        }
        public void LogError(string source, string message)
        {
            // no-op
        }
        public void LogInfo(string source, string message)
        {
            // no-op
        }
        public void LogWarning(string source, string message)
        {
            // no-op
        }
        public void NoteModification(Modification modification)
        {
            _modificationsReported.Add(modification); 
        }
        public string ResolveRelativePath(string path)
        {
            return System.IO.Path.Combine("FW:\\", path); 
        }
        public void WriteFederationConfiguration()
        {
            throw new NotImplementedException(); 
        }
        private void LoadConfiguration()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FlexWikiWebApplicationConfiguration));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
<configuration>
  <DisableWikiEmoticons>false</DisableWikiEmoticons>
  <RemoveListItemWhitespace>false</RemoveListItemWhitespace>
  <OverrideStylesheet>wiki-override.css</OverrideStylesheet>
  <FederationConfiguration>
    <!-- This information is used by the default borders to print a short message 
          in the border of every page on the wiki. It appears as the ""About"" property
          of the Federation class in WikiTalk. -->
    <About>This is FlexWiki, an open source wiki engine.</About>

    <!-- These are the default rules for who is allowed to do what at the wiki level. See 
      http://www.flexwiki.com/default.aspx/FlexWiki/FlexWikiAuthorization.html
      for more information. -->
    <AuthorizationRules>
      <Rule Type=""Allow"" Action=""ManageNamespace"" Principal=""all"" />
      <!-- <Rule Type=""Deny"" Action=""Edit"" Principal=""anonymous"" /> -->
    </AuthorizationRules>

    <!-- When populated via the /admin interface, lists the link prefixes that are not allowed 
        in wiki topics. (Anti-spam measure.) -->
    <BlacklistedExternalLinks />

    <!-- A comma-separated list of namespace-qualified names from which to load border
          definitions (usually rendered with WikiTalk). The default is nothing, which
          causes borders to be generated from the built-in topic _NormalBorders in
          each namespace.
          
          See http://www.flexwiki.com/default.aspx/FlexWiki/CustomBorders.html for more info. -->
    <!-- <Borders>MyNamespace.MyBorders, CompanyNamespace.CompanyBorders</Borders>-->

    <!-- Namespaces can ""import"" other namespaces - see 
          http://www.flexwiki.com/default.aspx/FlexWiki/ImportedNamespace.html for more
          information. The value of <DefaultImportedNamespaces> is automatically copied
          to the Import property of a new namespaces when it is created.-->
    <!-- <DefaultImportedNamespaces></DefaultImportedNamespaces> -->

    <!-- The default namespace is the one that appears if a user visits the website
         without specifying the full URL of a topic. -->
    <DefaultNamespace>SampleNamespaceTwo</DefaultNamespace>

    <!-- Set to true to enable FlexWiki Windows Performance Counters. Set to false to disable. 
          Performance counters have been known to cause problems on some machines.-->
    <EnablePerformanceCounters>false</EnablePerformanceCounters>

    <!-- When set to true causes the title of topics to display with spaces between words.
          For example, the topic ""ThisIsATopic"" would display as ""This is A Topic"". 
          When set to false, causes the title of topics to display without spaces. 
          For example, the topic ""ThisIsATopic"" would display as ""ThisIsATopic"".-->
    <DisplaySpacesInWikiLinks>false</DisplaySpacesInWikiLinks>


    <!-- InterWikis are a convenience function that allows FlexWiki to generate shortcuts
          to topics in other wikis. See http://www.flexwiki.com/default.aspx/FlexWiki/InterWiki.html
          for more information. 
          
          The <InterWikisTopic> tag allows an administrator to set the topic where
          InterWiki behaviors are defined. The default value is _InterWiki -->
    <!-- <InterWikisTopic></InterWikisTopic> -->

    <!-- If set to 'true', FlexWiki will decorate external hyperlinks (that is, hyperlinks that
         start with http:// or https:// with the rel='nofollow' attribute. This is an anti-spam
         measure. Read more about nofollow here: http://en.wikipedia.org/wiki/Nofollow -->
    <NoFollowExternalHyperlinks>false</NoFollowExternalHyperlinks>

    <!-- Namespace providers are responsible for storing the information in FlexWiki topics. 
         Namespaces can be stored in the filesystem or in SQL Server. It is generally better 
         not to edit this section by hand. Use the administrative tools located at /admin
         off the wiki root URL instead. -->
    <NamespaceProviders>
      <!-- A sample provider that stores a wiki namespace called SampleNamespaceOne in the filesystem 
          in the directory Namespaces\SampleNamespaceOne (relative to the directory where FlexWiki 
          is installed). Note that the ID must be unique amongst all providers. -->
      <Provider Id=""5a2caaff-c139-40ad-85df-cdf136c95458"" Type=""FlexWiki.FileSystemNamespaceProvider"" AssemblyName=""FlexWiki"">
        <Parameters>
          <Parameter Name=""Namespace"" Value=""SampleNamespaceOne"" />
          <Parameter Name=""Root"" Value=""Namespaces\SampleNamespaceOne"" />
        </Parameters>
      </Provider>

      <!-- A second sample provider. -->
      <Provider Id=""efa37414-d13e-487a-bf13-ab506f05bd08"" Type=""FlexWiki.FileSystemNamespaceProvider"" AssemblyName=""FlexWiki"">
        <Parameters>
          <Parameter Name=""Namespace"" Value=""SampleNamespaceTwo"" />
          <Parameter Name=""Root"" Value=""Namespaces\SampleNamespaceTwo"" />
        </Parameters>
      </Provider>

    </NamespaceProviders>


    <!-- Deprecated section - do not use. -->
    <Namespaces />

    <!-- Plugins are simply assemblies that FlexWiki ensures get loaded into the wiki 
          application. The value of each entry is the full name of an assembly. 
          
          See http://www.flexwiki.com/default.aspx/FlexWiki/PlugInOverview.html for
          more information. -->
    <!--
    <Plugins>
      <Plugin>MyAssembly</Plugin>
      <Plugin>MyOtherAssembly, Version=1.2.3.4, PublicKeyToken=abcd1234abcd1234abcd1234</Plugin>
    </Plugins>
    -->

    <!-- Determines whether HTTPS will be required by default for namespaces in this wiki. 
          Possible values include: 
          
            None = HTTPS is not required. 
            Content = HTTPS is required. 
            
          Note that this value is the default for the wiki, and can be overridden on a namespace-
          by-namespace basis via the RequireTransportSecurityFor property in _ContentBaseDefinition. 
          See http://www.flexwiki.com/default.aspx/FlexWiki/FlexWikiTransportSecurity.html for
          more information. 
      -->
    <RequireTransportSecurityFor>None</RequireTransportSecurityFor>

    <!-- Should always be set to 1. -->
    <WikiTalkVersion>1</WikiTalkVersion>

  </FederationConfiguration>

</configuration>");

            XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            _applicationConfiguration = (FlexWikiWebApplicationConfiguration)serializer.Deserialize(reader);
            this["DisableWikiEmoticons"] = _applicationConfiguration.DisableWikiEmoticons;
            this["RemoveListItemWhitespace"] = _applicationConfiguration.RemoveListItemWhitespace;
            this["OverrideStylesheet"] = _applicationConfiguration.OverrideStylesheet;

        }
        /// <summary>
        /// Allows a normally read-only property to be set in test conditions
        /// </summary>
        /// <param name="key">The name of an Application property, eg. DisableWikiEmoticons</param>
        /// <param name="value">The boolean value the property can take</param>
        public void SetApplicationProperty(string key, bool value)
        {
            this[key] = value;
        }

    }
}
