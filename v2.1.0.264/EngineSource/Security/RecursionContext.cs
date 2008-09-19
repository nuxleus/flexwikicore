#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

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
