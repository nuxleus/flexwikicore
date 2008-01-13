using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public class NamespaceContentsDeletedModification : NamespaceModification
    {
        public NamespaceContentsDeletedModification(string ns)
            : base(ns)
        {
        }

        public override string ToString()
        {
            return string.Format("Contents of namespace {0} deleted.", Namespace); 
        }

    }
}
