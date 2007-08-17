using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public class VersionInfo
    {
        private string _author;
        private DateTime _timestamp; 

        public VersionInfo(DateTime timestamp, string author)
        {
            _timestamp = timestamp;
            _author = author; 
        }

        public string Author
        {
            get { return _author; }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
        }
    }
}
