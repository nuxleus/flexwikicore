using System;
using System.Threading; 

namespace FlexWiki.Security
{
    internal class RecursionContext
    {
        private int _count;
        private static LocalDataStoreSlot s_slot = Thread.AllocateDataSlot();

        private RecursionContext()
        {
        }

        public int Count
        {
            get { return _count; }
        }

        public static RecursionContext Current
        {
            get
            {
                RecursionContext context = Thread.GetData(s_slot) as RecursionContext;
                if (context == null)
                {
                    context = new RecursionContext();
                    Thread.SetData(s_slot, context);
                }

                return context;
            }
        }

        public void Decrement()
        {
            if (_count <= 0)
            {
                throw new InvalidOperationException("Count is " + _count.ToString() + " - decrement is invalid.");
            }
            --_count;
        }

        public void Increment()
        {
            ++_count;
        }

    }


}
