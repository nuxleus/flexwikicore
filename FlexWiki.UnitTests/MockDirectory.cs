using System;

namespace FlexWiki.UnitTests
{
    public class MockDirectory : MockFile
    {
        public MockDirectory(string name, params MockFile[] children) : base(name, DateTime.MinValue, DateTime.MinValue, "", 
            MockTopicStorePermissions.ReadWrite)
        {
            foreach (MockFile child in children)
            {
                Children.Add(child);
            }
        }

        public override bool IsDirectory
        {
            get
            {
                return true; 
            }
        }
    }
}
