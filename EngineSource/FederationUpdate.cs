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
using System.Collections.Specialized;


namespace FlexWiki
{
	/// <summary>
	/// Summary description for FederationUpdate.
	/// </summary>
	public class FederationUpdate 
	{
        public enum PropertyChangeType
        {
            PropertyAdd,
            PropertyRemove,
            PropertyUpdate
        };

        // key = topic
        // value = iEnum of propertyName names
        private HybridDictionary _addedPropertiesByTopic = new HybridDictionary();
        // key = propertyname
        // value = iEnum of topic names
        private HybridDictionary _addedPropertiesByProperty = new HybridDictionary();
        // key = propertyname
        // value = iEnum of topic names
        private HybridDictionary _changedPropertiesByProperty = new HybridDictionary();
        // key = topic
        // value = iEnum of propertyName names
        private HybridDictionary _changedPropertiesByTopic = new HybridDictionary();
        private ArrayList _createdTopics = new ArrayList();
        private ArrayList _deletedTopics = new ArrayList();
        private ArrayList _emptyList = new ArrayList();
        private bool _federationPropertiesChanged = false;
        private bool _namespaceListChanged = false;
        // key = propertyname
        // value = iEnum of topic names
        private HybridDictionary _removedPropertiesByProperty = new HybridDictionary();
        // key = topic
        // value = iEnum of propertyName names
        private HybridDictionary _removedPropertiesByTopic = new HybridDictionary();
        private ArrayList _updatedTopics = new ArrayList();
		
        public FederationUpdate()
		{
		}

        public IList AllAddedOrDeletedProperties
        {
            get
            {
                Set<string> s = new Set<string>();
                s.AddRange(_addedPropertiesByProperty.Keys);
                s.AddRange(_removedPropertiesByProperty.Keys);
                ArrayList answer = new ArrayList();
                foreach (object each in s)
                {
                    answer.Add(each);
                }
                return answer;
            }
        }
        public IList AllProperties
        {
            get
            {
                Set<string> s = new Set<string>();
                s.AddRange(_addedPropertiesByProperty.Keys);
                s.AddRange(_changedPropertiesByProperty.Keys);
                s.AddRange(_removedPropertiesByProperty.Keys);
                ArrayList answer = new ArrayList();
                foreach (object each in s)
                {
                    answer.Add(each);
                }
                return answer;
            }
        }
        public IList AllTopicsWithChangedProperties
        {
            get
            {
                Set<string> s = new Set<string>();
                s.AddRange(_addedPropertiesByTopic.Keys);
                s.AddRange(_changedPropertiesByTopic.Keys);
                s.AddRange(_removedPropertiesByTopic.Keys);
                ArrayList answer = new ArrayList();
                foreach (object each in s)
                {
                    answer.Add(each);
                }
                return answer;
            }
        }
        public IList CreatedTopics
        {
            get
            {
                return _createdTopics;
            }
        }
        public IList DeletedTopics
        {
            get
            {
                return _deletedTopics;
            }
        }
        public bool FederationPropertiesChanged
        {
            get
            {
                return _federationPropertiesChanged;
            }
            set
            {
                _federationPropertiesChanged = value;
            }
        }
        public bool NamespaceListChanged
        {
            get
            {
                return _namespaceListChanged;
            }
            set
            {
                _namespaceListChanged = value;
            }
        }
        public IList UpdatedTopics
        {
            get
            {
                return _updatedTopics;
            }
        }

        public IList AddedPropertiesForTopic(QualifiedTopicRevision topic)
        {
            IList answer = (IList)(_addedPropertiesByTopic[topic]);
            if (answer == null)
                return _emptyList;
            return answer;
        }
        private void AddToDictionaries(HybridDictionary byProperty, HybridDictionary byTopic, QualifiedTopicRevision topic, string prop)
        {
            ArrayList tList;
            ArrayList pList;

            tList = (ArrayList)(byProperty[prop]);
            if (tList == null)
            {
                tList = new ArrayList();
                byProperty[prop] = tList;
            }
            tList.Add(topic);

            pList = (ArrayList)(byTopic[topic]);
            if (pList == null)
            {
                pList = new ArrayList();
                byTopic[topic] = pList;
            }
            pList.Add(prop);
        }
        public void AddUpdatesFrom(FederationUpdate src)
        {
            foreach (QualifiedTopicRevision top in src.CreatedTopics)
                RecordCreatedTopic(top);
            foreach (QualifiedTopicRevision top in src.DeletedTopics)
                RecordDeletedTopic(top);
            foreach (QualifiedTopicRevision top in src.UpdatedTopics)
                RecordUpdatedTopic(top);
            foreach (DictionaryEntry e in src._addedPropertiesByTopic)
                _addedPropertiesByTopic[e.Key] = e.Value;
            foreach (DictionaryEntry e in src._removedPropertiesByTopic)
                _removedPropertiesByTopic[e.Key] = e.Value;
            foreach (DictionaryEntry e in src._changedPropertiesByTopic)
                _changedPropertiesByTopic[e.Key] = e.Value;
            foreach (DictionaryEntry e in src._addedPropertiesByProperty)
                _addedPropertiesByProperty[e.Key] = e.Value;
            foreach (DictionaryEntry e in src._removedPropertiesByProperty)
                _removedPropertiesByProperty[e.Key] = e.Value;
            foreach (DictionaryEntry e in src._changedPropertiesByProperty)
                _changedPropertiesByProperty[e.Key] = e.Value;
            if (src.FederationPropertiesChanged)
                RecordFederationPropertiesChanged();
            if (src.NamespaceListChanged)
                RecordNamespaceListChanged();
        }
        public IList ChangedPropertiesForTopic(QualifiedTopicRevision topic)
        {
            IList answer = (IList)(_changedPropertiesByTopic[topic]);
            if (answer == null)
                return _emptyList;
            return answer;
        }
        public void RecordCreatedTopic(QualifiedTopicRevision name)
        {
            if (_createdTopics.Contains(name))
            {
                return;
            }
            _createdTopics.Add(name);
        }
        public void RecordNamespaceListChanged()
        {
            NamespaceListChanged = true;
        }
        public void RecordPropertyChange(QualifiedTopicRevision topic, string propertyName, PropertyChangeType aType)
		{
			switch (aType)
			{
				case PropertyChangeType.PropertyAdd:
					AddToDictionaries(_addedPropertiesByProperty, _addedPropertiesByTopic, topic, propertyName);
					break;

				case PropertyChangeType.PropertyRemove:
					AddToDictionaries(_removedPropertiesByProperty, _removedPropertiesByTopic, topic, propertyName);
					break;

				case PropertyChangeType.PropertyUpdate:
					AddToDictionaries(_changedPropertiesByProperty, _changedPropertiesByTopic, topic, propertyName);
					break;
			}
		}
		public void RecordFederationPropertiesChanged()
		{
			FederationPropertiesChanged = true;
		}
        public void RecordDeletedTopic(QualifiedTopicRevision name)
        {
            if (_deletedTopics.Contains(name))
                return;
            _deletedTopics.Add(name);
        }
        public void RecordUpdatedTopic(QualifiedTopicRevision name)
        {
            if (_updatedTopics.Contains(name))
                return;
            _updatedTopics.Add(name);
        }
        public IList RemovedPropertiesForTopic(QualifiedTopicRevision topic)
        {
            IList answer = (IList)(_removedPropertiesByTopic[topic]);
            if (answer == null)
                return _emptyList;
            return answer;
        }
	}
}
