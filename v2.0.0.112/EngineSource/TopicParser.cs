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
using System.Collections.Generic;
using System.IO;
using System.Text; 
using System.Text.RegularExpressions;

using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki
{
    /// <summary>
    /// Summary description for TopicParser.
    /// </summary>
    public static class TopicParser
    {
        /// <summary>
        /// Regex pattern string for extracting a single line propertyName
        /// </summary>
        private const string c_propertyPattern = "^(?<leader>:?)(?<name>([A-Z_][_a-zA-Z0-9.]+)):(?<val>[^\\[{].*)";
        /// <summary>
        /// Regex pattern string for extracting the first line of a multi-line propertyName
        /// </summary>
        private const string c_multilinePropertyPattern = "^(?<leader>:?)(?<name>([A-Z_][_a-zA-Z0-9]+)):(?<delim>[\\[{])(?<val>.*)";

        /// <summary>
        /// Regex for extracting a multi-line propertyName
        /// </summary>
        private static Regex s_multilinePropertyRegex = new Regex(c_multilinePropertyPattern);
        /// <summary>
        /// Regex for extracting a single line propertyName
        /// </summary>
        private static Regex s_propertyRegex = new Regex(c_propertyPattern);

        public static string ClosingDelimiterForOpeningMultilinePropertyDelimiter(string open)
        {
            if (open == null)
            {
                throw new ArgumentNullException("open");
            }

            switch (open)
            {
                case "[":
                    return "]";
                case "{":
                    return "}";
            }
            throw new FormatException("Illegal multiline property delimiter: " + open);
        }
        public static bool IsBehaviorPropertyDelimiter(string s)
        {
            return s == "{" || s == "}";
        }
        public static Regex MultilinePropertyRegex
        {
            get
            {
                return s_multilinePropertyRegex;
            }
        }
        public static Regex PropertyRegex
        {
            get
            {
                return s_propertyRegex;
            }
        }

        public static int CountExternalLinks(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0; 
            }

            int index = 0;
            int countHttp = 0; 
            int countHttps = 0; 

            while (true)
            {
                int nextHttp = text.IndexOf("http://", index, StringComparison.InvariantCultureIgnoreCase);
                int nextHttps = text.IndexOf("https://", index, StringComparison.InvariantCultureIgnoreCase); 
 
                if ((nextHttp == -1) && (nextHttps == -1))
                {
                    break; 
                }
                
                if (nextHttp == -1)
                {
                    nextHttp = text.Length; 
                }

                if (nextHttps == -1)
                {
                    nextHttps = text.Length; 
                }
                
                if (nextHttp < nextHttps)
                {
                    ++countHttp; 
                    index = nextHttp + "http://".Length; 
                }
                else if (nextHttps < nextHttp)
                {
                    ++countHttps; 
                    index = nextHttps + "https://".Length; 
                }
                else
                {
                    throw new Exception("The values for nextHttps and nextHttp should never be equal. Unexpected situation."); 
                }
            }

            return countHttp + countHttps; 
        }
        public static ParsedTopic Parse(string text)
        {
            TopicPropertyCollection properties = ParseProperties(text);
            TopicRevisionCollection topicLinks = ParseTopicLinks(text);
            ExternalReferencesMap externalReferences = ParseExternalReferences(text);

            ParsedTopic parsedTopic = new ParsedTopic(properties, topicLinks, externalReferences);

            return parsedTopic;
        }
        public static ParsedTopic Parse(TextReader textReader)
        {
            return Parse(textReader.ReadToEnd());
        }
        public static IList<string> SplitTopicPropertyValue(string value)
        {
            string[] values = value.Split(',');

            List<string> listOfValues = new List<string>(values.Length);

            foreach (string v in values)
            {
                listOfValues.Add(v.Trim());
            }

            return listOfValues;
        }
        /// <summary>
        /// Remove any escape characters that are used to force string to be wiki names that wouldn't otherwise be (e.g., '[' and ']')
        /// </summary>
        // TODO: Make this private once parsing moves out of Formatter and into this class       
        public static string StripTopicNameEscapes(string v)
        {
            string answer = v;
            answer = answer.Replace("[", "");
            answer = answer.Replace("]", "");
            return answer;
        }

        /// <summary>
        /// Returns true if the delimiter should be included in the property value. 
        /// </summary>
        /// <param name="delimiter">The delimiter to check.</param>
        /// <returns>True if the delimiter should be part of the property value. False otherwise.</returns>
        private static bool IsInclusivePropertyDelimiter(string delimiter)
        {
            return delimiter == "{" || delimiter == "}";
        }
        private static ExternalReferencesMap ParseExternalReferences(string text)
        {
            // TODO: Move Formatter functionality to Parser

            ExternalReferencesMap references = new ExternalReferencesMap();

            foreach (string line in text.Split('\n'))
            {
                string l = line.Replace("\r", "");
                Formatter.StripExternalWikiDef(references, l);
            }

            return references;

        }
        private static void ParseMultiLineProperties(string text, TopicPropertyCollection properties)
        {
            string[] lines = text.Split('\n');

            Regex propertyPattern = s_multilinePropertyRegex;
            bool inMultilineProperty = false;

            string currentProperty = null;
            string endDelimiter = null;

            StringBuilder rawValueBuilder = new StringBuilder();

            foreach (string line in lines)
            {
                if (inMultilineProperty)
                {
                    if (endDelimiter != null && line.TrimEnd() == endDelimiter)
                    {
                        inMultilineProperty = false;

                        // Properties enclosed by braces have to preserve the braces
                        // as part of the value of the property

                        if (IsInclusivePropertyDelimiter(endDelimiter))
                        {
                            rawValueBuilder.Append(endDelimiter); 
                        }

                        TopicProperty property;
                        if (properties.Contains(currentProperty))
                        {
                            property = properties[currentProperty];
                        }
                        else
                        {
                            property = new TopicProperty(currentProperty);
                            properties.Add(property);
                        }
                        property.Values.Add(new TopicPropertyValue(rawValueBuilder.ToString()));
                    }
                    else
                    {
                        rawValueBuilder.Append(line);
                        rawValueBuilder.Append("\n");
                    }
                }
                else
                {
                    Match match = propertyPattern.Match(line);
                    if (match.Success)
                    {
                        inMultilineProperty = true;
                        currentProperty = match.Groups["name"].Value;
                        string delimiter = match.Groups["delim"].Value;

                        switch (delimiter)
                        {
                            case "[":
                                endDelimiter = "]";
                                break;
                            case "{":
                                endDelimiter = "}";
                                break;
                            default:
                                endDelimiter = null;
                                break;
                        }

                        string rawValue = match.Groups["val"].Value;
                        
                        // If it was delimited by a curly brace, we have to preserve everything, including
                        // the curly brace. 
                        if (IsInclusivePropertyDelimiter(endDelimiter))
                        {
                            rawValue = delimiter + rawValue + "\n";
                        }
                        else
                        {
                            rawValue = rawValue.Trim();
                        }

                        rawValueBuilder.Length = 0;
                        rawValueBuilder.Append(rawValue);
                    }
                }
            }
        }
        private static TopicPropertyCollection ParseProperties(string text)
        {
            TopicPropertyCollection properties = new TopicPropertyCollection();

            ParseSingleLineProperties(text, properties);
            ParseMultiLineProperties(text, properties); 

            return properties;
        }
        private static void ParseSingleLineProperties(string text, TopicPropertyCollection properties)
        {
            Regex propertyPattern = new Regex(c_propertyPattern, RegexOptions.Multiline);

            foreach (Match match in propertyPattern.Matches(text))
            {
                string name = match.Groups["name"].Value;
                string rawValue = match.Groups["val"].Value.Trim();

                TopicProperty topicProperty = null;
                if (!properties.Contains(name))
                {
                    topicProperty = new TopicProperty(name);
                    properties.Add(topicProperty);
                }
                else
                {
                    topicProperty = properties[name];
                }

                TopicPropertyValue value = new TopicPropertyValue(rawValue);

                topicProperty.Values.Add(value);

            }
        }
        private static TopicRevisionCollection ParseTopicLinks(string text)
        {
            TopicRevisionCollection referencedTopics = new TopicRevisionCollection();

            // TODO: Move Formatter functionality to TopicParser
            MatchCollection wikiNames = Formatter.extractWikiNames.Matches(text);

            List<string> processed = new List<string>();

            foreach (Match m in wikiNames)
            {
                string each = m.Groups["topic"].ToString();
                if (processed.Contains(each))
                {
                    continue;   // skip duplicates
                }

                processed.Add(each);

                TopicRevision referencedTopic = new TopicRevision(StripTopicNameEscapes(each));
                referencedTopics.Add(referencedTopic);
            }

            return referencedTopics;
        }

    }
}
