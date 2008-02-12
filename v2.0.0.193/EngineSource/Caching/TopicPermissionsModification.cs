using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public class TopicPermissionsModification : TopicModification
    {
        public TopicPermissionsModification(QualifiedTopicName topic) : base(topic)
        {
        }

        public override string ToString()
        {
            return string.Format("The permissions of topic {0} were modified.", Topic);
        }

    }
}
