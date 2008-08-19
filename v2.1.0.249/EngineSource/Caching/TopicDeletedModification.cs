using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public class TopicDeletedModification : TopicModification
    {
        public TopicDeletedModification(QualifiedTopicName topic) : base(topic)
        {
        }

        public override string ToString()
        {
            return string.Format("Topic {0} deleted.", Topic); 
        }

    }
}
