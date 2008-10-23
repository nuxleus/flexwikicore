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

namespace FlexWiki.UnitTests.Security
{
    internal class TransportSecurityRequirementTestConfiguration
    {
        private bool _isTransportSecure;
        private TransportSecurityRequirement _namespaceTransportSecurityRequirement;
        private TransportSecurityRequirement _wikiTransportSecurityRequirement;

        public TransportSecurityRequirementTestConfiguration(TransportSecurityRequirement wikiTransportSecurityRequirement,
            TransportSecurityRequirement namespaceTransportSecurityRequirement,
            bool isTransportSecure)
        {
            _namespaceTransportSecurityRequirement = namespaceTransportSecurityRequirement;
            _wikiTransportSecurityRequirement = wikiTransportSecurityRequirement;
            _isTransportSecure = isTransportSecure;
        }

        public bool IsTransportSecure
        {
            get { return _isTransportSecure; }
        }

        public TransportSecurityRequirement NamespaceTransportSecurityRequirement
        {
            get { return _namespaceTransportSecurityRequirement; }
        }

        public bool ShouldExceptionBeThrown
        {
            get
            {
                // An exception should be thrown if transport security is required at
                // the wiki level (or overridden to be so at the namespace level) and
                // if the transport is not secured. 

                // If the transport is secure, an exception will never be thrown.
                if (_isTransportSecure)
                {
                    return false; 
                }

                bool isRequired = false;

                if (_wikiTransportSecurityRequirement == TransportSecurityRequirement.RequiredOnContent)
                {
                    isRequired = true; 
                }

                if (_namespaceTransportSecurityRequirement == TransportSecurityRequirement.RequiredOnNone)
                {
                    isRequired = false; 
                }
                else if (_namespaceTransportSecurityRequirement == TransportSecurityRequirement.RequiredOnContent)
                {
                    isRequired = true; 
                }

                return isRequired; 
            }
        }


        public TransportSecurityRequirement WikiTransportSecurityRequirement
        {
            get { return _wikiTransportSecurityRequirement; }
        }
    }
}
