using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public class NamespaceExistenceDependency : NamespaceDependency
    {
        public NamespaceExistenceDependency(string ns)
            : base(ns)
        {
        }
    }
}
