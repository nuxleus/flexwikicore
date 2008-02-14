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
