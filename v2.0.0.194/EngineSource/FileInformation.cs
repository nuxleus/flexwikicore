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

        public bool IsReadOnly
        {
            get { return _fileInfo.IsReadOnly; }
        }

    }
}
