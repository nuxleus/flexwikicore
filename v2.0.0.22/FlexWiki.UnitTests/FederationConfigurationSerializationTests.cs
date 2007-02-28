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
            doc.LoadXml(@"<configuration>
<authorization>
    <rule type='allow' action='ManageNamespace' principal='all' />
    <rule type='deny' action='Edit' principal='user:candera' />
</authorization>
</configuration>");

            XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            FederationConfiguration configuration = (FederationConfiguration)serializer.Deserialize(reader);

            Assert.AreEqual(2, configuration.AuthorizationRules.Count, "Checking that the right number of rules were read.");
            AssertRuleCorrect(new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.GenericAll), SecurityRulePolarity.Allow, 
                SecurityRuleScope.Wiki, SecurableAction.ManageNamespace, 0), configuration.AuthorizationRules[0], 
                "first rule");
            AssertRuleCorrect(new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.User, "candera"), SecurityRulePolarity.Deny,
                SecurityRuleScope.Wiki, SecurableAction.Edit, 0), configuration.AuthorizationRules[1],
                "second rule");

        }

        [Test]
        [Ignore("Not yet implemented")]
        public void SerializeAuthorization()
        {
            FederationConfiguration configuration = new FederationConfiguration();
            SecurityRule rule = new SecurityRule(new SecurityRuleWho(SecurityRuleWhoType.GenericAnonymous),
                SecurityRulePolarity.Allow, SecurityRuleScope.Wiki, SecurableAction.Edit, 0);
            configuration.AuthorizationRules.Add(new WikiAuthorizationRule(rule));

            XmlSerializer serializer = new XmlSerializer(typeof(FederationConfiguration));
            serializer.Serialize(Console.Out, configuration);

            Assert.Fail("Need to add real verification to this."); 
        }

        private void AssertRuleCorrect(SecurityRule expected, WikiAuthorizationRule actual, string message)
        {
            Assert.AreEqual(expected.Action, actual.Action, "Checking that action was correct for " + message);
            Assert.AreEqual(expected.Polarity, actual.Polarity, "Checking that polarity was correct for " + message);
            Assert.AreEqual(expected.Who.Who, actual.Who, "Checking that who was correct for " + message);
            Assert.AreEqual(expected.Who.WhoType, actual.WhoType, "Checking that who type was correct for " + message);
        }

    }
}
