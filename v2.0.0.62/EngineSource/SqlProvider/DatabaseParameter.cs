using System;
using System.Data; 

namespace FlexWiki.SqlProvider
{
    public class DatabaseParameter
    {
        private ParameterDirection _direction; 
        private string _name;
        private object _value; 

        public DatabaseParameter(string name, object value)
        {
            _name = name;
            _value = value; 
        }

        public ParameterDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public string Name
        {
            get { return _name; }
        }

        public object Value
        {
            get { return _value; }
            set
            {
                if (Direction == ParameterDirection.Input)
                {
                    throw new ReadOnlyException("Cannot change the value of an input parameter."); 
                }

                _value = value; 
            }
        }
    }
}
