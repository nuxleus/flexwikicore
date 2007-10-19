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
using System.Data;
using System.Data.SqlClient;
using System.Reflection; 

using FlexWiki.SqlProvider; 

namespace FlexWiki.UnitTests.SqlProvider
{
    internal class MockDatabase : IDatabase
    {
        private string _connectionString;
        private DataSet _data; 

        internal MockDatabase(DataSet data)
        {
            _data = data; 
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString; 
            }
            set
            {
                _connectionString = value; 
            }
        }
        public DataTable NamespaceTable
        {
            get { return _data.Tables["Namespace"]; }
        }
        public DataTable TopicTable
        {
            get { return _data.Tables["Topic"]; }
        }

        public void ExecuteNonQuery(CommandType commandType, string sql, params DatabaseParameter[] parameters)
        {
            if (commandType != CommandType.StoredProcedure)
            {
                throw new ArgumentException("Only stored procedures are supported by MockDatabase.");
            }

            // Reflect over this class and find a method whose name matches "Sproc" plus the stored procedure name. 
            MethodInfo method = GetMethodForSproc(sql);

            if (method == null)
            {
                throw new NotImplementedException("Support for stored procedure not implemented: " + sql);
            }

            method.Invoke(this, new object[] { parameters }); 

        }
        public void ExecuteReader(CommandType commandType, string sql, Action<IDataReader> action, 
            params DatabaseParameter[] parameters)
        {
            if (commandType != CommandType.StoredProcedure)
            {
                throw new ArgumentException("Only stored procedures are supported by MockDatabase."); 
            }

            // Reflect over this class and find a method whose name matches "Sproc" plus the stored procedure name. 
            MethodInfo method = GetMethodForSproc(sql);

            if (method == null)
            {
                throw new NotImplementedException("Support for stored procedure not implemented: " + sql);
            }

            using (IDataReader reader = (IDataReader)method.Invoke(this, new object[] { parameters }))
            {
                action(reader); 
            }
        }
        public object ExecuteScalar(CommandType commandType, string sql, params DatabaseParameter[] parameters)
        {
            if (commandType != CommandType.StoredProcedure)
            {
                throw new ArgumentException("Only stored procedures are supported by MockDatabase.");
            }

            // Reflect over this class and find a method whose name matches "Sproc" plus the stored procedure name. 
            MethodInfo method = GetMethodForSproc(sql);

            if (method == null)
            {
                throw new NotImplementedException("Support for stored procedure not implemented: " + sql);
            }

            return method.Invoke(this, new object[] { parameters }); 
        }

        private string BuildLikeClause(string column, string pattern)
        {
            // We can't quite do exactly the same thing that SQL does, but we can
            // change 
            //
            // Name LIKE 'Foo%Bar'
            //
            // into
            // 
            // Name LIKE 'Foo%' AND name LIKE '%Bar'
            //
            // which isn't quite the same thing, but close enough/

            int numWildcards = 0;
            foreach (char c in pattern)
            {
                if (c == '%')
                {
                    ++numWildcards;
                }
            }

            if (numWildcards == 0)
            {
                return string.Format("{0} LIKE '{1}'", column, pattern);
            }
            else if (numWildcards == 1)
            {
                int indexOfWildcard = pattern.IndexOf('%');

                string before = pattern.Substring(0, indexOfWildcard + 1);
                string after = pattern.Substring(indexOfWildcard);

                return string.Format("({0} LIKE '{1}') AND ({0} LIKE '{2}')", column, before, after);
            }
            else
            {
                throw new ArgumentException("Only a single wildcard is supported: " + pattern);
            }

        }
        private DataRow FindTopicRow(int namespaceId, string topicName)
        {
            foreach (DataRow row in TopicTable.Rows)
            {
                if ((int)row["NamespaceId"] == namespaceId)
                {
                    if ((string)row["Name"] == topicName)
                    {
                        return row;
                    }
                }
            }
            return null;
        }
        private MethodInfo GetMethodForSproc(string sproc)
        {
            MethodInfo method = this.GetType().GetMethod("Sproc" + sproc,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            return method;
        }
        private int GetNamespaceId(string nsName)
        {
            foreach (DataRow row in NamespaceTable.Rows)
            {
                if (row["Name"].ToString().Equals(nsName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return Convert.ToInt32(row["NamespaceId"]);
                }
            }

            return -1; 
        }
        private object GetParameterValue(DatabaseParameter[] parameters, string parameterName)
        {
            foreach (DatabaseParameter parameter in parameters)
            {
                if (string.Equals(parameter.Name, parameterName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return parameter.Value;
                }
            }

            return null;
        }
        private bool SprocCheckNamespaceExists(DatabaseParameter[] parameters)
        {
            int namespaceId = GetNamespaceId((string)GetParameterValue(parameters, "namespace"));

            bool found = (namespaceId != -1);

            parameters[1].Value = found;
            return found; 
        }
        private bool SprocCheckTopicExists(DatabaseParameter[] parameters)
        {
            int namespaceId = GetNamespaceId((string)GetParameterValue(parameters, "namespace")); 
            string topicName = (string) GetParameterValue(parameters, "topicName");

            DataRow row = FindTopicRow(namespaceId, topicName);

            bool found = (row != null); 

            parameters[2].Value = found; 
            return found; 
        }
        private void SprocDeleteNamespace(DatabaseParameter[] parameters)
        {
            int namespaceId = GetNamespaceId((string)GetParameterValue(parameters, "namespace")); 

            List<DataRow> rowsToRemove = new List<DataRow>();
            foreach (DataRow row in TopicTable.Rows)
            {
                if (((int) row[TopicTable.Columns["namespaceId"]]) == namespaceId)
                {
                    rowsToRemove.Add(row); 
                }
            }

            rowsToRemove.ForEach(delegate(DataRow row) { TopicTable.Rows.Remove(row); });

            for (int i = 0; i < NamespaceTable.Rows.Count; i++)
            {
                if ((int)NamespaceTable.Rows[i][NamespaceTable.Columns["NamespaceId"]] == namespaceId)
                {
                    NamespaceTable.Rows.RemoveAt(i);
                    break; 
                }
            }

            _data.AcceptChanges(); 
        }
        private void SprocDeleteTopic(DatabaseParameter[] parameters)
        {
            int namespaceId = GetNamespaceId((string)GetParameterValue(parameters, "namespace"));
            string topicName = (string)GetParameterValue(parameters, "topicName");

            DataRow row = FindTopicRow(namespaceId, topicName);

            TopicTable.Rows.Remove(row);
            TopicTable.AcceptChanges(); 
        }
        private IDataReader SprocGetSqlTopicArchiveInfosSince(DatabaseParameter[] parameters)
        {
            string name = (string) GetParameterValue(parameters, "topicName");
            DateTime stamp = (DateTime) GetParameterValue(parameters, "stamp");
            int namespaceId = GetNamespaceId((string) GetParameterValue(parameters, "namespace")); 

            DataTable results = new DataTable();
            results.Columns.Add("Name", typeof(string));
            results.Columns.Add("LastWriteTime", typeof(DateTime));

            // Unfortunately DataTable only allows for wildcards at the beginning or end
            // of the pattern, so we have to cheat. 
            string nameLikeClause = BuildLikeClause("Name", name); 

            string filter = string.Format("Archive = 1 AND {0} AND LastWriteTime >= '{1}' AND NamespaceId = {2}", 
                nameLikeClause, stamp, namespaceId);
            string sort = "LastWriteTime DESC";

            DataRow[] rows; 
            try
            {
                rows = TopicTable.Select(filter, sort);
            }
            catch (Exception x)
            {
                System.Diagnostics.Debugger.Log(0, string.Empty, x.ToString()); 
                throw; 
            }

            foreach (DataRow row in rows)
            {
                results.ImportRow(row);
            }

            return new MockDataReader(results); 
            
        }
        private IDataReader SprocGetSqlTopicInfoForNonArchiveTopics(DatabaseParameter[] parameters)
        {
            int namespaceId = GetNamespaceId((string)GetParameterValue(parameters, "namespace"));

            DataTable results = new DataTable();
            results.Columns.Add("Name", typeof(string));
            results.Columns.Add("LastWriteTime", typeof(DateTime));

            string filter = string.Format("Archive = 0 AND NamespaceId = {0}", namespaceId);

            DataRow[] rows;
            try
            {
                rows = TopicTable.Select(filter);
            }
            catch (Exception x)
            {
                System.Diagnostics.Debugger.Log(0, string.Empty, x.ToString());
                throw;
            }

            foreach (DataRow row in rows)
            {
                results.ImportRow(row);
            }

            return new MockDataReader(results); 

        }
        private string SprocGetTopicBody(DatabaseParameter[] parameters)
        {
            int namespaceId = GetNamespaceId((string)GetParameterValue(parameters, "namespace"));
            string topicName = (string)GetParameterValue(parameters, "topicName");

            DataRow row = FindTopicRow(namespaceId, topicName);

            return row["Body"] as string; 
        }
        private bool SprocIsExistingTopicWritable(DatabaseParameter[] parameters)
        {
            int namespaceId = GetNamespaceId((string)GetParameterValue(parameters, "namespace"));
            string topicName = (string)GetParameterValue(parameters, "topicName");

            DataRow row = FindTopicRow(namespaceId, topicName);

            bool writable = (Convert.ToInt32(row["Writable"]) != 0);
            parameters[2].Value = writable;
            return writable; 
        }
        private void SprocWriteTopic(DatabaseParameter[] parameters)
        {
            int namespaceId = GetNamespaceId((string)GetParameterValue(parameters, "namespace"));
            string topicName = (string)GetParameterValue(parameters, "topicName");
            string body = (string)GetParameterValue(parameters, "body");
            bool archive = (bool)GetParameterValue(parameters, "archive");
            DateTime lastWriteTime = (DateTime)GetParameterValue(parameters, "lastWriteTime");

            DataRow row = FindTopicRow(namespaceId, topicName);

            if (row == null)
            {
                DataRow newRow = TopicTable.NewRow();
                newRow["NamespaceId"] = namespaceId;
                newRow["Name"] = topicName;
                newRow["Archive"] = archive;
                newRow["CreationTime"] = lastWriteTime;
                newRow["LastWriteTime"] = lastWriteTime;
                newRow["Body"] = body;

                TopicTable.Rows.Add(newRow);
            }
            else
            {
                row["Body"] = body;
                row["LastWriteTime"] = lastWriteTime; 
            }

            TopicTable.AcceptChanges(); 
        }

    }
}
