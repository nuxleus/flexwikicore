using System;
using System.Xml;
using System.Xml.Serialization;

using FlexWiki.Web.Newsletters; 

namespace FlexWiki.Web
{
    [XmlRoot("configuration")]
    public class FlexWikiWebApplicationConfiguration
    {
        // Fields

        private string _defaultNamespaceProviderForNamespaceCreation;
        private bool _disableNewsletters;
        private bool _disableRenameFixup; 
        private bool _editOnDoubleClick = true;    // Required for legacy support - new properties should default to false
        private FederationConfiguration _federationConfiguration;
        private string _logPath;
        private bool _makeAbsoluteUrls;
        private string _newsletterRootUrl;
        private string _newslettersFrom;
        private string _overrideStylesheet;
        private string _sendBanNotificationsToMailAddress;
        private string _sendNamespaceCreationMailFrom;
        private string _sendNamespaceCreationMailToCC;
        private string _sendNamespaceCreationRequestsTo;
        private string _sendNamespaceRequestMailFrom;
        private bool _sendNewslettersAsAttachments;
        private bool _signNamespaceCreationMail;
        private string _smtpPassword;
        private string _smtpServer;
        private string _smtpUser;
        
        // Properties

        public string DefaultNamespaceProviderForNamespaceCreation
        {
            get { return _defaultNamespaceProviderForNamespaceCreation; }
            set { _defaultNamespaceProviderForNamespaceCreation = value; }
        }
        public bool DisableNewsletters
        {
            get { return _disableNewsletters; }
            set { _disableNewsletters = value; }
        }
        public bool DisableRenameFixup
        {
            get { return _disableRenameFixup; }
            set { _disableRenameFixup = value; }
        }
        public bool EditOnDoubleClick
        {
            get { return _editOnDoubleClick; }
            set { _editOnDoubleClick = value; }
        }
        public FederationConfiguration FederationConfiguration
        {
            get { return _federationConfiguration; }
            set { _federationConfiguration = value; }
        }
        public string LogPath
        {
            get { return _logPath; }
            set { _logPath = value; }
        }
        public bool MakeAbsoluteUrls
        {
            get { return _makeAbsoluteUrls; }
            set { _makeAbsoluteUrls = value; }
        }
        public string NewsletterRootUrl
        {
            get { return _newsletterRootUrl; }
            set { _newsletterRootUrl = value; }
        }
        public string NewslettersFrom
        {
            get { return _newslettersFrom; }
            set { _newslettersFrom = value; }
        }
        public string OverrideStylesheet
        {
            get { return _overrideStylesheet; }
            set { _overrideStylesheet = value; }
        }
        public string SendBanNotificationsToMailAddress
        {
            get { return _sendBanNotificationsToMailAddress; }
            set { _sendBanNotificationsToMailAddress = value; }
        }
        public string SendNamespaceCreationMailFrom
        {
            get { return _sendNamespaceCreationMailFrom; }
            set { _sendNamespaceCreationMailFrom = value; }
        }
        public string SendNamespaceCreationMailToCC
        {
            get { return _sendNamespaceCreationMailToCC; }
            set { _sendNamespaceCreationMailToCC = value; }
        }
        public string SendNamespaceCreationRequestsTo
        {
            get { return _sendNamespaceCreationRequestsTo; }
            set { _sendNamespaceCreationRequestsTo = value; }
        }
        public string SendNamespaceRequestMailFrom
        {
            get { return _sendNamespaceRequestMailFrom; }
            set { _sendNamespaceRequestMailFrom = value; }
        }
        public bool SendNewslettersAsAttachments
        {
            get { return _sendNewslettersAsAttachments; }
            set { _sendNewslettersAsAttachments = value; }
        }
        public bool SignNamespaceCreationMail
        {
            get { return _signNamespaceCreationMail; }
            set { _signNamespaceCreationMail = value; }
        }
        public string SmtpPassword
        {
            get { return _smtpPassword; }
            set { _smtpPassword = value; }
        }
        public string SmtpServer
        {
            get { return _smtpServer; }
            set { _smtpServer = value; }
        }
        public string SmtpUser
        {
            get { return _smtpUser; }
            set { _smtpUser = value; }
        }

    }
}
