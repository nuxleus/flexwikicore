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
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

using FlexWiki.Caching; 
using FlexWiki.Collections;
using FlexWiki.Formatting;
using FlexWiki.Security; 

namespace FlexWiki
{
    /// <summary>
    /// 
    /// </summary>
    [ExposedClass("Federation", "Represents the entire Wiki federation")]
    public class Federation : BELObject
    {
        private delegate T NamespaceAction<T>(NamespaceManager manager); 

        /*
        /// <summary>
        /// Answer a new Federation
        /// </summary>
        public Federation(OutputFormat format, LinkMaker linker)
        {
            Initialize(new FederationConfiguration(), format, linker);
        }

        ///// <summary>
        ///// Answer a new federation loaded from the given configuration file
        ///// </summary>
        ///// <param name="configFile">Path to the config file</param>
        //public Federation(string configFile, OutputFormat format, LinkMaker linkMaker)
        //{
        //    Initialize(FederationConfiguration.FromFile(configFile), format, linkMaker); 
        //}

        public Federation(FederationConfiguration configuration, OutputFormat format, LinkMaker linkMaker)
        {
            Initialize(configuration, format, linkMaker);
        }
        */

        private const string c_anonymousUserName = "anonymous";

        private static Dictionary<string, PerformanceCounter> s_performanceCounterMap = new Dictionary<string, PerformanceCounter>();
        private static string s_performanceCounterCategoryName = "FlexWiki";

        private string _aboutWikiString;
        private IWikiApplication _application; 
        private Set<string> _blacklistedExternalLinkPrefixes = new Set<string>();
        private string _borders;
        private DateTime _created = DateTime.Now;
        private ILogEventFactory _logEventFactory;
        /// <summary>
        /// The registry of all known <see cref="NamespaceManager" /> objects.  Keys are namespace names and values are 
        /// <see cref="NamespaceManager" />.
        /// </summary>
        private readonly NamespaceManagerMap _namespaceToNamespaceManagerMap = new NamespaceManagerMap();
        private bool _noFollowExternalHyperlinks = false;
        private ParserEngine _parser;
        private ITimeProvider _timeProvider = new DefaultTimeProvider();
        private FederationUpdateGenerator _updateGenerator = null;
        private int _wikiTalkVersion;

        public Federation(IWikiApplication application)
        {
            Initialize(application);
        }


        public string AboutWikiString
        {
            get
            {
                return _aboutWikiString;
            }
            set
            {
                _aboutWikiString = value;
                RecordFederationPropertiesChanged();
            }
        }
        public NamespaceManagerCollection AllNamespaces
        {
            get
            {
                NamespaceManagerCollection answer = new NamespaceManagerCollection();
                foreach (string ns in Namespaces)
                {
                    answer.Add(NamespaceManagerForNamespace(ns));
                }
                return answer;
            }
        }
        public static string AnonymousUserName
        {
            get { return c_anonymousUserName; }
        }
        public IWikiApplication Application
        {
            get { return _application; }
        }
        /// <summary>
        /// Answer the set of blacklisted link prefixes.  Treat this set as read-only, please, and use AddBlacklistedExternalLink() and RemoveBlacklistedExternalLink().
        /// </summary>
        public Set<string> BlacklistedExternalLinkPrefixes
        {
            get
            {
                return _blacklistedExternalLinkPrefixes;
            }
        }
        public string Borders
        {
            get
            {
                return _borders;
            }
            set
            {
                _borders = value;
                RecordFederationPropertiesChanged();
            }
        }
        public DateTime Created
        {
            get { return _created; }
            set { _created = value; }
        }
        public FederationConfiguration Configuration
        {
            get
            {
                return Application.FederationConfiguration;
            }
        }
        public NamespaceManager DefaultNamespaceManager
        {
            get
            {
                return NamespaceManagerForNamespace(DefaultNamespace);
            }
        }
        public string DefaultNamespace
        {
            get
            {
                return Configuration.DefaultNamespace;
            }
            set
            {
                Configuration.DefaultNamespace = value;
                RecordFederationPropertiesChanged();
            }
        }
        [ExposedMethod("LinkMaker", ExposedMethodFlags.Default, "Answer a LinkMaker configured to produce hyperlinks for the federation")]
        public LinkMaker ExposedLinkMaker
        {
            get
            {
                return LinkMaker;
            }
        }
        public ILogEventFactory LogEventFactory
        {
            get { return _logEventFactory; }
            set { _logEventFactory = value; }
        }
        /// <summary>
        /// Answer an enumeration of all the <see cref="NamespaceManager" /> objects.
        /// </summary>
        public IEnumerable<NamespaceManager> NamespaceManagers
        {
            get
            {
                // Only existing namespaces are exposed. 
                foreach (NamespaceManager manager in _namespaceToNamespaceManagerMap.Values)
                {
                    if (manager.Exists)
                    {
                        yield return manager; 
                    }
                }
            }
        }
        public ICollection<string> Namespaces
        {
            get
            {
                // Only existing namespaces are exposed. 
                List<string> namespaces = new List<string>();
                foreach (NamespaceManager manager in _namespaceToNamespaceManagerMap.Values)
                {
                    if (manager.Exists)
                    {
                        namespaces.Add(manager.Namespace); 
                    }
                }

                return namespaces; 
            }
        }
        public bool NoFollowExternalHyperlinks
        {
            get
            {
                return _noFollowExternalHyperlinks;
            }
            set
            {
                if (value == _noFollowExternalHyperlinks)
                {
                    return;
                }
                _noFollowExternalHyperlinks = value;
                RecordFederationPropertiesChanged();
            }
        }
        // a fresh version of the parser should be retrieved from the cache and set here before any use
        // it should be reset for each border and each topic being processed
        // TODO: check for bugs with IncludeTopic - a better(?) way may be a reset context method
        public ParserEngine Parser
        {
            get { return _parser; }
            set { _parser = value; }
        }
        /// <summary>
        /// Gets a value indicating if the FlexWiki performance counters are configured
        /// to be enabled or disabled.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This propertyName looks at the <b>DisablePerformanceCounter</b> configuration
        /// key. If set to <b>true</b>, performance counters are disabled. If absent or 
        /// set to any other value, performance counters are enabled.
        /// </p>
        /// <p>
        /// This feature was added because performance counters occasionally cause
        /// huge delays in FlexWiki. At the time of this writing, the exact cause is 
        /// unknown, although it is probably related to this: 
        /// http://pluralsight.com/blogs/craig/archive/2005/06/22/11709.aspx
        /// </p>
        /// </remarks>
        public bool PerformanceCountersEnabled
        {
            get
            {
                return Configuration.EnablePerformanceCounters; 
            }
        }
        public ITimeProvider TimeProvider
        {
            get { return Application.TimeProvider; }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the current version of the WikITalk language enabled for use in this Federation")]
        public int WikiTalkVersion
        {
            get
            {
                return _wikiTalkVersion;
            }
            set
            {
                _wikiTalkVersion = value;
                RecordFederationPropertiesChanged();
            }
        }

        private OutputFormat Format
        {
            get
            {
                return Application.OutputFormat;
            }
        }
        private LinkMaker LinkMaker
        {
            get
            {
                return Application.LinkMaker;
            }
        }
        private static bool PerformanceCounterCategoryExists
        {
            get
            {
                try
                {
                    return PerformanceCounterCategory.Exists(s_performanceCounterCategoryName);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        private FederationUpdateGenerator UpdateGenerator
        {
            get
            {
                if (_updateGenerator != null)
                {
                    return _updateGenerator;
                }
                _updateGenerator = new FederationUpdateGenerator();
                _updateGenerator.GenerationComplete += new FederationUpdateGenerator.GenerationCompleteEventHandler(FederationUpdateGeneratorGenerationComplete);
                return _updateGenerator;
            }
        }

        public event FederationUpdateEventHandler FederationUpdated;

        public void AddBlacklistedExternalLinkPrefix(string link)
        {
            if (_blacklistedExternalLinkPrefixes.Contains(link))
            {
                return;
            }
            _blacklistedExternalLinkPrefixes.Add(link);
            RecordFederationPropertiesChanged();
        }
/*
 * public static void AddImplicitPropertiesToHash(Hashtable hash, QualifiedTopicRevision topic, string lastModBy, DateTime creation, DateTime modification, string content)
        {
            // Remember that this list is closely bound to some implicit knowledge of what these imports are in the logic in WriteTopic that send change notifications for imports
            // If you add/change these imports, you need to carefully revie wthat code too to be sure it fires the right changed events
            hash["_TopicName"] = topic.LocalName;
            hash["_TopicFullName"] = topic.DottedNameWithVersion;
            hash["_LastModifiedBy"] = lastModBy;
            hash["_CreationTime"] = creation.ToString();
            hash["_ModificationTime"] = modification.ToString();
            hash["_Body"] = content;
        }
 */
        public QualifiedTopicNameCollection AllQualifiedTopicNamesThatExist(TopicName topicName, string nsRelativeTo)
        {
            return AllQualifiedTopicNamesThatExist(topicName, nsRelativeTo, AlternatesPolicy.DoNotIncludeAlternates); 
        }
        public QualifiedTopicNameCollection AllQualifiedTopicNamesThatExist(TopicName topicName, string nsRelativeTo, AlternatesPolicy alternatesPolicy)
        {
            QualifiedTopicNameCollection names = new QualifiedTopicNameCollection();

            NamespaceManager manager = null; 
            ImportPolicy importPolicy = ImportPolicy.IncludeImports;
            // If it's qualified, don't include imports
            if (topicName.IsQualified)
            {
                importPolicy = ImportPolicy.DoNotIncludeImports;
                manager = NamespaceManagerForNamespace(topicName.Namespace);
            }
            // Otherwise, do
            else
            {
                manager = NamespaceManagerForNamespace(nsRelativeTo); 
            }

            if (manager == null)
            {
                return names;
            }

            names.AddRange(manager.AllQualifiedTopicNamesThatExist(topicName.LocalName, importPolicy));

            if (alternatesPolicy == AlternatesPolicy.IncludeAlternates)
            {
                foreach (TopicName alternate in topicName.AlternateForms())
                {
                    names.AddRange(manager.AllQualifiedTopicNamesThatExist(alternate.LocalName, importPolicy));
                }
            }

            return names; 
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the ability for a registered user of this Federation to reset their own password")]
        public bool CanResetPassword()
        {
            return _application.Membership.PasswordResetEnabled;
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the ability for a registered user of this Federation to retrieve their own password")]
        public bool CanRetrievePassword()
        {
            return _application.Membership.PasswordRetrievalEnabled;
        }
        public static bool CreatePerformanceCounters()
        {
            if (!PerformanceCounterCategoryExists)
            {
                CounterCreationDataCollection CCDC = new CounterCreationDataCollection();

                // Add the counter.
                CreateCounter(CCDC, PerformanceCounterType.RateOfCountsPerSecond64, PerformanceCounterNames.TopicReads);
                CreateCounter(CCDC, PerformanceCounterType.RateOfCountsPerSecond64, PerformanceCounterNames.TopicWrite);
                CreateCounter(CCDC, PerformanceCounterType.RateOfCountsPerSecond64, PerformanceCounterNames.TopicFormat);
                CreateCounter(CCDC, PerformanceCounterType.RateOfCountsPerSecond64, PerformanceCounterNames.MethodInvocation);

                // Create the category.
                PerformanceCounterCategory.Create(s_performanceCounterCategoryName,
                  "FlexWiki application performance",
                    // TODO: What's the right setting here? 
                  PerformanceCounterCategoryType.MultiInstance,
                  CCDC);
                return (true);
            }
            else
            {
                return (false);
            }
        }
        public static void DeletePerformanceCounters()
        {
            PerformanceCounterCategory.Delete(s_performanceCounterCategoryName);
        }
        public void DeleteTopic(QualifiedTopicRevision topic, bool removeHistory)
        {
            NamespaceManager namespaceManager = NamespaceManagerForTopic(topic);
            if (namespaceManager == null)
            {
                return;
            }
            namespaceManager.DeleteTopic(topic.LocalName, removeHistory);
        }
        /// <summary>
        /// Answer the DynamicNamespace for the given namespace (or null if it's an absent namespace)
        /// </summary>
        /// <param name="ns">Name of the namespace</param>
        /// <returns></returns>
        [ExposedMethod("GetNamespace",  ExposedMethodFlags.NeedContext, "Answer an object whose imports are all of the topics in the given namespace")]
        public DynamicNamespace DynamicNamespaceForNamespace(ExecutionContext ctx, string ns)
        {
            if (ExposedNamespaceManagerForNamespace(ctx, ns) == null)
            {
                return null;
            }
            return new DynamicNamespace(this, ns);
        }
        /// <summary>
        /// Answer the DynamicTopic for the given topic (or null if it's an absent)
        /// </summary>
        /// <param name="top">Name of the topic (including namespace)</param>
        /// <returns></returns>
        [ExposedMethod("GetTopic",  ExposedMethodFlags.NeedContext, "Answer an object whose imports are those of the given topic")]
        public DynamicTopic DynamicTopicForTopic(ExecutionContext ctx, string top)
        {
            QualifiedTopicRevision abs = new QualifiedTopicRevision(top);
            if (abs.Namespace == null)
            {
                throw new Exception("Only fully-qualified topic names can be used with GetTopic(): only got " + top);
            }
            DynamicNamespace dns = DynamicNamespaceForNamespace(ctx, abs.Namespace);
            return dns.DynamicTopicFor(abs.LocalName);
        }
        //public override bool Equals(object obj)
        //{
        //    Federation other = obj as Federation;
        //    if (other == null)
        //    {
        //        return false;
        //    }
        //    if (other.FederationNamespaceMapFilename != FederationNamespaceMapFilename)
        //    {
        //        return false;
        //    }
        //    return true;
        //}
        [ExposedMethod("About",  ExposedMethodFlags.NeedContext, "Answer the 'about' string for the federation")]
        public string ExposedAbout(ExecutionContext ctx)
        {
            return AboutWikiString;
        }
        /// <summary>
        /// Answer the NamespaceManager for the given namespace (or null if it's an absent namespace)
        /// </summary>
        /// <param name="ns">Name of the namespace</param>
        /// <returns></returns>
        [ExposedMethod("GetNamespaceInfo",  ExposedMethodFlags.NeedContext, "Answer the given namespace")]
        public NamespaceManager ExposedNamespaceManagerForNamespace(ExecutionContext ctx, string ns)
        {
            NamespaceManager answer = NamespaceManagerForNamespace(ns);
            return answer;
        }
        [ExposedMethod("Namespaces",  ExposedMethodFlags.NeedContext, "Answer an array of namespaces in the federation")]
        public ArrayList ExposedNamespaces(ExecutionContext ctx)
        {
            // We do this because we changed to using generic types during the 2.0 upgrade.
            // Eventually this method should change so it's not using ArrayList any more. 
            ArrayList exposedNamespaces = new ArrayList();
            foreach (NamespaceManager manager in AllNamespaces)
            {
                exposedNamespaces.Add(manager); 
            }
            return exposedNamespaces;
        }
		[ExposedMethod("Application", ExposedMethodFlags.Default, "Answer the value associated with the key from the IWikiApplication")]
		public object ExposedApplicationProperty(string key)
		{
			return _application[key];
		}

        public static PerformanceCounter GetPerformanceCounter(string name)
        {
            if (!s_performanceCounterMap.ContainsKey(name))
            {
                return null;
            }
            return (PerformanceCounter) (s_performanceCounterMap[name]);
        }
        public TopicChangeCollection GetTopicChanges(TopicName name)
        {
            return ForwardToNamespaceManager<TopicChangeCollection>(name, delegate(NamespaceManager manager)
            {
                return manager.AllChangesForTopic(name.LocalName);
            }
            );
        }
        //public override int GetHashCode()
        //{
        //    int answer = 0;
        //    if (FederationNamespaceMapFilename != null)
        //    {
        //        answer ^= FederationNamespaceMapFilename.GetHashCode();
        //    }
        //    return answer;
        //}
        public DateTime GetTopicCreationTime(QualifiedTopicRevision name)
        {
            NamespaceManager manager = NamespaceManagerForNamespace(name.Namespace);

            if (manager == null)
            {
                throw new ArgumentException("Could not find the namespace " + name.Namespace); 
            }

            return manager.GetTopicCreationTime(name.AsUnqualifiedTopicRevision()); 
        }
        public DateTime GetTopicCreationTime(TopicName name)
        {
            return GetTopicCreationTime(new QualifiedTopicRevision(name)); 
        }
        public string GetTopicFormattedBorder(QualifiedTopicRevision name, Border border)
        {
            bool useNewParser = false;
            if (!(bool)Application["DisableNewParser"])
            {
                if ((bool)Application["EnableNewParser"])
                {
                    useNewParser = true;
                }
                else if (GetTopicPropertyValue(name, "DisplayUsingNewParser").ToLower().Contains("true"))
                {
                    useNewParser = true;
                }
            }
            string answer = null;
            // OK, we need to figure it out.  
            if (useNewParser == false)
            {
                IEnumerable borderText = BorderText(name, border);
                WikiOutput output = new HTMLWikiOutput(null);
                foreach (IBELObject borderComponent in borderText)
                {
                    IOutputSequence seq = borderComponent.ToOutputSequence();
                    // output sequence -> pure presentation tree
                    IWikiToPresentation presenter = Formatter.WikiToPresentation(name, output, NamespaceManagerForTopic(name), LinkMaker, null, 0);
                    IPresentation pres = seq.ToPresentation(presenter);
                    pres.OutputTo(output);
                }
                answer = output.ToString();
            }
            else
            {     //use new parser - some convolution due to less than straightforward processing method in original processes
                IEnumerable borderText = BorderText(name, border);
                WikiOutput output = new WomDocument(null);
                foreach (IBELObject borderComponent in borderText)
                {
                    IOutputSequence seq = borderComponent.ToOutputSequence();
                    // output sequence -> pure presentation tree
                    
                    IWikiToPresentation presenter = _parser.WikiToPresentation(name, output, NamespaceManagerForTopic(name), LinkMaker, null, 0, true);
                    IPresentation pres = seq.ToPresentation(presenter);
                    pres.OutputTo(output);
                }
                answer = "";
                WomDocument doc = (WomDocument) output;
                if (!String.IsNullOrEmpty(doc.ParsedDocument))
                {
                    WomDocument xmldoc = _parser.ProcessText(doc.ParsedDocument, name, NamespaceManagerForTopic(name), true, 1200);
                    answer = _parser.WikiToPresentation(xmldoc.XmlDoc);
                    xmldoc = null;
                    doc = null;
                }
            }
            return answer;
        }
        public string GetTopicFormattedBorder(TopicName name, Border border)
        {
            return GetTopicFormattedBorder(new QualifiedTopicRevision(name), border); 
        }
        public string GetTopicFormattedContent(QualifiedTopicRevision name, QualifiedTopicRevision withDiffsToThisTopic)
        {
            bool useNewParser = false;
            if (!(bool)Application["DisableNewParser"])
            {
                if ((bool)Application["EnableNewParser"])
                {
                    useNewParser = true;
                }
                else if (GetTopicPropertyValue(name, "DisplayUsingNewParser").ToLower().Contains("true"))
                {
                    useNewParser = true;
                }
            }
            string answer = null;

            // If the content is blacklisted and this is a historical version, answer dummy content
            if (name.Version != null && IsBlacklisted(Read(name)))
            {
                if (useNewParser == false)
                {
                    answer = Formatter.FormattedString(name, @"%red big%This historical version of this topic contains content that has been banned by policy from appearing on this site.",
                      Format, this.NamespaceManagerForTopic(name), LinkMaker);
                }
                else
                {
                    WomDocument xmldoc = _parser.ProcessText(@"%red big%This historical version of this topic contains content that has been banned by policy from appearing on this site.", 
                        name, NamespaceManagerForTopic(name), false, 200);
                    answer = _parser.WikiToPresentation(xmldoc.XmlDoc);
                    xmldoc = null;
                }
            }
            else
            {
                if (useNewParser == false)
                {
                    answer = Formatter.FormattedTopic(name, Format, withDiffsToThisTopic, this, LinkMaker);
                }
                else
                { // use new parser
                    answer = _parser.FormattedTopic(name, Format, withDiffsToThisTopic);
                }
            }
            return answer;
        }
        public string GetTopicHeaders(QualifiedTopicRevision topic)
        {
            NamespaceManager namespaceManager = NamespaceManagerForTopic(topic);
            if (namespaceManager == null)
            {
                return null;
            }
            return namespaceManager.GetTopicHeaders(topic.LocalName);
        }
        /// <summary>
        /// Answer the TopicInfo for the given topic (or null if it's an absent)
        /// </summary>
        /// <param name="top">Name of the topic (including namespace)</param>
        /// <returns></returns>
        public TopicVersionInfo GetTopicInfo(string top)
        {
            QualifiedTopicRevision abs = new QualifiedTopicRevision(top);
            if (abs.Namespace == null)
            {
                throw new Exception("Only fully-qualified topic names can be used with GetTopicInfo(): only got " + top);
            }
            return new TopicVersionInfo(this, abs);
        }
        /// <summary>
        /// Answer the TopicInfo for the given topic (or null if it's an absent)
        /// </summary>
        /// <param name="top">Name of the topic (including namespace)</param>
        /// <returns></returns>
        [ExposedMethod(ExposedMethodFlags.NeedContext, "Get information about the given topic")]
        public TopicVersionInfo GetTopicInfo(ExecutionContext ctx, string top)
        {
            QualifiedTopicRevision abs = new QualifiedTopicRevision(top);
            return GetTopicInfo(top);
        }
        public string GetTopicLastModifiedBy(TopicName topic)
        {
            NamespaceManager manager = NamespaceManagerForTopic(topic);

            if (manager == null)
            {
                throw FlexWikiException.NamespaceDoesNotExist(topic);
            }

            TopicChangeCollection changes = manager.AllChangesForTopic(topic.LocalName);

            if ((changes == null) || (changes.Count == 0))
            {
                return null; 
            }

            return changes.Latest.Author; 
        }
        public DateTime GetTopicLastModificationTime(TopicName topic)
        {
            NamespaceManager manager = NamespaceManagerForTopic(topic);

            if (manager == null)
            {
                throw FlexWikiException.NamespaceDoesNotExist(topic); 
            }

            return manager.GetTopicLastModificationTime(topic.LocalName); 
        }
        public ArrayList GetTopicListPropertyValue(QualifiedTopicRevision topic, string field)
        {

            NamespaceManager manager = NamespaceManagerForTopic(topic);

            if (manager == null)
            {
                throw FlexWikiException.NamespaceDoesNotExist(topic.AsQualifiedTopicName()); 
            }

            TopicProperty property = manager.GetTopicProperty(topic.AsUnqualifiedTopicRevision(), field);

            ArrayList propertyValues = new ArrayList();

            if (property != null)
            {
                foreach (string value in property.AsList())
                {
                    propertyValues.Add(value);
                }
            }

            return propertyValues; 

            //return ParseListPropertyValue(GetTopicProperty(topic, propertyName));
        }
        public ArrayList GetTopicListPropertyValue(TopicName topic, string field)
        {
            return GetTopicListPropertyValue(new QualifiedTopicRevision(topic), field); 
        }
        public DateTime GetTopicModificationTime(QualifiedTopicRevision topicRevision)
        {
            return ForwardToNamespaceManager<DateTime>(topicRevision, delegate(NamespaceManager manager)
            {
                TopicChangeCollection changes = manager.AllChangesForTopic(topicRevision.LocalName);

                if (changes == null)
                {
                    return DateTime.MinValue; 
                }

                if (topicRevision.Version == null)
                {
                    return changes.Latest.Modified;
                }
                else
                {
                    foreach (TopicChange change in changes)
                    {
                        if (topicRevision.Version == change.Version)
                        {
                            return change.Modified; 
                        }
                    }

                    return DateTime.MinValue; 
                }
            }
            );
        }
        public DateTime GetTopicModificationTime(TopicName topic)
        {
            return GetTopicModificationTime(new QualifiedTopicRevision(topic)); 
        }
        /// <summary>
        /// Answer all the imports (aka properties) for the given topic.  Uncached!
        /// This includes both the  imports defined in the topic plus the extra imports that every 
        /// topic has (e.g., _TopicName, _TopicFullName, _LastModifiedBy, etc.)
        /// </summary>
        /// <param name="topic"></param>
        /// <returns>Hashtable (keys = string propertyName names, values = values [as strings?]);  or null if the topic doesn't exist</returns>
        public TopicPropertyCollection GetTopicProperties(QualifiedTopicRevision topic)
        {
            NamespaceManager namespaceManager = NamespaceManagerForTopic(topic);
            if (namespaceManager == null)
            {
                return null;
            }
            return namespaceManager.GetTopicProperties(topic.LocalName);
        }
        public TopicProperty GetTopicProperty(TopicName topic, string property)
        {
            NamespaceManager manager = NamespaceManagerForTopic(topic);

            if (manager == null)
            {
                throw FlexWikiException.NamespaceDoesNotExist(topic); 
            }

            return manager.GetTopicProperty(topic.LocalName, property); 
        }
        public string GetTopicPropertyValue(TopicName topic, string property)
        {
            TopicProperty topicProperty = GetTopicProperty(topic, property);

            if (topicProperty == null)
            {
                return null; 
            }

            return topicProperty.LastValue; 
        }
        public string GetTopicPropertyValue(QualifiedTopicRevision topic, string property)
        {
            TopicPropertyCollection properties = GetTopicProperties(topic);
            if (properties == null || !properties.Contains(property))
            {
                return "";
            }
            return properties[property].LastValue;
        }
        public TopicInfoArray GetTopicsWithProperty(string property)
        {
            throw new NotImplementedException();
        }
        public string GetTopicUnformattedContent(QualifiedTopicRevision name)
        {
            throw new NotImplementedException();
        }
        public bool HasPermission(QualifiedTopicRevision revision, TopicPermission permission)
        {
            NamespaceManager manager = NamespaceManagerForTopic(revision);
            // It seems odd to say that we have permission for a nonexistent namespace, but we don't 
            // want to indicate that there's a security constraint when the real issue is that 
            // the namespace can't be found. 
            if (manager == null)
            {
                return true;
            }

            return manager.HasPermission(revision.AsUnqualifiedTopicRevision().AsUnqualifiedTopicName(), permission);
        }
        public bool IsBlacklisted(string wikiText)
        {
            if (wikiText == null)
            {
                return false;
            }
            string proposed = wikiText;
            foreach (string each in BlacklistedExternalLinkPrefixes)
            {
                if (proposed.ToUpper().IndexOf(each.ToUpper()) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Answer whether a topic exists and is writable
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <returns>true if the topic exists AND is writable by the current user; else false</returns>
        public bool IsExistingTopicWritable(QualifiedTopicRevision topic)
        {
            NamespaceManager namespaceManager = NamespaceManagerForTopic(topic);
            if (namespaceManager == null)
            {
                return false;
            }
            return namespaceManager.HasPermission(topic.AsUnqualifiedTopicRevision().AsUnqualifiedTopicName(), TopicPermission.Edit);
        }
        /// <summary>
        /// Answer whether or not a link is blacklisted.  You pass in a full URL and this does checking against all blacklist rules.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>true if blacklisted</returns>
        public bool IsLinkBlacklisted(string url)
        {
            foreach (string prefix in BlacklistedExternalLinkPrefixes)
            {
                if (url.ToUpper().StartsWith(prefix.ToUpper()))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Answer the NamespaceManager for the given namespace (or null if it's an absent namespace)
        /// </summary>
        /// <param name="ns">Name of the namespace</param>
        /// <returns></returns>
        public NamespaceManager NamespaceManagerForNamespace(string ns)
        {
            if (ns == null)
            {
                return null;
            }

            if (!_namespaceToNamespaceManagerMap.ContainsKey(ns))
            {
                return null;
            }

            NamespaceManager manager = _namespaceToNamespaceManagerMap[ns];
            // Providers like the security provider can mask the existence of a namespace. So we need to 
            // remove any registered namespaces from the list if they return false from Exists. 
            if (!manager.Exists)
            {
                return null;
            }
            else
            {
                return manager; 
            }
        }
        /// <summary>
        /// Answer the NamespaceManager for the given topic (will be based on its namespace)
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public NamespaceManager NamespaceManagerForTopic(QualifiedTopicRevision topic)
        {
            if (topic == null)
            {
                return null;
            }
            return NamespaceManagerForNamespace(topic.Namespace);
        }
        public NamespaceManager NamespaceManagerForTopic(TopicRevision topic, string nsRelativeTo)
        {
            if (topic.IsQualified)
            {
                return NamespaceManagerForNamespace(topic.Namespace);
            }

            return NamespaceManagerForNamespace(nsRelativeTo);
        }
        public NamespaceManager NamespaceManagerForTopic(TopicName topic, string nsRelativeTo)
        {
            return NamespaceManagerForTopic(new TopicRevision(topic.DottedName), nsRelativeTo);
        }
        public NamespaceManager NamespaceManagerForTopic(TopicName topic)
        {
            if (topic == null)
            {
                return null;
            }

            if (topic.IsQualified)
            {
                return NamespaceManagerForNamespace(topic.Namespace);
            }
            else
            {
                throw FlexWikiException.QualifiedTopicNameExpected(topic);
            }
        }
        public static ArrayList ParseListPropertyValue(string val)
        {
            ArrayList answer = new ArrayList();
            if (val == null || val.Length == 0)
            {
                return answer;
            }
            string[] vals = val.Split(new char[] { ',' });
            foreach (string s in vals)
            {
                answer.Add(s.Trim());
            }
            return answer;
        }
        /// <summary>
        /// Answer the contents of a given topic (uncached!)
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <returns>The contents of the topic or null if it can't be read (e.g., doesn't exist)</returns>
        public string Read(QualifiedTopicRevision topic)
        {
            NamespaceManager namespaceManager = NamespaceManagerForTopic(topic);
            if (namespaceManager == null)
            {
                return null;
            }
            return namespaceManager.Read(topic.AsUnqualifiedTopicRevision());
        }
        public string Read(TopicName topic)
        {
            if (topic.IsQualified)
            {
                return Read(new QualifiedTopicRevision(topic)); 
            }

            throw FlexWikiException.QualifiedTopicNameExpected(topic); 
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the number of registered users for this Federation")]
        public int RegisteredNumberOfUsers()
        {
            return _application.Membership.RegisteredUsers;
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the number of registered users online for this Federation")]
        public int RegisteredNumberOfUsersOnline()
        {
            return _application.Membership.RegisteredUsersOnline;
        }
        /// <summary>
        /// Register the given content store in the federation.
        /// </summary>
        public NamespaceManager RegisterNamespace(IContentProvider contentStore, string ns)
        {
            return RegisterNamespace(contentStore, ns, null); 
        }
        /// <summary>
        /// Register the given content store in the federation.
        /// </summary>
        public NamespaceManager RegisterNamespace(IContentProvider contentStore, string ns, 
            NamespaceProviderParameterCollection parameters)
        {
            // These are the default providers that every namespace gets. Later we might
            // want to make these configurable
            IContentProvider providerChain = new ModificationRecorder(contentStore); 
            providerChain = new BuiltinTopicsProvider(providerChain); 
            providerChain = new ParsingProvider(providerChain);
            providerChain = new TopicCacheProvider(providerChain); 
            providerChain = new AuthorizationProvider(providerChain);
            providerChain = new TransportSecurityRequirementProvider(providerChain);
            providerChain = new DependencyRecorder(providerChain); 
            
            NamespaceManager namespaceManager = new NamespaceManager(this, providerChain, ns, parameters);

            if (_namespaceToNamespaceManagerMap.ContainsKey(ns))
            {
                throw new Exception("Cannot register namespace " + ns + " because it is already registered."); 
            }

            _namespaceToNamespaceManagerMap.Add(ns, namespaceManager);

            return namespaceManager;
        }
        public void RemoveBlacklistedExternalLinkPrefix(string link)
        {
            if (!_blacklistedExternalLinkPrefixes.Contains(link))
            {
                return;
            }
            _blacklistedExternalLinkPrefixes.Remove(link);
            RecordFederationPropertiesChanged();
        }
        /// <summary>
        /// Rename the given topic.  If requested, find references and fix them up.  Answer a report of what was fixed up.  Throw a DuplicationTopicException
        /// if the new name is the name of a topic that already exists.
        /// </summary>
        /// <param name="oldName">Old topic name</param>
        /// <param name="newName">The new name</param>
        /// <param name="fixup">true to fixup referenced topic *in this namespace*; false to do no fixups</param>
        /// <returns>ArrayList of strings that can be reported back to the user of what happened during the fixup process</returns>
        public RenameTopicDetails RenameTopic(QualifiedTopicName oldName, UnqualifiedTopicName newName, ReferenceFixupPolicy fixupPolicy, string author)
        {
            NamespaceManager namespaceManager = NamespaceManagerForTopic(oldName);
            if (namespaceManager == null)
            {
                throw NamespaceNotFoundException.ForNamespace(oldName.Namespace);
            }
            return namespaceManager.RenameTopic(new UnqualifiedTopicName(oldName.LocalName), new UnqualifiedTopicName(newName.LocalName), fixupPolicy, author);
        }
        public void SetTopicProperty(QualifiedTopicRevision topic, string field, string value, bool writeNewVersion, string author)
        {
            NamespaceManagerForTopic(topic).SetTopicPropertyValue(topic.LocalName, field, value, writeNewVersion, author);
        }

        /// <summary>
        /// Answer true if the topic exists; else false
        /// </summary>
        public bool TopicExists(QualifiedTopicRevision topic)
        {
            NamespaceManager namespaceManager = NamespaceManagerForTopic(topic);
            if (namespaceManager == null)
            {
                return false;
            }
            return namespaceManager.TopicExists(topic.LocalName, ImportPolicy.DoNotIncludeImports);
        }
        public bool TopicIsReadOnly(QualifiedTopicRevision topic)
        {
            NamespaceManager namespaceManager = NamespaceManagerForTopic(topic);
            if (namespaceManager == null)
            {
                return false;
            }
            return namespaceManager.TopicIsReadOnly(topic.LocalName);
        }
        /// <summary>
        /// Answer the full name of the topic (qualified with namespace) if it exists.  
        /// If it doesn't exist at all, answer null.
        /// If it does, but it's ambiguous, then throw TopicIsAmbiguousException
        /// </summary>
        /// <param name="topic"></param>
        /// <returns>Full name or null if it doesn't exist (by throw TopicIsAmbiguousException if it's ambiguous)</returns>
        public QualifiedTopicRevision UnambiguousTopicNameFor(TopicRevision topic, string nsRelativeTo)
        {
            NamespaceManager manager = NamespaceManagerForTopic(topic, nsRelativeTo);

            if (manager == null)
            {
                return null;                     
            }

            NamespaceCollection namespaces = manager.TopicNamespaces(topic.LocalName);
            if (namespaces.Count == 0)
            {
                return null;
            }
            if (namespaces.Count > 1)
            {
                throw TopicIsAmbiguousException.ForTopic(topic);
            }
            return new QualifiedTopicRevision(topic.LocalName, (string)namespaces[0]);
        }
        /// <summary>
        /// Unregister the given NamespaceManager in the federation.
        /// </summary>
        /// <param name="namespaceManager"></param>
        public void UnregisterNamespace(NamespaceManager namespaceManager)
        {
            _namespaceToNamespaceManagerMap.Remove(namespaceManager.Namespace);
            RecordNamespacesListChanged();
        }

        // Invoke the FederationUpdated event; called whenever topics or other things about the federation change
        protected virtual void OnFederationUpdated(FederationUpdateEventArgs e)
        {
            if (FederationUpdated != null)
            {
                FederationUpdated(this, e);
            }
        }


        private IBELObject BorderPropertyFromTopic(QualifiedTopicRevision relativeToTopic, QualifiedTopicRevision abs, Border border)
        {
            NamespaceManager namespaceManager = NamespaceManagerForTopic(abs);
            if (namespaceManager == null)
            {
                return null;
            }

            if (!namespaceManager.TopicExists(abs.LocalName, ImportPolicy.DoNotIncludeImports))
            {
                return null;
            }

            // OK, looks like the topic exist -- let's see if the propertyName is there
            string borderPropertyName = BorderPropertyName(border);
            string prop = GetTopicPropertyValue(abs, borderPropertyName);
            if (prop == null || prop == "")
            {
                return null;
            }

            // Yup, so evaluate it!
            string code = "federation.GetTopic(\"" + abs.DottedName + "\")." + borderPropertyName + "(federation.GetTopicInfo(\"" + relativeToTopic + "\"))";

            BehaviorInterpreter interpreter = new BehaviorInterpreter(abs.DottedName + "#" + borderPropertyName, code, this, this.WikiTalkVersion, null);
            if (!interpreter.Parse())
            {
                throw new Exception("Border property expression failed to parse.");
            }
            TopicContext topicContext = new TopicContext(this, this.NamespaceManagerForTopic(abs), new TopicVersionInfo(this, abs));
            IBELObject obj = interpreter.EvaluateToObject(topicContext, null);
            if (interpreter.ErrorString != null)
            {
                obj = new BELString(interpreter.ErrorString);
            }

            return obj;
        }
        private string BorderPropertyName(Border border)
        {
            switch (border)
            {
                case Border.Bottom:
                    return "BottomBorder";
                case Border.Left:
                    return "LeftBorder";
                case Border.Right:
                    return "RightBorder";
                case Border.Top:
                    return "TopBorder";
                default:
                    return "";		// shouldn't really happen
            }
        }
        /// <summary>
        /// Answer a list of the wikitext components (IBELObjects) of the given border.  If nothing specifies any border; answer the system default
        /// </summary>
        /// <param name="name"></param>
        /// <param name="border"></param>
        /// <param name="rule"></param> 
        /// <returns></returns>
        private IEnumerable BorderText(QualifiedTopicRevision name, Border border)
        {
            ArrayList answer = new ArrayList();
            NamespaceManager namespaceManager;
            string bordersTopicsProperty = "Borders";

            ArrayList borderTopics = new ArrayList();


            // Start with whatever the namespace defines
            if (Borders != null)
            {
                foreach (string at in ParseListPropertyValue(Borders))
                {
                    QualifiedTopicRevision abs = new QualifiedTopicRevision(at);
                    namespaceManager = NamespaceManagerForTopic(abs);
                    if (abs == null || namespaceManager == null)
                    {
                        throw new Exception("Unknown namespace listed in border topic (" + at + ") listed in federation configuration Borders property.");
                    }
                    borderTopics.Add(at);
                }
            }


            // If the namespace, specifies border topics, get them
            namespaceManager = NamespaceManagerForTopic(name);
            if (namespaceManager != null)
            {
                borderTopics.AddRange(GetTopicListPropertyValue(
                    new QualifiedTopicRevision(namespaceManager.DefinitionTopicName), 
                    bordersTopicsProperty));
            }

            // If there are no border topics specified for the federation or the namespace, add the default (_NormalBorders from the local namespace)
            if (borderTopics.Count == 0)
            {
                borderTopics.Add("_NormalBorders");
            }


            // Finally, any border elements form the topic itself (skip the def topic so we don't get double borders!)
            if (namespaceManager == null || namespaceManager.DefinitionTopicName.ToString() != name.ToString())
            {
                borderTopics.AddRange(GetTopicListPropertyValue(name, bordersTopicsProperty));
            }


            Set<QualifiedTopicRevision> done = new Set<QualifiedTopicRevision>();
            foreach (string borderTopicName in borderTopics)
            {
                // Figure out what the qualified topic name is that we're going to get this topic from
                TopicRevision rel = new TopicRevision(borderTopicName);
                if (!rel.IsQualified)
                {
                    rel = new TopicRevision(borderTopicName, name.Namespace);
                }
                QualifiedTopicRevision abs = new QualifiedTopicRevision(rel.LocalName, rel.Namespace);
                if (done.Contains(abs))
                {
                    continue;
                }
                done.Add(abs);
                IBELObject s = BorderPropertyFromTopic(name, abs, border);
                if (s != null)
                {
                    answer.Add(s);
                }
            }

            return answer;
        }
        private static void CreateCounter(CounterCreationDataCollection ccdc, PerformanceCounterType type, string name)
        {
            CounterCreationData ccd;
            ccd = new CounterCreationData();
            ccd.CounterType = type;
            ccd.CounterName = name;
            ccdc.Add(ccd);
        }
        /// <summary>
        /// A FederationUpdateGenerator has completed generation.  Fire a FederationUpdate event for this content base.
        /// </summary>
        private void FederationUpdateGeneratorGenerationComplete(object sender, GenerationCompleteEventArgs e)
        {
            OnFederationUpdated(new FederationUpdateEventArgs(e.Updates));
        }
        private void FederationUpdateMonitor(object sender, FederationUpdateEventArgs e)
        {
            // One of the content bases we're hooked to has fired a federation update event -- just pass it on
            OnFederationUpdated(new FederationUpdateEventArgs(e.Updates));
        }
        private T ForwardToNamespaceManager<T>(TopicName name, NamespaceAction<T> action)  
        {
            if (!name.IsQualified)
            {
                return default(T);
            }
            NamespaceManager manager = NamespaceManagerForNamespace(name.Namespace);

            if (manager == null)
            {
                return default(T);
            }

            return action(manager);
        }
        private T ForwardToNamespaceManager<T>(TopicRevision revision, NamespaceAction<T> action)
        {
            if (!revision.IsQualified)
            {
                return default(T);
            }
            NamespaceManager manager = NamespaceManagerForNamespace(revision.Namespace);

            if (manager == null)
            {
                return default(T);
            }

            return action(manager);
        }
        private T ForwardToNamespaceManager<T>(QualifiedTopicRevision revision, NamespaceAction<T> action)
        {
            NamespaceManager manager = NamespaceManagerForNamespace(revision.Namespace);

            if (manager == null)
            {
                return default(T);
            }

            return action(manager);
        }
        private void Initialize(IWikiApplication application)
        {
            // FederationConfiguration configuration, OutputFormat format, LinkMaker linker)
            _application = application; 
            InitializePerformanceCounters();

            LoadFromConfiguration();
            //_parser = new ParserEngine(this);

        }
        private void InitializePerformanceCounter(string name)
        {
            PerformanceCounter counter = new PerformanceCounter(s_performanceCounterCategoryName, name, false);
            counter.RawValue = 0;
            s_performanceCounterMap[name] = counter;
        }
        private void InitializePerformanceCounters()
        {
            if (PerformanceCountersEnabled)
            {
                // If the counters exist, use them
                if (!PerformanceCounterCategoryExists)
                {
                    return;
                }
                InitializePerformanceCounter(PerformanceCounterNames.TopicReads);
                InitializePerformanceCounter(PerformanceCounterNames.TopicWrite);
                InitializePerformanceCounter(PerformanceCounterNames.TopicFormat);
                InitializePerformanceCounter(PerformanceCounterNames.TopicsCompared);
                InitializePerformanceCounter(PerformanceCounterNames.MethodInvocation);
            }
        }
        /// <summary>
        /// Load the Federation up with all of the configuration information in the given FederationConfiguration.  
        /// Directories listed in each NamespaceToRoot are made relative to the given relativeDirectoryBase.
        /// If relativeDirectoryBase is null and a relative reference is used, an Exception will be throw
        /// </summary>
        /// <param name="config"></param>
        public void LoadFromConfiguration()
        {
            FederationConfiguration configuration = Application.FederationConfiguration;

            if (configuration != null)
            {
                if (configuration.NamespaceMappings != null)
                {
                    foreach (NamespaceProviderDefinition definition in configuration.NamespaceMappings)
                    {
                        LoadNamespacesFromProviderDefinition(definition);
                    }
                }

                Borders = configuration.Borders;
                AboutWikiString = configuration.AboutWikiString;
                WikiTalkVersion = configuration.WikiTalkVersion;
                NoFollowExternalHyperlinks = configuration.NoFollowExternalHyperlinks;

                if (configuration.BlacklistedExternalLinks != null)
                {
                    foreach (string link in configuration.BlacklistedExternalLinks)
                    {
                        AddBlacklistedExternalLinkPrefix(link);
                    }
                }
            }
        }
        /// <summary>
        /// Create a namespace provider for the description and then give it a chance to create its namespace(s)
        /// </summary>
        /// <param name="def"></param>
        private void LoadNamespacesFromProviderDefinition(NamespaceProviderDefinition def)
        {
            // We need to create it
            Type providerType = def.ProviderType;
            if (providerType == null)
            {
                throw new Exception("Unrecognized type (" + def.Type + ") in assembly (" + (def.AssemblyName == null ? "[default]" : def.AssemblyName) + ")");
            }
            object c = Activator.CreateInstance(providerType);
            INamespaceProvider provider = c as INamespaceProvider;
            if (provider == null)
            {
                throw new Exception("Illegal type (" + def.Type + ") in assembly (" + (def.AssemblyName == null ? "[default]" : def.AssemblyName) + ").  Type must implement INamespaceProvider.");
            }
            def.SetParametersInProvider(provider);
            provider.LoadNamespaces(this);
        }
        private void RecordFederationPropertiesChanged()
        {
            UpdateGenerator.Push();
            try
            {
                UpdateGenerator.RecordFederationPropertiesChanged();
            }
            finally
            {
                UpdateGenerator.Pop();
            }
        }
        private void RecordNamespacesListChanged()
        {
            UpdateGenerator.Push();
            try
            {
                UpdateGenerator.RecordNamespaceListChanged();
            }
            finally
            {
                UpdateGenerator.Pop();
            }
        }
    }
}
