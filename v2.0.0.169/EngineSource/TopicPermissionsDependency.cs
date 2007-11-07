using System;
using System.Collections.Generic;
using System.Text;

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
