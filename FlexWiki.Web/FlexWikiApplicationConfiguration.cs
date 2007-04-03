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
        private bool _disableRenameFixup; 
        private bool _editOnDoubleClick = true;    // Required for legacy support - new properties should default to false
        private FederationConfiguration _federationConfiguration;
        private string _logPath;
        private string _log4NetConfigPath; 
        private bool _makeAbsoluteUrls;
        private NewsletterConfiguration _newsletterConfiguration; 
        private string _overrideStylesheet;
        private string _sendBanNotificationsToMailAddress;
        private string _sendNamespaceCreationMailFrom;
        private string _sendNamespaceCreationMailToCC;
        private string _sendNamespaceCreationRequestsTo;
        private string _sendNamespaceRequestMailFrom;
        private bool _signNamespaceCreationMail;
        
        // Properties

        public string DefaultNamespaceProviderForNamespaceCreation
        {
            get { return _defaultNamespaceProviderForNamespaceCreation; }
            set { _defaultNamespaceProviderForNamespaceCreation = value; }
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
        public string Log4NetConfigPath
        {
            get { return _log4NetConfigPath; }
            set { _log4NetConfigPath = value; }
        }
        public bool MakeAbsoluteUrls
        {
            get { return _makeAbsoluteUrls; }
            set { _makeAbsoluteUrls = value; }
        }
        public NewsletterConfiguration NewsletterConfiguration
        {
            get { return _newsletterConfiguration; }
            set { _newsletterConfiguration = value; }
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
        public bool SignNamespaceCreationMail
        {
            get { return _signNamespaceCreationMail; }
            set { _signNamespaceCreationMail = value; }
        }

    }
}
