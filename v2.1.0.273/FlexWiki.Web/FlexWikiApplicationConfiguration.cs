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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;
using System.Xml.Serialization;

using FlexWiki.Web.Newsletters; 

namespace FlexWiki.Web
{
    [XmlRoot("configuration")]
    public class FlexWikiWebApplicationConfiguration
    {
        // Fields

        private AlternateStylesheetConfiguration[] _alternateStylesheets;
        private AttachmentIconConfiguration[] _attachmentIcons;
        private string _captchaKey;
        private int _captchaLinkThreshold;
        private string _containerUploadType = "None";
        private string _contentUploadPath;
		private bool _autoCreateUploadDirectories = false;
        private string _defaultNamespaceProviderForNamespaceCreation = typeof(FileSystemNamespaceProvider).FullName;
        private string _defaultNamespaceSearchScope = "Current";
        private bool _disableEditServiceWrite = false;
        private bool _disableFavicon = false;
        private bool _disableNewParser = false;
        private bool _disableRenameFixup;
        private bool _disableWikiEmoticons = false;
        private bool _disableXslTransform = false;
        private bool _disableThreadedMessaging = false;
        private bool _enableBordersAllPages = false;
        private bool _enableNewParser = false;
        private bool _removeListItemWhitespace = false;
        private string _threadedMessagingEditPermissions;
        private int _xsrfProtectionMessagePostTimeout = 10;
        private int _xsrfProtectionWikiEditTimeout = 15;
        private bool _editOnDoubleClick = true;    // Required for legacy support - new properties should default to false
        private FederationConfiguration _federationConfiguration;
        private string _logPath;
        private string _log4NetConfigPath;
        private string _localJavascript;
        private NewsletterConfiguration _newsletterConfiguration;
        private string _overrideBordersScope = "None";
        private string _overrideStylesheet;
        private CaptchaRequired _requireCaptchaOnEdit; 
        private string _sendBanNotificationsToMailAddress;
        private string _sendNamespaceCreationMailFrom;
        private string _sendNamespaceCreationMailToCC;
        private string _sendNamespaceCreationRequestsTo;
        private string _sendNamespaceRequestMailFrom;
        private bool _signNamespaceCreationMail;
        private string _outputCacheDuration;
        private bool _outputCacheDurationSpecified;

        private string _foregroundColorCAPTCHA = "LightGray";
        private string _backgroundColorCAPTCHA = "DarkGray";
        private string _hatchStyleCAPTCHA = "LargeConfetti";
        private string _obscuringLinesCAPTCHA = "5";
        private string _warpModeCAPTCHA = "Perspective";

        // Properties

        [XmlArrayItem("AlternateStylesheet")]
        public AlternateStylesheetConfiguration[] AlternateStylesheets
        {
            get
            {
                if (_alternateStylesheets == null)
                {
                    _alternateStylesheets = new AlternateStylesheetConfiguration[0];
                }
                return _alternateStylesheets;
            }
            set { _alternateStylesheets = value; }
        }

        [XmlArrayItem("AttachmentIcon")]
        public AttachmentIconConfiguration[] AttachmentIcons
        {
            get
            {
                if (_attachmentIcons == null)
                {
                    _attachmentIcons = new AttachmentIconConfiguration[0];
                }
                return _attachmentIcons;
            }
            set { _attachmentIcons = value; }
        }

        public string CaptchaKey
        {
            get { return _captchaKey; }
            set { _captchaKey = value; }
        }
        public int CaptchaLinkThreshold
        {
            get { return _captchaLinkThreshold; }
            set { _captchaLinkThreshold = value; }
        }
        public string ContainerUploadType
        {
            get { return _containerUploadType; }
            set 
            { 
            	_containerUploadType = value; 
            }
        }        
        public string ContentUploadPath
        {
            get { return _contentUploadPath; }
            set { _contentUploadPath = value; }
        }
        public bool AutoCreateUploadDirectories
        {
        	get { return _autoCreateUploadDirectories; }
        	set { _autoCreateUploadDirectories = value; }
        }
        public string DefaultNamespaceProviderForNamespaceCreation
        {
            get { return _defaultNamespaceProviderForNamespaceCreation; }
            set { _defaultNamespaceProviderForNamespaceCreation = value; }
        }
        public string DefaultNamespaceSearchScope
        {
            get { return _defaultNamespaceSearchScope; }
            set { _defaultNamespaceSearchScope = value; }
        }
        public bool DisableEditServiceWrite
        {
            get { return _disableEditServiceWrite; }
            set { _disableEditServiceWrite = value; }
        }
        public bool DisableFavicon
        {
            get { return _disableFavicon; }
            set { _disableFavicon = value; }
        }
        public bool DisableNewParser
        {
            get { return _disableNewParser; }
            set { _disableNewParser = value; }
        }
        public bool DisableRenameFixup
        {
            get { return _disableRenameFixup; }
            set { _disableRenameFixup = value; }
        }
        public bool DisableWikiEmoticons
        {
            get { return _disableWikiEmoticons; }
            set { _disableWikiEmoticons = value; }
        }
        public bool DisableXslTransform
        {
            get { return _disableXslTransform; }
            set { _disableXslTransform = value; }
        }
        public bool DisableThreadedMessaging
        {
            get { return _disableThreadedMessaging; }
            set { _disableThreadedMessaging = value; }
        }
        public bool EnableBordersAllPages
        {
            get { return _enableBordersAllPages; }
            set { _enableBordersAllPages = value; }
        }
        public bool EnableNewParser
        {
            get { return _enableNewParser; }
            set { _enableNewParser = value; }
        }
        public string LocalJavascript
        {
            get { return _localJavascript; }
            set { _localJavascript = value; }
        }
        public bool RemoveListItemWhitespace
        {
            get { return _removeListItemWhitespace; }
            set { _removeListItemWhitespace = value; }
        }
        public string ThreadedMessagingEditPermissions
        {
            get { return _threadedMessagingEditPermissions; }
            set { _threadedMessagingEditPermissions = value; }
        }
        public int XsrfProtectionMessagePostTimeout
        {
            get { return _xsrfProtectionMessagePostTimeout; }
            set { _xsrfProtectionMessagePostTimeout = value; }
        }
        public int XsrfProtectionWikiEditTimeout
        {
            get { return _xsrfProtectionWikiEditTimeout; }
            set { _xsrfProtectionWikiEditTimeout = value; }
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
        public NewsletterConfiguration NewsletterConfiguration
        {
            get { return _newsletterConfiguration; }
            set { _newsletterConfiguration = value; }
        }
        public string OverrideBordersScope
        {
            get { return _overrideBordersScope; }
            set { _overrideBordersScope = value; }
        }
        public string OverrideStylesheet
        {
            get { return _overrideStylesheet; }
            set { _overrideStylesheet = value; }
        }
        public CaptchaRequired RequireCaptchaOnEdit
        {
            get { return _requireCaptchaOnEdit; }
            set { _requireCaptchaOnEdit = value; }
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
        public string OutputCacheDuration
        {
            get { return _outputCacheDuration; }
            set { _outputCacheDuration = value; }
        }
        [XmlIgnore]
        public bool OutputCacheDurationSpecified
        {
            get { return _outputCacheDurationSpecified; }
            set { _outputCacheDurationSpecified = value; }
        }
        public string ForegroundColorCAPTCHA
        {
            get { return _foregroundColorCAPTCHA; }
            set { _foregroundColorCAPTCHA = value; }
        }
        public string BackgroundColorCAPTCHA
        {
            get { return _backgroundColorCAPTCHA; }
            set { _backgroundColorCAPTCHA = value; }
        }
        public string HatchStyleCAPTCHA
        {
            get { return _hatchStyleCAPTCHA; }
            set { _hatchStyleCAPTCHA = value; }
        }
        public string ObscuringLinesCAPTCHA
        {
            get { return _obscuringLinesCAPTCHA; }
            set { _obscuringLinesCAPTCHA = value; }
        }
        public string WarpModeCAPTCHA
        {
            get { return _warpModeCAPTCHA; }
            set { _warpModeCAPTCHA = value; }
        }

    }
}
