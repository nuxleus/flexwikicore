using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Caching;

namespace FlexWiki
{
    public class TopicExistenceDependency : TopicDependency
    {
        public TopicExistenceDependency(QualifiedTopicName topicName)
            : base(topicName)
        {
        }

    }
}
