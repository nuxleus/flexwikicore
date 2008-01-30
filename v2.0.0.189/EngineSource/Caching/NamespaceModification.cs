using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public abstract class NamespaceModification : Modification
    {
        private readonly string _namespace;

        public NamespaceModification(string ns)
        {
            _namespace = ns; 
        }

        public string Namespace
        {
            get { return _namespace; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return _namespace.Equals(((NamespaceModification)obj)._namespace); 
            }

            return false; 
        }

        public override int GetHashCode()
        {
            return _namespace.GetHashCode(); 
        }


    }
}
