using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Collections.Generic;

namespace FlexWiki.Collections
{
    public class DependencyCollection : Collection<Dependency>
    {
        public void AddRange(IEnumerable<Dependency> items)
        {
            foreach (Dependency item in items)
            {
                Add(item); 
            }
        }
    }
}
