using System;
using System.Security.Principal;
using System.Threading; 

namespace FlexWiki.UnitTests.Security
{
    internal class TestSecurityContext : IDisposable
    {
        private IPrincipal _oldPrincipal; 

        internal TestSecurityContext(string user, params string[] roles)
        {
            _oldPrincipal = Thread.CurrentPrincipal;
            IIdentity newIdentity;
            if (user == null)
            {
                newIdentity = new TestAnonymousIdentity(); 
            }
            else 
            {
                newIdentity = new GenericIdentity(user);
            }

            Thread.CurrentPrincipal = new GenericPrincipal(newIdentity, roles); 
        }

        void IDisposable.Dispose()
        {
            Thread.CurrentPrincipal = _oldPrincipal; 
        }
    }
}
