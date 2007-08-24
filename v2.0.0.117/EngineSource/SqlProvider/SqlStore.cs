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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using FlexWiki;
using FlexWiki.Collections; 

namespace FlexWiki.SqlProvider
{
    /// <summary>
    /// Summary description for SqlStore.
    /// </summary>
    public class SqlStore : ContentProviderBase
    {
        private IDatabase _database;
        private SqlHelper _sqlHelper;

        private static Regex s_topicNameRegex = new Regex("[^(]+\\((?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?\\)");

        public SqlStore() : this(new SqlDatabase())
        {
        }

        public SqlStore(IDatabase database) : base(null)
        {
            _database = database;
            _sqlHelper = new SqlHelper(database); 
        }

        public override bool Exists
        {
            get { return _sqlHelper.NamespaceExists(Namespace); }
        }
        public override bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Answer an (unsorted) enumeration of all topic in the ContentProviderChain (doesn't include imports)
        /// </summary>
        /// <returns>Enumeration of AbsoluteTopicNames</returns>
        public override QualifiedTopicNameCollection AllTopics()
        {
            QualifiedTopicNameCollection results = new QualifiedTopicNameCollection();
            SqlInfoForTopic[] topicInfos = _sqlHelper.GetSqlTopicInfoForNonArchiveTopics(Namespace, false);

            foreach (SqlInfoForTopic topicInfo in topicInfos)
            {
                results.Add(new QualifiedTopicName(topicInfo.Name, Namespace));
            }

            return results; 
        }
        /// <summary>
        /// A list of TopicChanges to a topic since a given date [sorted by date]
        /// </summary>
        /// <param name="topic">A given date</param>
        /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
        /// <param name="rule">A composite cache rule to fill with rules that represented accumulated dependencies (or null)</param>
        /// <returns>Enumeration of TopicChanges</returns>
        public override TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            TopicChangeCollection answer = new TopicChangeCollection();
            SqlInfoForTopic[] infos = _sqlHelper.GetSqlTopicInfosForTopicSince(Namespace, topic.LocalName, stamp);
            ArrayList sortable = new ArrayList();
            foreach (SqlInfoForTopic each in infos)
            {
                sortable.Add(new SqlInfoTopicData(each, Namespace));
            }
            sortable.Sort(new TimeSort());

            foreach (TopicData each in sortable)
            {
                if (each.LastModificationTime < stamp)
                {
                    continue;
                }
                QualifiedTopicRevision name = new QualifiedTopicRevision(topic.LocalName, Namespace);
                name.Version = each.Version;
                TopicChange change = TopicChangeFromName(name);
                answer.Add(change);
            }
            return answer;

        }
        ///// <summary>
        ///// Rename the given topic.  If requested, find references and fix them up.  Answer a report of what was fixed up.  Throw a DuplicationTopicException
        ///// if the new name is the name of a topic that already exists.
        ///// </summary>
        ///// <param name="oldName">Old topic name</param>
        ///// <param name="newName">The new name</param>
        ///// <param name="fixup">true to fixup referenced topic *in this namespace*; false to do no fixups</param>
        ///// <returns>ArrayList of strings that can be reported back to the user of what happened during the fixup process</returns>
        //ArrayList IContentProvider.RenameTopic(LocalTopicName oldName, string newName, bool fixup, string author)
        //{
        //    //FederationUpdateGenerator gen = CreateFederationUpdateGenerator();

        //    // TRIGGER
        //    ArrayList answer = new ArrayList();
        //    string currentTopicName = oldName.Name;
        //    string newTopicName = newName;
        //    AbsoluteTopicName newFullName = new AbsoluteTopicName(newName, Namespace);

        //    // Make sure it's not goign to overwrite an existing topic
        //    if (StoreManager.TopicExists(new LocalTopicName(newName), ImportPolicy.DoNotIncludeImports))
        //    {
        //        throw DuplicateTopicException.ForTopic(newFullName);
        //    }

        //    // If the topic does not exist (e.g., it's a backing topic), don't bother...
        //    if (!TipTopicRecordExists(oldName.Name))
        //    {
        //        answer.Add("This topic can not be renamed (it is probably a backing topic).");
        //        return answer;
        //    }

        //    try
        //    {
        //        // TODO: Renable when updates get figured out
        //        //gen.Push();

        //        // Rename the archive files, too
        //        foreach (SqlInfoForTopic each in SqlTopicInfosForTopic(oldName))
        //        {
        //            AbsoluteTopicName newNameForThisVersion = new AbsoluteTopicName(newName, Namespace);
        //            newNameForThisVersion.Version = ExtractVersionFromTopicName(each.Name);

        //            AbsoluteTopicName oldNameForThisVersion = new AbsoluteTopicName(oldName.Name, Namespace);
        //            oldNameForThisVersion.Version = newNameForThisVersion.Version;

        //            SqlHelper.RenameTopic(Namespace, each.Name, MakeTopicName(newNameForThisVersion.LocalName), _connectionString);

        //            // record changes (a delete for the old one and an add for the new one)
        //            // TODO: Renable when updates get figured out
        //            //gen.RecordCreatedTopic(newNameForThisVersion);
        //            //gen.RecordDeletedTopic(oldNameForThisVersion);
        //        }

        //        // Rename the topic file
        //        SqlHelper.RenameTopic(Namespace, currentTopicName, newTopicName, _connectionString);

        //        // Record changes (a delete for the old one and an add for the new one)
        //        // TODO: Renable when updates get figured out
        //        //        gen.RecordCreatedTopic(newFullName);
        //        //				gen.RecordDeletedTopic(oldName.AsAbsoluteTopicName(Namespace));

        //        // Now get ready to do fixups
        //        if (!fixup)
        //            return answer;

        //        // OK, we need to do the hard work
        //        AbsoluteTopicName oldabs = oldName.AsAbsoluteTopicName(Namespace);
        //        AbsoluteTopicName newabs = new AbsoluteTopicName(newName, oldabs.Namespace);

        //        // Now the master loop
        //        foreach (AbsoluteTopicName topic in StoreManager.AllTopics(ImportPolicy.DoNotIncludeImports))
        //        {
        //            if (StoreManager.RenameTopicReferences(topic.LocalName, oldabs, newabs, author))
        //            {
        //                answer.Add("Found and replaced references in " + topic);
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        // TODO: Renable when updates get figured out
        //        //gen.Pop();
        //    }

        //    return answer;
        //}

        /// Delete a content base (kills everything inside recursively).  Note that this does *not* include unregistering
        /// the content base within the federation.
        /// </summary>
        public override void DeleteAllTopicsAndHistory()
        {
            _sqlHelper.DeleteNamespace(Namespace);
            //OnFederationUpdated(new FederationUpdateEventArgs(update));
        }
        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        public override void DeleteTopic(UnqualifiedTopicName topic)
        {
            if (!_sqlHelper.TopicExists(Namespace, topic.LocalName))
            {
                return;
            }

            _sqlHelper.DeleteTopic(Namespace, topic.LocalName);
        }
        public override ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            throw new NotImplementedException(); 
        }
        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            if (!_sqlHelper.TopicExists(Namespace, topic.LocalName))
            {
                // It might seem a little weird to return true if the topic doesn't exist, but 
                // basically what we're saying is that there's no reason to deny read/edit
                return true;
            }

            if (permission == TopicPermission.Read)
            {
                return true; 
            }
            else if (permission == TopicPermission.Edit)
            {
                return _sqlHelper.IsExistingTopicWritable(Namespace, topic.LocalName);
            }
            else
            {
                throw new ArgumentException("Unrecognized topic permission: " + permission.ToString()); 
            }
        }
        public override void Initialize(NamespaceManager namespaceManager)
        {
            base.Initialize(namespaceManager);
            
            bool connectionStringAbsent = false; 
            if (!NamespaceManager.Parameters.Contains(ConfigurationParameterNames.ConnectionString))
            {
                connectionStringAbsent = true; 
            }
            else if (string.IsNullOrEmpty(NamespaceManager.Parameters[ConfigurationParameterNames.ConnectionString].Value))
            {
                connectionStringAbsent = true; 
            }

            if (connectionStringAbsent)
            {
                throw new FlexWikiException("SqlStore requires that the ConnectionString parameter be specified."); 
            }

            _database.ConnectionString = NamespaceManager.Parameters[ConfigurationParameterNames.ConnectionString].Value;
        }
        public override void LockTopic(UnqualifiedTopicName topic)
        {
            throw new NotImplementedException("Not implemented in this version of FlexWiki. Future releases may support locking topics in SQL stores."); 
        }
        /// <summary>
        /// Answer true if a topic exists in this ContentProviderChain
        /// </summary>
        /// <param name="name">Name of the topic</param>
        /// <returns>true if it exists</returns>
        public override bool TopicExists(UnqualifiedTopicName name)
        {
            return _sqlHelper.TopicExists(Namespace, MakeTopicName(name));
        }
        /// <summary>
        /// Answer a TextReader for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
        /// <returns>TextReader</returns>
        public override TextReader TextReaderForTopic(UnqualifiedTopicRevision revision)
        {
            string topicName = MakeTopicName(revision);
            if (topicName == null || !_sqlHelper.TopicExists(Namespace, topicName))
            {
                return null; 
            }
            return new StringReader(_sqlHelper.GetTopicBody(Namespace, topicName));
        }
        public override void UnlockTopic(UnqualifiedTopicName topic)
        {
            throw new NotImplementedException("Not implemented in this version of FlexWiki. Future releases may support locking topics in SQL stores."); 
        }
        /// <summary>
        /// Write new contents to a topic revision (doesn't write a new version).  
        /// </summary>
        /// <param name="revision">Revision to write</param>
        /// <param name="content">New content</param>
        public override void WriteTopic(UnqualifiedTopicRevision revision, string content)
        {
            string topicName = MakeTopicName(revision);
            bool isArchive = (revision.Version != null) && (revision.Version.Length > 0);

            DateTime lastWriteTime = Federation.TimeProvider.Now;

            _sqlHelper.WriteTopic(Namespace, topicName, lastWriteTime, content, isArchive);
        }

        private static string MakeTopicName(UnqualifiedTopicName topic)
        {
            return MakeTopicName(new UnqualifiedTopicRevision(topic));
        }
        private static string MakeTopicName(UnqualifiedTopicRevision revision)
        {
            return revision.DottedNameWithVersion;
        }
        private bool TipTopicRecordExists(string topicName)
        {
            return _sqlHelper.TopicExists(Namespace, MakeTopicName(new UnqualifiedTopicRevision(topicName)));
        }
        private static TopicChange TopicChangeFromName(QualifiedTopicRevision topic)
        {
            try
            {
                VersionInfo versionInfo = TopicRevision.ParseVersion(topic.Version);
                TopicChange change = new TopicChange(topic, versionInfo.Timestamp, versionInfo.Author);

                return change; 
            }
            catch (Exception ex)
            {
                throw new Exception("Exception processing topic change " + topic.Version + " - " + ex.ToString());
            }
        }

    }
}
