using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Caching;

namespace FlexWiki
{
    public class TopicListDependency : NamespaceDependency
    {
        public TopicListDependency(string ns)
            : base(ns)
        {
        }

    }
}
