using System;
using System.Collections;
using System.Text;

namespace FlexWiki.SqlProvider
{
    internal class TimeSort : IComparer
    {
        public int Compare(object left, object right)
        {
            return ((TopicData)right).LastModificationTime.CompareTo(((TopicData)left).LastModificationTime);
        }
    }
}
