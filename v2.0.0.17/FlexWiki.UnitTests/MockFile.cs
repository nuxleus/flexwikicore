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
        private bool _readOnly; 

        public MockFile(string name, DateTime created, string contents)
            : this(name, created, created, contents, false)
        {
        }

        public MockFile(string name, DateTime created, string contents, bool readOnly)
            : this(name, created, created, contents, readOnly)
        {
        }

        public MockFile(string name, DateTime created, DateTime lastModified, string contents, bool readOnly)
        {
            _name = name;
            _created = created; 
            _lastModified = lastModified; 
            _contents = contents;
            _readOnly = readOnly; 
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

        public bool IsReadOnly
        {
            get { return _readOnly; }
            set { _readOnly = value; }
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
