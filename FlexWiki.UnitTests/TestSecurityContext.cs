using System;
using System.Security.Principal;
using System.Threading; 

namespace FlexWiki.UnitTests
{
    internal class TestSecurityContext : IDisposable
    {
        private IPrincipal _oldPrincipal; 

        internal TestSecurityContext(string user, params string[] roles)
        {
            _oldPrincipal = Thread.CurrentPrincipal; 
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(user), roles); 
        }

        void IDisposable.Dispose()
        {
            Thread.CurrentPrincipal = _oldPrincipal; 
        }
    }
}
