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

namespace FlexWiki.Caching
{
    public interface IWikiCache
    {
        object this[string key] { get; set; }
        // We use an array because we need the collection to be stable - a live
        // collection can't be iterated over and modified at the same time, which 
        // is a common situation when enumerating the cache keys. 
        void Clear();
        void ClearTopic(string topic);
        string[] Keys { get; }
    }
}
