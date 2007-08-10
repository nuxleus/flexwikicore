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
        private DateTime _modified;
        private MockTopicStorePermissions _permissions = MockTopicStorePermissions.ReadWrite; 
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
            get 
            {
                if (_permissions == MockTopicStorePermissions.NoAccess)
                {
                    return false;
                }
                return true; 
            }
            set 
            {
                if (value == true)
                {
                    if (_permissions == MockTopicStorePermissions.NoAccess)
                    {
                        _permissions = MockTopicStorePermissions.ReadOnly;
                    }
                }
                else
                {
                    _permissions = MockTopicStorePermissions.NoAccess;
                }
            }
        }

        internal bool CanWrite
        {
            get 
            {
                if (_permissions == MockTopicStorePermissions.ReadWrite)
                {
                    return true; 
                }
                return false; 
            }
            set 
            {
                if (value == true)
                {
                    _permissions = MockTopicStorePermissions.ReadWrite;
                }
                else
                {
                    if (_permissions == MockTopicStorePermissions.ReadWrite)
                    {
                        _permissions = MockTopicStorePermissions.ReadOnly; 
                    }
                }
            }
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
