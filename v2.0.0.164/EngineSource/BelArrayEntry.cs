using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    /// <summary>
    /// This class temporarily holds entries in a BELArray so we can do a sort on them
    /// that doesn't require O(n log(n)) evaluations of the SortBy block. 
    /// </summary>
    internal class BelArrayEntry
    {
        internal class Comparer : IComparer<BelArrayEntry>
        {
            public int Compare(BelArrayEntry x, BelArrayEntry y)
            {
                return x.Value.CompareTo(y.Value); 
            }
        }

        private IBELObject _entry;
        private IComparable _value;

        public BelArrayEntry(IBELObject entry, IComparable value)
        {
            _entry = entry;
            _value = value; 
        }

        public IBELObject Entry
        {
            get { return _entry; }
        }

        public IComparable Value
        {
            get { return _value; }
        }
    }
}
