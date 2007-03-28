using System;
using System.Collections.Generic; 
using System.IO;

using FlexWiki.Collections; 

namespace FlexWiki.UnitTests
{
    public class MockFileSystem : IFileSystem
    {
        private readonly MockFileCollection _children = new MockFileCollection();

        public MockFileSystem(params MockFile[] children)
        {
            foreach (MockFile child in children)
            {
                _children.Add(child); 
            }
        }

        public MockFile this[string path]
        {
            get
            {
                MockFileCollection pathComponents = GetFilePath(path);

                if (pathComponents == null)
                {
                    return null; 
                }
                return pathComponents.Last; 
            }
        }

        public MockFileCollection Children
        {
            get { return _children; }
        }

        public void CreateDirectory(string path)
        {
            List<string> components = GetPathComponents(path);

            MockFileCollection children = _children;
            foreach (string component in components)
            {
                MockDirectory directory = children[component] as MockDirectory;
                if (directory == null)
                {
                    directory = new MockDirectory(component); 
                    children.Add(directory); 
                }
                children = directory.Children;
            }
        }

        public void DeleteDirectory(string path)
        {
            MockFileCollection pathElements = GetFilePath(path);
            MockDirectory directory = pathElements.Last as MockDirectory;
            pathElements.Penultimate.Children.Remove(directory); 
        }

        public void DeleteFile(string path)
        {
            MockFileCollection pathElements = GetFilePath(path); 
            MockFile file = pathElements.Last;
            MockDirectory directory = pathElements[pathElements.Count - 2] as MockDirectory;

            directory.Children.Remove(file); 
        }

        
        public bool DirectoryExists(string path)
        {
            MockFileCollection pathElements = GetFilePath(path);

            if (pathElements != null)
            {
                return pathElements.Last.IsDirectory; 
            }

            return false; 
        }

        public bool FileExists(string path)
        {
            MockFileCollection pathElements = GetFilePath(path);

            return pathElements != null; 
        }


        public FileInformationCollection GetFiles(string directory)
        {
            return GetFiles(directory, ""); 
        }

        public FileInformationCollection GetFiles(string directory, string pattern)
        {
            MockFile mockDirectory = GetFilePath(directory).Last;

            if (!mockDirectory.IsDirectory)
            {
                return null; 
            }

            string firstPart = pattern;
            string lastPart = pattern; 

            int starIndex = pattern.IndexOf("*");

            if (starIndex != -1)
            {
                firstPart = pattern.Substring(0, starIndex);
                lastPart = pattern.Substring(starIndex + 1, pattern.Length - starIndex - 1); 
            }

            FileInformationCollection files = new FileInformationCollection(); 
            foreach (MockFile child in mockDirectory.Children)
            {
                if (child.Name.StartsWith(firstPart) && child.Name.EndsWith(lastPart))
                {
                    MockFileInformation info = new MockFileInformation(child, directory);
                    files.Add(info); 
                }
            }

            return files; 
        }

        public DateTime GetLastWriteTime(string path)
        {
            return this[path].LastModified; 
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            return this[path].LastModified; 
        }

        public bool HasReadPermission(string path)
        {
            return this[path].CanRead; 
        }

        public bool HasWritePermission(string path)
        {
            return this[path].CanWrite; 
        }

        public void MakeReadOnly(string path)
        {
            if (this[path] == null)
            {
                throw new FileNotFoundException("Couldn't find file " + path); 
            }
            this[path].CanRead = true;
            this[path].CanWrite = false; 
        }

        public void MakeWritable(string path)
        {
            if (this[path] == null)
            {
                throw new FileNotFoundException("Couldn't find file " + path);
            }

            this[path].CanWrite = true;
        }

        public void SetLastWriteTimeUtc(string path, DateTime time)
        {
            this[path].LastModified = time; 
        }

        public System.IO.Stream OpenRead(string path)
        {
            return OpenRead(path, FileMode.Open, FileAccess.Read, FileShare.None); 
        }

        public System.IO.Stream OpenRead(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare sharing)
        {
            MockFile file = this[path];

            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(file.Contents)); 
        }

        public void WriteFile(string path, string contents)
        {
            if (!FileExists(path))
            {
                MockFile file = new MockFile(Path.GetFileName(path), DateTime.Now, contents);

                this[Path.GetDirectoryName(path)].Children.Add(file);
            }
            else
            {
                this[path].Contents = contents;
            }
        }

        private MockFileCollection GetFilePath(string path)
        {
            MockFileCollection pathElements = new MockFileCollection();
            List<string> items = GetPathComponents(path);

            MockFileCollection files = _children;
            MockFile match = null;
            foreach (string item in items)
            {
                if (files.Contains(item))
                {
                    match = files[item];
                    pathElements.Add(match); 
                    if (files[item].IsDirectory)
                    {
                        if (match.IsDirectory)
                        {
                            files = match.Children;
                        }
                    }
                    else
                    {
                        files = null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return pathElements;

        }

        private static List<string> GetPathComponents(string path)
        {
            List<string> items = new List<string>();

            string parsedPath = path;
            string pathElement = Path.GetFileName(path);
            while (!string.IsNullOrEmpty(pathElement))
            {
                items.Insert(0, pathElement);
                parsedPath = Path.GetDirectoryName(parsedPath);
                pathElement = Path.GetFileName(parsedPath);
            }

            items.Insert(0, Path.GetPathRoot(path));
            return items;
        }

    }
}
