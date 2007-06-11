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
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki
{
    /// <summary>
    /// 
    /// </summary>
    public class FileSystemStore : ContentProviderBase
    {
        // Fields

        private DateTime _created = DateTime.Now;
        private DateTime _definitionTopicLastRead = DateTime.MinValue;
        private IFileSystem _fileSystem; 
        /// <summary>
        /// The file system path to the folder that contains the content
        /// </summary>
        private string _root;

        // Constructors

        public FileSystemStore() : this(new FileSystem()) 
        {
        }
        
        public FileSystemStore(IFileSystem fileSystem) : base(null)
        {
            _fileSystem = fileSystem;
        }

        // Properties

        /// <summary>
        /// Answer the file system path to the definition topic file
        /// </summary>
        public string Root
        {
            get { return _root; }
        }

        public override bool Exists
        {
            get
            {
                return FileSystem.DirectoryExists(Root);
            }
        }
        /// <summary>
        /// Implements <see cref="IContentProvider.IsReadOnly"/>.
        /// </summary>
        public override bool IsReadOnly
        {
            get { return false; }
        }

        private IFileSystem FileSystem
        {
            get { return _fileSystem; }
        }

        // Methods

        /// <summary>
        /// A list of TopicChanges to a topic since a given date [sorted by date]
        /// </summary>
        /// <param name="stamp">Specifies that we are only interested in changes after this date.</param>
        /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
        /// <returns>Enumeration of TopicChanges</returns>
        /// <remarks>Returns a collection with zero elements in it when the topic name does not exist, or has been deleted, 
        /// or if the calling user does not have the <see cref="Permission.Read"/> permission.</remarks>
        public override TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            TopicChangeCollection answer = new TopicChangeCollection();

            FileInformationCollection infos = FileInfosForTopic(topic.LocalName);
            ArrayList sortable = new ArrayList();
            foreach (IFileInformation each in infos)
            {
                sortable.Add(new FileInfoTopicData(each, Namespace));
            }
            sortable.Sort(new TimeSort());

            foreach (TopicData each in sortable)
            {
                if (each.LastModificationTime < stamp)
                {
                    continue;
                }
                QualifiedTopicRevision name = new QualifiedTopicRevision(topic.ResolveRelativeTo(Namespace));
                name.Version = each.Version;

                // Version might be null if we grabbed the .wiki file instead of a .awiki file
                if (each.Version == null)
                {
                    answer.Add(new TopicChange(name, each.LastModificationTime, ""));
                }
                else
                {
                    TopicChange change = TopicChangeFromName(name);
                    answer.Add(change);
                }
            }
            return answer;
        }
        public override QualifiedTopicNameCollection AllTopics()
        {
            QualifiedTopicNameCollection topics = new QualifiedTopicNameCollection();
            foreach (IFileInformation file in FileSystem.GetFiles(Root, "*.wiki"))
            {
                FileInfoTopicData topicData = new FileInfoTopicData(file, Namespace);
                topics.Add(new QualifiedTopicName(topicData.Name, topicData.Namespace));
            }

            return topics;
        }
        /// <summary>
        /// Delete a content base (kills all .wiki and .awiki files; removed the dir if empty)
        /// </summary>
        public override void DeleteAllTopicsAndHistory()
        {
            foreach (IFileInformation each in FileSystem.GetFiles(Root, "*.wiki"))
            {
                FileSystem.DeleteFile(each.FullName);
            }
            foreach (IFileInformation each in FileSystem.GetFiles(Root, "*.awiki"))
            {
                FileSystem.DeleteFile(each.FullName);
            }
            if (FileSystem.GetFiles(Root).Count== 0)
            {
                FileSystem.DeleteDirectory(Root);
            }

            //OnFederationUpdated(new FederationUpdateEventArgs(update));
        }
        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        public override void DeleteTopic(UnqualifiedTopicName topic)
        {
            string path = TopicPath(topic, null);
            if (!FileSystem.FileExists(path))
            {
                return;
            }

            FileSystem.DeleteFile(path);

            // Fire the event
            //OnFederationUpdated(new FederationUpdateEventArgs(update));
        }
        public override ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            throw new NotImplementedException();
        }
        public override void Initialize(NamespaceManager namespaceManager)
        {
            base.Initialize(namespaceManager);

            if (!NamespaceManager.Parameters.Contains("Root"))
            {
                throw new FlexWikiException("Missing parameter 'Root' in namespace definition: " + Namespace); 
            }

            _root = NamespaceManager.Parameters["Root"].Value;

            // Turn the root into an absolute path
            _root = NamespaceManager.Federation.Application.ResolveRelativePath(_root); 

            // Create the directory if it doesn't already exist.
            _fileSystem.CreateDirectory(_root); 
        }
        /// <summary>
        /// Answer whether a topic is readable or writable
        /// </summary>
        /// <param name="topic">The topic (must directly be in this content base)</param>
        /// <returns>true is writable by the current user (or does not exist); else false</returns>
        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            string path = TopicPath(topic, null);
            if (!FileSystem.FileExists(path))
            {
                // It might seem a little weird to return true if the topic doesn't exist, but 
                // basically what we're saying is that there's no reason to deny read/edit.
                return true;
            }

            if (permission == TopicPermission.Edit)
            {
                if (!FileSystem.HasWritePermission(path))
                {
                    return false;
                }
            }
            else if (permission == TopicPermission.Read)
            {
                if (!FileSystem.HasReadPermission(path))
                {
                    return false; 
                }
            }

            return true;
        }
        /// <summary>
        /// Implements <see cref="ContentProviderBase.LockTopic"/>. 
        /// </summary>
        /// <param name="topic"></param>
        public override void LockTopic(UnqualifiedTopicName topic)
        {
            FileSystem.MakeReadOnly(MakePath(Root, topic.LocalName)); 
        }
        /// <summary>
        /// Answer a TextReader for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
        /// <returns>TextReader</returns>
        public override TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            string topicFile = TopicPath(topicRevision);
            if (topicFile == null || !FileSystem.FileExists(topicFile))
            {
                return null; 
            }
            return new StreamReader(FileSystem.OpenRead(topicFile, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
        /// <summary>
        /// Answer true if a topic exists in this ContentProviderChain
        /// </summary>
        /// <param name="name">Name of the topic</param>
        /// <returns>true if it exists</returns>
        public override bool TopicExists(UnqualifiedTopicName topicName)
        {
            return FileSystem.FileExists(MakePath(Root, topicName.LocalName));
        }
        /// <summary>
        /// Implements <see cref="ContentProviderBase.LockTopic"/>.
        /// </summary>
        /// <param name="topic"></param>
        public override void UnlockTopic(UnqualifiedTopicName topic)
        {
            FileSystem.MakeWritable(MakePath(Root, topic.LocalName));
        }
        /// <summary>
        /// Write a new version of the topic (doesn't write a new version).  Generate all needed federation update changes via the supplied generator.
        /// </summary>
        /// <param name="topic">Topic to write</param>
        /// <param name="content">New content</param>
        /// <param name="sink">Object to recieve change info about the topic</param>
        public override void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            string root = Root;
            string fullpath = MakePath(root, topicRevision.LocalName, topicRevision.Version);
            FileSystem.WriteFile(fullpath, content); 
        }


        //private void AssertSameNamespace(TopicName topicName)
        //{
        //    if (topicName.IsQualified && topicName.Namespace != Namespace)
        //    {
        //        throw new FlexWikiException("FileSystemStore can't work with topics not in this namespace. " +
        //            topicName.DottedName);
        //    }
        //}
        //private void AssertSameNamespace(TopicRevision revision)
        //{
        //    if (revision.IsQualified && revision.Namespace != Namespace)
        //    {
        //        throw new FlexWikiException("FileSystemStore can't work with topics not in this namespace. " +
        //            revision.DottedNameWithVersion);
        //    }
        //}
        //private string FilenameFromTopic(TopicName topicName)
        //{
        //    AssertSameNamespace(topicName);
        //    return topicName.LocalName + ".wiki";
        //}
        //private string FilenameFromTopic(TopicRevision revision)
        //{
        //    AssertSameNamespace(revision);
        //    return revision.LocalName + "(" + revision.Version + ")";
        //}
        /// <summary>
        /// All of the FileInfos for the historical versions of a given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns>FileInfos</returns>
        private FileInformationCollection FileInfosForTopic(string topic)
        {
            FileInformationCollection answer = new FileInformationCollection();

            // If the topic does not exist, we ignore any historical versions (the result of a delete)
            if (!TipFileExists(topic))
            {
                return answer;
            }

            try
            {
                answer = FileSystem.GetFiles(Root, topic + "(*).awiki");
            }
            catch (DirectoryNotFoundException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            // If someone deleted the .awiki files, we still need to return info about the 
            // topic. Use the .wiki file instead.
            if (answer.Count == 0)
            {
                answer = FileSystem.GetFiles(Root, topic + ".wiki"); 
            }

            return answer;
        }
        /// <summary>
        /// Answer the full file system path for a given topic in a given folder.  
        /// </summary>
        /// <param name="root">File system path to the root directory for the containing content base</param>
        /// <param name="name">The name of the topic</param>
        /// <returns>Full path to the file containing the content for the most recent version of the topic</returns>
        private static string MakePath(string root, string name)
        {
            return MakePath(root, name, null); 
        }
        private static string MakePath(string root, string name, string version)
        {
            if (version == null || version.Length == 0)
            {
                return Path.Combine(root, name + ".wiki");
            }
            else
            {
                return Path.Combine(root, name + "(" + version + ").awiki");
            }
        }

        private bool TipFileExists(string name)
        {
            return FileSystem.FileExists(MakePath(Root, name));
        }
        private static TopicChange TopicChangeFromName(QualifiedTopicRevision topic)
        {
            try
            {
                VersionInfo versionInfo = TopicRevision.ParseVersion(topic.Version);

                if (versionInfo == null)
                {
                    throw new FormatException("Illegal wiki archive filename: " + topic.Version);
                }

                return new TopicChange(topic, versionInfo.Timestamp, versionInfo.Author);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception processing topic change " + topic.Version + " - " + ex.ToString());
            }
        }
        private string TopicPath(UnqualifiedTopicRevision revision)
        {
            return TopicPath(revision.LocalName, revision.Version); 
        }
        private string TopicPath(UnqualifiedTopicName topic, string version)
        {
            return TopicPath(topic.LocalName, version); 
        }
        private string TopicPath(string topicName, string version)
        {
            return MakePath(Root, topicName, version);
        }
        //private string PathFromTopic(TopicName topicName)
        //{
        //    AssertSameNamespace(topicName);
        //    return Path.Combine(Root, FilenameFromTopic(topicName)); 
        //}
        //private string PathFromTopic(TopicRevision revision)
        //{
        //    AssertSameNamespace(revision); 

        //    if (revision.IsQualified && revision.Namespace != Namespace)
        //    {
        //        throw new FlexWikiException("FileSystemStore can't resolve paths for topics not in this namespace. " +
        //            revision.DottedNameWithVersion); 
        //    }


        //}

    }
}
