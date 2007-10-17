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
using System.IO;

using FlexWiki.Collections; 

namespace FlexWiki
{
    public interface IContentProvider
    {
        /// <summary>
        /// Gets a value indicating whether the namespace actually exists. 
        /// </summary>
        /// <remarks>Note that this value could change dynamically, depending on 
        /// (for instance) the security context of the calling user.
        /// </remarks>
        bool Exists
        {
            get;
        }
        /// <summary>
        /// Returns true if the content store is read-only.
        /// </summary>
        /// <remarks>
        /// A read-only content store may not be written to. Note that due to security, 
        /// some users may find a content store to be read-only, while others 
        /// will find it writable.
        /// </remarks>
        bool IsReadOnly
        {
            get;
        }
        IContentProvider Next
        {
            get;
            set;
        }

        /// <summary>
        /// A list of TopicChanges to a topic since a given date [sorted by date]
        /// </summary>
        /// <param name="topic">A given date</param>
        /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
        /// <param name="rule">A composite cache rule to fill with rules that represented accumulated dependencies (or null)</param>
        /// <returns>List of <see cref="TopicChange" /> objects, sorted so that the newest appears first in the list.</returns>
        TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp);
        /// <summary>
        /// Gets a list of all the <see cref="LocalTopicName"/>s in this content store.
        /// </summary>
        /// <returns>A list of all the <see cref="LocalTopicName"/>s in this content store</returns>
        QualifiedTopicNameCollection AllTopics();
        /// <summary>
        /// Delete the contents of a namespace (kills everything inside recursively).  Note that this does *not* include unregistering
        /// the content base within the federation.
        /// </summary>
        void DeleteAllTopicsAndHistory();
        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        void DeleteTopic(UnqualifiedTopicName topic);
        /// <summary>
        /// Returns the parsed representation of the specified topic. 
        /// </summary>
        /// <param name="topic">The topic for which to return the parsed representation.</param>
        /// <returns>A <see cref="ParsedTopic"/> object containing the parsed representation of the topic.</returns>
        ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision);
        /// <summary>
        /// Answers whether the current user has the given permission for this namespace. 
        /// </summary>
        /// <param name="permission">The permission to query about.</param>
        /// <returns>True if the current user has the specified permission, false otherwise.</returns>
        bool HasNamespacePermission(NamespacePermission permission);
        /// <summary>
        /// Answer whether the current user has the given permission for the specified topic.
        /// </summary>
        /// <param name="topic">The topic </param>
        /// <returns>true if the topic exists AND the specified permission is allowed for the current user; else false</returns>
        bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission);
        void Initialize(NamespaceManager namespaceManager);
        /// <summary>
        /// Makes an existing topic read-only. 
        /// </summary>
        /// <param name="topic">The topic to modify.</param>
        /// <exception cref="TopicNotFoundException">
        /// Thrown if the specified topic does not exist.
        /// </exception>
        void LockTopic(UnqualifiedTopicName topic);
        /// <summary>
        /// Answer a TextReader for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
        /// <returns>TextReader</returns>
        TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision);
        /// <summary>
        /// Answer true if a topic exists in this namespace
        /// </summary>
        /// <param name="name">Name of the topic</param>
        /// <returns>true if it exists</returns>
        bool TopicExists(UnqualifiedTopicName name);
        /// <summary>
        /// Makes an existing topic read-write. 
        /// </summary>
        /// <param name="topic">The topic to modify.</param>
        /// <exception cref="TopicNotFoundException">
        /// Thrown if the specified topic does not exist.
        /// </exception>
        void UnlockTopic(UnqualifiedTopicName topic);
        void WriteTopic(UnqualifiedTopicRevision topicRevision, string content);
    }
}
