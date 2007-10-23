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
        public void NestedContextOverrides()
        {
            using (RequestContext.Create())
            {
                RequestContext.Current["foo"] = "bar";

                // This new context will clobber the old one - this is by design. 
                // The other option is to have a stack and keep pushing them on, 
                // but since this is a weird case and it's always safe to destroy
                // current context, this is much easier. 
                using (RequestContext.Create())
                {
                    RequestContext.Current["foo"] = "blah"; 
                }

                Assert.IsNull(RequestContext.Current,
                    "Checking that termination of inner context terminates the outer as well."); 
            }
        }

        [Test]
        public void NewContextOverridesOld()
        {
            RequestContext context1 = RequestContext.Create();

            RequestContext context2 = RequestContext.Create();

            Assert.AreNotSame(context1, context2,
                "Checking that if a context is leaked, the next creation overwrites it.");

            Assert.AreSame(context2, RequestContext.Current,
                "Checking that if a context is leaked, the next creation establishes a new current context."); 
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
