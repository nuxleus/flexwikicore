using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace FlexWiki.Security
{
    public class AuthorizationRuleWho
    {
        private string _name;
        private AuthorizationRuleWhoType _whoType;

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

        public override string ToString()
        {
            if (WhoType == AuthorizationRuleWhoType.User)
            {
                return StringLiterals.UserPrefix + Who;
            }
            else if (WhoType == AuthorizationRuleWhoType.Role)
            {
                return  StringLiterals.RolePrefix + Who;
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

        public static AuthorizationRuleWho Parse(string input)
        {
            AuthorizationRuleWho who;
            if (!TryParse(input, out who))
            {
                throw new FormatException("Unrecognized input string " + input);
            }

            return who; 
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

        public bool IsMatch(IPrincipal principal)
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
                return principal.IsInRole(_name); 
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
