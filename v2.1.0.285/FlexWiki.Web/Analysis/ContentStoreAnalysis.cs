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

using FlexWiki.Collections;

namespace FlexWiki.Web.Analysis
{
    internal class ContentStoreAnalysis
    {
        private NamespaceManager _namespaceManager;
        private readonly Ocean _ocean = new Ocean();
        private ReferenceMap _referenceMap;
        private readonly Dictionary<QualifiedTopicRevision, TopicAnalysis> _topicToTopicAnalysis = 
            new Dictionary<QualifiedTopicRevision,TopicAnalysis>();

        public ContentStoreAnalysis(NamespaceManager aNamespaceManager)
        {
            _namespaceManager = aNamespaceManager;
            Analyze();
        }

        // TODO: This should change to some more specific type - as soon as someone
        // can figure out what type it should be...
        public Ocean Islands
        {
            get
            {
                return _ocean;
            }
        }

        public TopicAnalysis AnalysisFor(QualifiedTopicRevision topic)
        {
            return (TopicAnalysis)(_topicToTopicAnalysis[topic]);
        }

        private void Analyze()
        {
            // Build the ocean
            //	- an ocean (set) of islands (set)
            //	- also a hash for TopicAnalysis (topic->{island set,refcount}) for quick check if already present
            _referenceMap = _namespaceManager.GetReferenceMap(ExistencePolicy.ExistingOnly);

            foreach (string outerTopic in _referenceMap.Keys)
            {
                Ocean islands = new Ocean();
                QualifiedTopicRevisionCollection linkedTopics = _referenceMap[outerTopic];

                QualifiedTopicRevision outerRevision = new QualifiedTopicRevision(outerTopic, _namespaceManager.Namespace);
                TopicAnalysis outerTopicAnalysis = null;
                if (!_topicToTopicAnalysis.ContainsKey(outerRevision))
                {
                    outerTopicAnalysis = new TopicAnalysis();
                    _topicToTopicAnalysis[outerRevision] = outerTopicAnalysis;
                }
                else
                {
                    outerTopicAnalysis = _topicToTopicAnalysis[outerRevision];
                }

                if (outerTopicAnalysis.Island != null)
                {
                    islands.Add(outerTopicAnalysis.Island);
                }

                //	- foreach outer topic
                //		islands = new set
                //		foreach linked topic
                //			increment refcount for linked topic			
                //			if (linkedtopic is on an island)
                //				islands add that island
                Island inNamespaceLinks = new Island();
                foreach (QualifiedTopicRevision linkedTopic in linkedTopics)
                {
                    // Only analyze in this namespace
                    if (linkedTopic.Namespace != _namespaceManager.Namespace)
                    {
                        // Response.Write("Skiping linked topic (" + linkedTopic.Name + ") because namespace doesn't match<br>");
                        continue;
                    }
                    // Only do each topic once; have we seen this one?
                    if (inNamespaceLinks.Contains(linkedTopic))
                    {
                        // Response.Write("Skiping linked topic (" + linkedTopic.Name + ") because seen before<br>");
                        continue;
                    }
                    // Skip self-references
                    if (linkedTopic.Equals(outerTopic))
                    {
                        continue;
                    }

                    inNamespaceLinks.Add(linkedTopic);
                    TopicAnalysis linkedTopicAnalysis = null; 
                    if (!_topicToTopicAnalysis.ContainsKey(linkedTopic))
                    {
                        linkedTopicAnalysis = new TopicAnalysis();
                        _topicToTopicAnalysis[linkedTopic] = linkedTopicAnalysis;
                    }
                    else
                    {
                        linkedTopicAnalysis = _topicToTopicAnalysis[linkedTopic]; 
                    }
                    linkedTopicAnalysis.RefCount++;
                    if (linkedTopicAnalysis.Island != null)
                    {
                        islands.Add(linkedTopicAnalysis.Island);
                    }
                }

                //		if (islands is empty)
                //			create new island
                //			add outer topic and all linked topics
                //		else if (islands size == 1)
                //			add all links and the outer topic to that islands
                //		else
                //			// need to merge islands
                //			newset = merged set of all islands
                //			TopicAnalysiss and replace and of the old islands with the new island

                Island newIsland;
                if (islands.Count == 1)
                {
                    newIsland = islands.First;	// if there's only one, we can just use that one
                }
                else
                {
                    newIsland = new Island();
                    _ocean.Add(newIsland);
                }
                // Add the island and the linkedTopics
                newIsland.Add(new QualifiedTopicRevision(outerTopic, _namespaceManager.Namespace));
                outerTopicAnalysis.Island = newIsland;
                foreach (QualifiedTopicRevision linkedTopic in inNamespaceLinks)
                {
                    newIsland.Add(linkedTopic);
                    _topicToTopicAnalysis[linkedTopic].Island = newIsland;
                    // Response.Write("Placing " + linkedTopic.Name + "<br>");
                }
                // Now merge if there was originally more than one
                if (islands.Count > 1)
                {
                    foreach (Island eachIsland in islands)
                    {
                        foreach (QualifiedTopicRevision revision in eachIsland)
                        {
                            newIsland.Add(revision);
                        }
                        _ocean.Remove(eachIsland);
                        // Now update all the pointers from the TopicAnalysiss
                        foreach (QualifiedTopicRevision eachTopic in eachIsland)
                        {
                            _topicToTopicAnalysis[eachTopic].Island = newIsland;
                        }
                    }
                }
            }
        }

    }
}
