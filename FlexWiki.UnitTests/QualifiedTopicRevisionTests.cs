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
    [TestFixture]
    public class QualifiedTopicRevisionTests
    {
        [Test]
        public void AsQualifiedTopicName()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("LocalName", "Namespace", "Version");
            QualifiedTopicName qualifiedName = revision.AsQualifiedTopicName();

            Assert.AreEqual("Namespace.LocalName", qualifiedName.DottedName,
                "Checking that the namespace and local name were set correctly."); 
        }

        [Test]
        public void AsUnqualifiedTopicRevision()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("LocalName", "Namespace", "Version");
            UnqualifiedTopicRevision unqualifiedRevision = revision.AsUnqualifiedTopicRevision();

            Assert.AreEqual("LocalName(Version)", unqualifiedRevision.DottedNameWithVersion,
                "Checking that the conversion to an unqualified topic revision was successful.");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "A namespace is required.")]
        public void ConstructByStringNoNamespace()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("LocalNameOnly");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "A namespace is required.")]
        public void ConstructByTopicNameNoNamespace()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision(new TopicName("LocalNameOnly"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "A namespace is required.")]
        public void ConstructByLocalNameAndNullNamespace()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("LocalNameOnly", null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "A namespace is required.")]
        public void ConstructByLocalNameAndVersionNullNamespace()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("LocalNameOnly", null, "version");
        }

        [Test]
        public void NewOfSameType()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("Foo.Bar").NewOfSameType("Namespace.LocalName(Version)")
                as QualifiedTopicRevision;

            Assert.AreEqual(typeof(QualifiedTopicRevision), revision.GetType(), 
                "Checking that the newly created type is a QualifiedTopicRevision.");
            Assert.AreEqual("Namespace.LocalName(Version)", revision.DottedNameWithVersion, "Checking that the correct properties were parsed."); 

        }
    }
}
