using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.UnitTests
{
    class MockTopicRevision
    {
        private string _author;
        private string _contents;
        private DateTime _created;
        private bool _denyRead;
        private bool _denyWrite; 
        private bool _isReadOnly; 
        private DateTime _modified; 
        private string _version;

        internal MockTopicRevision(string contents, string author, DateTime created)
        {
            _contents = contents;
            _author = author;
            _created = created;
            _modified = created; 
            _version = TopicRevision.NewVersionStringForUser(author, created);
        }

        internal string Author
        {
            get { return _author; }
        }

        internal bool CanRead
        {
            get { return !_denyRead; }
            set { _denyRead = !value; }
        }

        internal bool CanWrite
        {
            get { return !_denyWrite; }
            set { _denyWrite = !value; }
        }

        internal string Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }

        internal DateTime Created
        {
            get { return _created; }
        }

        internal bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }

        internal DateTime Modified
        {
            get { return _modified; }
            set { _modified = value; } 
        }

        internal string Version
        {
            get
            {
                return _version;

            }
        }
    }
}
