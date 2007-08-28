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
using System.Text;

using FlexWiki.Collections; 

namespace FlexWiki.Caching
{
    internal class TopicChangesCacheItem
    {
        private readonly TopicChangeCollection _changes; 
        private readonly DateTime _since; 

        internal TopicChangesCacheItem(TopicChangeCollection changes, DateTime since)
        {
            _changes = changes;
            _since = since; 
        }

        internal TopicChangeCollection Changes
        {
            get { return _changes; }
        }

        internal DateTime Since
        {
            get { return _since; }
        }
    }
}
