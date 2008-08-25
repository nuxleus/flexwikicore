using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public abstract class TopicDependency : Dependency
    {
       private QualifiedTopicName _topicName;

        public TopicDependency(QualifiedTopicName topicName)
        {
            _topicName = topicName; 
        }

        public QualifiedTopicName TopicName
        {
            get { return _topicName; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false; 
            }

            if (obj.GetType() != this.GetType())
            {
                return false; 
            }

            return ((TopicDependency)obj).TopicName.Equals(_topicName); 
        }

        public override int GetHashCode()
        {
            return _topicName.GetHashCode();
        }

    }
}
