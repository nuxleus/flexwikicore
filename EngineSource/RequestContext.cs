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

using FlexWiki.Collections; 

namespace FlexWiki
{
    /// <summary>
    /// Provides a place to track the details of one particular request
    /// </summary>
    public class RequestContext : IDisposable
    {
        [ThreadStatic]
        private static Stack<RequestContext> s_contextChain;

        private DependencyCollection _dependencies = new DependencyCollection(); 
        private Dictionary<string, object> _items = new Dictionary<string, object>();

        private RequestContext()
        {
        }

        public static RequestContext Current
        {
            get
            {
                if (ContextChain.Count == 0)
                {
                    return null; 
                }
                return ContextChain.Peek();
            }
        }
        public DependencyCollection Dependencies
        {
            get { return _dependencies; }
        }
        public string[] Keys
        {
            get
            {
                // We copy it to an array because consumers want to be able to 
                // iterate over the results. But in order to do that, we need
                // to hold the collection stable. 
                string[] keys = new string[_items.Keys.Count];
                _items.Keys.CopyTo(keys, 0);
                return keys; 
            }
        }

        public object this[string key]
        {
            get
            {
                if (!_items.ContainsKey(key))
                {
                    return null;
                }
                return _items[key]; 
            }
            set
            {
                _items[key] = value; 
            }
        }

        private static Stack<RequestContext> ContextChain
        {
            get
            {
                if (s_contextChain == null)
                {
                    s_contextChain = new Stack<RequestContext>(); 
                }

                return s_contextChain; 
            }
        }

        /// <summary>
        /// Creates a <see cref="RequestContext"/>. Use this overload whenever possible. 
        /// </summary>
        /// <returns></returns>
        public static RequestContext Create()
        {
            return RequestContext.Create(RequestContextOptions.None);
        }

        /// <summary>
        /// Creates a <see cref="RequestContext"/> with the specified options. 
        /// Do not use this overload except for unit tests! Misuse of nested
        /// contexts can result in cache corruption!
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <remarks>Do not use this overload except for unit tests! Misuse of nested
        /// contexts can result in cache corruption!</remarks>
        public static RequestContext Create(RequestContextOptions options)
        {
            if (Current != null)
            {
                if (options != RequestContextOptions.UnitTestConfiguration)
                {
                    throw new NestedContextUnexpectedException(); 
                }
            }

            ContextChain.Push(new RequestContext()); 
            return Current; 
        }

        void IDisposable.Dispose()
        {
            RequestContext previous = ContextChain.Pop();

            // Propagate dependencies picked up in the inner context - the outer
            // context is also dependent on them. 
            if (Current != null)
            {
                Current.Dependencies.AddRange(previous.Dependencies); 
            }
        }
    }
}
