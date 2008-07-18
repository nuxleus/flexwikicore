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
using System.Xml;
using System.Xml.Serialization;

using FlexWiki;
using FlexWiki.Caching;
using FlexWiki.Web;
using FlexWiki.Web.UnitTests.Caching;

namespace FlexWiki.Web.UnitTests
{
    internal class MockWikiApplication : IWikiApplication
    {
        private MockCache _cache = new MockCache();
        private FederationConfiguration _configuration;
        private FlexWikiWebApplicationConfiguration _applicationConfiguration;
        private bool _isTransportSecure;
        private LinkMaker _linkMaker;
        private IMembership _membership = new FlexWikiWebMembership(); 
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
        public void NoteModification(Modification modification)
        {
            throw new Exception("The method or operation is not implemented.");
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
    <AttachmentIcon Href=""page_white_picture.png"" IconKey="".png"" />
    <AttachmentIcon Href=""page_white_picture.png"" IconKey="".gif"" />
    <AttachmentIcon Href=""page_white_picture.png"" IconKey="".jpg"" />
    <AttachmentIcon Href=""page_white_picture.png"" IconKey="".jpeg"" />
    <AttachmentIcon Href=""page_white_word.png"" IconKey="".doc"" />
    <AttachmentIcon Href=""page_excel.png"" IconKey="".xls"" />
    <AttachmentIcon Href=""page_white_acrobat.png"" IconKey="".pdf"" />
    <AttachmentIcon Href=""page_white_csharp.png"" IconKey="".cs"" />
    <AttachmentIcon Href=""html.png"" IconKey="".html"" />
    <AttachmentIcon Href=""html.png"" IconKey="".htm"" />
    <AttachmentIcon Href=""html.png"" IconKey="".aspx"" />
    <AttachmentIcon Href=""page_white_database.png"" IconKey="".mdb"" />
    <AttachmentIcon Href=""page_white_database.png"" IconKey="".sql"" />
    <AttachmentIcon Href=""page_white_compressed.png"" IconKey="".zip"" />
    <AttachmentIcon Href=""page_white_text.png"" IconKey="".txt"" />
    <AttachmentIcon Href=""film.png"" IconKey="".avi"" />
    <AttachmentIcon Href=""film.png"" IconKey="".mpg"" />
    <AttachmentIcon Href=""film.png"" IconKey="".mpeg"" />
    <AttachmentIcon Href=""film.png"" IconKey="".divx"" />
    <AttachmentIcon Href=""film.png"" IconKey="".wma"" />
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

  <!-- FederationConfiguration is empty. Test for this are in FlexWiki.UnitTests as it is part of
        FlexWikiEngine namespace rather than FlexWiki.Web namespace -->
  <FederationConfiguration>
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
