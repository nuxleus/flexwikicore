using System;
using System.Collections.Generic;
using System.Security.Principal;

using NUnit.Framework;

using FlexWiki.Collections;
using FlexWiki.Security;

namespace FlexWiki.UnitTests.Security
{
    [TestFixture]
    public class SecurityProviderTests
    {
        [Test]
        public void AllChangesForTopicSinceAllowed()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              new TestContentSet(new TestNamespace("NamespaceOne")));
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            manager.WriteTopicAndNewVersion("TopicOne", GetAllowReadRule().ToString("T"), "test");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                TopicChangeCollection changes = provider.AllChangesForTopicSince(
                    new UnqualifiedTopicName("TopicOne"), DateTime.MinValue);

                Assert.AreEqual(1, changes.Count, "Checking that the proper number of changes were returned.");
            }
        }

        [Test]
        [ExpectedException(typeof(FlexWikiAuthorizationException), "Permission to Read Topic NamespaceOne.TopicOne is denied.")]
        public void AllChangesForTopicSinceDenied()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              new TestContentSet(new TestNamespace("NamespaceOne")));
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            manager.WriteTopicAndNewVersion("TopicOne", GetDenyReadRule().ToString("T"), "test");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                TopicChangeCollection changes = provider.AllChangesForTopicSince(
                    new UnqualifiedTopicName("TopicOne"), DateTime.MinValue);
            }
        }

        [Test]
        public void AllChangesForTopicSinceAllowWhenDeniedInHistory()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              new TestContentSet(new TestNamespace("NamespaceOne")));
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            // Make sure that a deny somewhere in the history does not deny permission to read the topic
            manager.WriteTopicAndNewVersion("TopicOne", GetDenyReadRule().ToString("T"), "test");
            manager.WriteTopicAndNewVersion("TopicOne", GetAllowReadRule().ToString("T"), "test");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                TopicChangeCollection changes = provider.AllChangesForTopicSince(
                    new UnqualifiedTopicName("TopicOne"), DateTime.MinValue);

                Assert.AreEqual(2, changes.Count, "Checking that the proper number of changes were returned.");
            }
        }

        [Test]
        public void AllTopicsAllowed()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // We should be able to get the list of topics even if we can't read some of them. 
            SecurityRule rule = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.GenericAll),
                SecurityRulePolarity.Deny, SecurityRuleScope.Topic, SecurableAction.Read, 0);
            manager.WriteTopicAndNewVersion("DeniedTopic", rule.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                QualifiedTopicNameCollection topics = provider.AllTopics();

                Assert.AreEqual(4, topics.Count, "Checking that the right number of topics were returned.");
            }
        }

        [Test]
        public void DeleteAllTopicsAndHistoryAllowed()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the ManageNamespace permission, which should be what is needed.
            SecurityRule allowManageNamespace = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.ManageNamespace, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allowManageNamespace.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.AreEqual(4, manager.AllTopics(ImportPolicy.DoNotIncludeImports).Count,
                    "Checking that the right number of topics were returned before deletion");
                provider.DeleteAllTopicsAndHistory();
                // Only built-in topics should remain
                Assert.AreEqual(2, manager.AllTopics(ImportPolicy.DoNotIncludeImports).Count,
                    "Checking that the right number of topics were returned after deletion");
            }
        }

        [Test]
        [ExpectedException(typeof(FlexWikiAuthorizationException), "Permission to ManageNamespace Namespace NamespaceOne is denied.")]
        public void DeleteAllTopicsAndHistoryDenied()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Edit permission, which shouldn't be enough: ManageNamespace is required.
            SecurityRule allowEdit = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Edit, 0);
            manager.WriteTopicAndNewVersion(manager.DefinitionTopicName.LocalName, allowEdit.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.AreEqual(4, manager.AllTopics(ImportPolicy.DoNotIncludeImports).Count,
                    "Checking that the right number of topics were returned before deletion");
                provider.DeleteAllTopicsAndHistory();
                Assert.Fail("DeleteAllTopicsAndHistory should have thrown an exception");
            }
        }

        [Test]
        public void DeleteTopicAllowed()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Edit permission, which should be what is needed.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Edit, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.AreEqual(4, manager.AllTopics(ImportPolicy.DoNotIncludeImports).Count,
                    "Checking that the right number of topics were returned before deletion");
                provider.DeleteTopic(new UnqualifiedTopicName("TopicOne"));
                Assert.AreEqual(3, manager.AllTopics(ImportPolicy.DoNotIncludeImports).Count,
                    "Checking that the right number of topics were returned after deletion");
            }
        }

        [Test]
        [ExpectedException(typeof(FlexWikiAuthorizationException), "Permission to Edit Topic NamespaceOne.TopicOne is denied.")]
        public void DeleteTopicDenied()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Read permission, which should be insufficient.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.AreEqual(4, manager.AllTopics(ImportPolicy.DoNotIncludeImports).Count,
                    "Checking that the right number of topics were returned before deletion");
                provider.DeleteTopic(new UnqualifiedTopicName("TopicOne"));
                Assert.Fail("A security exception should have been thrown");
            }
        }

        [Test]
        public void ExistsAllowedStoreExists()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Read permission at namespace, which should be what is needed.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsTrue(provider.Exists, "Checking that the security provider returns true when read permission is granted on the namespace.");
            }
        }

        [Test]
        public void ExistsDeniedStoreExists()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                bool exists = provider.Exists;
                Assert.IsFalse(exists, "Checking that security provider returns false when read permission is not granted on the namespace");
            }
        }

        [Test]
        public void ExistsAllowedStoreNonexistent()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, MockSetupOptions.StoreDoesNotExist, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Read permission at namespace, which should be what is needed.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsFalse(provider.Exists, 
                    "Checking that the security provider returns false when read permission is granted on the namespace and store does not exist.");
            }
        }

        [Test]
        public void ExistsDeniedStoreNonexistent()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, MockSetupOptions.StoreDoesNotExist, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                bool exists = provider.Exists;
                Assert.IsFalse(exists, 
                    "Checking that security provider returns false when read permission is not granted on the namespace and store does not exist.");
            }
        }

        [Test]
        public void GetParsedTopicAllowed()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Read permission, which should be what is needed.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                ParsedTopic parsedTopic = provider.GetParsedTopic(new UnqualifiedTopicRevision("TopicOne"));
                Assert.IsNotNull(parsedTopic,
                    "Checking that the parsed topic was returned.");
            }
        }

        [Test]
        [ExpectedException(typeof(FlexWikiAuthorizationException), "Permission to Read Topic NamespaceOne.TopicOne is denied.")]
        public void GetParsedTopicDenied()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                ParsedTopic parsedTopic = provider.GetParsedTopic(new UnqualifiedTopicRevision("TopicOne"));
                Assert.Fail("A security exception should have been thrown");
            }
        }

        [Test]
        public void HasPermission()
        {
            foreach (SecurityRule firstRule in SecurityRules.GetAll("someuser", "somerole", 0))
            {
                foreach (SecurityRule secondRule in SecurityRules.GetAll("someuser", "somerole", 1))
                {
                    FederationConfiguration federationConfiguration = new FederationConfiguration();
                    if (firstRule.Scope == SecurityRuleScope.Wiki)
                    {
                        AddWikiRule(federationConfiguration, firstRule);
                    }
                    if (secondRule.Scope == SecurityRuleScope.Wiki)
                    {
                        AddWikiRule(federationConfiguration, secondRule);
                    }
                    Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
                        new TestContentSet(
                            new TestNamespace("NamespaceOne")
                        ),
                        federationConfiguration
                    );

                    NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");

                    string namespaceContent = "";
                    if (firstRule.Scope == SecurityRuleScope.Namespace)
                    {
                        namespaceContent += firstRule.ToString("T") + "\n";
                    }
                    if (secondRule.Scope == SecurityRuleScope.Namespace)
                    {
                        namespaceContent += secondRule.ToString("T") + "\n";
                    }
                    WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager, manager.DefinitionTopicName.LocalName,
                        namespaceContent, "test");

                    string content = "";
                    if (firstRule.Scope == SecurityRuleScope.Topic)
                    {
                        content = firstRule.ToString("T") + "\n";
                    }
                    if (secondRule.Scope == SecurityRuleScope.Topic)
                    {
                        content += secondRule.ToString("T") + "\n";
                    }
                    content += "Some content";

                    WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager, "TopicOne", content, "test");

                    bool isReadGrantExpected = SecurityRules.GetGrantExpectation(
                        TopicPermission.Read, new TestIdentity("someuser", "somerole"), firstRule, secondRule);
                    bool isEditGrantExpected = SecurityRules.GetGrantExpectation(
                        TopicPermission.Edit, new TestIdentity("someuser", "somerole"), firstRule, secondRule);

                    SecurityProvider provider = GetSecurityProvider(federation, "NamespaceOne");

                    using (new TestSecurityContext("someuser", "somerole"))
                    {
                        Assert.AreEqual(isReadGrantExpected,
                            provider.HasPermission(new UnqualifiedTopicName("TopicOne"), TopicPermission.Read),
                            string.Format("Checking that user should {0}have read permission on topic. (Rule one: {1}. Rule two: {2}.)",
                                isReadGrantExpected ? "" : "not ", firstRule, secondRule));

                        Assert.AreEqual(isEditGrantExpected,
                            provider.HasPermission(new UnqualifiedTopicName("TopicOne"), TopicPermission.Edit),
                            string.Format("Checking that user should {0}have edit permission on topic. (Rule one: {1}. Rule two: {2}.)",
                                isEditGrantExpected ? "" : "not ", firstRule, secondRule));

                    }

                }
            }
        }


        [Test]
        public void IsReadOnlyContentStoreReadOnlySecurityReadWrite()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, MockSetupOptions.ReadOnlyStore, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Read permission, which should be what is needed.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Edit, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsTrue(provider.IsReadOnly, "Checking that the store being read-only causes a true return.");
            }

        }

        [Test]
        public void IsReadOnlyContentStoreReadOnlySecurityReadOnly()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, MockSetupOptions.ReadOnlyStore, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Read permission, which should be what is needed.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager, manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsTrue(provider.IsReadOnly,
                    "Checking that content store is read only when security and store are both read-only");
            }
        }

        [Test]
        public void IsReadOnlyContentStoreReadWriteSecurityReadWrite()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Read permission, which should be what is needed.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Edit, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsFalse(provider.IsReadOnly,
                    "Checking that the store is read-write when store and security policy are read-write.");
            }

        }

        [Test]
        public void IsReadOnlyContentStoreReadWriteSecurityReadOnly()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Read permission, which should be what is needed.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsFalse(provider.IsReadOnly,
                    "Checking that content store is read-write only when security is read-only and store is read-write.");
            }
        }

        [Test]
        public void LockTopicAllowed()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the ManageNamespace permission, which should be what is needed.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.ManageNamespace, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                UnqualifiedTopicName topic = new UnqualifiedTopicName("TopicOne");
                Assert.IsTrue(provider.HasPermission(topic, TopicPermission.Edit),
                    "Checking that topic is editable before LockTopic.");
                Assert.IsTrue(provider.HasPermission(topic, TopicPermission.Read),
                    "Checking that topic is readable before LockTopic.");
                provider.LockTopic(topic);
                Assert.IsFalse(provider.HasPermission(topic, TopicPermission.Edit),
                    "Checking that topic is not editable after LockTopic.");
                Assert.IsTrue(provider.HasPermission(topic, TopicPermission.Read),
                    "Checking that topic is still readable after LockTopic.");
            }
        }

        [Test]
        [ExpectedException(typeof(FlexWikiAuthorizationException), "Permission to ManageNamespace Namespace NamespaceOne is denied.")]
        public void LockTopicDenied()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Edit permission, which should not be enough.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Edit, 0);
            manager.WriteTopicAndNewVersion(manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                UnqualifiedTopicName topic = new UnqualifiedTopicName("TopicOne");
                Assert.IsTrue(provider.HasPermission(topic, TopicPermission.Edit),
                    "Checking that topic is editable before LockTopic.");
                Assert.IsTrue(provider.HasPermission(topic, TopicPermission.Read),
                    "Checking that topic is readable before LockTopic.");
                provider.LockTopic(topic);

                Assert.Fail("A security exception should have been thrown.");
            }
        }

        [Test]
        public void TextReaderForTopicAllowed()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Read permission, which should be enough.
            SecurityRule deny = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, deny.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                string contents = provider.TextReaderForTopic(new UnqualifiedTopicRevision("TopicOne")).ReadToEnd();
                Assert.AreEqual(7, contents.Length, "Checking that the right contents were returned.");
            }

        }

        [Test]
        [ExpectedException(typeof(FlexWikiAuthorizationException), "Permission to Read Topic NamespaceOne.TopicOne is denied.")]
        public void TextReaderForTopicDenied()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                string contents = provider.TextReaderForTopic(new UnqualifiedTopicRevision("TopicOne")).ReadToEnd();
                Assert.Fail("A security exception should have been thrown.");
            }

        }

        [Test]
        public void TopicExists()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Deny the Read permission. Topic existence shouldn't be affected by security policy. 
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Deny, SecurityRuleScope.Topic, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager, "TopicOne", allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsTrue(provider.TopicExists(new UnqualifiedTopicName("TopicOne")),
                    "Checking that TopicExists returns true even when read permission is denied.");
            }
        }

        [Test]
        public void TopicExistsNonexistentTopic()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Deny the Read permission. Topic existence shouldn't be affected by security policy. 
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Deny, SecurityRuleScope.Topic, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager, "TopicOne", allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsFalse(provider.TopicExists(new UnqualifiedTopicName("NoSuchTopic")),
                    "Checking that TopicExists returns false even when read permission is denied.");
            }
        }

        [Test]
        public void UnlockTopicAllowed()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the ManageNamespace permission, which should be what is needed.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.ManageNamespace, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                UnqualifiedTopicName topic = new UnqualifiedTopicName("TopicOne");
                provider.LockTopic(topic);
                Assert.IsFalse(provider.HasPermission(topic, TopicPermission.Edit),
                    "Checking that topic is not editable before UnlockTopic.");
                Assert.IsTrue(provider.HasPermission(topic, TopicPermission.Read),
                    "Checking that topic is readable before UnlockTopic.");
                provider.UnlockTopic(topic);
                Assert.IsTrue(provider.HasPermission(topic, TopicPermission.Edit),
                    "Checking that topic is editable after UnlockTopic.");
                Assert.IsTrue(provider.HasPermission(topic, TopicPermission.Read),
                    "Checking that topic is still readable after UnlockTopic.");
            }
        }

        [Test]
        [ExpectedException(typeof(FlexWikiAuthorizationException), "Permission to ManageNamespace Namespace NamespaceOne is denied.")]
        public void UnlockTopicDenied()
        {
            // Use the default configuration, where everything is denied
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            // Grant the Edit permission, which should not be enough.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Namespace, SecurableAction.Edit, 0);
            manager.WriteTopicAndNewVersion(manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                UnqualifiedTopicName topic = new UnqualifiedTopicName("TopicOne");
                provider.UnlockTopic(topic);

                Assert.Fail("A security exception should have been thrown.");
            }
        }

        [Test]
        public void WriteTopicRegularTopicAllowed()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            // Grant the Edit permission, which should be enough.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Wiki, SecurableAction.Edit, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(allow));
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                UnqualifiedTopicRevision topic = new UnqualifiedTopicRevision("TopicOne");
                provider.WriteTopic(topic, "New content");
            }
        }

        [Test]
        [ExpectedException(typeof(FlexWikiAuthorizationException), "Permission to Edit Topic NamespaceOne.TopicOne is denied.")]
        public void WriteTopicRegularTopicDenied()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            // Grant the Read permission, which should not be enough.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Wiki, SecurableAction.Read, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(allow));
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                UnqualifiedTopicRevision topic = new UnqualifiedTopicRevision("TopicOne");
                provider.WriteTopic(topic, "New content");
                Assert.Fail("A security exception should have been thrown.");
            }
        }

        [Test]
        public void WriteTopicDefinitionTopicAllowed()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            // Grant the ManageNamespace permission, which should be enough.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Wiki, SecurableAction.ManageNamespace, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(allow));
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                provider.WriteTopic(new UnqualifiedTopicRevision(manager.DefinitionTopicName.LocalName), "New content");
            }
        }

        [Test]
        [ExpectedException(typeof(FlexWikiAuthorizationException), "Permission to ManageNamespace Namespace NamespaceOne is denied.")]
        public void WriteTopicDefinitionTopicDenied()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            // Grant the Edit permission, which should not be enough.
            SecurityRule allow = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Wiki, SecurableAction.Edit, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(allow));
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            SecurityProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                provider.WriteTopic(new UnqualifiedTopicRevision(manager.DefinitionTopicName.LocalName), "New content");
                Assert.Fail("A security exception should have been thrown.");
            }
        }

        private void AddWikiRule(FederationConfiguration federationConfiguration, SecurityRule rule)
        {
            federationConfiguration.AuthorizationRules.Add(new WikiAuthorizationRule(rule));
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
        private SecurityRule GetAllowReadRule()
        {
            return new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Allow, SecurityRuleScope.Topic, SecurableAction.Read, 0);
        }
        private SecurityRule GetDenyReadRule()
        {
            return new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "someuser"),
                SecurityRulePolarity.Deny, SecurityRuleScope.Topic, SecurableAction.Read, 0);
        }
        private static SecurityProvider GetSecurityProvider(NamespaceManager manager)
        {
            return (SecurityProvider)manager.GetProvider(typeof(SecurityProvider));
        }
        private static SecurityProvider GetSecurityProvider(Federation federation, string ns)
        {
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, ns);
            return (SecurityProvider)manager.GetProvider(typeof(SecurityProvider));
        }



    }
}
