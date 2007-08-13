using System;
using System.IO;

using FlexWiki.Collections;

namespace FlexWiki
{
    public interface IFileSystem
    {
        void CreateDirectory(string path);
        void DeleteDirectory(string directory); 
        void DeleteFile(string path); 
        bool DirectoryExists(string path);
        bool FileExists(string path); 
        FileInformationCollection GetFiles(string directory); 
        FileInformationCollection GetFiles(string directory, string pattern);
        DateTime GetLastWriteTime(string path); 
        DateTime GetLastWriteTimeUtc(string path);
        bool HasReadPermission(string path); 
        bool HasWritePermission(string path);
        void MakeReadOnly(string path);
        void MakeWritable(string path);
        void SetLastWriteTimeUtc(string path, DateTime time);
        Stream OpenRead(string path); 
        Stream OpenRead(string path, FileMode mode, FileAccess access, FileShare sharing); 
        void WriteFile(string path, string contents);
    }
}
