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
using System.IO;

using NUnit.Framework;

using FlexWiki.Collections;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class FileSystemStoreTests
    {
        private Federation _federation;
        private MockFileSystem _fileSystem;
        private FileSystemStore _provider;

        private Federation Federation
        {
            get { return _federation; }
        }

        private string Root
        {
            get
            {
                return @"C:\flexwiki\namespaces\namespaceone";
            }
        }

        [SetUp]
        public void SetUp()
        {
            MockWikiApplication application = new MockWikiApplication(new FederationConfiguration(),
                new LinkMaker("test://FileSystemStoreTests/"), OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            _federation = new Federation(application);
            _fileSystem = new MockFileSystem(
                new MockDirectory(@"C:\",
                    new MockDirectory("flexwiki",
                        new MockDirectory("namespaces",
                            new MockDirectory("namespaceone",
                                new MockFile(@"TopicOne.wiki", new DateTime(2004, 10, 28), @"This is some content"),
                                new MockFile(@"TopicTwo.wiki", new DateTime(2004, 10, 29), @"This is some other content"),

                                new MockFile(@"HomePage.wiki", new DateTime(2004, 10, 30), @"Home page."),
                                new MockFile(@"HomePage(2003-11-24-20-31-20-WINGROUP-davidorn).awiki",
                                    new DateTime(2004, 10, 31), @"Old home page."),

                                new MockFile(@"CodeImprovementIdeas.wiki", new DateTime(2004, 11, 10), @"Latest"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-03-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 09), @"Latest"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-04.8890-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 08), @"Older"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-05.1000-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 07), @"Still older"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-06.1-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 06), @"Even older"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-07-Name).awiki",
                                    new DateTime(2004, 11, 05), @"Really old"),
                                new MockFile(@"CodeImprovementIdeas(2003-11-23-14-34-08.123-Name).awiki",
                                    new DateTime(2004, 11, 04), @"Oldest"),

                                new MockFile(@"TestDeleteHistory.wiki", new DateTime(2004, 11, 10), @"Latest"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-03-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 09), @"Latest"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-04.8890-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 08), @"Older"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-05.1000-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 07), @"Still older"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-06.1-127.0.0.1).awiki",
                                    new DateTime(2004, 11, 06), @"Even older"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-07-Name).awiki",
                                    new DateTime(2004, 11, 05), @"Really old"),
                                new MockFile(@"TestDeleteHistory(2003-11-23-14-34-08.123-Name).awiki",
                                    new DateTime(2004, 11, 04), @"Oldest"),

                                new MockFile(@"ReadOnlyTopic.wiki",
                                    new DateTime(2004, 11, 05), @"", MockTopicStorePermissions.ReadOnly),
                                new MockFile(@"ReadOnlyTopic(2004-11-05-00-00-00-Name).awiki",
                                    new DateTime(2004, 11, 05), @""),
                                new MockFile(@"ReadOnlyTopic2.wiki",
                                    new DateTime(2004, 11, 05), new DateTime(2007, 10, 22), @"", MockTopicStorePermissions.ReadOnly, true),
                                new MockFile(@"ReadWriteTopic.wiki",
                                    new DateTime(2004, 11, 05), new DateTime(2007, 10, 22), @"", MockTopicStorePermissions.ReadWrite, false),

                                new MockFile(@"DeletedTopic(2004-11-11-00-00-00-Name).awiki",
                                    new DateTime(2004, 11, 11), @"This topic was deleted.")
                             )
                         )
                     )
                 )
             );

            _federation.RegisterNamespace(new FileSystemStore(_fileSystem), "NamespaceOne",
                new NamespaceProviderParameterCollection(
                    new NamespaceProviderParameter("Root", Root)));

            // Necessary to bypass security because a non-existent manager can't be
            // retrieved directly from the federation
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(_federation, "NamespaceOne");

            _provider = (FileSystemStore)manager.GetProvider(typeof(FileSystemStore));
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

            Assert.AreEqual(1, changes.Count,
                "Checking that the absence of .awiki files doesn't break the change history");
            Assert.AreEqual(new DateTime(2004, 10, 28), changes[0].Created,
                "Checking that creation time is correct.");
            Assert.AreEqual(new DateTime(2004, 10, 28), changes[0].Modified,
                "Checking that modification time is correct.");
            Assert.AreEqual("", changes[0].Author,
                "Checking that author is blank when no history is available.");
        }

        [Test]
        public void AllTopics()
        {
            QualifiedTopicNameCollection topics = _provider.AllTopics();

            Assert.AreEqual(8, topics.Count, "Checking that the right number of topics was returned.");

            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.HomePage")),
                "Checking that HomePage is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.TopicOne")),
                "Checking that TopicOne is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.TopicTwo")),
                "Checking that TopicTwo is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.CodeImprovementIdeas")),
                "Checking that CodeImprovementIdeas is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.TestDeleteHistory")),
                "Checking that CodeImprovementIdeas is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.ReadOnlyTopic")),
                "Checking that ReadOnlyTopic is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.ReadWriteTopic")),
                "Checking that CodeImprovementIdeas is present.");
            Assert.IsTrue(topics.Contains(new QualifiedTopicName("NamespaceOne.ReadOnlyTopic2")),
                "Checking that ReadOnlyTopic is present.");
        }

        [Test]
        public void DeleteAllTopicsAndHistory()
        {
            string basedir = @"C:\flexwiki\namespaces\namespaceone";
            _provider.DeleteAllTopicsAndHistory();

            Assert.IsFalse(_fileSystem.DirectoryExists(basedir),
                "Checking that the base directory was deleted.");
        }

        [Test]
        public void DeleteAllTopicsAndHistoryWithExtraFiles()
        {
            string basedir = @"C:\flexwiki\namespaces\namespaceone";
            _fileSystem[basedir].Children.Add(
                new MockFile(@"foo.bar", DateTime.MinValue, ""));
            _provider.DeleteAllTopicsAndHistory();

            Assert.IsTrue(_fileSystem.DirectoryExists(basedir),
                "Checking that namespace directory was not deleted.");
            Assert.AreEqual(1, _fileSystem[basedir].Children.Count,
                "Checking that only one file remains.");
            Assert.AreEqual("foo.bar", _fileSystem[basedir].Children[0].Name,
                "Checking that the extra file was not deleted.");
        }

        [Test]
        public void DeleteTopic()
        {
            _provider.DeleteTopic(new UnqualifiedTopicName("HomePage"), false);

            Assert.IsFalse(_fileSystem.FileExists(Path.Combine(Root, "HomePage.wiki")),
                "Checking that tip file was deleted.");
            Assert.IsTrue(_fileSystem.FileExists(Path.Combine(Root, "HomePage(2003-11-24-20-31-20-WINGROUP-davidorn).awiki")),
                "Checking that revision file was not deleted.");
        }

        [Test]
        public void DeleteTopicAndHistory()
        {
            _provider.DeleteTopic(new UnqualifiedTopicName("TestHistoryDelete"), true);

            Assert.IsFalse(_fileSystem.FileExists(Path.Combine(Root, "TestHistoryDelete.wiki")),
                "Checking that tip file was deleted.");
            Assert.IsFalse(_fileSystem.FileExists(Path.Combine(Root, "TestHistoryDelete(2003-11-23-14-34-03-127.0.0.1).awiki")),
                "Checking that revision file was deleted.");
            Assert.IsFalse(_fileSystem.FileExists(Path.Combine(Root, "TestHistoryDelete(2003-11-23-14-34-04.8890-127.0.0.1).awiki")),
                "Checking that revision file was deleted.");
            Assert.IsFalse(_fileSystem.FileExists(Path.Combine(Root, "TestHistoryDelete(2003-11-23-14-34-05.1000-127.0.0.1).awiki")),
                "Checking that revision file was deleted.");
            Assert.IsFalse(_fileSystem.FileExists(Path.Combine(Root, "TestHistoryDelete(2003-11-23-14-34-06.1-127.0.0.1).awiki")),
                "Checking that revision file was deleted.");
            Assert.IsFalse(_fileSystem.FileExists(Path.Combine(Root, "TestHistoryDelete(2003-11-23-14-34-07-Name).awiki")),
                "Checking that revision file was deleted.");
            Assert.IsFalse(_fileSystem.FileExists(Path.Combine(Root, "TestHistoryDelete(2003-11-23-14-34-08.123-Name).awiki")),
                "Checking that revision file was deleted.");
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
            MockWikiApplication application = new MockWikiApplication(new FederationConfiguration(),
                new LinkMaker("test://FileSystemStoreTests/"), OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            Federation federation = new Federation(application);
            MockFileSystem fileSystem = new MockFileSystem(new MockDirectory(@"C:\"));

            federation.RegisterNamespace(new FileSystemStore(fileSystem), "NamespaceOne",
                new NamespaceProviderParameterCollection(
                    new NamespaceProviderParameter("Root", Root)));

            // Necessary to bypass security because a non-existent manager can't be
            // retrieved directly from the federation
            NamespaceManager manager = WikiTestUtilities.GetNamespaceManagerBypassingSecurity(federation, "NamespaceOne");

            FileSystemStore provider = (FileSystemStore)manager.GetProvider(typeof(FileSystemStore));

            // Need to delete the directory, because registration creates it if it doesn't alreayd exist.
            fileSystem.DeleteDirectory(Root);

            Assert.IsFalse(provider.Exists, "Checking that Exists returns false when the root directory does not exist.");
        }

        [Test]
        public void ExistsPositive()
        {
            Assert.IsTrue(_provider.Exists, "Checking that Exists returns true.");
        }

        [Test]
        public void EnsureContentDirectoryIsMadeIfAbsent()
        {
            MockWikiApplication application = new MockWikiApplication(new FederationConfiguration(),
                new LinkMaker("test://FileSystemStoreTests/"), OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            Federation federation = new Federation(application);
            MockFileSystem fileSystem = new MockFileSystem();

            federation.RegisterNamespace(new FileSystemStore(fileSystem), "NamespaceOne",
                new NamespaceProviderParameterCollection(
                    new NamespaceProviderParameter("Root", Root)));

            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsTrue(fileSystem.DirectoryExists(Root));
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
                "Checking that a nonexistent topic is reported as writable.");
            Assert.IsTrue(_provider.HasPermission(new UnqualifiedTopicName("NoSuchTopic"), TopicPermission.Read),
                "Checking that a nonexistent topic is reported as readable.");
        }

        [Test]
        public void IsReadOnly()
        {
            Assert.IsFalse(_provider.IsReadOnly,
                "Checking that the filesystem provider always returns false for IsReadOnly.");
        }

        [Test]
        public void LockTopic()
        {
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanRead,
                "Checking that file starts out readable.");
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanWrite,
                "Checking that file starts out writable.");
            _provider.LockTopic(new UnqualifiedTopicName("TopicOne"));
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanRead,
                "Checking that file is still readable after a call to LockTopic");
            Assert.IsFalse(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanWrite,
                "Checking that file was set non-writable by a call to LockTopic");
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LockTopicNonexistentTopic()
        {
            _provider.LockTopic(new UnqualifiedTopicName("NoSuchTopic"));
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
        public void TopicIsReadOnly()
        {
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "ReadOnlyTopic2.wiki")].CanRead,
                "Checking that file starts out readable.");
            Assert.IsFalse(_fileSystem[Path.Combine(Root, "ReadOnlyTopic2.wiki")].CanWrite,
                "Checking that file starts not writable.");
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "ReadOnlyTopic2.wiki")].IsReadOnly,
                "Checking that file starts as read-only.");
        }

        [Test]
        public void TopisIsReadOnlyNegative()
        {
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "ReadWriteTopic.wiki")].CanRead,
                "Checking that file starts out readable");
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "ReadWriteTopic.wiki")].CanWrite,
                "Checking that file starts out writable.");
            Assert.IsFalse(_fileSystem[Path.Combine(Root, "ReadWriteTopic.wiki")].IsReadOnly,
                "Checking that file is starts as read-write.");
        }

        [Test]
        public void TopicIsReadOnlyLockTopic()
        {
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanRead,
                "Checking that file starts out readable.");
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanWrite,
                "Checking that file starts out writable.");
            Assert.IsFalse(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].IsReadOnly,
                "Checking that file starts as read-write.");
            _provider.LockTopic(new UnqualifiedTopicName("TopicOne"));
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanRead,
                "Checking that file is still readable after a call to LockTopic");
            Assert.IsFalse(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanWrite,
                "Checking that file was set non-writable by a call to LockTopic");
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].IsReadOnly,
                "Checking that file is now read-only.");
        }

        [Test]
        public void TopicIsReadOnlyUnlockTopic()
        {
            _provider.LockTopic(new UnqualifiedTopicName("TopicOne"));
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanRead,
                "Checking that file starts out readable");
            Assert.IsFalse(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanWrite,
                "Checking that file starts out non-writable");
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].IsReadOnly,
                "Checking that file starts as read-only.");
            _provider.UnlockTopic(new UnqualifiedTopicName("TopicOne"));
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanRead,
                "Checking that file ends up readable");
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanWrite,
                "Checking that file ends up writable");
            Assert.IsFalse(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].IsReadOnly,
                "Checking that file starts is now read-write.");
        }

        [Test]
        public void UnlockTopic()
        {
            _provider.LockTopic(new UnqualifiedTopicName("TopicOne"));
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanRead,
                "Checking that file starts out readable");
            Assert.IsFalse(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanWrite,
                "Checking that file starts out non-writable");
            _provider.UnlockTopic(new UnqualifiedTopicName("TopicOne"));
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanRead,
                "Checking that file ends up readable");
            Assert.IsTrue(_fileSystem[Path.Combine(Root, "TopicOne.wiki")].CanWrite,
                "Checking that file ends up writable");
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void UnlockTopicNonexistentTopic()
        {
            _provider.UnlockTopic(new UnqualifiedTopicName("NoSuchTopic"));
        }

        [Test]
        public void WriteTopicNoVersion()
        {
            _provider.WriteTopic(new UnqualifiedTopicRevision("HomePage"), "New home page content");

            Assert.AreEqual("New home page content",
                _fileSystem[Path.Combine(Root, "HomePage.wiki")].Contents,
                "Checking that a write with no version changes the .wiki file.");
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
                _fileSystem[Path.Combine(Root, "HomePage(2003-11-24-20-31-20-WINGROUP-davidorn).awiki")].Contents,
                "Checking that the historical version was updated.");
            Assert.AreEqual("Home page.",
                _fileSystem[Path.Combine(Root, "HomePage.wiki")].Contents,
                "Checking that the tip file was not changed.");
        }

        [Test]
        public void WriteTopicNewTopic()
        {
            _provider.WriteTopic(new UnqualifiedTopicRevision("NewTopic"), "New contents");

            Assert.AreEqual("New contents",
                _fileSystem[Path.Combine(Root, "NewTopic.wiki")].Contents,
                "Checking that a new file was written.");

            foreach (MockFileInformation file in _fileSystem.GetFiles(Root, "*.awiki"))
            {
                if (file.Name.StartsWith("NewTopic("))
                {
                    Assert.Fail("No historical files should have been written: " + file.Name);
                }
            }
        }

    }
}
