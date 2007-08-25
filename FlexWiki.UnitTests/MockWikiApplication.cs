using System;
using System.Xml;
using System.Xml.Serialization;


using FlexWiki.Caching; 
using FlexWiki.UnitTests.Caching;
using FlexWiki.Web;

namespace FlexWiki.UnitTests
{
    internal class MockWikiApplication : IWikiApplication
    {
        private MockCache _cache = new MockCache(); 
        private FederationConfiguration _configuration;
        private FlexWikiWebApplicationConfiguration _applicationConfiguration;
        private bool _isTransportSecure; 
        private LinkMaker _linkMaker;
        private OutputFormat _ouputFormat;
        private ITimeProvider _timeProvider; 


        public MockWikiApplication(FederationConfiguration configuration, LinkMaker linkMaker,
            OutputFormat outputFormat, ITimeProvider timeProvider)
        {
            _configuration = configuration;
            _linkMaker = linkMaker;
            _ouputFormat = outputFormat;
            _timeProvider = timeProvider; 

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

        public OutputFormat OutputFormat
        {
            get { return _ouputFormat; }
        }

		public object this[string key]
		{
			get { return null; }
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
  <!-- The below keys define CAPTCHA behavior. CAPTCHA is an anti-spam measure. 
        Read more about CAPTCHA at http://en.wikipedia.org/wiki/Captcha. -->

  <!-- Defines the CAPTCHA behavior of FlexWiki. Possible values: 
  
        Never = Never require CAPTCHA verification. 
        Always = Always require CAPTCHA verification. 
        IfAnonymous = Require CAPTCHA verification whenever an anonymous user makes edits. 
        WhenOverLinkThreshold = Require CAPTCHA verification if n or more links are added 
          to a topic (see CaptchaLinkThreshold below). -->
  <RequireCaptchaOnEdit>Never</RequireCaptchaOnEdit>

  <!-- Defines the secret key used to generate CAPTCHA verification images (when enabled). 
        CHANGE THIS TO A RANDOM 16-DIGIT HEX NUMBER! -->
  <CaptchaKey>0123456789ABCDEF</CaptchaKey>

  <!-- Defines the number of links that must be added to a topic before CAPTCHA will be 
        required when <RequireCaptchaOnEdit> is set to CaptchaLinkThreshold. -->
  <CaptchaLinkThreshold>5</CaptchaLinkThreshold>

  <!-- Identifies the path to a directory tree where uploads to the wiki will be stored. If
        this key does not exist, then the file upload and attachment controls will not be 
        accessible in WikiEdit.aspx. The directory tree is placed in the virtual directory at
        the root of the wiki and all directories must be precreated. In addition to the 
        directories in the key, there are 3 other idrectories that must be created as subdirectories:
        images, html or doc. Using the key below as a sample the directory tree would be:
             RootUrl
               content
                    upload
                        images
                        html
                        doc   -->
  <ContentUploadPath>content\upload</ContentUploadPath>
  
  <!-- The Silk Icons are provided by Mark James at http://www.famfamfam.com/lab/icons/silk/ . 
        The license for these icons is Creative Commons 2.5 Attribution license.
        These icons are used to denote attachments to topics when a file is uploaded to the wiki.
        Depending upon the file extension of the upload, that extension is matched with the IconKey
        to determine what icon is displayed -->
  <AttachmentIcons>
    <AttachmentIcon Href=""page_white_picture.png"" IconKey=""png"" />
    <AttachmentIcon Href=""page_white_picture.png"" IconKey=""gif"" />
    <AttachmentIcon Href=""page_white_picture.png"" IconKey=""jpg"" />
    <AttachmentIcon Href=""page_white_picture.png"" IconKey=""jpeg"" />
    <AttachmentIcon Href=""page_white_word.png"" IconKey=""doc"" />
    <AttachmentIcon Href=""page_excel.png"" IconKey=""xls"" />
    <AttachmentIcon Href=""page_white_acrobat.png"" IconKey=""pdf"" />
    <AttachmentIcon Href=""page_white_csharp.png"" IconKey=""cs"" />
    <AttachmentIcon Href=""html.png"" IconKey=""html"" />
    <AttachmentIcon Href=""html.png"" IconKey=""htm"" />
    <AttachmentIcon Href=""html.png"" IconKey=""aspx"" />
    <AttachmentIcon Href=""page_white_database.png"" IconKey=""mdb"" />
    <AttachmentIcon Href=""page_white_database.png"" IconKey=""sql"" />
    <AttachmentIcon Href=""page_white_compressed.png"" IconKey=""zip"" />
    <AttachmentIcon Href=""page_white_text.png"" IconKey=""txt"" />
    <AttachmentIcon Href=""film.png"" IconKey=""avi"" />
    <AttachmentIcon Href=""film.png"" IconKey=""mpg"" />
    <AttachmentIcon Href=""film.png"" IconKey=""mpeg"" />
    <AttachmentIcon Href=""film.png"" IconKey=""divx"" />
    <AttachmentIcon Href=""film.png"" IconKey=""wma"" />
  </AttachmentIcons>
  
  
  <!-- Identifies the default namespace provider in the ""Add Namespace"" section of the /admin
        pages. Set this to the full type name of a namespace provider to default to something 
        other than the filesystem provider. The default value is 
        FlexWiki.FileSystemNamespaceProvider. -->
  <!-- <DefaultNamespaceProviderForNamespaceCreation /> -->

  <!-- When set to true, FlexWiki will not automatically update other topics when a topic
        is renamed. When set to false, FlexWiki will. Automatic update allows users to change
        many topics at once, and as such can lead to abuse of the wiki. -->
  <DisableRenameFixup>false</DisableRenameFixup>

  <!-- When set to true, a user can double-click a topic web page to go to the edit page. 
        When set to false, a user must click the Edit link to go to the edit page. Note that
        setting this value to true makes it impossible to select a word by double-clicking it. -->
  <EditOnDoubleClick>true</EditOnDoubleClick>

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

  <!-- A path (relative to the application root) used by parts of FlexWiki for old-style 
        (non-log4net) logging. Has no default, but safe to leave blank or missing. -->
  <!-- <LogPath />-->

  <!-- The path to the log4net configuration file. Defaults to log4net.config in the root
        folder of the web application. Relative paths are relative to the root folder of the 
        web application. -->
  <!-- <Log4NetConfigPath>path\to\log4net.config</Log4NetConfigPath>-->

  <!-- FlexWiki has a feature called newsletters, where the wiki will email members when 
        certain topics have changed. See http://www.flexwiki.com/default.aspx/FlexWiki/WikiNewsletter.html
        for more information. -->
  <NewsletterConfiguration>

    <!-- Because wiki topics can be secured, we need to tell FlexWiki who the newsletter
          engine will run as. Possible values include:
          
          ""anonymous"" = run as the anonymous user - same permissions as a user who does not authenticate.
          ""user:xxxx"" = run as the authenticated user ""xxxx"" 
          
          Note that when using the ""user:xxxx"" form, the user does not belong to any roles. -->
    <AuthenticateAs>anonymous</AuthenticateAs>

    <!-- Is the newsletter engine enabled? 'true' to enable it, 'false' to disable it. -->
    <Enabled>true</Enabled>

    <!-- Specifies the address that will appear in the 'from' field of newsletter emails. Change this to be
          appropriate for your site. -->
    <NewslettersFrom>newsletters@flexwiki.com</NewslettersFrom>

    <!-- Specifies the root URL for wiki links that appear in the newsletter. For example, if set to
          'http://www.flexwiki.com' a link to a topic called 'HomePage' in the namespace called
          'SampleNamespaceOne' will appear as 
          'http://www.flexwiki.com/default.aspx/SampleNamespaceOne/HomePage.html'. 
          
          Change this value to be appropriate for your site. -->
    <RootUrl>http://www.flexwiki.com/</RootUrl>

    <!-- If set to true, newsletters will be sent as attachments to the email. If set to false
          newsletters will appear in the body of the email.-->
    <SendAsAttachments>false</SendAsAttachments>
  </NewsletterConfiguration>

  <!-- If there are AlternateStylesheet elements, then 
        <LINK rel='alternate stylesheet' type='text/css' title='@Title' > 
        tags will be emitted into the <head> of every page, allowing the admin 
        to override CSS styles. The value in the @Href attribute becomes the 
        href attribute of the <LINK>. To provide a method of allowing the 
        OverrideStylesheet and the original stylesheet to coexist, you can add 
        the original stylesheet as an alternate. See the 'Chocolate' example 
        below. -->
  <AlternateStylesheets>
      <AlternateStylesheet Href=""classic.css"" Title=""Classic""/>
  </AlternateStylesheets>
  <!-- If set, will emit an additional <LINK rel='stylesheet' type='text/css' >
        tag into the <head> of every page, allowing the admin to override any
        CSS styles. The value below becomes the href attribute of the <LINK>. -->
  <OverrideStylesheet>/FlexWiki69jwd/wiki_jwd.css</OverrideStylesheet>

  <!-- If set, when a user tries to edit a page to include a URL from the URL blacklist 
        (see <BlackListedExternalLinks> above), an email about the event is sent to the 
        address listed here. -->
  <!-- <SendBanNotificationsToMailAddress>user@domain.com</SendBanNotificationsToMailAddress> -->

  <!-- When set to a non-blank value, does not actually create a namespace, but instead sends
        an email to the address listed here for further action. -->
  <!-- <SendNamespaceCreationRequestsTo>example@example.org</SendNamespaceCreationRequestsTo>-->

  <!-- An email address from which to send a notification whenever a new namespace is created.
        The email is sent to whatever address is listed for the Contact of the new namespace. 
        Leave blank to disable this feature. -->
  <!-- <SendNamespaceCreationMailFrom>example@example.org</SendNamespaceCreationMailFrom> -->

  <!-- An email address to cc on namespace creation, or empty for none. -->
  <SendNamespaceCreationMailToCC />

  <!-- If set to true, append a signature indicating who created the namespace. Note that this
        is not a digital signature - just a piece of text listing a username. -->
  <SignNamespaceCreationMail>false</SignNamespaceCreationMail>
</configuration>");

            XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            _applicationConfiguration = (FlexWikiWebApplicationConfiguration)serializer.Deserialize(reader);

        }

    }
}
