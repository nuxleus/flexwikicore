using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class HasPermissionTests
    {
        [Test]
        [Ignore("Implement later - not ready for this yet")]
        public void ManageImpliesReadAndEdit()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne", 
                        new TestTopic("_ContentBaseDefinition", "test", "AllowManage: user:someuser"), 
                        new TestTopic("TopicOne", "test", "Some content")
                    )
                )
            );
            SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne"); 

            using (new TestSecurityContext("someuser"))
            {
                AssertAllowed(provider, "TopicOne", TopicPermission.Edit);
                AssertAllowed(provider, "TopicTwo", TopicPermission.Read); 
            }
        }

        [Test]
        public void CanReadTopicWhenRoleAllowedReadTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "test", "AllowRead: role:somerole\nSome content")
                    )
                )
            );
            SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                AssertAllowed(provider, "TopicOne", TopicPermission.Read);
                AssertDenied(provider, "TopicOne", TopicPermission.Edit);
            }

        }
        [Test]
        public void CanReadTopicWhenUserAllowedReadTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "test", "AllowRead: user:someuser\nSome content")
                    )
                )
            );
            SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne");

            using (new TestSecurityContext("someuser"))
            {
                AssertAllowed(provider, "TopicOne", TopicPermission.Read);
                AssertDenied(provider, "TopicOne", TopicPermission.Edit);
            }

        }
        [Test]
        public void CanReadAndEditTopicWhenRoleAllowedEditTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "test", "AllowEdit: role:somerole\nSome content")
                    )
                )
            );
            SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                AssertAllowed(provider, "TopicOne", TopicPermission.Read);
                AssertAllowed(provider, "TopicOne", TopicPermission.Edit);
            }
        }
        [Test]
        public void CanReadAndEditTopicWhenUserAllowedEditTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "test", "AllowEdit: user:someuser\nSome content")
                    )
                )
            );
            SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne");

            using (new TestSecurityContext("someuser"))
            {
                AssertAllowed(provider, "TopicOne", TopicPermission.Read);
                AssertAllowed(provider, "TopicOne", TopicPermission.Edit);
            }

        }
        [Test]
        public void CantEditWhenRoleDeniedEditTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "test", @"DenyEdit: role:somerole
AllowRead: role:somerole
Some content")
                    )
                )
            );
            SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                AssertAllowed(provider, "TopicOne", TopicPermission.Read);
                AssertDenied(provider, "TopicOne", TopicPermission.Edit);
            }
        }
        [Test]
        public void CantEditWhenUserDeniedEditTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "test", @"DenyEdit: user:someuser
AllowRead: user:someuser
Some content")
                    )
                )
            );
            SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne");

            using (new TestSecurityContext("someuser"))
            {
                AssertAllowed(provider, "TopicOne", TopicPermission.Read);
                AssertDenied(provider, "TopicOne", TopicPermission.Edit);
            }

        }
        [Test]
        public void CantReadOrEditWhenNoPermissions()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "test", "Some content")
                    )
                )
            );
            SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne");

            using (new TestSecurityContext("someuser"))
            {
                AssertDenied(provider, "TopicOne", TopicPermission.Read);
                AssertDenied(provider, "TopicOne", TopicPermission.Edit);
            }

        }
        [Test]
        public void CantReadOrEditWhenRoleDeniedReadTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "test", "DenyRead: role:somerole\nSome content")
                    )
                )
            );
            SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                AssertDenied(provider, "TopicOne", TopicPermission.Read);
                AssertDenied(provider, "TopicOne", TopicPermission.Edit);
            }
        }
        [Test]
        public void CantReadOrEditWhenUserDeniedReadTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "test", "DenyRead: user:someuser\nSome content")
                    )
                )
            );
            SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne");

            using (new TestSecurityContext("someuser"))
            {
                AssertDenied(provider, "TopicOne", TopicPermission.Read);
                AssertDenied(provider, "TopicOne", TopicPermission.Edit);
            }
        }

        private void AssertAllowed(SecurityProvider provider, string topic, TopicPermission topicPermission)
        {
            Assert.IsTrue(provider.HasPermission(new UnqualifiedTopicName(topic), topicPermission),
                string.Format("Checking that user has permission {0} on topic {1}", topicPermission, topic));

        }
        private void AssertDenied(SecurityProvider provider, string topic, TopicPermission topicPermission)
        {
            Assert.IsFalse(provider.HasPermission(new UnqualifiedTopicName(topic), topicPermission),
                string.Format("Checking that user is denied permission {0} on topic {1}", topicPermission, topic));
        }
        private static SecurityProvider GetSecurityProvider(Federation federation, string ns)
        {
            NamespaceManager manager = federation.NamespaceManagerForNamespace(ns);
            return (SecurityProvider)manager.GetProvider(typeof(SecurityProvider));
        }


    }
}
