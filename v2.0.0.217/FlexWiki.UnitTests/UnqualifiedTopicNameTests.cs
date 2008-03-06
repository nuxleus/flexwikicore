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

using NUnit.Framework; 

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class UnqualifiedTopicNameTests
    {
        [Test]
        public void ConstructionByEmptyNamespace()
        {
            UnqualifiedTopicName topicName = new UnqualifiedTopicName(".FooBar");
            Assert.IsNull(topicName.Namespace, "Checking that the namespace is null.");
            Assert.AreEqual("FooBar", topicName.LocalName, "Checking that the local name is correct.");
        }
        [Test]
        public void ConstructionByLocalName()
        {
            UnqualifiedTopicName topicName = new UnqualifiedTopicName("LocalName");

            Assert.AreEqual("LocalName", topicName.LocalName);
            Assert.IsNull(topicName.Namespace);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), 
           "An illegal local name was specified: the namespace separator is not allowed as part of a local name.")]
        public void ConstructionByQualifiedName()
        {
            UnqualifiedTopicName topicName = new UnqualifiedTopicName("Dotted.Namespace.LocalName");

        }
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void NamespaceSet()
        {
            UnqualifiedTopicName topicName = new UnqualifiedTopicName();
            topicName.Namespace = "Foo";    
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "A null topic name is not legal.")]
        public void NullLocalName()
        {
            UnqualifiedTopicName topicName = new UnqualifiedTopicName(null);
        }

    }
}
