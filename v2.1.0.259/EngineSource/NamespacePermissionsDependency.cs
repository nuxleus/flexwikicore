using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Caching;

namespace FlexWiki
{
    public class NamespacePermissionsDependency : NamespaceDependency
    {
        public NamespacePermissionsDependency(string ns)
            : base(ns)
        {
        }

    }
}
