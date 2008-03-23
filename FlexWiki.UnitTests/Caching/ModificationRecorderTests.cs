using System;

using NUnit.Framework;
using FlexWiki.Caching;

namespace FlexWiki.UnitTests.Caching
{
    [TestFixture]
    public class ModificationRecorderTests
    {
        [Test]
        public void AllChangesForTopicSince()
        {
            TestForNoModifications(delegate(ModificationRecorder provider)
            {
                provider.AllChangesForTopicSince(new UnqualifiedTopicName("TopicOne"), DateTime.MinValue);
            });
        }

        [Test]
        public void AllTopics()
        {
            TestForNoModifications(delegate(ModificationRecorder provider)
            {
                provider.AllTopics();
            });
        }

        [Test]
        public void DeleteAllTopicsAndHistory()
        {
            TestForModifications(delegate(ModificationRecorder provider)
            {
                provider.DeleteAllTopicsAndHistory();
            }, 
            new NamespaceContentsDeletedModification("NamespaceOne"));
        }

        [Test]
        public void DeleteTopic()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("TopicOne", "NamespaceOne"); 
            TestForModifications(delegate(ModificationRecorder provider)
            {
                provider.DeleteTopic(new UnqualifiedTopicName(topicName.LocalName), false);
            }, 
            new TopicDeletedModification(topicName));
        }

        [Test]
        public void Exists()
        {
            TestForNoModifications(delegate(ModificationRecorder provider)
            {
                bool exists = provider.Exists;
            });
        }

        [Test]
        public void GetParsedTopic()
        {
            TestForNoModifications(delegate(ModificationRecorder provider)
            {
                provider.GetParsedTopic(new UnqualifiedTopicRevision("TopicOne"));
            });
        }

        [Test]
        public void HasNamespacePermission()
        {
            TestForNoModifications(delegate(ModificationRecorder provider)
            {
                provider.HasNamespacePermission(NamespacePermission.Manage);
            });
        }

        [Test]
        public void HasPermission()
        {
            TestForNoModifications(delegate(ModificationRecorder provider)
            {
                provider.HasPermission(new UnqualifiedTopicName("TopicOne"), TopicPermission.Edit);
            });
        }

        [Test]
        public void IsReadOnly()
        {
            TestForNoModifications(delegate(ModificationRecorder provider)
            {
                bool isReadOnly = provider.IsReadOnly;
            });
        }

        [Test]
        public void LockTopic()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("TopicOne", "NamespaceOne");
            TestForModifications(delegate(ModificationRecorder provider)
            {
                provider.LockTopic(new UnqualifiedTopicName(topicName.LocalName));
            },
            new TopicPermissionsModification(topicName)); 
        }

        [Test]
        public void TextReaderForTopic()
        {
            TestForNoModifications(delegate(ModificationRecorder provider)
            {
                provider.TextReaderForTopic(new UnqualifiedTopicRevision("TopicOne")); 
            }); 
        }

        [Test]
        public void TopicExists()
        {
            TestForNoModifications(delegate(ModificationRecorder provider)
            {
                provider.TopicExists(new UnqualifiedTopicName("TopicOne"));
            });
        }

        [Test]
        public void TopicIsReadOnly()
        {
            TestForNoModifications(delegate(ModificationRecorder provider)
            {
                provider.TopicIsReadOnly(new UnqualifiedTopicName("TopicOne"));
            });
        }

        [Test]
        public void UnlockTopic()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("TopicOne", "NamespaceOne");
            TestForModifications(delegate(ModificationRecorder provider)
            {
                provider.UnlockTopic(new UnqualifiedTopicName(topicName.LocalName));
            },
            new TopicPermissionsModification(topicName));
        }


        [Test]
        public void WriteDefinitionTopic()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("_ContentBaseDefinition", "NamespaceOne");
            TestForModifications(delegate(ModificationRecorder provider)
            {
                provider.WriteTopic(new UnqualifiedTopicRevision(topicName.LocalName), "A change of seasons");
            },
            new TopicContentsModification(topicName), 
            new NamespacePropertiesModification(topicName.Namespace));

        }

        [Test]
        public void WriteTopic()
        {
            QualifiedTopicName topicName = new QualifiedTopicName("TopicOne", "NamespaceOne");
            TestForModifications(delegate(ModificationRecorder provider)
            {
                provider.WriteTopic(new UnqualifiedTopicRevision(topicName.LocalName), "A change of seasons");
            },
            new TopicContentsModification(topicName));
        }

        private void DoTest(Action<TestParameters<ModificationRecorder>> test)
        {
            WikiTestUtilities.DoProviderTest(
                TestContentSets.MultipleTopicsWithProperties,
                "test://ModificationRecorderTests/",
                test);
        }

        private void TestForModifications(Action<ModificationRecorder> action, params Modification[] modifications)
        {
            DoTest(delegate(TestParameters<ModificationRecorder> parameters)
            {
                parameters.Application.ModificationsReported.Clear();
                action(parameters.Provider);
                VerifyModifications(parameters, modifications);
            });
        }

        private void TestForNoModifications(Action<ModificationRecorder> action)
        {
            DoTest(delegate(TestParameters<ModificationRecorder> parameters)
            {
                parameters.Application.ModificationsReported.Clear();
                action(parameters.Provider);
                Assert.AreEqual(0, parameters.Application.ModificationsReported.Count,
                    "Checking that the modification report remains empty.");
            });
        }

        private void VerifyModifications(TestParameters<ModificationRecorder> parameters,
            params Modification[] modifications)
        {
            Assert.AreEqual(modifications.Length, parameters.Application.ModificationsReported.Count,
                "Checking that the correnct number of modifications were reported.");

            for (int i = 0; i < modifications.Length; i++)
            {
                Modification expected = modifications[i];

                Assert.IsTrue(parameters.Application.ModificationsReported.Contains(expected),
                    "Checking that modification {0} was present.");
            }
        }


    }
}
