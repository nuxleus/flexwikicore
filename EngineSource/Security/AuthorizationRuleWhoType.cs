using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Security
{
    public enum AuthorizationRuleWhoType
    {
        GenericAll,
        GenericAnonymous,
        GenericAuthenticated,
        Role,
        User,
    }
}
