using System;
using System.Collections.Generic;
using System.Text;

using FlexWiki.Collections; 

namespace FlexWiki.Caching
{
    internal class TopicChangesCacheItem
    {
        private readonly TopicChangeCollection _changes; 
        private readonly DateTime _since; 

        internal TopicChangesCacheItem(TopicChangeCollection changes, DateTime since)
        {
            _changes = changes;
            _since = since; 
        }

        internal TopicChangeCollection Changes
        {
            get { return _changes; }
        }

        internal DateTime Since
        {
            get { return _since; }
        }
    }
}
