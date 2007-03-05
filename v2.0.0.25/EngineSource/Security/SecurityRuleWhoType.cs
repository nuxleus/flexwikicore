using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Security
{
    public enum SecurityRuleWhoType
    {
        GenericAll,
        GenericAnonymous,
        GenericAuthenticated,
        Role,
        User,
    }
}
