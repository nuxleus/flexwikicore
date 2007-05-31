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
using System.Xml;
using System.Xml.Serialization;

using FlexWiki.Security; 

using NUnit.Framework; 

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class FederationConfigurationSerializationTests
    {
        [Test]
        public void DeserializeAuthorization()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FederationConfiguration));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"<FederationConfiguration>
<AuthorizationRules>
    <Rule Type='Allow' Action='ManageNamespace' Principal='all' />
    <Rule Type='Deny' Action='Edit' Principal='user:candera' />
</AuthorizationRules>
</FederationConfiguration>");

            XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            FederationConfiguration configuration = (FederationConfiguration)serializer.Deserialize(reader);

            Assert.AreEqual(2, configuration.AuthorizationRules.Count, "Checking that the right number of rules were read.");
            AssertRuleCorrect(new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.GenericAll), AuthorizationRulePolarity.Allow, 
                AuthorizationRuleScope.Wiki, SecurableAction.ManageNamespace, 0), configuration.AuthorizationRules[0], 
                "first rule");
            AssertRuleCorrect(new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.User, "candera"), AuthorizationRulePolarity.Deny,
                AuthorizationRuleScope.Wiki, SecurableAction.Edit, 0), configuration.AuthorizationRules[1],
                "second rule");

        }

        [Test]
        [Ignore("Not yet implemented")]
        public void SerializeAuthorization()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            AuthorizationRule rule = new AuthorizationRule(new AuthorizationRuleWho(AuthorizationRuleWhoType.GenericAnonymous),
                AuthorizationRulePolarity.Allow, AuthorizationRuleScope.Wiki, SecurableAction.Edit, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(rule));

            XmlSerializer serializer = new XmlSerializer(typeof(FederationConfiguration));
            serializer.Serialize(Console.Out, configuration);

            Assert.Fail("Need to add real verification to this."); 
        }

        private void AssertRuleCorrect(AuthorizationRule expected, WikiAuthorizationRule actual, string message)
        {
            Assert.AreEqual(expected.Action, actual.Action, "Checking that action was correct for " + message);
            Assert.AreEqual(expected.Polarity, actual.Polarity, "Checking that polarity was correct for " + message);
            Assert.AreEqual(expected.Who.Who, actual.Who, "Checking that who was correct for " + message);
            Assert.AreEqual(expected.Who.WhoType, actual.WhoType, "Checking that who type was correct for " + message);
        }

    }
}
