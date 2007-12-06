using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public class TopicListModification : NamespaceModification
    {
        public TopicListModification(string ns)
            : base(ns)
        {
        }

        public override string ToString()
        {
            return string.Format("The list of topics in namespace {0} changed", Namespace); 
        }
    }
}
