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
            if (CacheEnabled)
            {
                string key = GetKeyForParsedTopic(topicRevision);
                Cache[key] = null; 
            }

            Next.WriteTopic(topicRevision, content); 
        }

        private string GetKeyForParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            return string.Format("cache://{0}/{1}/ParsedTopic", Namespace, topicRevision.DottedNameWithVersion); 
        }
    }
}
