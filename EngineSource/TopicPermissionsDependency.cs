using System;
using System.Collections.Generic;
using System.Text;
using FlexWiki.Caching;

namespace FlexWiki
{
    public class TopicPermissionsDependency : TopicDependency
    {
        public TopicPermissionsDependency(QualifiedTopicName topicName)
            : base(topicName)
        {
        }

    }
}
