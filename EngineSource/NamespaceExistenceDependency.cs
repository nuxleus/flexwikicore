using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Caching;

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
