using System;
using System.Collections;
using System.Collections.Generic;

namespace FlexWiki
{
    internal class ComparerAdaptor<T> : IComparer<T> where T : new()
    {
        private IComparer _inner; 

        internal ComparerAdaptor(IComparer inner)
        {
            _inner = inner; 
        }

        int IComparer<T>.Compare(T x, T y)
        {
            return _inner.Compare(x, y); 
        }
    }
}
