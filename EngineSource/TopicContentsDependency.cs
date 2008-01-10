using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Caching;

namespace FlexWiki
{
    public class TopicContentsDependency : TopicDependency
    {
        public TopicContentsDependency(QualifiedTopicName topicName)
            : base(topicName)
        {
        }

    }
}
