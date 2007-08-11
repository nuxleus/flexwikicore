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
