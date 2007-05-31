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
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization; 

using FlexWiki;
using FlexWiki.Formatting;
using FlexWiki.Security; 
using FlexWiki.Web;


namespace FlexWiki.Web.Admin
{
    /// <summary>
    /// Summary description for ConfigurationChecker.
    /// </summary>
    public class ConfigurationChecker
    {
        // Fields

        private readonly List<Result> _results = new List<Result>();

        // Constructors

        public ConfigurationChecker()
        {
        }

        // Properties

        public Level AggregateResultLevel
        {
            get
            {
                Level answer = Level.OK;
                foreach (Result each in Results)
                {
                    if ((answer == Level.OK || answer == Level.Warning) && each.ResultLevel == Level.Error)
                    {
                        answer = each.ResultLevel;
                    }
                    if ((answer == Level.OK) && each.ResultLevel == Level.Warning)
                    {
                        answer = each.ResultLevel;
                    }
                }
                return answer;
            }
        }
        public string ApplicationConfigurationPath
        {
            get
            {
                FlexWikiWebApplication application = new FlexWikiWebApplication(new LinkMaker("dummy://here"));
                return application.ApplicationConfigurationPath; 
            }
        }
        public IList<Result> Results
        {
            get
            {
                return _results;
            }
        }

        // Methods

        public void Check()
        {
            _results.Clear();
            FlexWikiWebApplication application = null;
            try
            {
                application = new FlexWikiWebApplication(
                    new LinkMaker("http://dummy"));
            }
            catch (Exception e)
            {
                Result r = new Result("Error initializing the application", Level.Error);
                r.Writer.WritePara("The error was: " + HtmlWriter.Escape(e.Message));
                AddResult(r); 
            }

            if (application == null)
            {
                return; 
            }

            FederationConfiguration config = application.FederationConfiguration;

            CheckPluginTypes();

            if (config != null)
            {
                CheckProviders(config);
                try
                {
                    Federation aFed = new Federation(application);
                    ValidateDefaultNamespace(aFed);
                    ValidateWritableNamespaces(aFed);
                }
                catch (Exception e)
                {
                    Result r = new Result("Error loading federation", Level.Error);
                    r.Writer.WritePara("Error loading federation: " + HtmlWriter.Escape(e.Message));
                    AddResult(r);
                }
            }
        }
        public IList ResultsOfLevel(Level aLevel)
        {
            ArrayList answer = new ArrayList();
            foreach (Result each in Results)
                if (each.ResultLevel == aLevel)
                    answer.Add(each);
            return answer;
        }
        public void WriteStoplightTo(HtmlWriter w)
        {
            string image = null;
            string text = null;
            switch (AggregateResultLevel)
            {
                case Level.OK:
                    image = "../images/green-light";
                    text = "FlexWiki is correctly configured.";
                    break;

                case Level.Warning:
                    image = "../images/yellow-light";
                    text = "FlexWiki is probably correctly configured, but you should review configuration warnings.";
                    break;

                case Level.Error:
                    image = "../images/red-light";
                    text = "FlexWiki is not configured correctly.";
                    break;
            }

            w.WriteCenter(
              w.Link("Config.aspx", w.Image(image + ".gif"), "Review configuration details") +
              w.Para(w.Bold(HtmlWriter.Escape(text)) + "<br/>" + "For more information, click on the stoplight."));
        }
        public void WriteTo(HtmlWriter writer)
        {
            UITable t = new UITable();
            UIColumn color = new UIColumn();
            t.AddColumn(color);
            UIColumn text = new UIColumn("Description");

            writer.WriteStartTable(t);

            foreach (Result each in Results)
            {
                string bg = null;

                switch (each.ResultLevel)
                {
                    case Level.OK:
                        bg = "lightgreen";
                        break;

                    case Level.Info:
                        bg = "lightgreen";
                        break;

                    case Level.Warning:
                        bg = "yellow";
                        break;

                    case Level.Error:
                        bg = "red";
                        break;
                }

                writer.WriteStartRow();
                writer.Write("<td valign='top' bgcolor=" + bg + ">&nbsp;&nbsp;</td>");
                writer.Write("<td valign='top' class='GenericCell' bgcolor='#e0e0e0'>" + writer.Bold(HtmlStringWriter.Escape(each.Title)) + "</td>");
                writer.WriteEndRow();

                writer.WriteStartRow();
                writer.Write("<td valign='top' bgcolor=" + bg + ">&nbsp;&nbsp;</td>");
                writer.WriteCell(text, each.Writer.ToString());
                writer.WriteEndRow();
            }

            writer.WriteEndTable();
        }

        private void AddResult(Result aRes)
        {
            Results.Add(aRes);
        }
        //////////
        ///Make sure we can read the configuration file in
        ///
        private void CheckPluginTypes()
        {
            FlexWikiWebApplication application = new FlexWikiWebApplication(new LinkMaker(""));
            FederationConfiguration configuration = application.FederationConfiguration;
            if (configuration == null)
                return;

            foreach (string plugin in configuration.Plugins)
            {
                Assembly assembly = null;
                string assemblyError = null;
                try
                {
                    assembly = Assembly.Load(plugin);
                }
                catch (FileNotFoundException)
                {
                    assemblyError = "Assembly not found.";
                }
                catch (BadImageFormatException)
                {
                    assemblyError = "Assembly is not a valid managed assembly.";
                }
                if (assemblyError == null)
                {
                    Result r = new Result("Plugin found and loaded successfully", Level.OK);
                    r.Writer.Write(@"<p>The plugin <b>" + HtmlWriter.Escape(plugin) + "</b> has been successfully loaded.</p>");
                    AddResult(r);
                }
                else
                {
                    Result r = new Result("Plugin not found", Level.Error);
                    r.Writer.Write(@"<p>The plugin <b>" + HtmlWriter.Escape(plugin) + "</b> could not be loaded.</p>");
                    r.Writer.Write(@"<p>" + HtmlWriter.Escape(assemblyError) + "</p>");
                    AddResult(r);
                }
            }

        }
        private void CheckProviders(FederationConfiguration config)
        {
            ArrayList uniqueProviders = new ArrayList();

            // Now make sure each provider is well-defined
            foreach (NamespaceProviderDefinition provider in config.NamespaceMappings)
            {
                if (provider.Id == null || (provider.Id != null && provider.Id.Length == 0))
                {
                    Result r = new Result("Missing Id attribute in <Namespace> element", Level.Error);
                    r.Writer.Write(@"<p>You did not specify the a unique Id for the NamespaceProvider in the in the &lt;Namespaces&gt; section of the configuration file.
	<p>Here is an example of a valid federation configuration file:");
                    r.Writer.Write(ExampleConfig());
                    AddResult(r);
                }
                if (provider.Id != null)
                {
                    if (uniqueProviders.Contains(provider.Id))
                    {
                        Result r = new Result("Id attribute value in <Namespace> element must be unique", Level.Error);
                        r.Writer.Write(@"<p>You specified a non-unique Id:&quot;" + provider.Id + @"&quot; for the NamespaceProvider in the in the &lt;Namespaces&gt; section of the configuration file.
	<p>Here is an example of a valid federation configuration file:");
                        r.Writer.Write(ExampleConfig());
                        AddResult(r);
                    }
                    else
                    {
                        uniqueProviders.Add(provider.Id);
                    }
                }
                if (provider.Type == null)
                {
                    Result r = new Result("Missing Type attribute in &lt;Namespace&gt; element", Level.Error);
                    r.Writer.Write(@"<p>You did not specify the Type of a NamespaceProvider in the in the &lt;Namespaces&gt; section of the configuration file.
	<p>Here is an example of a valid federation configuration file:");
                    r.Writer.Write(ExampleConfig());
                    AddResult(r);
                }
                // If an assembly is specified, make sure it can be found
                if (provider.AssemblyName != null)
                {
                    try
                    {
                        System.Reflection.Assembly.Load(provider.AssemblyName);
                        OK("Successfully loaded assembly " + HtmlWriter.Escape(provider.AssemblyName));
                    }
                    catch (Exception e)
                    {
                        Result r = new Result("Error loading namespace provider assembly", Level.Error);
                        r.Writer.Write(@"<p>The assembly <b>" + HtmlWriter.Escape(provider.AssemblyName) + @"</b> you specified could not be loaded.  The following error occurred: <p>" + HtmlWriter.Escape(e.ToString(), true));
                        AddResult(r);
                    }
                }
                // Make sure the type is a valid type and can be created
                if (provider.Type != null)
                {
                    try
                    {
                        Type x = provider.ProviderType;	// see if it'll resolve, but catch exceptions
                        if (x == null)
                            throw new Exception("Unable to find type.");
                        OK("Successfully loaded type " + HtmlWriter.Escape(provider.Type));
                    }
                    catch (Exception e)
                    {
                        Result r = new Result("Error loading namespace provider type", Level.Error);
                        r.Writer.Write(@"<p>The type <b>" + HtmlWriter.Escape(provider.Type) + @"</b> you specified could not be loaded.");
                        if (provider.AssemblyName == null)
                            r.Writer.Write(@"The assembly used was the base FlexWiki engine.");
                        else
                            r.Writer.Write(@"The assembly used was: " + HtmlWriter.Escape(provider.AssemblyName));
                        r.Writer.Write("<p>The following error occurred: <p>" + HtmlWriter.Escape(e.Message, true));
                        AddResult(r);
                    }
                }

                // TODO: Check for per-provider parms
            }
        }

        private string ExampleConfig()
        {
            HtmlStringWriter w = new HtmlStringWriter();
            w.Write("<blockquote><pre>");
            FlexWikiWebApplicationConfiguration appConfig = new FlexWikiWebApplicationConfiguration();
            appConfig.FederationConfiguration = new FederationConfiguration();
            appConfig.FederationConfiguration.AboutWikiString = "Text about the wiki here";
            appConfig.FederationConfiguration.AuthorizationRules.Add(
                new WikiAuthorizationRule(
                    new AuthorizationRule(
                        new AuthorizationRuleWho(AuthorizationRuleWhoType.GenericAll),
                        AuthorizationRulePolarity.Allow,
                        AuthorizationRuleScope.Wiki,
                        SecurableAction.Edit,
                        0)
                )
            );
            appConfig.FederationConfiguration.AuthorizationRules.Add(
                new WikiAuthorizationRule(
                    new AuthorizationRule(
                        new AuthorizationRuleWho(AuthorizationRuleWhoType.GenericAuthenticated),
                        AuthorizationRulePolarity.Allow,
                        AuthorizationRuleScope.Wiki,
                        SecurableAction.ManageNamespace,
                        1)
                )
            );
            appConfig.FederationConfiguration.DefaultNamespace = "SampleNamespaceOne"; 
            
            NamespaceProviderDefinition sampleNamespaceOne = new NamespaceProviderDefinition(
                typeof(FileSystemNamespaceProvider).Assembly.FullName, 
                typeof(FileSystemNamespaceProvider).FullName, 
                string.Empty);
            sampleNamespaceOne.Parameters.Add(new NamespaceProviderParameter("Root", @".\SampleNamespaceOne"));

            appConfig.FederationConfiguration.NamespaceMappings.Add(sampleNamespaceOne);

            appConfig.FederationConfiguration.WikiTalkVersion = 1;

            XmlSerializer serializer = new XmlSerializer(typeof(FlexWikiWebApplicationConfiguration));
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            writerSettings.IndentChars = "  ";
            StringWriter stringWriter = new StringWriter();
            XmlWriter xmlWriter = XmlWriter.Create(stringWriter, writerSettings);

            serializer.Serialize(xmlWriter, appConfig);

            xmlWriter.Close(); 
                        
            w.Write(HtmlStringWriter.Escape(stringWriter.ToString()));
            w.Write("</pre></blockquote>");
            return w.ToString();
        }
        private void Error(string title)
        {
            Error(title, null);
        }
        private void Error(string title, string htmlBody)
        {
            Result r = new Result(title, Level.Error);
            if (htmlBody != null)
                r.Writer.Write(htmlBody);
            AddResult(r);
        }
        private void Info(string title)
        {
            Info(title, null);
        }
        private void Info(string title, string htmlBody)
        {
            Result r = new Result(title, Level.Info);
            if (htmlBody != null)
                r.Writer.Write(htmlBody);
            AddResult(r);
        }
        private static IContentProvider ContentStore(NamespaceManager storeManager)
        {
            BuiltinTopicsProvider builtInTopicsProvider = 
                (BuiltinTopicsProvider) storeManager.GetProvider(typeof(BuiltinTopicsProvider));
            IContentProvider provider = builtInTopicsProvider.Next;
            while (provider.Next != null)
            {
                provider = provider.Next; 
            }

            return provider; 
        }
        private void OK(string title)
        {
            OK(title, null);
        }
        private void OK(string title, string htmlBody)
        {
            Result r = new Result(title, Level.OK);
            if (htmlBody != null)
                r.Writer.Write(htmlBody);
            AddResult(r);
        }
        private void ValidateWritableNamespaces(Federation aFed)
        {
            /// Make sure we can write data (correct ASP.NET permissions)
            ArrayList exceptions = new ArrayList();
            ArrayList paths = new ArrayList();

            foreach (NamespaceManager namespaceManager in aFed.NamespaceManagers)
            {
                // TODO: This bakes in knowledge of the internals. Probably not a good idea. 
                if (!(ContentStore(namespaceManager) is FileSystemStore))
                    continue;

                FileSystemStore store = ContentStore(namespaceManager) as FileSystemStore;
                string directoryToWriteTo = store.Root;

                try
                {
                    System.IO.StreamWriter sw = File.CreateText(Path.Combine(directoryToWriteTo, "testFile.txt"));
                    sw.Close();

                    Result r = new Result("Namespace (" + HtmlWriter.Escape(namespaceManager.Namespace) + ") confirmed as writable.", Level.OK);
                    r.Writer.WritePara(String.Format("Path\"{0}\" is writable", directoryToWriteTo));
                    AddResult(r);
                }
                catch (Exception ex)
                {
                    Result r = new Result("Namespace (" + HtmlWriter.Escape(namespaceManager.Namespace) + ") not writable.", Level.Error);
                    r.Writer.WritePara(String.Format("Path\"{0}\" is not writable", directoryToWriteTo));
                    r.Writer.WritePara("Error was: <br>" + HtmlWriter.Escape(ex.ToString(), true));
                    AddResult(r);
                }

                if (File.Exists(Path.Combine(directoryToWriteTo, "testFile.txt")))
                {
                    File.Delete(Path.Combine(directoryToWriteTo, "testFile.txt"));
                }
            }
        }
        private void ValidateDefaultNamespace(Federation aFed)
        {
            ///////////
            ///Make sure the default namespace is configured
            string defaultNamespace = aFed.Configuration.DefaultNamespace;
            if (defaultNamespace == null)
            {
                Result r = new Result("Default namespace not specified", Level.Error);
                r.Writer.Write(@"<p>You have not specified the default namespace for your federation in the federation configuration file (<b>" + HtmlWriter.Escape(ApplicationConfigurationPath) + @"</b>).
This setting must be present and must name a namespace listed in your configuration file. <p>Here is an example of a valid federation configuration file:");
                r.Writer.Write(ExampleConfig());
                AddResult(r);
                return;
            }

            if (aFed.NamespaceManagerForNamespace(defaultNamespace) == null)
            {
                Result r = new Result("Default namespace not found", Level.Error);
                r.Writer.Write(@"<p>You have specified the default namespace for your federation in the federation configuration file (<b>" + HtmlWriter.Escape(ApplicationConfigurationPath) + @"</b>),
but the namespace you specified can not be found (either because it's not listed in the configuration file or it's not valid).
<p>Here is an example of a valid federation configuration file:");
                r.Writer.Write(ExampleConfig());
                AddResult(r);
                return;
            }

            OK("Valid default namespace setting detected: " + HtmlWriter.Escape(defaultNamespace));
        }
        private void Warning(string title)
        {
            Warning(title, null);
        }
        private void Warning(string title, string htmlBody)
        {
            Result r = new Result(title, Level.Warning);
            if (htmlBody != null)
                r.Writer.Write(htmlBody);
            AddResult(r);
        }
        private static string WriteExampleWebConfig()
        {
            HtmlStringWriter w = new HtmlStringWriter();
            w.Write("<blockquote><pre>");
            w.Write(HtmlStringWriter.Escape(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
	<appSettings>

		<!-- PUT THE LOGICAL PATH TO THE FEDERATION CONFIGURATION FILE HERE -->
		<add key=""FederationNamespaceMapFile"" value=""/NamespaceMap.xml"" />

		<!-- SET THE FOLLOWING KEY TO AN SMTP SERVER THAT WILL DELIVER MAIL.
                NEEDED IF YOU WANT YOUR WIKI SITE TO BE ABLE TO DELIVER NEWSLETTERS -->
		<add key=""SMTPServer"" value=""mail.my-server.com"" />

		<!-- SET THE FOLLOWING KEY TO THE FULL FULLY-QUALIFIED ADDRESS OF THE USER NAME 
                TO USE TO AUTHENTICATE AGAINST THE SMTP SERVER -->
		<add key=""SMTPUser"" value=""user@domain.com"" />

		<!-- SET THE FOLLOWING KEY IF THE SMTP SERVER NEEDS LOGIN AUTHENTICATION -->
		<add key=""SMTPPassword"" value=""password goes here"" />

		<!-- SET THE FOLLOWING KEY TO THE DESIRED FROM ADDRESS FOR NEWSLETTERS -->
		<add key=""NewslettersFrom"" value=""newsletters@mysite.com"" />

	</appSettings>
	<system.web> 
		<authentication mode=""Windows"" />
		<authorization> 
			<deny users=""?""/> 
		</authorization> 
		<pages validateRequest = ""false"" />
	</system.web>
</configuration>"));
            w.Write("</pre></blockquote>");
            return w.ToString();
        }
    }
}
