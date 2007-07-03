using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.UnitTests.Security
{
    internal class TestIdentity
    {
        private string _name;
        private readonly List<string> _roles = new List<string>(); 

        /// <summary>
        /// Creates an anonymous identity
        /// </summary>
        internal TestIdentity()
        {
        }

        /// <summary>
        /// Creates an authenticated identity.
        /// </summary>
        internal TestIdentity(string name, params string[] roles)
        {
            if (name == null)
            {
                throw new ArgumentException("name cannot be null", "name"); 
            }

            _name = name;
            _roles.AddRange(roles); 
        }

        internal bool IsAuthenticated
        {
            get { return _name != null; }
        }

        internal string Name
        {
            get { return _name; }
        }

        internal IList<string> Roles
        {
            get { return _roles; }
        }
    }
}
