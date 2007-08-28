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
using System.Data;

namespace FlexWiki.UnitTests.SqlProvider
{
    internal static class TestDataSets
    {
        internal static DataSet Default()
        {
            DataSet result = Empty();

            result.Tables["Namespace"].Rows.Add(1, "NamespaceOne");
            result.Tables["Namespace"].Rows.Add(2, "NamespaceTwo");

            int topicId = 1;
            AddTopicRow(result,
                ref topicId,
                1,
                "TopicOne", 
                new DateTime(2003, 3, 23, 16, 1, 0),
                new DateTime(2003, 3, 23, 16, 0, 0),
                0,
                0,
                1,
                0,
                "This is some content");
            AddTopicRow(result,
                ref topicId,
                1,
                "TopicTwo",
                new DateTime(2003, 3, 23, 16, 3, 0),
                new DateTime(2003, 3, 23, 16, 2, 0),
                0,
                0,
                1,
                0,
                "This is some other content");
            AddTopicRow(result,
                ref topicId,
                1,
                "HomePage(2003-11-24-20-31-20-WINGROUP-davidorn)",
                new DateTime(2003, 11, 24, 20, 31, 20),
                new DateTime(2003, 11, 24, 20, 31, 20),
                1,
                0,
                1,
                0,
                "Modified content.");
            AddTopicRow(result,
                ref topicId,
                1,
                "HomePage",
                new DateTime(2003, 11, 24, 20, 31, 20),
                new DateTime(2003, 11, 24, 20, 31, 20),
                0,
                0,
                1,
                0,
                "Home page."); 

            AddTopicRow(result, 
                ref topicId, 
                1, 
                "CodeImprovementIdeas", 
                new DateTime(2004, 11, 10), 
                new DateTime(2004, 11, 10), 
                0, 
                0, 
                1, 
                0, 
                @"Latest");

            AddTopicRow(result, 
                ref topicId, 
                1, 
                "CodeImprovementIdeas(2003-11-23-14-34-03-127.0.0.1)", 
                new DateTime(2004, 11, 09), 
                new DateTime(2004, 11, 09), 
                1, 
                0, 
                1, 
                0, 
                @"Latest");

            AddTopicRow(result, 
                ref topicId, 
                1, 
                "CodeImprovementIdeas(2003-11-23-14-34-04.8890-127.0.0.1)", 
                new DateTime(2004, 11, 08), 
                new DateTime(2004, 11, 08), 
                1, 
                0, 
                1, 
                0, 
                @"Older");
                       
            AddTopicRow(result, 
                ref topicId, 
                1, 
                "CodeImprovementIdeas(2003-11-23-14-34-05.1000-127.0.0.1)", 
                new DateTime(2004, 11, 07), 
                new DateTime(2004, 11, 07), 
                1, 
                0, 
                1, 
                0, 
                @"Still older");
            
            AddTopicRow(result, 
                ref topicId, 
                1, 
                "CodeImprovementIdeas(2003-11-23-14-34-06.1-127.0.0.1)", 
                new DateTime(2004, 11, 06), 
                new DateTime(2004, 11, 06), 
                1, 
                0, 
                1, 
                0, 
                @"Even older");
            
            AddTopicRow(result, 
                ref topicId, 
                1, 
                "CodeImprovementIdeas(2003-11-23-14-34-07-Name)", 
                new DateTime(2004, 11, 05), 
                new DateTime(2004, 11, 05), 
                1, 
                0, 
                1, 
                0, 
                @"Really old");
            
            AddTopicRow(result, 
                ref topicId, 
                1, 
                "CodeImprovementIdeas(2003-11-23-14-34-08.123-Name)", 
                new DateTime(2004, 11, 04), 
                new DateTime(2004, 11, 04), 
                1, 
                0, 
                1, 
                0, 
                @"Oldest");

            AddTopicRow(result,
                ref topicId,
                1,
                "ReadOnlyTopic(2003-11-23-14-34-09.123-Name)",
                new DateTime(2004, 11, 04),
                new DateTime(2004, 11, 04),
                1,
                0,
                0,
                0,
                @"Content of a read-only topic");

            AddTopicRow(result,
                ref topicId,
                1,
                "ReadOnlyTopic",
                new DateTime(2004, 11, 04),
                new DateTime(2004, 11, 04),
                0,
                0,
                0,
                0,
                @"Content of a read-only topic");

            return result; 

        }
        internal static DataSet Empty()
        {
            DataSet result = new DataSet();
            DataTable namespaceTable = new DataTable("Namespace");

            namespaceTable.Columns.Add("NamespaceId", typeof(int));
            namespaceTable.Columns.Add("Name", typeof(string));

            DataTable topicTable = new DataTable("Topic");

            topicTable.Columns.Add("TopicId", typeof(int));
            topicTable.Columns.Add("NamespaceId", typeof(int));
            topicTable.Columns.Add("Name", typeof(string));
            topicTable.Columns.Add("LastWriteTime", typeof(DateTime));
            topicTable.Columns.Add("CreationTime", typeof(DateTime));
            topicTable.Columns.Add("Archive", typeof(int));
            topicTable.Columns.Add("Deleted", typeof(int));
            topicTable.Columns.Add("Writable", typeof(int));
            topicTable.Columns.Add("Hidden", typeof(int));
            topicTable.Columns.Add("Body", typeof(string));

            result.Tables.Add(namespaceTable);
            result.Tables.Add(topicTable);

            return result; 
        }

        private static void AddTopicRow(DataSet result,
            ref int topicId,
            int namespaceId,
            string topicName, 
            DateTime lastWriteTime,
            DateTime creationTime,
            int archive,
            int deleted,
            int writable,
            int hidden,
            string body)
        {
            result.Tables["Topic"].Rows.Add(
                topicId++,
                namespaceId,
                topicName, 
                lastWriteTime,
                creationTime,
                archive,
                deleted,
                writable,
                hidden,
                body);
        }

    }
}
