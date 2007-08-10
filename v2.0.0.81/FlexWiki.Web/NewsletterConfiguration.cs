using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Web
{
    public class NewsletterConfiguration
    {
        private string _authenticateAs;
        private bool _enabled; 
        private string _newslettersFrom;
        private string _rootUrl;
        private bool _sendAsAttachments;

        public string AuthenticateAs
        {
            get { return _authenticateAs; }
            set { _authenticateAs = value; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public string NewslettersFrom
        {
            get { return _newslettersFrom; }
            set { _newslettersFrom = value; }
        }

        public string RootUrl
        {
            get { return _rootUrl; }
            set { _rootUrl = value; }
        }

        public bool SendAsAttachments
        {
            get { return _sendAsAttachments; }
            set { _sendAsAttachments = value; }
        }
    }
}
