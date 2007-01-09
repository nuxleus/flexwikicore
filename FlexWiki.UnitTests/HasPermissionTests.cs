using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class SecurityProviderTests
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
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");
            SecurityProvider provider = (SecurityProvider)manager.GetProvider(typeof(SecurityProvider));

            using (new TestSecurityContext("user:someuser"))
            {
                Assert.IsTrue(provider.HasPermission(new UnqualifiedTopicName("TopicOne"), TopicPermission.Edit)); 
                Assert.IsTrue(provider.HasPermission(new UnqualifiedTopicName("TopicOne"), TopicPermission.Read)); 
            }
        }

        [Test]
        public void ExplicitlyAllowUserEdit()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "test", "AllowEdit: user:someuser\nSome content")
                    )
                )
            );

            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");
            SecurityProvider provider = (SecurityProvider)manager.GetProvider(typeof(SecurityProvider));

            using (new TestSecurityContext("someuser"))
            {
                Assert.IsTrue(provider.HasPermission(new UnqualifiedTopicName("TopicOne"), TopicPermission.Edit), 
                    "Checking that user has edit permission on the specified topic.");
                Assert.IsTrue(provider.HasPermission(new UnqualifiedTopicName("TopicOne"), TopicPermission.Read), 
                    "Checking that user has implied read permission on the specified topic.");
            }

        }
    }
}
