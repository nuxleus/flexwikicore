using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Caching
{
    public abstract class TopicModification : Modification
    {
        private QualifiedTopicName _topic;

        public TopicModification(QualifiedTopicName topic)
        {
            _topic = topic;
        }

        public QualifiedTopicName Topic
        {
            get { return _topic; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return _topic.Equals(((TopicModification)obj)._topic);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _topic.GetHashCode();
        }


    }
}
