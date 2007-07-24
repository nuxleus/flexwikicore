using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Security
{
    public enum TransportSecurityRequiredFor
    {
        None,       // Default: must be listed first.
        Content,
        // More may be added later
    }
}
