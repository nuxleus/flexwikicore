using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public class MissingContextException : Exception
    {
        public override string Message
        {
            get
            {
                return "RequestContext.Current has not been set. All FlexWiki operations must have a RequestContext in effect during any operations against the wiki."; 
            }
        }
    }
}
