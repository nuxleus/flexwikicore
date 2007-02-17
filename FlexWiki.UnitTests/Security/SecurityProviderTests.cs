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
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");
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
        [ExpectedException(typeof(FlexWikiSecurityException), "Permission to Read Topic NamespaceOne.TopicOne is denied.")]
        public void AllChangesForTopicSinceDenied()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://SecurityProviderTests",
              new TestContentSet(new TestNamespace("NamespaceOne")));
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");
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
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");
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

                    NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

                    string namespaceContent = "";
                    if (firstRule.Scope == SecurityRuleScope.Namespace)
                    {
                        namespaceContent += firstRule.ToString("T") + "\n";
                    }
                    if (secondRule.Scope == SecurityRuleScope.Namespace)
                    {
                        namespaceContent += secondRule.ToString("T") + "\n";
                    }
                    manager.WriteTopicAndNewVersion(manager.DefinitionTopicName.LocalName, namespaceContent, "test");

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

                    manager.WriteTopicAndNewVersion("TopicOne", content, "test");

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
            NamespaceManager manager = federation.NamespaceManagerForNamespace(ns);
            return (SecurityProvider)manager.GetProvider(typeof(SecurityProvider));
        }



    }
}
