using System;
using System.IO;

using FlexWiki;

namespace FlexWiki.Formatting
{
    public class XsltDocument
    {
        private string _output;
        private string _filename;
        //private XmlTextReader _reader;



        public XsltDocument(string filename)
        {
            _filename = filename; ;
            _output = "";
        }


        public string Output
        {
            get { return _output; }
            set { _output = value; }
        }

        public void Read()
        {
            try
            {
                TextReader tr = new StreamReader(_filename);
                _output = tr.ReadToEnd();
                tr.Close();
            }
            catch
            {

            }
        }
    }
}