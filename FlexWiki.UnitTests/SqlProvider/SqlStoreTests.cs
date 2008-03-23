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
using System.Data; 

using NUnit.Framework;

using FlexWiki.Collections; 
using FlexWiki.SqlProvider; 

namespace FlexWiki.UnitTests.SqlProvider
{
    [TestFixture]
    public class SqlStoreTests
    {
        private readonly string _connectionString = "server=.;initial catalog=flexwiki"; 
        private MockDatabase _database; 
        private Federation _federation; 
        private SqlStore _provider;

        [SetUp]
        public void SetUp()
        {
            MockWikiApplication application = new MockWikiApplication(new FederationConfiguration(),
                new LinkMaker("test://SqlStoreTests/"), OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            _federation = new Federation(application);
            _database = new MockDatabase(TestDataSets.Default());

            _federation.RegisterNamespace(new SqlStore(_database), "NamespaceOne",
                new NamespaceProviderParameterCollection(
                    new NamespaceProviderParameter("ConnectionString", _connectionString)));

            // Necessary to bypass security because a non-existent manager can't be
            // retrieved directly from the federation
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(_federation, "NamespaceOne");

            _provider = (SqlStore)manager.GetProvider(typeof(SqlStore));
        }

        [Test]
        public void AllChangesForTopicSinceSimple()
        {
            TopicChangeCollection changes = _provider.AllChangesForTopicSince(
                new UnqualifiedTopicName("HomePage"), DateTime.MinValue);

            Assert.AreEqual(1, changes.Count, "Checking that the right number of changes were returned.");
            Assert.AreEqual(new DateTime(2003, 11, 24, 20, 31, 20), changes[0].Created,
                "Checking that the date on the change is correct");
            Assert.AreEqual(@"WINGROUP-davidorn", changes[0].Author,
                "Checking that the author of the change is correct.");
            Assert.AreEqual("NamespaceOne.HomePage", changes[0].DottedName,
                "Checking that dotted name is correct.");
            Assert.AreEqual(new DateTime(2003, 11, 24, 20, 31, 20), changes[0].Modified,
                "Checking that modification time is correct.");
            Assert.AreEqual(new DateTime(2003, 11, 24, 20, 31, 20), changes[0].Created,
                "Checking that creating time is correct.");
            Assert.AreEqual("NamespaceOne.HomePage(2003-11-24-20-31-20-WINGROUP-davidorn)",
                changes[0].TopicRevision.DottedNameWithVersion,
                "Checking that dotted name with version is correct.");

        }

        [Test]
        public void AllChangesForTopicSinceWithDate()
        {
            TopicChangeCollection changes = _provider.AllChangesForTopicSince(
                new UnqualifiedTopicName("CodeImprovementIdeas"),
                new DateTime(2004, 11, 07));

            Assert.AreEqual(3, changes.Count, "Checking that the right number of changes were returned.");

            Assert.AreEqual(new DateTime(2003, 11, 23, 14, 34, 03),
                changes.Latest.Modified,
                "Checking that the latest change is correct.");
            Assert.AreEqual(new DateTime(2003, 11, 23, 14, 34, 05, 100),
                changes.Oldest.Modified,
                "Checking that the oldest change is correct.");
        }

        [Test]
        public void AllChangesForTopicSinceLongHistory()
        {
            TopicChangeCollection changes = _provider.AllChangesForTopicSince(
                new UnqualifiedTopicName("CodeImprovementIdeas"), DateTime.MinValue);

            Assert.AreEqual(6, changes.Count, "Checking that the right number of changes were returned.");

            Assert.AreEqual(new DateTime(2003, 11, 23, 14, 34, 3, 0), changes[0].Created,
                "Checking that a version with a leading digit for a name works");
            Assert.AreEqual(new DateTime(2003, 11, 23, 14, 34, 4, 889), changes[1].Created,
                "Checking that a version with a large millisecond value works.");
            Assert.AreEqual(new DateTime(2003, 11, 23, 14, 34, 5, 100), changes[2].Created,
                "Checking that a version with milliseconds with an extra tail works.");
            Assert.AreEqual(new DateTime(2003, 11, 23, 14, 34, 6, 100), changes[3].Created,
                "Checking that a version with miliseconds with a leading digit for a name works.");
            Assert.AreEqual(new DateTime(2003, 11, 23, 14, 34, 7, 0), changes[4].Created,
                "Checking that a version with a simple name works.");
            Assert.AreEqual(new DateTime(2003, 11, 23, 14, 34, 8, 123), changes[5].Created,
                "Checking that a version with a simple name with milliseconds works.");
        }

        [Test]
        public void AllChangesForTopicSinceNoHistory()
        {
            TopicChangeCollection changes = _provider.AllChangesForTopicSince(new UnqualifiedTopicName("TopicOne"), DateTime.MinValue);

            Assert.AreEqual(0, changes.Count,
                "Checking that the absence of archive data breaks the change history");
        }

        [Test]
        public void AllTopics()
        {
            QualifiedTopicNameCollection topics = _provider.AllTopics();

            Assert.AreEqual(6, topics.Count, "Checking that the right number of topics was returned.");

            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.HomePage")),
                "Checking that HomePage is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.TopicOne")),
                "Checking that TopicOne is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.TopicTwo")),
                "Checking that TopicTwo is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.CodeImprovementIdeas")),
                "Checking that CodeImprovementIdeas is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.TestHistoryDelete")),
                "Checking that TestHistoryDelete is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.ReadOnlyTopic")),
                "Checking that ReadOnlyTopic is present.");
        }

        [Test]
        public void DeleteAllTopicsAndHistory()
        {
            int namespacesBefore = _database.NamespaceTable.Rows.Count; 

            _provider.DeleteAllTopicsAndHistory();

            Assert.AreEqual(0, _database.TopicTable.Rows.Count,
                "Checking that appropriate topic rows were deleted.");
            Assert.AreEqual(namespacesBefore - 1, _database.NamespaceTable.Rows.Count,
                "Checking that one namespace row was deleted."); 

        }

        [Test]
        public void DeleteTopic()
        {
            _provider.DeleteTopic(new UnqualifiedTopicName("HomePage"), false);

            Assert.IsFalse(RowExists("HomePage"),
                "Checking that tip row was deleted.");
            Assert.IsTrue(RowExists("HomePage(2003-11-24-20-31-20-WINGROUP-davidorn)"),
                "Checking that archive row was not deleted.");
        }

        [Test]
        public void DeleteTopicAndHistory()
        {
            _provider.DeleteTopic(new UnqualifiedTopicName("TestHistoryDelete"), true);

            Assert.IsFalse(RowExists("TestHistoryDelete"),
                "Checking that tip row was deleted.");
            Assert.IsFalse(RowExists("TestHistoryDelete(2003-11-23-14-34-03-127.0.0.1)"),
                "Checking that archive row was deleted.");
            Assert.IsFalse(RowExists("TestHistoryDelete(2003-11-23-14-34-06.1-127.0.0.1)"),
                "Checking that archive row was deleted.");
            Assert.IsFalse(RowExists("TestHistoryDelete(2003-11-23-14-34-07-Name)"),
                "Checking that archive row was deleted.");
            Assert.IsFalse(RowExists("TestHistoryDelete(2003-11-23-14-34-08.123-Name)"),
                "Checking that archive row was deleted.");
        }

        [Test]
        public void DeleteTopicNonexistentTopic()
        {
            _provider.DeleteTopic(new UnqualifiedTopicName("NoSuchTopic"), false);

            // Just need to check that no exception is thrown
        }

        [Test]
        public void ExistsNegative()
        {
            _provider.DeleteAllTopicsAndHistory(); 
            Assert.IsFalse(_provider.Exists, 
                "Checking that Exists returns false when the namespace does not exist in the namespace table.");
        }

        [Test]
        public void ExistsPositive()
        {
            Assert.IsTrue(_provider.Exists, "Checking that Exists returns true.");
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void GetParsedTopic()
        {
            _provider.GetParsedTopic(new UnqualifiedTopicRevision("HomePage"));
        }

        [Test]
        public void HasPermission()
        {
            Assert.IsTrue(_provider.HasPermission(
                new UnqualifiedTopicName("HomePage"), TopicPermission.Edit),
                "Checking that a read-write topic shows as writable.");
            Assert.IsTrue(_provider.HasPermission(
                new UnqualifiedTopicName("HomePage"), TopicPermission.Read),
                "Checking that a read-write topic shows as readable.");
            Assert.IsFalse(_provider.HasPermission(
                new UnqualifiedTopicName("ReadOnlyTopic"), TopicPermission.Edit),
                "Checking that a read-write topic shows as non-writable.");
            Assert.IsTrue(_provider.HasPermission(
                new UnqualifiedTopicName("ReadOnlyTopic"), TopicPermission.Read),
                "Checking that a read-only topic shows as readable.");
        }

        [Test]
        public void HasPermissionNonExistentTopic()
        {
            // It might seem a little weird to return true if the topic doesn't exist, but 
            // basically what we're saying is that there's no reason to deny read/edit
            Assert.IsTrue(_provider.HasPermission(new UnqualifiedTopicName("NoSuchTopic"), TopicPermission.Edit),
                "Checking that a nonexistent topic is reported as not writable.");
            Assert.IsTrue(_provider.HasPermission(new UnqualifiedTopicName("NoSuchTopic"), TopicPermission.Read),
                "Checking that a nonexistent topic is reported as not readable.");
        }

        [Test]
        public void IsReadOnly()
        {
            Assert.IsFalse(_provider.IsReadOnly,
                "Checking that the SQL provider always returns false for IsReadOnly.");
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void LockTopic()
        {
            // The SQL Provider does not implement LockTopic because the 1.8 SQL build doesn't have
            // anything that would set the writable bit, and we don't want to change the database at
            // all for 2.0. Future releases can implement it. 
            _provider.LockTopic(new UnqualifiedTopicName("TopicOne"));
        }

        [Test]
        public void TextReaderForTopic()
        {
            Assert.AreEqual(
                "Latest",
                _provider.TextReaderForTopic(new UnqualifiedTopicRevision("CodeImprovementIdeas")).ReadToEnd(),
                "Checking that topic content retrieved without version is correct.");

            Assert.AreEqual(
                "Older",
                _provider.TextReaderForTopic(new UnqualifiedTopicRevision("CodeImprovementIdeas",
                    "2003-11-23-14-34-04.8890-127.0.0.1")).ReadToEnd(),
                "Checking that topic content retrieved by revision is correct.");

            Assert.IsNull(
                _provider.TextReaderForTopic(new UnqualifiedTopicRevision("NoSuchTopic")),
                "Checking that a nonexistent topic returns a null reader.");
        }

        [Test]
        public void TopicExists()
        {
            Assert.IsFalse(_provider.TopicExists(new UnqualifiedTopicName("NoSuchTopic")),
                "Checking that a nonexistent topic returns false.");
            Assert.IsFalse(_provider.TopicExists(new UnqualifiedTopicName("DeletedTopic")),
                "Checking that a deleted topic returns false.");
            Assert.IsTrue(_provider.TopicExists(new UnqualifiedTopicName("HomePage")),
                "Checking that an existing topic returns true.");
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void UnlockTopic()
        {
            // The SQL Provider does not implement UnlockTopic because the 1.8 SQL build doesn't have
            // anything that would set the writable bit, and we don't want to change the database at
            // all for 2.0. Future releases can implement it. 
            _provider.UnlockTopic(new UnqualifiedTopicName("HomePage")); 
        }

        [Test]
        public void WriteTopicNoVersion()
        {
            _provider.WriteTopic(new UnqualifiedTopicRevision("HomePage"), "New home page content");

            Assert.AreEqual("New home page content",
                FindRowContents("HomePage"),
                "Checking that a write with no version changes the tip row.");
            Assert.AreEqual(1,
                _provider.AllChangesForTopicSince(new UnqualifiedTopicName("HomePage"), DateTime.MinValue).Count,
                "Checking that no new revisions were written");
        }


        [Test]
        public void WriteTopicByVersion()
        {
            _provider.WriteTopic(new UnqualifiedTopicRevision("HomePage", "2003-11-24-20-31-20-WINGROUP-davidorn"),
                "New historical content");

            Assert.AreEqual("New historical content",
                FindRowContents("HomePage(2003-11-24-20-31-20-WINGROUP-davidorn)"), 
                "Checking that the historical version was updated.");
            Assert.AreEqual("Home page.",
                FindRowContents("HomePage"),
                "Checking that the tip row was not changed.");
        }

        [Test]
        public void WriteTopicNewTopic()
        {
            _provider.WriteTopic(new UnqualifiedTopicRevision("NewTopic"), "New contents");

            Assert.AreEqual("New contents",
                FindRowContents("NewTopic"),
                "Checking that a new row was written.");

            foreach (DataRow row in _database.TopicTable.Rows)
            {
                if (row["Name"].ToString().StartsWith("NewTopic("))
                {
                    Assert.Fail("No historical revisions should have been written: " + row["Name"]);
                }
            }
        }

        private DataRow FindRow(string topicName)
        {
            foreach (DataRow row in _database.TopicTable.Rows)
            {
                if (row["Name"].Equals(topicName))
                {
                    return row;
                }
            }

            return null; 
        }

        private string FindRowContents(string topicName)
        {
            DataRow row = FindRow(topicName);

            if (row == null)
            {
                return null; 
            }

            return row["Body"].ToString(); 

        }


        private bool RowExists(string topicName)
        {
            return FindRow(topicName) != null; 
        }


    }
}
