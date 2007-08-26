using System;
using System.Security.Principal; 

namespace FlexWiki.UnitTests.Security
{
    internal class TestAnonymousIdentity : IIdentity
    {
        public string AuthenticationType
        {
            get { return "Test"; }
        }

        public bool IsAuthenticated
        {
            get { return false; }
        }

        public string Name
        {
            get { return string.Empty; }
        }

    }
}
