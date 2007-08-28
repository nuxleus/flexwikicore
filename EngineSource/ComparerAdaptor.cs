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
