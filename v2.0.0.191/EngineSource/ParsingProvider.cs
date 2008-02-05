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
using System.Text;

using FlexWiki.Collections;

namespace FlexWiki
{
    public sealed class ParsingProvider : ContentProviderBase
    {
        public ParsingProvider(IContentProvider next)
            : base(next)
        {
        }

        public override ParsedTopic GetParsedTopic(UnqualifiedTopicRevision revision)
        {
            TextReader textReader = TextReaderForTopic(revision);
            if (textReader == null)
            {
                return null;
            }
            string contents = null;
            try
            {
                contents = textReader.ReadToEnd();
            }
            finally
            {
                textReader.Close();
            }

            ParsedTopic parsedTopic = TopicParser.Parse(contents);

            TopicChange latest = Next.AllChangesForTopicSince(revision.AsUnqualifiedTopicName(), DateTime.MinValue).Latest; 
            AddBuiltInProperty(parsedTopic, "_TopicName", revision.LocalName);
            AddBuiltInProperty(parsedTopic, "_TopicFullName", new QualifiedTopicName(revision.LocalName, Namespace).DottedName);
            // Note that even in an non-tip revision, the values of _LastModifiedBy and _CreationTime are of the tip
            // revision. The logic here is that since you must use WikiTalk to access these values, you won't be too
            // surprised that they change on you when someone commits a new revision. 
            // Latest can be null in some pathological cases where history information has been deleted. 
            AddBuiltInProperty(parsedTopic, "_LastModifiedBy", 
                latest == null ? "" : latest.Author);
            AddBuiltInProperty(parsedTopic, "_CreationTime", 
                latest == null ? DateTime.MinValue.ToString() : latest.Created.ToString());
            AddBuiltInProperty(parsedTopic, "_ModificationTime", 
                latest == null ? DateTime.MinValue.ToString() : latest.Modified.ToString());
            AddBuiltInProperty(parsedTopic, "_Body", contents); 

            return parsedTopic;
        }

        private void AddBuiltInProperty(ParsedTopic parsedTopic, string propertyName, string value)
        {
            // The built-in topics show up as the last value if the property already exists. 
            // This should be reasonably back-compatible, as the last value wins when people
            // don't remember that properties can have multiple values. 
            TopicProperty property = null;
            if (parsedTopic.Properties.Contains(propertyName))
            {
                property = parsedTopic.Properties[propertyName];
            }
            else
            {
                property = new TopicProperty(propertyName);
                parsedTopic.Properties.Add(property); 
            }

            TopicPropertyValue topicPropertyValue = new TopicPropertyValue(value); 
            property.Values.Add(topicPropertyValue); 
        }

    }
}
