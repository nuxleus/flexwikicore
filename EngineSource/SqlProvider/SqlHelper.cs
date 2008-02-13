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
using System.Data;

namespace FlexWiki.SqlProvider
{
    internal class SqlHelper
    {
        private IDatabase _database;

        public SqlHelper(IDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Method to create Namespace.
        /// </summary>
        /// <param name="ns">Namespace to be created.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        public void CreateNamespace(string ns)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[1];
            parameters[0] = new DatabaseParameter("namespace", ns);

            _database.ExecuteNonQuery(CommandType.StoredProcedure, "CreateNamespace", parameters);
        }

        /// <summary>
        /// Method to delete a Namespace.
        /// </summary>
        /// <param name="ns">Namespace to delete.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        public void DeleteNamespace(string ns)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[1];
            parameters[0] = new DatabaseParameter("namespace", ns);

            _database.ExecuteNonQuery(CommandType.StoredProcedure, "DeleteNamespace", parameters);
        }

        /// <summary>
        /// Delete a specified topic.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="topicName">Name of the topic being deleted.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        public void DeleteTopic(string ns, string topicName)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[2];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName);

            _database.ExecuteNonQuery(CommandType.StoredProcedure, "DeleteTopic", parameters);
        }

        /// <summary>
        /// Get the latest version for the specified topic.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns>SqlTopicInfo containing the topic information.</returns>
        public SqlInfoForTopic GetSqlTopicInfoForLatestTopicVersion(string ns, string topicName)
        {
            SqlInfoForTopic topicInfo = null;

            DatabaseParameter[] parameters = new DatabaseParameter[2];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName + "(%)"); // pattern for sql wild card search

            _database.ExecuteReader(CommandType.StoredProcedure, 
                "GetSqlTopicInfoForLatestVersion", 
                delegate(IDataReader reader)
                {
                    while (reader.Read())
                    {
                        topicInfo = new SqlInfoForTopic(reader.GetString(0), reader.GetDateTime(1));
                    }
                }, 
                parameters); 

            return topicInfo;
        }

        /// <summary>
        /// Get all the non archive topics in the specified namespace.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="sort">Boolean value to indicate if the topics need to be sorted in the 
        /// descending order based on last write time.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns>Array of SqlTopicInfo containing the information for all non archive topics in the namespace.</returns>
        public SqlInfoForTopic[] GetSqlTopicInfoForNonArchiveTopics(string ns, bool sort)
        {
            ArrayList topicInfos = new ArrayList();

            DatabaseParameter[] parameters = new DatabaseParameter[1];
            parameters[0] = new DatabaseParameter("namespace", ns);

            string storedProcedureName = "GetSqlTopicInfoForNonArchiveTopics";
            if (sort)
            {
                // To benefit from Sql compiled stored procedure cache using 2 different
                // stored procedures instead of passing the sort value thru to Sql.
                storedProcedureName = "GetSqlTopicInfoForNonArchiveTopicsSortedDescending";
            }
            _database.ExecuteReader(CommandType.StoredProcedure, 
                storedProcedureName, 
                delegate(IDataReader reader)
                {
                    while (reader.Read())
                    {
                        SqlInfoForTopic topicInfo = new SqlInfoForTopic(reader.GetString(0), reader.GetDateTime(1));
                        topicInfos.Add(topicInfo);
                    }
                }, 
                parameters);
    
            return (SqlInfoForTopic[])topicInfos.ToArray(typeof(SqlInfoForTopic));
        }

        /// <summary>
        /// Gets all versions for the specified topic.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="topicName">Name of the topic to fetch the archive topics.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns>Array of SqlTopicInfo for all the archive versions of the specified topic.</returns>
        public SqlInfoForTopic[] GetSqlTopicInfosForTopic(string ns, string topicName)
        {
            ArrayList topicInfos = new ArrayList();

            DatabaseParameter[] parameters = new DatabaseParameter[2];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName + "(%)"); // pattern for sql wild card search

            _database.ExecuteReader(CommandType.StoredProcedure, 
                "GetSqlTopicArchiveInfos",
                delegate (IDataReader reader)
                {
                    while (reader.Read())
                    {
                        SqlInfoForTopic topicInfo = new SqlInfoForTopic(reader.GetString(0), reader.GetDateTime(1));
                        topicInfos.Add(topicInfo);
                    }
                }, 
                parameters); 

            return (SqlInfoForTopic[])topicInfos.ToArray(typeof(SqlInfoForTopic));
        }

        /// <summary>
        /// Gets all versions for the specified topic since the specified date and including the 
        /// specified date.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="topicName">Name of the topic to fetch the archive topics.</param>
        /// <param name="stamp">Datetime indicating the lower bound for topic last write time.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns>Array of SqlTopicInfo for all the archive versions of the specified topic.</returns>
        public SqlInfoForTopic[] GetSqlTopicInfosForTopicSince(string ns, string topicName, DateTime stamp)
        {
            ArrayList topicInfos = new ArrayList();

            DatabaseParameter[] parameters = new DatabaseParameter[3];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName + "(%)"); // pattern for sql wild card search
            // Sql allow only dates between January 1, 1753 through December 31, 9999
            if (stamp.Year > 1800)
            {
                parameters[2] = new DatabaseParameter("stamp", stamp);
            }
            else
            {
                parameters[2] = new DatabaseParameter("stamp", new DateTime(1800, 1, 1));
            }

            _database.ExecuteReader(CommandType.StoredProcedure,
                "GetSqlTopicArchiveInfosSince",
                delegate(IDataReader reader)
                {
                    while (reader.Read())
                    {
                        SqlInfoForTopic topicInfo = new SqlInfoForTopic(reader.GetString(0), reader.GetDateTime(1));
                        topicInfos.Add(topicInfo);
                    }
                },
                parameters); 

            return (SqlInfoForTopic[])topicInfos.ToArray(typeof(SqlInfoForTopic));
        }

        /// <summary>
        /// Get the contents of the topic.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="topicName">Name of the topic to fetch the topic body.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns>Returns the topic contents</returns>
        public string GetTopicBody(string ns, string topicName)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[2];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName);

            return _database.ExecuteScalar(CommandType.StoredProcedure, "GetTopicBody", parameters) as string;
        }

        /// <summary>
        /// Get the time when the topic was created.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="topicName">Name of the topic</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns>Creation Time of the topic.</returns>
        public DateTime GetTopicCreationTime(string ns, string topicName)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[3];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName);
            parameters[2] = new DatabaseParameter("topicCreationTime", SqlDbType.DateTime);
            parameters[2].Direction = ParameterDirection.Output;

            _database.ExecuteScalar(CommandType.StoredProcedure, "GetTopicCreationTime", parameters);

            return (DateTime)parameters[2].Value;
        }

        /// <summary>
        /// Get the time when the topic was last updated.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="topicName">Name of the Topic.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns>Last Update Time for the topic.</returns>
        public DateTime GetTopicLastWriteTime(string ns, string topicName)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[3];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName);
            parameters[2] = new DatabaseParameter("topicLastWriteTime", SqlDbType.DateTime);
            parameters[2].Direction = ParameterDirection.Output;

            _database.ExecuteScalar(CommandType.StoredProcedure, "GetTopicLastWriteTime", parameters);

            return (DateTime)parameters[2].Value;
        }

        /// <summary>
        /// Check if the topic is writable. All topics are by default writable in the Sql store./
        /// We do not have support to set this property from the web interface.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns>Boolean value indicating if the topic is writable.</returns>
        public bool IsExistingTopicWritable(string ns, string topicName)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[3];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName);
            parameters[2] = new DatabaseParameter("topicWritable", SqlDbType.Bit);
            parameters[2].Direction = ParameterDirection.Output;

            _database.ExecuteScalar(CommandType.StoredProcedure, "IsExistingTopicWritable", parameters);

            return BooleanConvert(parameters[2].Value.ToString());
        }

        /// <summary>
        /// Check if the Topic Exists in the namespace.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns>Boolean value that indicates if the topic exists.</returns>
        public bool TopicExists(string ns, string topicName)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[3];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName);
            parameters[2] = new DatabaseParameter("topicExists", SqlDbType.Bit);
            parameters[2].Direction = ParameterDirection.Output;

            _database.ExecuteScalar(CommandType.StoredProcedure, "CheckTopicExists", parameters);

            return BooleanConvert(parameters[2].Value.ToString());
        }

        /// <summary>
        /// Method to check if the NamespaceExists.
        /// </summary>
        /// <param name="ns">Namespace to check for existence.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <returns></returns>
        public bool NamespaceExists(string ns)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[2];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("namespaceExists", SqlDbType.Bit);
            parameters[1].Direction = ParameterDirection.Output;

            _database.ExecuteScalar(CommandType.StoredProcedure, "CheckNamespaceExists", parameters);

            return BooleanConvert(parameters[1].Value.ToString());
        }


        /// <summary>
        /// Store the specified topic in the database.
        /// </summary>
        /// <param name="ns">Namespace of the topic.</param>
        /// <param name="topicName">Name of the topic being saved.</param>
        /// <param name="lastWriteTime">The write time for the topic.</param>
        /// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        /// <param name="body">The contents of the topic being saved.</param>
        /// <param name="archive">Indicates if this is the archive record for the topic being saved.</param>
        public void WriteTopic(string ns, string topicName, DateTime lastWriteTime, string body, bool archive)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[5];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName);
            parameters[2] = new DatabaseParameter("body", body);
            parameters[3] = new DatabaseParameter("archive", archive);
            parameters[4] = new DatabaseParameter("lastWriteTime", lastWriteTime);

            _database.ExecuteNonQuery(CommandType.StoredProcedure, "WriteTopic", parameters);
        }

        public void WriteTopicLock(string ns, string topicName)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[2];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName);

            _database.ExecuteNonQuery(CommandType.StoredProcedure, "WriteTopicLock", parameters);
        }
        public void WriteTopicUnlock(string ns, string topicName)
        {
            DatabaseParameter[] parameters = new DatabaseParameter[2];
            parameters[0] = new DatabaseParameter("namespace", ns);
            parameters[1] = new DatabaseParameter("topicName", topicName);

            _database.ExecuteNonQuery(CommandType.StoredProcedure, "WriteTopicUnlock", parameters);
        }
        private bool BooleanConvert(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input"); 
            }

            bool result;
            if (bool.TryParse(input, out result))
            {
                return result;
            }
            else
            {
                if (input.Equals("1"))
                {
                    return true; 
                }
                else if (input.Equals("0"))
                {
                    return false;
                }
                else
                {
                    throw new FormatException("String was not recognized as a valid boolean: " + input); 
                }
            }
        }


        ///// <summary>
        ///// Rename the specified topic with the new name.
        ///// </summary>
        ///// <param name="ns">Namespace of the topic.</param>
        ///// <param name="topicCurrentName">Current Name of the topic</param>
        ///// <param name="topicNewName">New Name for the topic.</param>
        ///// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
        //public  void RenameTopic(string ns, string topicCurrentName, string topicNewName, string connectionString)
        //{
        //    DatabaseParameter[] parameters = new DatabaseParameter[3];
        //    parameters[0] = new DatabaseParameter("namespace", ns);
        //    parameters[1] = new DatabaseParameter("topicCurrentName", topicCurrentName);
        //    parameters[2] = new DatabaseParameter("topicNewName", topicNewName);

        //    _database.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "RenameTopic", parameters);
        //}

    }
}
