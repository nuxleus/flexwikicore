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

using FlexWiki.Collections;

namespace FlexWiki.Caching
{
    public class TopicCacheProvider : ContentProviderBase
    {
        private const string c_disableParameterName = "caching.disabled"; 

        public TopicCacheProvider(IContentProvider next)
            : base(next)
        {
        }

        public IWikiCache Cache
        {
            get
            {
                return Federation.Application.Cache; 
            }
        }

        public override bool Exists
        {
            get
            {
                if (CacheEnabled)
                {
                    string key = GetKeyForNamespaceExistence();
                    object cachedValue = Cache[key];

                    if (cachedValue == null)
                    {
                        bool exists = Next.Exists;
                        Cache[key] = exists;
                        return exists;
                    }
                    else
                    {
                        return (bool)cachedValue;
                    }
                }
                else
                {
                    return Next.Exists; 
                }
            }
        }

        //public override bool IsReadOnly
        //{
        //    get
        //    {
        //        throw new NotImplementedException(); 
        //    }
        //}

        private bool CacheEnabled
        {
            get
            {
                if (!NamespaceManager.Parameters.Contains(c_disableParameterName))
                {
                    return true; 
                }

                NamespaceProviderParameter parameter = NamespaceManager.Parameters[c_disableParameterName];

                bool value;
                if (bool.TryParse(parameter.Value, out value))
                {
                    return !value; 
                }

                return true; 
            }
        }

        public override TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            if (CacheEnabled)
            {
                string key = GetKeyForTopicChanges(topic);
                TopicChangesCacheItem item = (TopicChangesCacheItem)Cache[key];

                if (item == null)
                {
                    item = new TopicChangesCacheItem(
                        Next.AllChangesForTopicSince(topic, stamp),
                        stamp);
                    Cache[key] = item;
                }
                else 
                {
                    // They've asked for information that's older than what we have stored. Fetch it anew.
                    if (stamp < item.Since)
                    {
                        item = new TopicChangesCacheItem(
                            Next.AllChangesForTopicSince(topic, stamp),
                            stamp);
                        Cache[key] = item;
                    }
                    // Otherwise, we should have it, but we have to filter it down to the relevant period.
                    else
                    {
                        TopicChangeCollection changes = new TopicChangeCollection();
                        foreach (TopicChange change in item.Changes)
                        {
                            if (change.Modified >= stamp)
                            {
                                changes.Add(change); 
                            }
                        }

                        return changes; 
                    }
                }

                return item.Changes; 
            }
            else
            {
                return Next.AllChangesForTopicSince(topic, stamp); 
            }
        }
        public override QualifiedTopicNameCollection AllTopics()
        {
            if (CacheEnabled)
            {
                string key = GetKeyForTopicList();
                QualifiedTopicNameCollection topics = (QualifiedTopicNameCollection)Cache[key];

                if (topics == null)
                {
                    topics = Next.AllTopics();
                    Cache[key] = topics;
                }

                return topics; 
            }

            return Next.AllTopics(); 
        }
        public override void DeleteAllTopicsAndHistory()
        {
            try
            {
                Next.DeleteAllTopicsAndHistory();
            }
            finally
            {
                if (CacheEnabled)
                {
                    InvalidateAllItemsForNamespace();
                }
            }
        }
        public override void DeleteTopic(UnqualifiedTopicName topic)
        {
            try
            {
                Next.DeleteTopic(topic);
            }
            finally
            {
                if (CacheEnabled)
                {
                    InvalidateAllItemsForNamespace();
                }
            }
        }
        public override ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            if (CacheEnabled)
            {
                string key = GetKeyForParsedTopic(topicRevision);
                ParsedTopic parsedTopic = (ParsedTopic)Cache[key];

                if (parsedTopic == null)
                {
                    parsedTopic = Next.GetParsedTopic(topicRevision);
                    Cache[key] = parsedTopic; 
                }

                return parsedTopic;

            }
            else
            {
                return Next.GetParsedTopic(topicRevision); 
            }
        }
        public override bool HasNamespacePermission(NamespacePermission permission)
        {
            if (CacheEnabled)
            {
                string key = GetKeyForNamespacePermission(permission);
                object cachedValue = Cache[key];

                if (cachedValue == null)
                {
                    bool hasPermission = Next.HasNamespacePermission(permission);
                    Cache[key] = hasPermission;
                    return hasPermission;
                }
                else
                {
                    return (bool)cachedValue;
                }
            }
            else
            {
                return Next.HasNamespacePermission(permission);
            }
        }

        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            if (CacheEnabled)
            {
                string key = GetKeyForTopicPermission(topic, permission);
                object cachedValue = Cache[key];

                if (cachedValue == null)
                {
                    bool hasPermission = Next.HasPermission(topic, permission);
                    Cache[key] = hasPermission;
                    return hasPermission;
                }
                else
                {
                    return (bool)cachedValue; 
                }
            }
            else
            {
                return Next.HasPermission(topic, permission);
            }
        }
        public override void LockTopic(UnqualifiedTopicName topic)
        {
            try
            {
                Next.LockTopic(topic);
            }
            finally
            {
                InvalidateTopicPermissionItems(topic);
                InvalidateTopicIsReadOnly(topic);
            }
        }
        public override TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            if (CacheEnabled)
            {
                // Because the contents are already cached in the built-in property _Body, 
                // it would be a waste to cache them again separately. So we just grab the 
                // _Body property and wrap it with a TextReader. 
                ParsedTopic parsedTopic = GetParsedTopic(topicRevision);

                if (parsedTopic == null)
                {
                    return null; 
                }

                return new StringReader(parsedTopic.Properties["_Body"].LastValue); 
            }
            else
            {
                return Next.TextReaderForTopic(topicRevision);
            }
        }
        public override bool TopicExists(UnqualifiedTopicName topic)
        {
            if (CacheEnabled)
            {
                string key = GetKeyForTopicExistence(topic);
                object cachedValue = Cache[key];

                if (cachedValue == null)
                {
                    bool existence = Next.TopicExists(topic);
                    Cache[key] = existence;
                    return existence;
                }
                else
                {
                    return (bool)cachedValue;
                }
            }
            else
            {
                return base.TopicExists(topic);
            }
        }
        public override bool TopicIsReadOnly(UnqualifiedTopicName topic)
        {
            if (CacheEnabled)
            {
                string key = GetKeyForTopicIsReadOnly(topic);
                object cachedValue = Cache[key];

                if (cachedValue == null)
                {
                    bool existence = Next.TopicIsReadOnly(topic);
                    Cache[key] = existence;
                    return existence;
                }
                else
                {
                    return (bool)cachedValue;
                }
            }
            else
            {
                return base.TopicIsReadOnly(topic);
            }
        }
        public override void UnlockTopic(UnqualifiedTopicName topic)
        {
            try
            {
                Next.UnlockTopic(topic);
            }
            finally
            {
                InvalidateTopicPermissionItems(topic);
                InvalidateTopicIsReadOnly(topic);
            }
        }
        public override void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            try
            {
                Next.WriteTopic(topicRevision, content);
            }
            finally
            {
                if (CacheEnabled)
                {
                    // This may seem like overkill - why not just invalidate only the revision that has 
                    // changed? But the problem is that there's an equivalence between the latest revision
                    // and a null revision, and we don't know which two are the same. So on the principal
                    // that writes are rare and reads of non-tip revisions are common, we invalidate 
                    // all revisions. 
                    // On top of that, writes to some topics (e.g. _ContentBaseDefinition) can change the
                    // results of other topics. Thus on write we flush the whole cache. 
                    InvalidateAllItemsForNamespace(); 
                }
            }
        }

        private string GetKeyForNamespaceExistence()
        {
            return new TopicCacheKey(new QualifiedTopicRevision("AllTopics", Namespace), "NamespaceExistence").ToString(); 
        }
        private string GetKeyForNamespacePermission(NamespacePermission permission)
        {
            return new TopicCacheKey(new QualifiedTopicRevision("AllTopics", Namespace), 
                "NamespacePermission-" + permission.ToString()).ToString(); 
        }
        private string GetKeyForParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            return new TopicCacheKey(topicRevision.ResolveRelativeTo(Namespace), "ParsedTopic").ToString(); 
        }
        private string GetKeyForTopicChanges(UnqualifiedTopicName topic)
        {
            return new TopicCacheKey(
                topic.ResolveRelativeTo(Namespace).AsQualifiedTopicRevision(), "TopicChanges").ToString(); 
        }
        private string GetKeyForTopicExistence(UnqualifiedTopicName topic)
        {
            return new TopicCacheKey(
                topic.ResolveRelativeTo(Namespace).AsQualifiedTopicRevision(), "TopicExistence").ToString();
        }
        private string GetKeyForTopicIsReadOnly(UnqualifiedTopicName topic)
        {
            return new TopicCacheKey(
                topic.ResolveRelativeTo(Namespace).AsQualifiedTopicRevision(), "TopicIsReadOnly").ToString();
        }
        private string GetKeyForTopicList()
        {
            return new TopicCacheKey(new QualifiedTopicRevision("AllTopics", Namespace), "TopicList").ToString(); 
        }
        private string GetKeyForTopicPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            string path; 
            if (permission == TopicPermission.Edit)
            {
                path = "EditPermission"; 
            }
            else if (permission == TopicPermission.Read)
            {
                path = "ReadPermission";
            }
            else
            {
                throw new ArgumentException("Unsupported topic permission " + permission.ToString(), "permission"); 
            }

            return new TopicCacheKey(topic.ResolveRelativeTo(Namespace).AsQualifiedTopicRevision(), path).ToString(); 
        }
        private void InvalidateAllItemsForNamespace()
        {
            foreach (string key in Cache.Keys)
            {
                if (IsKeyForNamespace(key))
                {
                    Cache[key] = null;
                }
            }
        }
        private void InvalidateItemsForAnyRevisionOfTopic(UnqualifiedTopicName topic)
        {
            foreach (string key in Cache.Keys)
            {
                if (IsKeyForAnyRevisionOfTopic(key, topic))
                {
                    Cache[key] = null;
                }
            }
        }
        private void InvalidateItemsForRevision(UnqualifiedTopicRevision topicRevision)
        {
            foreach (string key in Cache.Keys)
            {
                if (IsKeyForRevisionOfTopic(key, topicRevision))
                {
                    Cache[key] = null; 
                }
            }
        }
        private void InvalidateTopicIsReadOnly(UnqualifiedTopicName topic)
        {
            string key = GetKeyForTopicIsReadOnly(topic);
            Cache[key] = null;
        }
        private void InvalidateTopicListItem()
        {
            string key = GetKeyForTopicList();
            Cache[key] = null;
        }
        private void InvalidateTopicPermissionItems(UnqualifiedTopicName topic)
        {
            string readKey = GetKeyForTopicPermission(topic, TopicPermission.Read);
            string editKey = GetKeyForTopicPermission(topic, TopicPermission.Edit);
            Cache[readKey] = null;
            Cache[editKey] = null; 
        }

        private bool IsKeyForAnyRevisionOfTopic(string key, UnqualifiedTopicName topic)
        {
            TopicCacheKey parsedKey;
            if (TopicCacheKey.TryParse(key, out parsedKey))
            {
                QualifiedTopicName qualifiedName = topic.ResolveRelativeTo(Namespace);

                if (qualifiedName.Equals(parsedKey.Revision.AsQualifiedTopicName()))
                {
                    return true;
                }
            }

            return false;
        }
        private bool IsKeyForNamespace(string key)
        {
            TopicCacheKey parsedKey;
            if (TopicCacheKey.TryParse(key, out parsedKey))
            {
                if (parsedKey.Revision.Namespace.Equals(Namespace))
                {
                    return true; 
                }
            }

            return false;
        }
        private bool IsKeyForRevisionOfTopic(string key, UnqualifiedTopicRevision topicRevision)
        {
            QualifiedTopicRevision qualifiedRevision = topicRevision.ResolveRelativeTo(Namespace); 

            TopicCacheKey parsedKey;
            if (TopicCacheKey.TryParse(key, out parsedKey))
            {
                if (qualifiedRevision.Equals(parsedKey.Revision))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
