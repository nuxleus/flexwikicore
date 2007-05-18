using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Web
{
    public enum CaptchaRequired
    {
        Never = 0, 
        Always, 
        IfAnonymous,
        WhenOverLinkThreshold,
    }
}
