using System;
using System.IO;

using FlexWiki.Collections;

namespace FlexWiki
{
    public class FileSystem : IFileSystem
    {
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path); 
        }
        public void DeleteDirectory(string directory)
        {
            DirectoryInfo dir = new DirectoryInfo(directory);
            dir.Delete(true); 
        }
        public void DeleteFile(string path)
        {
            File.Delete(path); 
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path); 
        }

        public bool FileExists(string path)
        {
            return File.Exists(path); 
        }

        public FileInformationCollection GetFiles(string directory)
        {
            FileInfo[] files = new DirectoryInfo(directory).GetFiles();

            FileInformationCollection answer = new FileInformationCollection(); 

            foreach (FileInfo file in files)
            {
                answer.Add(new FileInformation(file)); 
            }

            return answer; 
        }

        public FileInformationCollection GetFiles(string directory, string pattern)
        {
            FileInfo[] files = new DirectoryInfo(directory).GetFiles(pattern);

            return FileInfoArrayToCollection(files);

        }

        public DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path); 
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(path); 
        }

        public bool HasReadPermission(string path)
        {
            try
            {
                // Hacky implementation, but there's no better way 
                // to do this that just to try and see what happens!!!
                FileStream stream = File.OpenRead(path);
                stream.Close();
            }
            catch (UnauthorizedAccessException)
            {
                return false; 
            }

            return true; 
        }
        public bool HasWritePermission(string path)
        {
            try
            {
                DateTime old = GetLastWriteTimeUtc(path);
                // Hacky implementation, but there's no better way 
                // to do this that just to try and see what happens!!!
                FileStream stream = File.OpenWrite(path);
                stream.Close();
                SetLastWriteTimeUtc(path, old);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            return true; 
        }

        public void MakeReadOnly(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            attributes |= FileAttributes.ReadOnly;
            File.SetAttributes(path, attributes); 
        }

        public void MakeWritable(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            attributes &= ~(FileAttributes.ReadOnly);
            File.SetAttributes(path, attributes);
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(path); 
        }

        public Stream OpenRead(string path, FileMode mode, FileAccess access, FileShare sharing)
        {
            return new FileStream(path, mode, access, sharing); 
        }

        public void SetLastWriteTimeUtc(string path, DateTime time)
        {
            File.SetLastAccessTimeUtc(path, time);
        }

        public void WriteFile(string path, string contents)
        {
            File.WriteAllText(path, contents); 
        }

        private static FileInformationCollection FileInfoArrayToCollection(FileInfo[] files)
        {
            FileInformationCollection answer = new FileInformationCollection();

            foreach (FileInfo file in files)
            {
                answer.Add(new FileInformation(file));
            }

            return answer;
        }



    }
}
