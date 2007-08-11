using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public class TopicPropertyValue
    {
        public TopicPropertyValue()
        {
        }

        public TopicPropertyValue(string value)
        {
            _value = value; 
        }

        private string _value;

        public string RawValue
        {
            get
            {
                return _value; 
            }
        }

        //CA Can't decide if this should be a method or a propertyName
        public IList<string> AsList()
        {
            return TopicParser.SplitTopicPropertyValue(_value); 
        }
    }
}
