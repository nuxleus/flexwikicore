using System;
using System.Text.RegularExpressions;

using FlexWiki;

namespace FlexWiki.Formatting
{
    public class ParserRule
    {
        private string _end;
        private string _jump;
        private string _pattern;
        private string _optimization;
        private string[] _elements;
        private Regex _optRegExp;
        private ParserContext _parentContext;
        private string _womElement;
        private ParserContext _context;

        public ParserRule(string womObject, string pattern, string end, string jump, 
                            string optimization, string[] elements, ParserContext ruleContext, ParserContext context)
        {
            _parentContext = context;
            if (!String.IsNullOrEmpty(womObject))
            {
                _womElement = womObject;
            }
            else
            {
                _womElement = context.WomElement;
            }
            _pattern = pattern;
            _end = end;
            _jump = jump;
            _optimization = optimization;
            _elements = elements;

            if (!String.IsNullOrEmpty(_jump))
            {
                string filename;
                switch (_jump)
                {
                    case "wikiTalkMultiline":
                    case "womMultilineCode":
                        filename = context.ParserApplication.GrammarMultiLinePath;
                        break;
                    case "womIncludeTopicName":
                        filename = context.ParserApplication.GrammarIncludeTopicPath;
                        break;
                    case "womCell":
                    case "womMultilineCell":
                        filename = context.ParserApplication.GrammarWomCellPath;
                        break;
                    case "womTableStyle":
                        filename = context.ParserApplication.GrammarWomTableStylePath;
                        break;
                    case "womStyledCode":
                        filename = context.ParserApplication.GrammarWomStyledCodePath;
                        break;
                    case "womWikiStyledText":
                    case "womStrongText":
                    case "womEmbeddedHeaderText":
                        filename = context.ParserApplication.GrammarWomStyledTextPath;
                        break;
                    default:
                            filename = context.ParserApplication.GrammarWomTextPath;
                        break;
                }
                _context = new ParserContext(_jump, context.ParserApplication, this, filename);
            }
            else if (!String.IsNullOrEmpty(_end))
            {
                    _context = new ParserContext("", context.ParserApplication, this);
            }
            else
            {
                //origina javascript read:
                //this.context = def.context || context;
                if (ruleContext != null)
                {
                    _context = ruleContext;
                }
                else
                {
                    _context = context;
                }
            }
        }

        public string Pattern
        {
            get { return _pattern; }
        }

        public string End
        {
            get { return _end; }
        }

        public string Jump
        {
            get { return _jump; }
        }
        public ParserContext Context
        {
            get { return _context; }
        }
        public ParserContext ParentContext
        {
            get { return _parentContext; }
        }
        public string[] Elements
        {
            get { return _elements; }
        }
        public Regex OptRegExp
        {
            get 
            {
                if (_optRegExp == null)
                {
                    _optRegExp = new Regex(_optimization, RegexOptions.Multiline | RegexOptions.Compiled);
                }
                return _optRegExp; }
        }
        public string WomElement
        {
            get { return _womElement; }
        }
    }
}
