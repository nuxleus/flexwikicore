using System;
using System.Data;

namespace FlexWiki.SqlProvider
{
    public interface IDatabase
    {
        string ConnectionString { get; set; }

        void ExecuteNonQuery(CommandType commandType, string sql, params DatabaseParameter[] parameters);
        void ExecuteReader(CommandType commandType, string sql, Action<IDataReader> action, 
            params DatabaseParameter[] parameters);
        object ExecuteScalar(CommandType commandType, string sql, params DatabaseParameter[] parameters);
    }
}
