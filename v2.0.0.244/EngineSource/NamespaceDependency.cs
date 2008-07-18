using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public abstract class NamespaceDependency : Dependency
    {
        private string _namespace;

        public NamespaceDependency(string ns)
        {
            _namespace = ns; 
        }

        public string Namespace
        {
            get { return _namespace; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return ((NamespaceDependency)obj).Namespace.Equals(_namespace); 

        }

        public override int GetHashCode()
        {
            return _namespace.GetHashCode(); 
        }
    }
}

