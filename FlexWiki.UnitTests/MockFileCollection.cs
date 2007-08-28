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
using System.Collections.ObjectModel; 

namespace FlexWiki.UnitTests
{
    public class MockFileCollection : Collection<MockFile>
    {
        public bool Contains(string item)
        {
            foreach (MockFile file in this)
            {
                if (string.Compare(file.Name, item, true) == 0)
                {
                    return true;
                }
            }

            return false; 
        }

        public MockFile this[string name]
        {
            get
            {
                foreach (MockFile file in this)
                {
                    if (string.Compare(file.Name, name, true) == 0)
                    {
                        return file;
                    }
                }

                return null;
            }
        }

        public MockFile Last
        {
            get
            {
                return this[this.Count - 1]; 
            }
        }

        public MockFile Penultimate
        {
            get { return this[this.Count - 2]; }
        }
    }
}
