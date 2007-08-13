using System;

namespace FlexWiki.UnitTests
{
    public class MockFile
    {
        private readonly MockFileCollection _children = new MockFileCollection();
        private string _contents;
        private DateTime _created;
        private DateTime _lastModified; 
        private string _name;
        private MockTopicStorePermissions _permissions; 
        
        public MockFile(string name, DateTime created, string contents)
            : this(name, created, created, contents, MockTopicStorePermissions.ReadWrite)
        {
        }

        public MockFile(string name, DateTime created, string contents, MockTopicStorePermissions permissions)
            : this(name, created, created, contents, permissions)
        {
        }

        public MockFile(string name, DateTime created, DateTime lastModified, string contents, MockTopicStorePermissions permissions)
        {
            _name = name;
            _created = created; 
            _lastModified = lastModified; 
            _contents = contents;
            _permissions = permissions; 
        }

        public bool CanRead
        {
            get 
            {
                if (_permissions == MockTopicStorePermissions.NoAccess)
                {
                    return false;
                }
                else
                {
                    return true; 
                }
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
                    if (_permissions == MockTopicStorePermissions.ReadWrite)
                    {
                        _permissions = MockTopicStorePermissions.ReadOnly; 
                    }
                }
            }
        }

        public bool CanWrite
        {
            get 
            {
                if (_permissions == MockTopicStorePermissions.ReadWrite)
                {
                    return true;
                }
                else
                {
                    return false; 
                }
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

        public MockFileCollection Children
        {
            get { return _children; }
        }

        public string Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }

        public DateTime Created
        {
            get { return _created; }
            set { _created = value; }
        }

        public virtual bool IsDirectory
        {
            get { return false; }
        }

        public DateTime LastModified
        {
            get { return _lastModified; }
            set { _lastModified = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}
