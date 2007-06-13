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
        private string _connectionString;

        private static Regex s_topicNameRegex = new Regex("[^(]+\\((?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?\\)");

        public SqlStore() : base(null)
        {

        }

        public override bool Exists
        {
            get { return SqlHelper.NamespaceExists(Namespace, _connectionString); }
        }
        public override bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Answer an (unsorted) enumeration of all topic in the ContentProviderChain (doesn't include imports)
        /// </summary>
        /// <returns>Enumeration of AbsoluteTopicNames</returns>
        public override QualifiedTopicNameCollection AllTopics()
        {
            return AllTopicsSorted(null);
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
            SqlInfoForTopic[] infos = SqlHelper.GetSqlTopicInfosForTopicSince(Namespace, topic.LocalName, stamp, _connectionString);
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
            SqlHelper.DeleteNamespace(Namespace, _connectionString);
            //OnFederationUpdated(new FederationUpdateEventArgs(update));
        }
        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        public override void DeleteTopic(UnqualifiedTopicName topic)
        {
            if (!SqlHelper.TopicExists(Namespace, topic.LocalName, _connectionString))
            {
                return;
            }

            SqlHelper.DeleteTopic(Namespace, topic.LocalName, _connectionString);

            //      // Fire the event
            //      FederationUpdate update = new FederationUpdate();
            //      update.RecordDeletedTopic(topic.AsAbsoluteTopicName(Namespace));
            //      OnFederationUpdated(new FederationUpdateEventArgs(update));

        }
        //public static string ExtractNameFromTopicName(string topicName)
        //{
        //    int p = topicName.IndexOf("(");
        //    if (p == -1)
        //        return topicName;
        //    return topicName.Substring(0, p);
        //}
        //public static string ExtractVersionFromTopicName(string topicName)
        //{
        //    // ab(xyz)
        //    // 
        //    int p = topicName.IndexOf("(");
        //    if (p == -1)
        //        return topicName;
        //    int close = topicName.LastIndexOf(")");
        //    return topicName.Substring(p + 1, close - p - 1);
        //}
        /// <summary>
        /// Answer whether a topic exists and is writable
        /// </summary>
        /// <param name="topic">The topic (must directly be in this content base)</param>
        /// <returns>true if the topic exists AND is writable by the current user; else false</returns>
        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            throw new NotImplementedException();
            //if (!SqlHelper.TopicExists(Namespace, topic.LocalName, _connectionString))
            //{
            //    return false;
            //}

            //return SqlHelper.IsExistingTopicWritable(Namespace, topic.LocalName, _connectionString);
        }
        public override void Initialize(NamespaceManager namespaceManager)
        {
            base.Initialize(namespaceManager); 
            _connectionString = NamespaceManager.Parameters["Connection String"].Value;
        }
        /// <summary>
        /// Answer true if a topic exists in this ContentProviderChain
        /// </summary>
        /// <param name="name">Name of the topic</param>
        /// <returns>true if it exists</returns>
        public override bool TopicExists(UnqualifiedTopicName name)
        {
            return SqlHelper.TopicExists(Namespace, MakeTopicName(name), _connectionString);
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
            if (topicName == null || !SqlHelper.TopicExists(Namespace, topicName, _connectionString))
            {
                throw TopicNotFoundException.ForTopic(revision, Namespace);
            }
            return new StringReader(SqlHelper.GetTopicBody(Namespace, topicName, _connectionString));
        }


        /// <summary>
        /// Answer an enumeration of all topic in the ContentProviderChain, sorted using the supplied IComparer (does not include imports)
        /// </summary>
        /// <param name="comparer">Used to sort the topics in the answer</param>
        /// <returns>Enumeration of AbsoluteTopicNames</returns>
        private QualifiedTopicNameCollection AllTopicsSorted(IComparer comparer)
        {
            throw new NotImplementedException();

            //ArrayList answer = new ArrayList();
            //ArrayList all = new ArrayList();
            //Set present = new Set();
            //foreach (SqlInfoForTopic each in SqlHelper.GetSqlTopicInfoForNonArchiveTopics(Namespace, comparer != null, _connectionString))
            //{
            //    SqlInfoTopicData td = new SqlInfoTopicData(each, Namespace);
            //    all.Add(td);
            //    present.Add(td.Name);
            //}
            //bool sortAgain = false;
            //foreach (BackingTopic each in StoreManager.BackingTopics.Values)
            //{
            //    BackingTopicData td = new BackingTopicData(each);
            //    if (present.Contains(td.Name))
            //        continue;
            //    sortAgain = true;
            //    all.Add(td);
            //}
            //if (comparer != null && sortAgain)
            //    all.Sort(comparer);
            //foreach (TopicData each in all)
            //{
            //    AbsoluteTopicName name = new AbsoluteTopicName(each.Name, each.Namespace);
            //    answer.Add(name);
            //}
            //return answer;
        }
        /// <summary>
        /// We need to extract the time from the Topic Name so as to do everything
        /// to avoid descrepancy between the time stamp in the topic name and Sql 
        /// the time stamp. The reason is that Sql supports only 1/300th of a second
        /// accuracy as opposed to 1/10000th of second accuracy in the topic name.
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        private DateTime LastWriteTime(string topicName)
        {
            int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, millisecond = 0;

            Match m = s_topicNameRegex.Match(topicName);
            if (!m.Success)
            {
                return DateTime.Now;
            }
            if (m.Groups["year"].Captures.Count == 0)
            {
                year = DateTime.Now.Year;
            }
            else
            {
                year = Int32.Parse(m.Groups["year"].Value);
            }
            if (m.Groups["month"].Captures.Count == 0)
            {
                month = DateTime.Now.Year;
            }
            else
            {
                month = Int32.Parse(m.Groups["month"].Value);
            }
            if (m.Groups["day"].Captures.Count == 0)
            {
                day = DateTime.Now.Year;
            }
            else
            {
                day = Int32.Parse(m.Groups["day"].Value);
            }
            if (m.Groups["hour"].Captures.Count == 0)
            {
                hour = DateTime.Now.Year;
            }
            else
            {
                if (m.Groups["minute"].Captures.Count == 0)
                {
                    minute = DateTime.Now.Year;
                }
                else
                {
                    minute = Int32.Parse(m.Groups["minute"].Value);
                }
            }
            hour = Int32.Parse(m.Groups["hour"].Value);

            if (m.Groups["second"].Captures.Count == 0)
            {
                second = DateTime.Now.Year;
            }
            else
            {
                second = Int32.Parse(m.Groups["second"].Value);
            }
            if (m.Groups["fraction"].Captures.Count == 0)
            {
                millisecond = DateTime.Now.Year;
            }
            else
            {
                millisecond = Int32.Parse(m.Groups["fraction"].Value.Substring(0, 3));
            }

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }
        private static string MakeTopicName(string name)
        {
            return MakeTopicName(new UnqualifiedTopicRevision(name));
        }
        private static string MakeTopicName(UnqualifiedTopicName topic)
        {
            return MakeTopicName(new UnqualifiedTopicRevision(topic));
        }
        private static string MakeTopicName(UnqualifiedTopicRevision revision)
        {
            return revision.DottedNameWithVersion;
        }
        private static int SafeIntegerParse(string input)
        {
            int result;

            if (Int32.TryParse(input, out result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }
        private SqlInfoForTopic[] SqlTopicInfosForTopic(UnqualifiedTopicRevision topic)
        {
            return SqlHelper.GetSqlTopicInfosForTopic(Namespace, topic.LocalName, _connectionString);
        }
        private bool TipTopicRecordExists(string topicName)
        {
            return SqlHelper.TopicExists(Namespace, MakeTopicName(new UnqualifiedTopicRevision(topicName)), _connectionString);
        }
        private static TopicChange TopicChangeFromName(QualifiedTopicRevision topic)
        {
            try
            {
                Regex re = new Regex("(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?");
                if (!re.IsMatch(topic.Version))
                {
                    throw new FormatException("Illegal wiki archive filename: " + topic.Version);
                }
                Match match = re.Match(topic.Version);
                // Format into: "2/16/1992 12:15:12";
                int frac = 0;
                if (match.Groups["fraction"] != null)
                {
                    string fracs = "0." + match.Groups["fraction"].Value;
                    try
                    {
                        Decimal f = Decimal.Parse(fracs, new System.Globalization.CultureInfo("en-US"));
                        frac = (int)(1000 * f);
                    }
                    catch (FormatException ex)
                    {
                        ex.ToString();

                        // shut up compiler;
                    }
                }

                DateTime ts = new DateTime(
                    SafeIntegerParse(match.Groups["year"].Value), // month
                    SafeIntegerParse(match.Groups["month"].Value), // day
                    SafeIntegerParse(match.Groups["day"].Value), // year
                    SafeIntegerParse(match.Groups["hour"].Value), // hour
                    SafeIntegerParse(match.Groups["minute"].Value), // minutes
                    SafeIntegerParse(match.Groups["second"].Value), // seconds
                    frac);
                return new TopicChange(topic, ts, match.Groups["name"].Value);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception processing topic change " + topic.Version + " - " + ex.ToString());
            }
        }
        /// <summary>
        /// Write a new version of the topic (doesn't write a new version).  Generate all needed federation update changes via the supplied generator.
        /// </summary>
        /// <param name="topic">Topic to write</param>
        /// <param name="content">New content</param>
        /// <param name="gen">Object to recieve change info about the topic</param>
        public override void WriteTopic(UnqualifiedTopicRevision revision, string content)
        {
            string topicName = MakeTopicName(revision);
            bool isNew = !(SqlHelper.TopicExists(Namespace, topicName, _connectionString));

            // Get old topic so we can analyze it for imports to compare with the new one
            string oldText = null;
            if (!isNew)
            {
                oldText = SqlHelper.GetTopicBody(Namespace, topicName, _connectionString);
                // TODO: Deal with how this API has changed
                //oldProperties = NamespaceManager.ExtractExplicitFieldsFromTopicBody(oldText);
                throw new NotImplementedException(); 
            }

            string nameWithVersion = revision.DottedNameWithVersion; 
            SqlHelper.WriteTopic(Namespace, topicName, LastWriteTime(nameWithVersion), _connectionString, content, ((revision.Version != null && revision.Version.Length > 0) ? true : false));

        }

    }
}
