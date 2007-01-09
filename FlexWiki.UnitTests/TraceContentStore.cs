using System;
using System.Collections; 
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using FlexWiki.Collections; 

namespace FlexWiki.UnitTests
{
    internal class TraceContentProvider : ContentProviderBase
    {
        public TraceContentProvider(ContentProviderBase next)
            : base(next)
        {
        }

        public override bool Exists
        {
            get 
            { 
                RegisterCall(MethodInfo.GetCurrentMethod());
                throw new NotImplementedException(); 
            }
        }
        public override bool IsReadOnly
        {
            get
            {
                RegisterCall(MethodInfo.GetCurrentMethod());
                throw new NotImplementedException(); 
            }
        }

        public override TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException(); 
        }
        public override QualifiedTopicNameCollection AllTopics()
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public override void DeleteAllTopicsAndHistory()
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public override void DeleteTopic(UnqualifiedTopicName topic)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public override void Initialize(NamespaceManager manager)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public override bool HasPermission(UnqualifiedTopicName topic, TopicPermission permission)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public override System.IO.TextReader TextReaderForTopic(UnqualifiedTopicRevision revision)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public override bool TopicExists(UnqualifiedTopicName name)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public override void WriteTopic(UnqualifiedTopicRevision revision, string content)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }

        private void RegisterCall(MethodBase method)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException(); 
        }

    }
}
