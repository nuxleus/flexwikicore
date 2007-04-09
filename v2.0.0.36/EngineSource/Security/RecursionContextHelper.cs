using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Security
{
    internal class RecursionContextHelper : IDisposable
    {
        internal RecursionContextHelper()
        {
            RecursionContext.Current.Increment(); 
        }

        void IDisposable.Dispose()
        {
            RecursionContext.Current.Decrement(); 
        }
    }
}
