using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace FlexWiki.Security
{
    public class SecurityRuleWho
    {
        private string _name;
        private SecurityRuleWhoType _whoType;

        public SecurityRuleWho(SecurityRuleWhoType whoType) : this(whoType, null)
        {
        }

        public SecurityRuleWho(SecurityRuleWhoType whoType, string who)
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

        public SecurityRuleWhoType WhoType
        {
            get { return _whoType; }
        }

        public override string ToString()
        {
            if (WhoType == SecurityRuleWhoType.User)
            {
                return StringLiterals.UserPrefix + Who;
            }
            else if (WhoType == SecurityRuleWhoType.Role)
            {
                return  StringLiterals.RolePrefix + Who;
            }
            else if (WhoType == SecurityRuleWhoType.GenericAuthenticated)
            {
                return StringLiterals.Authenticated;
            }
            else if (WhoType == SecurityRuleWhoType.GenericAnonymous)
            {
                return StringLiterals.Anonymous;
            }
            else if (WhoType == SecurityRuleWhoType.GenericAll)
            {
                return StringLiterals.All;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static SecurityRuleWho Parse(string input)
        {
            SecurityRuleWho who;
            if (!TryParse(input, out who))
            {
                throw new FormatException("Unrecognized input string " + input);
            }

            return who; 
        }


        public static bool TryParse(string input, out SecurityRuleWho who)
        {
            who = null; 
            SecurityRuleWhoType whoType;
            string principal = null;
            if (input.StartsWith(StringLiterals.UserPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                whoType = SecurityRuleWhoType.User;
                principal = input.Substring(StringLiterals.UserPrefix.Length);
            }
            else if (input.StartsWith(StringLiterals.RolePrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                whoType = SecurityRuleWhoType.Role;
                principal = input.Substring(StringLiterals.RolePrefix.Length);
            }
            else if (input.Equals(StringLiterals.Authenticated, StringComparison.InvariantCultureIgnoreCase))
            {
                whoType = SecurityRuleWhoType.GenericAuthenticated;
            }
            else if (input.Equals(StringLiterals.Anonymous, StringComparison.InvariantCultureIgnoreCase))
            {
                whoType = SecurityRuleWhoType.GenericAnonymous;
            }
            else if (input.Equals(StringLiterals.All, StringComparison.InvariantCultureIgnoreCase))
            {
                whoType = SecurityRuleWhoType.GenericAll;
            }
            else
            {
                return false; 
            }

            who = new SecurityRuleWho(whoType, principal);
            return true;             
        }

        public bool IsMatch(IPrincipal principal)
        {
            if (_whoType == SecurityRuleWhoType.GenericAll)
            {
                return true; 
            }
            else if (_whoType == SecurityRuleWhoType.GenericAnonymous)
            {
                return !principal.Identity.IsAuthenticated; 
            }
            else if (_whoType == SecurityRuleWhoType.GenericAuthenticated)
            {
                return principal.Identity.IsAuthenticated; 
            }
            else if (_whoType == SecurityRuleWhoType.Role)
            {
                return principal.IsInRole(_name); 
            }
            else if (_whoType == SecurityRuleWhoType.User)
            {
                return _name.Equals(principal.Identity.Name, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                throw new NotImplementedException("Principal type not supported: " + _whoType.ToString()); 
            }
        }

        private static bool IsGeneric(SecurityRuleWhoType whoType)
        {
            if (whoType == SecurityRuleWhoType.GenericAll ||
                whoType == SecurityRuleWhoType.GenericAnonymous ||
                whoType == SecurityRuleWhoType.GenericAuthenticated)
            {
                return true; 
            }
            else if (whoType == SecurityRuleWhoType.Role ||
                whoType == SecurityRuleWhoType.User)
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
