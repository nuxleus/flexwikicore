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

        public override void DeleteAllTopicsAndHistory()
        {
            Next.DeleteAllTopicsAndHistory();

            if (CacheEnabled)
            {
                foreach (string key in Cache.Keys)
                {
                    if (IsKeyForNamespace(key))
                    {
                        Cache[key] = null;
                    }
                }
            }
        }
        public override void DeleteTopic(UnqualifiedTopicName topic)
        {
            Next.DeleteTopic(topic);

            if (CacheEnabled)
            {
                foreach (string key in Cache.Keys)
                {
                    if (IsKeyForRevisionOfTopic(key, topic))
                    {
                        Cache[key] = null;
                    }
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
        public override void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            Next.WriteTopic(topicRevision, content);
            
            if (CacheEnabled)
            {
                string key = GetKeyForParsedTopic(topicRevision);
                Cache[key] = null; 
            }

        }

        private string GetKeyForParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            return new TopicCacheKey(topicRevision.ResolveRelativeTo(Namespace), "ParsedTopic").ToString(); 
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
        private bool IsKeyForRevisionOfTopic(string key, UnqualifiedTopicName topic)
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

    }
}
