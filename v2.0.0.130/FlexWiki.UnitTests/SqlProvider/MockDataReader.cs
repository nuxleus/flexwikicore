using System;
using System.Data; 

namespace FlexWiki.UnitTests.SqlProvider
{
    internal class MockDataReader : IDataReader
    {
        private DataTable _data;
        private int _rowIndex = -1; 

        internal MockDataReader(DataTable data)
        {
            _data = data; 
        }

        void IDataReader.Close()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        int IDataReader.Depth
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
        DataTable IDataReader.GetSchemaTable()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        bool IDataReader.IsClosed
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
        bool IDataReader.NextResult()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        bool IDataReader.Read()
        {
            if (_rowIndex >= _data.Rows.Count - 1)
            {
                return false; 
            }

            ++_rowIndex;
            return true; 
        }
        int IDataReader.RecordsAffected
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        void IDisposable.Dispose()
        {
        }

        int IDataRecord.FieldCount
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
        bool IDataRecord.GetBoolean(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        byte IDataRecord.GetByte(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        char IDataRecord.GetChar(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        IDataReader IDataRecord.GetData(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        string IDataRecord.GetDataTypeName(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        DateTime IDataRecord.GetDateTime(int i)
        {
            return (DateTime)GetCurrentRowValue(i); 
        }
        decimal IDataRecord.GetDecimal(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        double IDataRecord.GetDouble(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        Type IDataRecord.GetFieldType(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        float IDataRecord.GetFloat(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        Guid IDataRecord.GetGuid(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        short IDataRecord.GetInt16(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        int IDataRecord.GetInt32(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        long IDataRecord.GetInt64(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        string IDataRecord.GetName(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        int IDataRecord.GetOrdinal(string name)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        string IDataRecord.GetString(int i)
        {
            return (string) GetCurrentRowValue(i); 
        }
        object IDataRecord.GetValue(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        int IDataRecord.GetValues(object[] values)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        bool IDataRecord.IsDBNull(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        object IDataRecord.this[string name]
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
        object IDataRecord.this[int i]
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        private object GetCurrentRowValue(int i)
        {
            return _data.Rows[_rowIndex][i]; 
        }

    }
}
