using System;
using System.Collections.Generic;
using System.Text;

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
