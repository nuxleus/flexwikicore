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
using System.Threading; 

using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture] 
    public class RequestContextTests
    {
        private class BackgroundRunner
        {
            private string _data;
            private string _key; 
            private object _valueRetrievedAfter;
            private object _valueRetrievedBefore; 

            public BackgroundRunner()
            {
            }

            public object ValueRetrievedAfter
            {
                get { return _valueRetrievedAfter; }
            }

            public object ValueRetrievedBefore
            {
                get { return _valueRetrievedBefore; }
            }

            public void Run(string key, string data)
            {
                _key = key; 
                _data = data;

                Thread thread = new Thread(ThreadProc);
                thread.Start();
                thread.Join(); 
            }

            private void ThreadProc()
            {
                using (RequestContext.Create())
                {
                    _valueRetrievedBefore = RequestContext.Current[_key]; 
                    RequestContext.Current[_key] = _data;
                    _valueRetrievedAfter = RequestContext.Current[_key]; 
                }
            }

        }

        [Test]
        public void MultipleThreadOverlap()
        {
            using (RequestContext.Create())
            {
                RequestContext.Current["foo"] = "bar";

                BackgroundRunner runner = new BackgroundRunner();

                runner.Run("foo", "baaz");

                Assert.IsNull(runner.ValueRetrievedBefore,
                    "Checking that value starts null in a different context."); 
                Assert.AreEqual("baaz", runner.ValueRetrievedAfter,
                    "Checking that a second thread read a different context.");

                Assert.AreEqual("bar", RequestContext.Current["foo"],
                    "Checking that second thread didn't trash first thread's data."); 
            
            }
        }

        [Test]
        [ExpectedException(typeof(NestedContextUnexpectedException))]
        public void NestedContextFailure()
        {
            using (RequestContext outer = RequestContext.Create())
            {
                RequestContext.Current["foo"] = "bar";

                using (RequestContext inner = RequestContext.Create())
                {
                    Assert.Fail("Should never get here."); 
                }
            }
        }

        [Test]
        public void NestedContextSuccess()
        {
            using (RequestContext outer = RequestContext.Create())
            {
                RequestContext.Current["foo"] = "bar";
                RequestContext.Current.Dependencies.Add(new NamespaceExistenceDependency("Outer")); 

                using (RequestContext inner = RequestContext.Create(RequestContextOptions.AllowNestedContext))
                {
                    Assert.AreNotSame(inner, outer, "Checking that a new context was created.");
                    RequestContext.Current["foo"] = "blah";
                    RequestContext.Current.Dependencies.Add(new NamespaceExistenceDependency("Inner")); 
                }

                Assert.AreEqual(outer, RequestContext.Current,
                    "Checking that termination of inner context make outer current.");

                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new NamespaceExistenceDependency("Inner")),
                    "Checking that dependencies propagate outward.");
                Assert.IsTrue(RequestContext.Current.Dependencies.Contains(new NamespaceExistenceDependency("Outer")),
                    "Checking that dependencies are preserved across a nested context."); 

            }
        }

        [Test]
        public void NullWhenOutsideUsing()
        {
            // We need a fresh thread to run this, because some of our other tests intentionally leak
            
            bool isNull = false;
            Thread thread = new Thread((ThreadStart) delegate { 
                isNull = RequestContext.Current == null; 
            }); 

            thread.Start();
            thread.Join(); 

            Assert.IsTrue(isNull, "Checking that RequestContext.Current starts out null."); 
        }

        [Test]
        public void SimpleStoreAndRetrieve()
        {
            using (RequestContext.Create())
            {
                Assert.IsNull(RequestContext.Current["foo"],
                    "Checking that nonexistent keys return null.");
                RequestContext.Current["foo"] = "bar";
                Assert.AreEqual("bar", RequestContext.Current["foo"],
                    "Checking that an object can be retrieved correctly");
                RequestContext.Current["foo"] = "baaz";
                Assert.AreEqual("baaz", RequestContext.Current["foo"],
                    "Checking that an object can be overwritten safely");
            }
        }

        [Test]
        public void Using()
        {
            // Make sure we take care of any leaks first. 
            while (RequestContext.Current != null)
            {
                ((IDisposable)RequestContext.Current).Dispose(); 
            }

            using (RequestContext context = RequestContext.Create())
            {
                Assert.AreSame(context, RequestContext.Current,
                    "Checking that current request context is correct."); 
            }

            Assert.IsNull(RequestContext.Current,
                "Checking that current request context goes away when using block terminates."); 
        }
    }
}
