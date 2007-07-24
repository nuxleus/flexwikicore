using System;
using System.IO; 

namespace FlexWiki.UnitTests
{
    public class MockFileInformation : IFileInformation
    {
        private string _directory;
        private MockFile _mockFile;

        public MockFileInformation(MockFile mockFile, string directory)
        {
            _mockFile = mockFile;
            _directory = directory; 
        }

        public string Extension
        {
            get { return Path.GetExtension(_mockFile.Name); }
        }

        public string FullName
        {
            get { return Path.Combine(_directory, _mockFile.Name); }
        }

        public DateTime LastWriteTime
        {
            get { return _mockFile.LastModified; }
        }

        public string Name
        {
            get { return _mockFile.Name; }
        }

        public string NameWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension(Name); }
        }
    }
}
