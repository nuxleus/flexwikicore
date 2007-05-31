using System;
using System.Collections.Generic;
using System.Threading;

namespace FlexWiki.Security
{
    internal class RecursionContext
    {
        private int _count;
        private static object _lock = new object();

        private RecursionContext()
        {
        }

        public int Count
        {
            get { return _count; }
        }

        public void Decrement()
        {
            if (_count <= 0)
            {
                throw new InvalidOperationException("Count is " + _count.ToString() + " - decrement is invalid.");
            }
            --_count;
        }
        public static RecursionContext GetRecursionContext(string name)
        {
            lock (_lock)
            {
                RecursionContext context = Thread.GetData(Thread.GetNamedDataSlot(name)) as RecursionContext;
                if (context == null)
                {
                    context = new RecursionContext();
                    Thread.SetData(Thread.GetNamedDataSlot(name), context);
                }

                return context;
            }
        }
        public void Increment()
        {
            ++_count;
        }

    }


}
