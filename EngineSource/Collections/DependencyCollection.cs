using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Collections.Generic;
using FlexWiki.Caching;

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

        public bool IsInvalidatedBy(Modification modification)
        {
            foreach (Dependency dependency in this)
            {
                if (ModificationDependencyRelator.Invalidates(modification, dependency))
                {
                    return true;
                }
            }

            return false; 
        }
    }
}
