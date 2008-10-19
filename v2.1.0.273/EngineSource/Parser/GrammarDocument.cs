using System;
using System.IO;
using System.Text;
using System.Xml;

using FlexWiki;

namespace FlexWiki.Formatting
{
    public class GrammarDocument
    {
        private string _output;
        private string _filename;
        private XmlTextReader _reader;


        public GrammarDocument(string filename)
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

        public void Write()
        {
            try
            {
                FileInfo file = new FileInfo(_filename);
                if (file.Exists)
                {
                    file.Delete();
                }

                StreamWriter sw = file.CreateText();
                sw.Write(_output);
                sw.Close();
            }
            catch
            {
            }
        }

        public void ReadRules(ParserContext context)
        {
            _reader = new XmlTextReader(_filename);
            string womObject = "";
            string pattern = "";
            string end = "";
            string jump = "";
            string optimization = "";
            string temp = "";
            string[] elements = null;
            ParserRule rule;

            while (_reader.Read())
            {
                switch (_reader.NodeType)
                {
                    case XmlNodeType.Element:
                      
                        switch (_reader.LocalName)
                        {
                            case "Rule":
                                womObject = _reader.GetAttribute("Name");
                                    pattern = _reader.GetAttribute("Pattern");
                                    optimization = _reader.GetAttribute("Optimization");
                                    temp = _reader.GetAttribute("Elements");
                                    if (!String.IsNullOrEmpty(temp))
                                    {
                                        elements = temp.Split(',');
                                    }
                                    if (_reader.AttributeCount == 6)
                                    {
                                        end = _reader.GetAttribute("End");
                                        jump = _reader.GetAttribute("Jump");
                                    }
                                    rule = new ParserRule(womObject, pattern, end, jump, optimization, elements, null, context);
                                    context.AddRule(rule);
                                    womObject = "";
                                    pattern = "";
                                    end = "";
                                    jump = "";
                                    optimization = "";
                                    elements = null;
                                break;
                        }
                    break;
               }
            }
        }

    }
}