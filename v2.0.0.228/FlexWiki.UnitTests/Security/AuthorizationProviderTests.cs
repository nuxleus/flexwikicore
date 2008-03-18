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
using System.Security.Principal;

using NUnit.Framework;

using FlexWiki.Collections;
using FlexWiki.Security;

namespace FlexWiki.UnitTests.Security
{
    [TestFixture]
    public class AuthorizationProviderTests
    {
        [Test]
        public void AllChangesForTopicSinceAllowed()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              new TestContentSet(new TestNamespace("NamespaceOne")));

            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            manager.WriteTopicAndNewVersion("TopicOne", GetAllowReadRule().ToString("T"), "test");
            AuthorizationProvider provider = GetSecurityProvider(manager);

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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              new TestContentSet(new TestNamespace("NamespaceOne")));
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            manager.WriteTopicAndNewVersion("TopicOne", GetDenyReadRule().ToString("T"), "test");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                TopicChangeCollection changes = provider.AllChangesForTopicSince(
                    new UnqualifiedTopicName("TopicOne"), DateTime.MinValue);
            }
        }

        [Test]
        public void AllChangesForTopicSinceAllowWhenDeniedInHistory()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              new TestContentSet(new TestNamespace("NamespaceOne")));
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            // Make sure that a deny somewhere in the history does not deny permission to read the topic
            manager.WriteTopicAndNewVersion("TopicOne", GetDenyReadRule().ToString("T"), "test");
            manager.WriteTopicAndNewVersion("TopicOne", GetAllowReadRule().ToString("T"), "test");
            AuthorizationProvider provider = GetSecurityProvider(manager);

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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // We should be able to get the list of topics even if we can't read some of them. 
            AuthorizationRule rule = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.GenericAll),
                AuthorizationRulePolarity.Deny, AuthorizationRuleScope.Topic, SecurableAction.Read, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the ManageNamespace permission, which should be what is needed.
            AuthorizationRule allowManageNamespace = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.ManageNamespace, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Edit permission, which shouldn't be enough: ManageNamespace is required.
            AuthorizationRule allowEdit = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Edit, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Edit permission, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Edit, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Grant the Read permission, which should be insufficient.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Read, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Read permission at namespace, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Read, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            using (new TestSecurityContext("someuser", "somerole"))
            {
                bool exists = provider.Exists;
                Assert.IsFalse(exists, "Checking that security provider returns false when read permission is not granted on the namespace");
            }
        }

        [Test]
        public void ExistsAllowedStoreNonexistent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports, MockSetupOptions.StoreDoesNotExist);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Read permission at namespace, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Read, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports, MockSetupOptions.StoreDoesNotExist);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Read permission, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Read, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            using (new TestSecurityContext("someuser", "somerole"))
            {
                ParsedTopic parsedTopic = provider.GetParsedTopic(new UnqualifiedTopicRevision("TopicOne"));
                Assert.Fail("A security exception should have been thrown");
            }
        }

        [Test]
        public void HasNamespacePermissionManageAllowed()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the ManageNamespace permission, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.ManageNamespace, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsTrue(provider.HasNamespacePermission(NamespacePermission.Manage));
            }
        }

        [Test]
        public void HasNamespacePermissionManageDenied()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsFalse(provider.HasNamespacePermission(NamespacePermission.Manage));
            }
        }

        [Test]
        public void HasPermission()
        {
            foreach (AuthorizationRule firstRule in AuthorizationRules.GetAll("someuser", "somerole", 0))
            {
                foreach (AuthorizationRule secondRule in AuthorizationRules.GetAll("someuser", "somerole", 1))
                {
                    FederationConfiguration federationConfiguration = new FederationConfiguration();
                    if (firstRule.Scope == AuthorizationRuleScope.Wiki)
                    {
                        AddWikiRule(federationConfiguration, firstRule);
                    }
                    if (secondRule.Scope == AuthorizationRuleScope.Wiki)
                    {
                        AddWikiRule(federationConfiguration, secondRule);
                    }
                    Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
                        new TestContentSet(
                            new TestNamespace("NamespaceOne")
                        ),
                        federationConfiguration
                    );

                    NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");

                    string namespaceContent = "";
                    if (firstRule.Scope == AuthorizationRuleScope.Namespace)
                    {
                        namespaceContent += firstRule.ToString("T") + "\n";
                    }
                    if (secondRule.Scope == AuthorizationRuleScope.Namespace)
                    {
                        namespaceContent += secondRule.ToString("T") + "\n";
                    }
                    WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager, manager.DefinitionTopicName.LocalName,
                        namespaceContent, "test");

                    string content = "";
                    if (firstRule.Scope == AuthorizationRuleScope.Topic)
                    {
                        content = firstRule.ToString("T") + "\n";
                    }
                    if (secondRule.Scope == AuthorizationRuleScope.Topic)
                    {
                        content += secondRule.ToString("T") + "\n";
                    }
                    content += "Some content";

                    WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager, "TopicOne", content, "test");

                    bool isReadGrantExpected = AuthorizationRules.GetGrantExpectation(
                        TopicPermission.Read, new TestIdentity("someuser", "somerole"), firstRule, secondRule);
                    bool isEditGrantExpected = AuthorizationRules.GetGrantExpectation(
                        TopicPermission.Edit, new TestIdentity("someuser", "somerole"), firstRule, secondRule);

                    AuthorizationProvider provider = GetSecurityProvider(federation, "NamespaceOne");

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
        public void HasPermissionDefinitionTopic()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleEmptyNamespace, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Set it up so we have Edit but not ManageNamespace
            AuthorizationRule allowEdit = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Edit, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allowEdit.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsFalse(provider.HasPermission(new UnqualifiedTopicName(manager.DefinitionTopicName.LocalName),
                    TopicPermission.Edit),
                    "Checking that allowing edit is not enough to grant editpermisison on the definition topic.");
            }

            // Now try it where we're granted ManageNamespace
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.ManageNamespace, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsTrue(provider.HasPermission(new UnqualifiedTopicName(manager.DefinitionTopicName.LocalName),
                    TopicPermission.Edit),
                    "Checking that granting ManageNamespace implies ability to edit definition topic.");
            }
        }

        [Test]
        public void HasPermissionFromRequestContext()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports, MockSetupOptions.CacheDisabled);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);
            MockContentStore store = (MockContentStore)manager.GetProvider(typeof(MockContentStore));

            using (new TestSecurityContext("someuser", "somerole"))
            {
                store.TextReaderForTopicCalled = false;

                UnqualifiedTopicName topicName = new UnqualifiedTopicName("TopicOne");
                provider.HasPermission(topicName, TopicPermission.Read);

                Assert.IsTrue(store.TextReaderForTopicCalled,
                    "Checking that cache was not used when no RequestContext has been established.");

                using (RequestContext.Create())
                {
                    store.TextReaderForTopicCalled = false;
                    bool first = provider.HasPermission(topicName, TopicPermission.Read);

                    Assert.IsTrue(store.TextReaderForTopicCalled,
                        "Checking that store was called while populating cache.");

                    store.TextReaderForTopicCalled = false;
                    bool second = provider.HasPermission(topicName, TopicPermission.Read);

                    Assert.IsFalse(store.TextReaderForTopicCalled,
                        "Checking that the store was not called after cache is populated.");
                }
            }
        }

        [Test]
        public void HasPermissionNonexistentTopicAllowed()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleEmptyNamespace, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Set it up so we have Edit 
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Edit, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsTrue(provider.HasPermission(new UnqualifiedTopicName("NoSuchTopic"),
                    TopicPermission.Edit),
                    "Checking that allowing edit at the namespace level grants edit on nonexistent topics.");
            }
        }

        [Test]
        public void HasPermissionNonexistentTopicDenied()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleEmptyNamespace, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Set it up so we only have Read 
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                Assert.IsTrue(provider.HasPermission(new UnqualifiedTopicName("NoSuchTopic"),
                    TopicPermission.Read),
                    "Checking that allowing read at the namespace level grants read on nonexistent topics.");
                Assert.IsFalse(provider.HasPermission(new UnqualifiedTopicName("NoSuchTopic"),
                    TopicPermission.Edit),
                    "Checking that allowing read at the namespace level denies edit on nonexistent topics.");
            }
        }

        [Test]
        public void IsReadOnlyContentStoreReadOnlySecurityReadWrite()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports, MockSetupOptions.ReadOnlyStore);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Read permission, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Edit, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports, MockSetupOptions.ReadOnlyStore);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Read permission, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Read, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Read permission, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Edit, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Read permission, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Read, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the ManageNamespace permission, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.ManageNamespace, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Edit permission, which should not be enough.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Edit, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Read permission, which should be enough.
            AuthorizationRule deny = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Read, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            using (new TestSecurityContext("someuser", "somerole"))
            {
                string contents = provider.TextReaderForTopic(new UnqualifiedTopicRevision("TopicOne")).ReadToEnd();
                Assert.Fail("A security exception should have been thrown.");
            }
        }

        [Test]
        public void TopicExists()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Deny the Read permission. Topic existence shouldn't be affected by security policy. 
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Deny, AuthorizationRuleScope.Topic, SecurableAction.Read, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Deny the Read permission. Topic existence shouldn't be affected by security policy. 
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Deny, AuthorizationRuleScope.Topic, SecurableAction.Read, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the ManageNamespace permission, which should be what is needed.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.ManageNamespace, 0);
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
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Use the default configuration, where everything is denied
            federation.Configuration.AuthorizationRules.Clear();

            // Grant the Edit permission, which should not be enough.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Edit, 0);
            manager.WriteTopicAndNewVersion(manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            using (new TestSecurityContext("someuser", "somerole"))
            {
                UnqualifiedTopicName topic = new UnqualifiedTopicName("TopicOne");
                provider.UnlockTopic(topic);

                Assert.Fail("A security exception should have been thrown.");
            }
        }

        [Test]
        public void WriteNewTopicAllowed()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleEmptyNamespace, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Set it up so we have Edit 
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Edit, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            UnqualifiedTopicRevision revision = new UnqualifiedTopicRevision("NewTopic");
            string content = "New content to be written";
            using (new TestSecurityContext("someuser", "somerole"))
            {
                provider.WriteTopic(revision, content);
                Assert.AreEqual(content, provider.TextReaderForTopic(revision).ReadToEnd(),
                    "Checking that content was written okay.");
            }
        }

        [Test]
        [ExpectedException(typeof(FlexWikiAuthorizationException), "Permission to Edit Topic NamespaceOne.NewTopic is denied.")]
        public void WriteNewTopicDenied()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleEmptyNamespace, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            // Set it up so we have Read
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Namespace, SecurableAction.Read, 0);
            WikiTestUtilities.WriteTopicAndNewVersionBypassingSecurity(manager,
                manager.DefinitionTopicName.LocalName, allow.ToString("T"), "test");

            UnqualifiedTopicRevision revision = new UnqualifiedTopicRevision("NewTopic");
            string content = "New content to be written";
            using (new TestSecurityContext("someuser", "somerole"))
            {
                provider.WriteTopic(revision, content);
            }
        }

        [Test]
        public void WriteTopicRegularTopicAllowed()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            // Grant the Edit permission, which should be enough.
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Wiki, SecurableAction.Edit, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(allow));
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

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
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Wiki, SecurableAction.Read, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(allow));
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

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
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Wiki, SecurableAction.ManageNamespace, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(allow));
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

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
            AuthorizationRule allow = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Wiki, SecurableAction.Edit, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(allow));
            Federation federation = WikiTestUtilities.SetupFederation("test://AuthorizationProviderTests",
              TestContentSets.SingleTopicNoImports, configuration);
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            AuthorizationProvider provider = GetSecurityProvider(manager);

            using (new TestSecurityContext("someuser", "somerole"))
            {
                provider.WriteTopic(new UnqualifiedTopicRevision(manager.DefinitionTopicName.LocalName), "New content");
                Assert.Fail("A security exception should have been thrown.");
            }
        }

        private void AddWikiRule(FederationConfiguration federationConfiguration, AuthorizationRule rule)
        {
            federationConfiguration.AuthorizationRules.Add(new WikiAuthorizationRule(rule));
        }

        private void AssertAllowed(AuthorizationProvider provider, string topic, TopicPermission topicPermission)
        {
            Assert.IsTrue(provider.HasPermission(new UnqualifiedTopicName(topic), topicPermission),
                string.Format("Checking that user has permission {0} on topic {1}", topicPermission, topic));

        }
        private void AssertDenied(AuthorizationProvider provider, string topic, TopicPermission topicPermission)
        {
            Assert.IsFalse(provider.HasPermission(new UnqualifiedTopicName(topic), topicPermission),
                string.Format("Checking that user is denied permission {0} on topic {1}", topicPermission, topic));
        }
        private AuthorizationRule GetAllowReadRule()
        {
            return new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Topic, SecurableAction.Read, 0);
        }
        private AuthorizationRule GetDenyReadRule()
        {
            return new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "someuser"),
                AuthorizationRulePolarity.Deny, AuthorizationRuleScope.Topic, SecurableAction.Read, 0);
        }
        private static AuthorizationProvider GetSecurityProvider(NamespaceManager manager)
        {
            return (AuthorizationProvider)manager.GetProvider(typeof(AuthorizationProvider));
        }
        private static AuthorizationProvider GetSecurityProvider(Federation federation, string ns)
        {
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, ns);
            return (AuthorizationProvider)manager.GetProvider(typeof(AuthorizationProvider));
        }



    }
}
