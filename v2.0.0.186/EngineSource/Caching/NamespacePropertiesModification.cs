using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public class NamespacePropertiesModification : NamespaceModification
    {
        public NamespacePropertiesModification(string ns)
            : base(ns)
        {
        }

        public override string ToString()
        {
            return string.Format("The properties of namespace {0} were modified.", Namespace); 
        }
    }
}
