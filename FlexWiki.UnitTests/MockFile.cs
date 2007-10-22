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

namespace FlexWiki.UnitTests
{
    public class MockFile
    {
        private readonly MockFileCollection _children = new MockFileCollection();
        private string _contents;
        private DateTime _created;
        private DateTime _lastModified; 
        private string _name;
        private MockTopicStorePermissions _permissions;
        private bool _isReadOnly;
        
        public MockFile(string name, DateTime created, string contents)
            : this(name, created, created, contents, MockTopicStorePermissions.ReadWrite)
        {
        }

        public MockFile(string name, DateTime created, string contents, MockTopicStorePermissions permissions)
            : this(name, created, created, contents, permissions)
        {
        }

        public MockFile(string name, DateTime created, DateTime lastModified, string contents, MockTopicStorePermissions permissions)
            : this(name, created, lastModified, contents, permissions, false)
        {
        }

        public MockFile(string name, DateTime created, DateTime lastModified, string contents, MockTopicStorePermissions permissions, bool isReadOnly)
        {
            _name = name;
            _created = created;
            _lastModified = lastModified;
            _contents = contents;
            _permissions = permissions;
            _isReadOnly = isReadOnly;
        }

        public bool CanRead
        {
            get 
            {
                if (_permissions == MockTopicStorePermissions.NoAccess)
                {
                    return false;
                }
                else
                {
                    return true; 
                }
            }
            set 
            {
                if (value == true)
                {
                    if (_permissions == MockTopicStorePermissions.NoAccess)
                    {
                        _permissions = MockTopicStorePermissions.ReadOnly;
                    }
                }
                else
                {
                    if (_permissions == MockTopicStorePermissions.ReadWrite)
                    {
                        _permissions = MockTopicStorePermissions.ReadOnly; 
                    }
                }
            }
        }

        public bool CanWrite
        {
            get 
            {
                if (_permissions == MockTopicStorePermissions.ReadWrite)
                {
                    return true;
                }
                else
                {
                    return false; 
                }
            }
            set 
            {
                if (value == true)
                {
                    _permissions = MockTopicStorePermissions.ReadWrite;
                    _isReadOnly = false;
                }
                else
                {
                    if (_permissions == MockTopicStorePermissions.ReadWrite)
                    {
                        _permissions = MockTopicStorePermissions.ReadOnly;
                        _isReadOnly = true;
                    }
                }
            }
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
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }
    }
}
