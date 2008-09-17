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
using System.Data.SqlClient; 

namespace FlexWiki.SqlProvider
{
    internal class SqlDatabase : IDatabase
    {
        private string _connectionString;

        internal SqlDatabase()
        {
        }

        string IDatabase.ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        void IDatabase.ExecuteNonQuery(CommandType commandType, string sql, params DatabaseParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = commandType;
                    command.CommandText = sql;

                    foreach (DatabaseParameter parameter in parameters)
                    {
                        command.Parameters.Add(CreateParameter(parameter)); 
                    }

                    command.ExecuteNonQuery();

                    // Because some parameters might be output or retval parameters, 
                    // we need to be sure to copy the values back out. 
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        CopyOutputParameter(command.Parameters[i], parameters[i]); 
                    }
                }

                connection.Close();
                connection.Dispose(); 
            }
        }
        void IDatabase.ExecuteReader(CommandType commandType, string sql, Action<IDataReader> action,
            params DatabaseParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = commandType;
                    command.CommandText = sql;

                    foreach (DatabaseParameter parameter in parameters)
                    {
                        command.Parameters.Add(CreateParameter(parameter));
                    }

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        action(reader);
                    }

                    // Because some parameters might be output or retval parameters, 
                    // we need to be sure to copy the values back out. 
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        CopyOutputParameter(command.Parameters[i], parameters[i]);
                    }
                }

                connection.Close();
                connection.Dispose();
            }

        }
        object IDatabase.ExecuteScalar(CommandType commandType, string sql, params DatabaseParameter[] parameters)
        {
            object result; 
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = commandType;
                    command.CommandText = sql;

                    foreach (DatabaseParameter parameter in parameters)
                    {
                        command.Parameters.Add(CreateParameter(parameter));
                    }

                    result = command.ExecuteScalar();

                    // Because some parameters might be output or retval parameters, 
                    // we need to be sure to copy the values back out. 
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        CopyOutputParameter(command.Parameters[i], parameters[i]);
                    }
                }

                connection.Close();
                connection.Dispose();
            }

            return result; 
        }

        private void CopyOutputParameter(SqlParameter sqlParameter, DatabaseParameter databaseParameter)
        {
            if (sqlParameter.Direction != ParameterDirection.Input)
            {
                databaseParameter.Value = sqlParameter.Value; 
            }
        }

        private SqlParameter CreateParameter(DatabaseParameter parameter)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.Direction = parameter.Direction;
            sqlParameter.ParameterName = "@" + parameter.Name;
            sqlParameter.Value = parameter.Value;

            return sqlParameter; 
        }
    }
}
