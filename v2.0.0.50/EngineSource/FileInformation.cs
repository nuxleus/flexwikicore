using System;
using System.IO; 

namespace FlexWiki
{
    public class FileInformation : IFileInformation
    {
        private readonly FileInfo _fileInfo;

        public FileInformation(FileInfo fileInfo)
        {
            _fileInfo = fileInfo; 
        }

        public string Extension
        {
            get { return _fileInfo.Extension; }
        }

        public string FullName
        {
            get { return _fileInfo.FullName; }
        }

        public DateTime LastWriteTime
        {
            get { return _fileInfo.LastWriteTime; }
        }

        public string Name
        {
            get { return _fileInfo.Name; }
        }

        public string NameWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension(_fileInfo.Name); }
        }

    }
}
