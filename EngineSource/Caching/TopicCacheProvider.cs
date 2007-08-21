using System;

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
            Next.DeleteAllTopicsAndHistory();

            if (CacheEnabled)
            {
                InvalidateAllItemsForNamespace();
            }
        }
        public override void DeleteTopic(UnqualifiedTopicName topic)
        {
            Next.DeleteTopic(topic);

            if (CacheEnabled)
            {
                InvalidateItemsForAnyRevisionOfTopic(topic);
                InvalidateTopicListItem(); 
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
        public override void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            Next.WriteTopic(topicRevision, content);
            
            if (CacheEnabled)
            {
                // This may seem like overkill - why not just invalidate only the revision that has 
                // changed? But the problem is that there's an equivalence between the latest revision
                // and a null revision, and we don't know which two are the same. So on the principal
                // that writes are rare and reads of non-tip revisions are common, we invalidate 
                // all revisions. 
                InvalidateItemsForAnyRevisionOfTopic(topicRevision.AsUnqualifiedTopicName());
                InvalidateTopicListItem(); 
            }

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
        private string GetKeyForTopicList()
        {
            return new TopicCacheKey(new QualifiedTopicRevision("AllTopics", Namespace), "TopicList").ToString(); 
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
        private void InvalidateTopicListItem()
        {
            string key = GetKeyForTopicList();
            Cache[key] = null;
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
