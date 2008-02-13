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
using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    public static class ExtendedAssert
    {
        // Test whether a method throws expected exception type. 
        // This function should be part of NUnit framework. 
        public static void Exception(Type expectedException, TestMethod method)
        {
            try
            {
                method();
            }
            catch (Exception e)
            {
                if (e.GetType() != expectedException)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.StackTrace);
                    Assert.Fail("\n Exception expected: {0}\n Thrown: {1}", expectedException.Name, e.GetType().Name);
                }
                return;
            }
            Assert.Fail("{0} is expected", expectedException.Name);
        }

        public delegate void TestMethod();
    }
}
