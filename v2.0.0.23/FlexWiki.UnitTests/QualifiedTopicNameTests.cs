using System;

using NUnit.Framework; 

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class QualifiedTopicNameTests
    {
        [Test]
        public void AsQualifiedTopicRevision()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("Namespace.TopicName"); 
            QualifiedTopicRevision revision = topicName.AsQualifiedTopicRevision();

            Assert.AreEqual("Namespace", revision.Namespace, "Checking that namespace is preserved.");
            Assert.AreEqual("TopicName", revision.LocalName, "Checking that local name is preserved.");
            Assert.IsNull(revision.Version, "Checking that version is null."); 
        }
        [Test]
        public void ConstructionByDefaultConstructor()
        {
            QualifiedTopicName topicName = new QualifiedTopicName();

            Assert.IsNull(topicName.Namespace, "Checking that namespace is null by default.");
            Assert.IsNull(topicName.LocalName, "Checking that localname is null by default."); 
        }
        [Test]
        public void ConstructionByDottedName()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("Dotted.Namespace.LocalName");

            Assert.AreEqual("LocalName", topicName.LocalName);
            Assert.AreEqual("Dotted.Namespace", topicName.Namespace);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "A namespace is required.")]
        public void ConstructionByEmptyNamespaceExplicit()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("LocalName", "");
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), 
            "'.FooBar' does not contain a namespace - a namespace must be specified.")]
        public void ConstructionByEmptyNamespaceViaQName()
        {
            QualifiedTopicName topicName = new QualifiedTopicName(".FooBar");
        }
        [Test]
        [ExpectedException(typeof(ArgumentException),
           "An illegal local name was specified: the namespace separator is not allowed as part of a local name.")]
        public void ConstructionByIllegalLocalNameAndNamespace()
        {
            TopicName topicName = new TopicName("Dotted.LocalName", "Dotted.Namespace");
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), 
            "'LocalName' does not contain a namespace - a namespace must be specified.")]
        public void ConstructionByLocalName()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("LocalName");
        }
        [Test]
        public void ConstructionByLocalNameAndNamespace()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("LocalName", "Dotted.Namespace");

            Assert.AreEqual("LocalName", topicName.LocalName);
            Assert.AreEqual("Dotted.Namespace", topicName.Namespace);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "A namespace is required.")]
        public void ConstructionByNullNamespace()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("LocalName", null);
        }

    }
}
