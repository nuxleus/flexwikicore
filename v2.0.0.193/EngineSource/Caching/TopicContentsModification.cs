using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public class TopicContentsModification : TopicModification
    {
        public TopicContentsModification(QualifiedTopicName topic)
            : base(topic)
        {
        }

        public override string ToString()
        {
            return string.Format("The contents of topic {0} were modified.", Topic);
        }

    }
}
