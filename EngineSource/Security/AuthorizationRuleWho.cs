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
using System.Security.Principal;
using FlexWiki.Caching;

namespace FlexWiki.Security
{
    public class AuthorizationRuleWho
    {
        private string _name;
        private AuthorizationRuleWhoType _whoType;
        private readonly Dictionary<string, SecurityIdentifier> _windowsRoleCache = new Dictionary<string, SecurityIdentifier>(); 

        public AuthorizationRuleWho(AuthorizationRuleWhoType whoType) : this(whoType, null)
        {
        }
        public AuthorizationRuleWho(AuthorizationRuleWhoType whoType, string who)
        {
            _name = who;
            _whoType = whoType;

            if (IsGeneric(_whoType))
            {
                if (who != null)
                {
                    throw new ArgumentException("who must be null when whoType is generic");
                }
            }
            else
            {
                if (who == null)
                {
                    throw new ArgumentException("who may not be null when whoType is not generic"); 
                }
            }
        }
        public string Who
        {
            get { return _name; }
        }
        public AuthorizationRuleWhoType WhoType
        {
            get { return _whoType; }
        }

        public bool IsMatch(IPrincipal principal)
        {
            return IsMatch(principal, null); 
        }
        public bool IsMatch(IPrincipal principal, IWikiCache cache)
        {
            if (_whoType == AuthorizationRuleWhoType.GenericAll)
            {
                return true;
            }
            else if (_whoType == AuthorizationRuleWhoType.GenericAnonymous)
            {
                return !principal.Identity.IsAuthenticated;
            }
            else if (_whoType == AuthorizationRuleWhoType.GenericAuthenticated)
            {
                return principal.Identity.IsAuthenticated;
            }
            else if (_whoType == AuthorizationRuleWhoType.Role)
            {
                string key = null; 
                if (cache != null)
                {
                    // We cache role authentication because it can get really expensive. 
                    // Specifically, we noticed that WindowsPrincipal.IsInRole causes
                    // massive CPU utilization
                    string principalName;

                    if (principal.Identity.IsAuthenticated)
                    {
                        principalName = "user:" + principal.Identity.Name;
                    }
                    else
                    {
                        principalName = "anonymous";
                    }

                    key = string.Format("authorization-identity://IsInRole/{0}/{1}",
                        _name, principalName);

                    object value = cache[key];

                    if (value != null && value is bool)
                    {
                        return (bool)value; 
                    }
                }
                bool isInRole = principal.IsInRole(_name);

                if (cache != null)
                {
                    cache[key] = isInRole; 
                }

                return isInRole; 
            }
            else if (_whoType == AuthorizationRuleWhoType.User)
            {
                return _name.Equals(principal.Identity.Name, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                throw new NotImplementedException("Principal type not supported: " + _whoType.ToString());
            }
        }
        public static AuthorizationRuleWho Parse(string input)
        {
            AuthorizationRuleWho who;
            if (!TryParse(input, out who))
            {
                throw new FormatException("Unrecognized input string " + input);
            }

            return who; 
        }
        public override string ToString()
        {
            if (WhoType == AuthorizationRuleWhoType.User)
            {
                return StringLiterals.UserPrefix + Who;
            }
            else if (WhoType == AuthorizationRuleWhoType.Role)
            {
                return StringLiterals.RolePrefix + Who;
            }
            else if (WhoType == AuthorizationRuleWhoType.GenericAuthenticated)
            {
                return StringLiterals.Authenticated;
            }
            else if (WhoType == AuthorizationRuleWhoType.GenericAnonymous)
            {
                return StringLiterals.Anonymous;
            }
            else if (WhoType == AuthorizationRuleWhoType.GenericAll)
            {
                return StringLiterals.All;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public static bool TryParse(string input, out AuthorizationRuleWho who)
        {
            who = null; 
            AuthorizationRuleWhoType whoType;
            string principal = null;
            if (input.StartsWith(StringLiterals.UserPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                whoType = AuthorizationRuleWhoType.User;
                principal = input.Substring(StringLiterals.UserPrefix.Length);
            }
            else if (input.StartsWith(StringLiterals.RolePrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                whoType = AuthorizationRuleWhoType.Role;
                principal = input.Substring(StringLiterals.RolePrefix.Length);
            }
            else if (input.Equals(StringLiterals.Authenticated, StringComparison.InvariantCultureIgnoreCase))
            {
                whoType = AuthorizationRuleWhoType.GenericAuthenticated;
            }
            else if (input.Equals(StringLiterals.Anonymous, StringComparison.InvariantCultureIgnoreCase))
            {
                whoType = AuthorizationRuleWhoType.GenericAnonymous;
            }
            else if (input.Equals(StringLiterals.All, StringComparison.InvariantCultureIgnoreCase))
            {
                whoType = AuthorizationRuleWhoType.GenericAll;
            }
            else
            {
                return false; 
            }

            who = new AuthorizationRuleWho(whoType, principal);
            return true;             
        }

        private static bool IsGeneric(AuthorizationRuleWhoType whoType)
        {
            if (whoType == AuthorizationRuleWhoType.GenericAll ||
                whoType == AuthorizationRuleWhoType.GenericAnonymous ||
                whoType == AuthorizationRuleWhoType.GenericAuthenticated)
            {
                return true; 
            }
            else if (whoType == AuthorizationRuleWhoType.Role ||
                whoType == AuthorizationRuleWhoType.User)
            {
                return false;
            }
            else
            {
                throw new ArgumentException("Unsupported whoType " + whoType.ToString()); 
            }

        }
    }
}
