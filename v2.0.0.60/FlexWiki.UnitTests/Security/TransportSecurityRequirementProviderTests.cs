using System;
using System.Collections.Generic; 

using NUnit.Framework;

using FlexWiki.Security; 

namespace FlexWiki.UnitTests.Security
{
    [TestFixture]
    public class TransportSecurityRequirementProviderTests
    {
        private delegate void TestOperation(TestParameters parameters); 

        private class TestParameters
        {
            private MockWikiApplication _application; 
            private Federation _federation;
            private NamespaceManager _namespaceManager;
            private TransportSecurityRequirementProvider _provider;

            public TestParameters(Federation federation, NamespaceManager namespaceManager,
                MockWikiApplication application, TransportSecurityRequirementProvider provider)
            {
                _federation = federation;
                _namespaceManager = namespaceManager;
                _application = application;
                _provider = provider; 
            }

            public MockWikiApplication Application
            {
                get { return _application; }
                set { _application = value; }
            }

            public Federation Federation
            {
                get { return _federation; }
                set { _federation = value; }
            }

            public NamespaceManager NamespaceManager
            {
                get { return _namespaceManager; }
                set { _namespaceManager = value; }
            }

            public TransportSecurityRequirementProvider Provider
            {
                get { return _provider; }
                set { _provider = value; }
            }
        }

        [Test]
        public void AllChangesForTopicSince()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                parameters.Provider.AllChangesForTopicSince(new UnqualifiedTopicName("TopicOne"), DateTime.MinValue);
            }
            );
        }

        [Test]
        public void AllTopics()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                parameters.Provider.AllTopics(); 
            }
            );
        }

        [Test]
        public void DeleteAllTopicsAndHistory()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                parameters.Provider.DeleteAllTopicsAndHistory();
            }
            );
        }

        [Test]
        public void DeleteTopic()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                parameters.Provider.DeleteTopic(new UnqualifiedTopicName("TopicOne"));
            }
            );
        }

        [Test]
        public void Exists()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                bool exists = parameters.Provider.Exists;
            }
            );
        }

        [Test]
        public void GetParsedTopic()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                parameters.Provider.GetParsedTopic(new UnqualifiedTopicRevision("TopicOne"));
            }
            );
        }

        [Test]
        public void HasPermission()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                parameters.Provider.HasPermission(new UnqualifiedTopicName("TopicOne"), TopicPermission.Read);
            }
            );
        }

        [Test]
        public void IsReadOnly()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                bool isReadOnly = parameters.Provider.IsReadOnly;
            }
            );
        }

        [Test]
        public void LockTopic()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                parameters.Provider.LockTopic(new UnqualifiedTopicName("TopicOne"));
            }
            );
        }

        [Test]
        public void TextReaderForTopic()
        {
            TestAllPossibleConfigurations(true, delegate(TestParameters parameters)
            {
                parameters.Provider.TextReaderForTopic(new UnqualifiedTopicRevision("TopicOne"));
            }
            );
        }

        [Test]
        public void TopicExists()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                parameters.Provider.TopicExists(new UnqualifiedTopicName("TopicOne"));
            }
            );
        }

        [Test]
        public void UnlockTopic()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                parameters.Provider.UnlockTopic(new UnqualifiedTopicName("TopicOne"));
            }
            );
        }

        [Test]
        public void WriteTopic()
        {
            TestAllPossibleConfigurations(delegate(TestParameters parameters)
            {
                parameters.Provider.WriteTopic(new UnqualifiedTopicRevision("TopicOne"), "New content");
            }
            );
        }


        private static TestParameters Initialize(TransportSecurityRequirementTestConfiguration configuration)
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://TransportSecurityRequirementProviderTests",
                new TestContentSet(new TestNamespace("NamespaceOne")));
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");
            manager.WriteTopicAndNewVersion("TopicOne", "Test content", "test");

            MockWikiApplication application = federation.Application as MockWikiApplication;

            // We start out "true" so we don't trip any assertions while writing out the new content
            application.IsTransportSecure = true; 

            if (configuration.WikiTransportSecurityRequirement == TransportSecurityRequirement.RequiredOnContent)
            {
                federation.Configuration.RequireTransportSecurityFor = TransportSecurityRequiredFor.Content; 
            }
            else if (configuration.WikiTransportSecurityRequirement == TransportSecurityRequirement.RequiredOnNone)
            {
                federation.Configuration.RequireTransportSecurityFor = TransportSecurityRequiredFor.None; 
            }

            if (configuration.NamespaceTransportSecurityRequirement == TransportSecurityRequirement.RequiredOnContent)
            {
                manager.WriteTopicAndNewVersion(manager.DefinitionTopicName.LocalName,
                    TransportSecurityRequirementProvider.RequirementPropertyName + 
                    ": " + 
                    TransportSecurityRequiredFor.Content.ToString(), 
                    "test"); 
            }
            else if (configuration.NamespaceTransportSecurityRequirement == TransportSecurityRequirement.RequiredOnNone)
            {
                manager.WriteTopicAndNewVersion(manager.DefinitionTopicName.LocalName,
                    TransportSecurityRequirementProvider.RequirementPropertyName + 
                    ": " + 
                    TransportSecurityRequiredFor.None.ToString(), 
                    "test"); 
            }

            // We set it back to whatever it should be now that we're done modifying the wiki.
            application.IsTransportSecure = configuration.IsTransportSecure; 

            TransportSecurityRequirementProvider provider = 
                (TransportSecurityRequirementProvider) manager.GetProvider(typeof(TransportSecurityRequirementProvider)); 

            return new TestParameters(federation, manager, application, provider); 
        }
        private static IEnumerable<TransportSecurityRequirementTestConfiguration> PossibleConfigurations()
        {
            TransportSecurityRequirement[] wikiRequirements = new TransportSecurityRequirement[] { 
                TransportSecurityRequirement.RequiredOnNone, 
                TransportSecurityRequirement.RequiredOnContent,
            };

            TransportSecurityRequirement[] namespaceRequirements = new TransportSecurityRequirement[] { 
                TransportSecurityRequirement.Unspecified,
                TransportSecurityRequirement.RequiredOnNone, 
                TransportSecurityRequirement.RequiredOnContent,
            };

            bool[] transportIsSecureValues = new bool[] { true, false };

            foreach (TransportSecurityRequirement wikiRequirement in wikiRequirements)
            {
                foreach (TransportSecurityRequirement namespaceRequirement in namespaceRequirements)
                {
                    foreach (bool transportIsSecureValue in transportIsSecureValues)
                    {
                        yield return new TransportSecurityRequirementTestConfiguration(wikiRequirement, namespaceRequirement, transportIsSecureValue);
                    }
                }
            }
        }
        private static void TestAllPossibleConfigurations(TestOperation operation)
        {
            TestAllPossibleConfigurations(false, operation);
        }
        private static void TestAllPossibleConfigurations(bool exceptionsExpected, TestOperation operation)
        {
            foreach (TransportSecurityRequirementTestConfiguration configuration in PossibleConfigurations())
            {
                TestParameters parameters = Initialize(configuration);

                bool wasExceptionThrown = false;
                try
                {
                    operation(parameters);
                }
                catch (TransportSecurityRequirementException)
                {
                    wasExceptionThrown = true;
                }

                if (exceptionsExpected)
                {
                    Assert.AreEqual(configuration.ShouldExceptionBeThrown, wasExceptionThrown,
                        "Checking that TransportSecurityRequirementException was thrown if it should have been.");
                }
                else
                {
                    Assert.IsFalse(wasExceptionThrown,
                        "Checking that no TransportSecurityRequirementException was thrown."); 
                }
            }
        }

    }
}
