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
