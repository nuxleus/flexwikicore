using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public class NamespacePermissionsModification : NamespaceModification
    {
        public NamespacePermissionsModification(string ns)
            : base(ns)
        {
        }

        public override string ToString()
        {
            return string.Format("The permissions of namespace {0} were modified.",
                Namespace); 
        }
    }
}
