using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.Security
{
    internal class RecursionContextHelper : IDisposable
    {
        private string _key; 

        internal RecursionContextHelper(string key)
        {
            _key = key; 
            RecursionContext.GetRecursionContext(key).Increment(); 
        }

        void IDisposable.Dispose()
        {
            RecursionContext.GetRecursionContext(_key).Decrement(); 
        }

        internal static bool IsRecursing(string key)
        {
            return RecursionContext.GetRecursionContext(key).Count > 0; 
        }
    }
}
