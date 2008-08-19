using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using FlexWiki;

namespace FlexWiki.Formatting
{
    public class ParserContext
    {
        private bool _ignoreCase;
        private StringBuilder _regExpStr;
        private string _womElement;
        private GrammarDocument _grammar;
        private ParserRuleList _ruleList;
        private ParserRule _parentRule;
        private ParserEngine _engine;
        private Regex _regExp;

        public ParserContext(string womElement, ParserEngine engine, bool ignoreCase)
        {
            _ignoreCase = false;
            _womElement = womElement;
            _regExpStr = new StringBuilder();
            _regExp = null;
            _engine = engine;
            _grammar = new GrammarDocument(_engine.GetMainPath);
            _ruleList = new ParserRuleList();
            _parentRule = null;
            _grammar.ReadRules(this);
            
        }

        public ParserContext(string womElement, ParserEngine engine, ParserRule rule, string filename)
        {
            _womElement = womElement;
            _ignoreCase = rule.ParentContext.IgnoreCase;
            _regExpStr = new StringBuilder();
            _regExp = null;
            _engine = engine;
            _grammar = new GrammarDocument(filename);
            _ruleList = new ParserRuleList();
            _parentRule = rule;
            AddParentRule(rule);
            _grammar.ReadRules(this);
        }
        public ParserContext(string womElement, ParserEngine engine, ParserRule rule)
        {
            if (!String.IsNullOrEmpty(womElement))
            {
                _womElement = womElement;
            }
            else
            {
                _womElement = rule.WomElement;
            }
            _ignoreCase = rule.ParentContext.IgnoreCase;
            _regExpStr = new StringBuilder();
            _regExp = null;
            _engine = engine;
            _ruleList = new ParserRuleList();
            _parentRule = rule;
            AddParentRule(rule);
        }

        public Regex RegExp
        {

            get {
                    if (_regExp == null)
                    {
                        _regExp = new Regex(RegExpStr, RegexOptions.Multiline | RegexOptions.Compiled);
                        //Regex.CacheSize = 200;
                    }
                    return _regExp;
                }
        }

        public string RegExpStr
        {
            get
            {
                return _regExpStr.ToString();
            }
            set
            {
                if (!String.IsNullOrEmpty(_regExpStr.ToString()))
                {
                    _regExpStr.Append("|");
                }
                _regExpStr.AppendFormat("({0})", value);
            }
        }
        public bool IgnoreCase
        {
            get { return _ignoreCase; }
        }

        public ParserRuleList RuleList
        {
            get { return _ruleList; }
        }

        public GrammarDocument GrammarDocument
        {
            get { return _grammar; }
        }
        public ParserRule ParentRule
        {
            get { return _parentRule; }
        }
        public ParserEngine ParserApplication
        {
            get { return _engine; }
        }
        public string WomElement
        {
            get { return _womElement; }
        }
    
        public void AddRule(ParserRule rule)
        {
            _ruleList.Add(rule);
            if (!String.IsNullOrEmpty(rule.Pattern))
            {
                RegExpStr = rule.Pattern;
            }
        }
        private void AddParentRule(ParserRule rule)
        {
            if (!String.IsNullOrEmpty(rule.End))
            {
                AddRule(new ParserRule(rule.WomElement + "End", rule.End, "", "", "", null, rule.ParentContext, this));
            }
        }
    }
}