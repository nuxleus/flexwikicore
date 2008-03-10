#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

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
        bool FileIsReadOnly(string path);
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
