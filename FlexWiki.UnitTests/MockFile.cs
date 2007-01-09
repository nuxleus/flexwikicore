using System;

namespace FlexWiki.UnitTests
{
    public class MockFile
    {
        private readonly MockFileCollection _children = new MockFileCollection();
        private string _contents;
        private DateTime _created;
        private bool _denyRead;
        private bool _denyWrite;
        private DateTime _lastModified; 
        private string _name;
        
        public MockFile(string name, DateTime created, string contents)
            : this(name, created, created, contents, false, false)
        {
        }

        public MockFile(string name, DateTime created, string contents, bool denyRead, bool denyWrite)
            : this(name, created, created, contents, denyRead, denyWrite)
        {
        }

        public MockFile(string name, DateTime created, DateTime lastModified, string contents, bool denyRead, bool denyWrite)
        {
            _name = name;
            _created = created; 
            _lastModified = lastModified; 
            _contents = contents;
            _denyRead = denyRead;
            _denyWrite = denyWrite; 
        }

        public bool CanRead
        {
            get { return !_denyRead; }
            set { _denyRead = !value; }
        }

        public bool CanWrite
        {
            get { return !_denyWrite; }
            set { _denyWrite = !value; }
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
