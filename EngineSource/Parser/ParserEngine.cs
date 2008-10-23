using System;
//using System.Diagnostics;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Xsl;

using FlexWiki;
using FlexWiki.Collections;

namespace FlexWiki.Formatting
{
    public class ParserEngine : IWikiToPresentation
    {

        private QualifiedTopicRevision _behaviorTopic;
        private Federation _fed;
        private NamespaceManager _mgr;
        private ParserEngine _engine;
        private ParserContext _context;
        private QualifiedTopicRevision _topic;
        private string _appPath;
        private WomDocument _womDocument;
        //private WikiInputDocument _wikiInputDoc;
        private XslCompiledTransform _xslt;
        private XsltDocument _xsltDoc;
        private bool _processBehaviorToText;
        //private Stopwatch _stopwatch;
        private int chunkcnt;
        private const int _chunk = 400; //this is used to optimize the regex matches by creatng strings 2000 bytes long to perform
                                       // matches on, rather than working on the whole document at one time

        private static string emailAddressString = @"([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)";
        private static Regex emailAddress = new Regex(emailAddressString, RegexOptions.Compiled);
        private static Regex externalWikiRef = new Regex(@"(\s)*(?<param>[\w\d\.]+)@(?<behavior>[\w\d]+([\w\d]{1,})+([\w\d\.])*)(\s)*",RegexOptions.Compiled);

        //private static string multilinePreString = @"(?:\r\n)(?<PreBody>{@(?<KeyValue>[A-Z][a-zA-Z0-9]+)(?<PreText>.*?)\r\n}@(\k<KeyValue>+?))(?:\r\n)";
        private static string multilinePreString = @"(?<PreBody>\r\n{@(?<KeyValue>[A-Z][a-zA-Z0-9]+)(?<PreText>.*?)\r\n}@(\k<KeyValue>+?))(?:\r\n)";
        private static Regex multilinePre = new Regex(multilinePreString, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        private static string findEmoticonString = @"(\(A\)|\(a\))|(\(B\)|\(b\))|(\(C\)|\(c\))|(\(D\)|\(d\))|(\(E\)|\(e\))|(\(F\)|\(f\))|(\(G\)|\(g\))|(\(H\)|\(h\))|(\(I\)|\(i\))|(\(K\)|\(k\))|(\(L\)|\(l\))|(\(M\)|\(m\))|(\(N\))|(\(O\)|\(o\))|(\(P\)|\(p\))|(\(S\))|(\(T\)|\(t\))|(\(U\)|\(u\))|(\(W\)|\(w\))|(\(X\)|\(x\))|(\(Y\))|(\(Z\)|\(z\))|(\(6\))|(\(8\))|(\({\))|(\(}\))|(\(~\))|(\(@\))|(\(\*\))|(\(\^\))|(:-\[)|(:-\)|:\))|(:-D)|(:-O)|(:-P)|(:-\(|:\()|(:-S)|(:-\||:\|)|(:'\()|(:$|:-$)|(:-@|:@)|(;-\)|;\))|(\(n\))|(\(y\))|(\(Bn\))|(\(By\))|(%%-)";
        private static Regex findEmoticon = new Regex(findEmoticonString, RegexOptions.Compiled);

        private static string findIncludedTopicsString = @"(?<LineStart>\<Para\>\<IncludedTopic\>)\{\{(?<IncludedTopic>[^\}\r\n]+)\}\}(?<LineEnd>\</IncludedTopic\>\</Para\>)";
        private static Regex findIncludedTopic = new Regex(findIncludedTopicsString, RegexOptions.Multiline | RegexOptions.Compiled);

        // need to add a \| to LineStart set for behavior in a table
        //private static string findWikiBehaviorString = @"(?<LineStart>^[\t\p{N}\p{L}\p{S}\p{Zs}\.\*\{\};&%]*)@@(?<Behavior>[\[\{\p{L}\r\n]+[\p{L}\p{N}\p{C}\p{P}\p{Z}\p{S}\]\}\{\[#\*-[@]]*)@@(?!"""")(?<LineEnd>[\p{L}\p{N}\p{P}\p{S}\p{Z}]*\r\n*)";
        private static string findWikiBehaviorString = @"((?<LineStart>^[^@\r\n]*?)\<WikiTalkString\>@@(?<Behavior>(?<=@{2})[^@]+?(?=@{2}))@@\</WikiTalkString\>(?!"""")(?<LineEnd>.*?\r\n))";
        private static Regex findWikiBehavior = new Regex(findWikiBehaviorString, RegexOptions.Compiled | RegexOptions.Multiline);

        private static string preBehaviorString = @"(^ {1}[^@\{\|\}]*)|(\r\n\t[^1\*])";
        private static Regex preBehavior = new Regex(preBehaviorString, RegexOptions.Multiline | RegexOptions.Compiled);

        private static string insideTableString = @"\|\|\}|\{\|\|\}|\{\|\|";
        private static Regex insideTable = new Regex(insideTableString, RegexOptions.Compiled);

        //private static Regex escAmpersand = new Regex(@"(&(?!amp;|#|nbsp;|gt;|lt;script|lt;/script))", RegexOptions.Compiled);
        private static Regex escAmpersand = new Regex(@"(&(?!amp;|#|nbsp;|lt;|gt;))", RegexOptions.Compiled);
        private static Regex escLeftAngle = new Regex(@"<", RegexOptions.Compiled);
        private static Regex escRightAngle = new Regex(@">", RegexOptions.Compiled);

        ///// <summary>
        ///// Create a new formatter for a string of input content.
        ///// </summary>
        ///// <param name="source">Input wiki string</param>
        ///// <param name="output">Output object to which output is sent</param>
        ///// <param name="namespaceManager">The ContentProviderChain that contains the wiki string text</param>
        ///// <param name="maker">A link maker </param>
        ///// <param name="external">External wiki map</param>
        ///// <param name="headingLevelBase">Relative heading level</param>
        ///// 
        //Formatter(QualifiedTopicRevision topic, string source, WikiOutput output, NamespaceManager namespaceManager,
        //    LinkMaker maker, ExternalReferencesMap external, int headingLevelBase)
        //{
        //    _topic = topic;
        //    _source = SplitStringIntoStyledLines(source, LineStyle.Unchanged);
        //    _linkMaker = maker;
        //    NamespaceManager = namespaceManager;
        //    _output = output;
        //    _externalWikiMap = external;
        //    _headingLevelBase = headingLevelBase;
        //}

        ///// <summary>
        ///// Create a new formatter for an input list of StyledLines
        ///// </summary>
        ///// <param name="source">Input wiki content as a list of StyledLines</param>
        ///// <param name="output">Output object to which output is sent</param>
        ///// <param name="namespaceManager">The ContentProviderChain that contains the wiki string text</param>
        ///// <param name="maker">A link maker </param>
        ///// <param name="external">External wiki map</param>
        ///// <param name="headingLevelBase">Relative heading level</param>
        ///// 
        //Formatter(QualifiedTopicRevision topic, IList source, WikiOutput output, NamespaceManager namespaceManager,
        //    LinkMaker maker, ExternalReferencesMap external, int headingLevelBase)
        //{
        //    _topic = topic;
        //    _source = source;
        //    _output = output;
        //    _linkMaker = maker;
        //    NamespaceManager = namespaceManager;
        //    _externalWikiMap = external;
        //    _headingLevelBase = headingLevelBase;
        //}

        public ParserEngine(Federation fed)
        {
            Initialize();
            if ((bool)fed.Application["DisableNewParser"])
            {
                _engine = this;
            }
            else
            {
                if (fed.Application.ExecutionEnvironment == ExecutionEnvironment.Production)
                {
                    _appPath = fed.Application.ResolveRelativePath("admin");
                }
                else
                {
                    _appPath = System.Environment.CurrentDirectory;
                    _appPath = System.IO.Path.Combine(_appPath, @"..\..\..\FlexWiki.Web\admin");
                }
                _fed = fed;
                Regex.CacheSize = 250;
                _context = new ParserContext("WikiText", this, false);
                //_xslt = new XslCompiledTransform(true); //debug enabled for stylesheet during development
                _xslt = new XslCompiledTransform();
                _xslt.Load(XsltPath);
                _processBehaviorToText = false;
            }
           
        }

        private void Initialize()
        {
            _engine = this;
        }
        public string AppPath
        {
            get { return _appPath; }
        }
        public string GetMainPath
        {
            get { return Path.Combine(_appPath, "womContext.xml"); }
        }

        public string GrammarMultiLinePath
        {
            get { return Path.Combine(_appPath, "womMultiLine.xml"); }
        }
        public string GrammarIncludeTopicPath
        {
            get { return Path.Combine(_appPath, "womIncludeTopic.xml"); }
        }
        public string GrammarWomTextPath
        {
            get { return Path.Combine(_appPath, "womText.xml"); }
        }
        public string GrammarWomCellPath
        {
            get { return Path.Combine(_appPath, "womCell.xml"); }
        }
        public string GrammarWomTableStylePath
        {
            get { return Path.Combine(_appPath, "womTableStyle.xml"); }
        }
        public string GrammarWomStyledCodePath
        {
            get { return Path.Combine(_appPath, "womStyledCode.xml"); }
        }
        public string GrammarWomStyledTextPath
        {
            get { return Path.Combine(_appPath, "womStyledText.xml"); }
        }
        public string XsltPath
        {
            get { return Path.Combine(_appPath, "FlexWikiWeb.xslt"); }
        }
        //public WikiInputDocument WikiInputDocument
        //{
        //    get { return _wikiInputDoc; }
        //    set { _wikiInputDoc = value; }
        //}
        public GrammarDocument InitGrammarDocument(string path)
        {
            return new GrammarDocument(path);
        }
        public WomDocument WomDocument
        {
            get { return _womDocument; }
        }

        public ParserEngine Engine
        {
            get { return _engine; }
        }
        //The ParserEngine can return different formats depending upon the xslt document used
        public XsltDocument XsltDoc(string path)
        {
            return _xsltDoc = new XsltDocument(path);
        }
        public string FormattedTopic(QualifiedTopicRevision topic, OutputFormat fmt, QualifiedTopicRevision diff)
        {
            string wikitext = "";
            //OutputFormat is not really used at this point but is retained as it would be the selector for the xslt file to be used
            if (diff == null)
            {
                WomDocument topicDoc = new WomDocument(null);
                topicDoc.ParsedDocument = FormattedTopic(topic, fmt);
                wikitext = WikiToPresentation(topicDoc.XmlDoc);
            }
            else
            {
                ArrayList styledLines = new ArrayList();
                IList leftLines;
                IList rightLines;
                string leftString = FormattedTopic(topic, fmt);
                string rightString = FormattedTopic(diff, fmt);
                leftLines = leftString.Replace("\r", "").Split('\n');
                rightLines = rightString.Replace("\r", "").Split('\n');
                IEnumerable diffs = Diff.Compare(leftLines, rightLines);
                WomDocument diffDoc = new WomDocument(null);
                foreach (LineData ld in diffs)
                {
                    LineStyle style = LineStyle.Unchanged;
                    switch (ld.Type)
                    {
                        case LineType.Common:
                            style = LineStyle.Unchanged;
                            break;

                        case LineType.LeftOnly:
                            style = LineStyle.Add;
                            break;

                        case LineType.RightOnly:
                            style = LineStyle.Delete;
                            break;
                    }
                    if (ld.Text != "</WomDocument>")
                    {
                        diffDoc.AddDiff(ld.Text + "\r\n", style);
                    }
                }
                diffDoc.AddDiff("</WomDocument>", LineStyle.Unchanged);
                wikitext = WikiToPresentation(diffDoc.XmlDoc);
                //Format(topic, styledLines, output, relativeToBase, linker, relativeToBase.ExternalReferences, 0);
            }
            return wikitext;

        }
        public string FormattedTopic(QualifiedTopicRevision topic, OutputFormat fmt)
        {
            //initial version does not handle diffs
            string wikitext = "";
            string interpreted = "";
            _mgr = _fed.NamespaceManagerForNamespace(topic.Namespace);
            _topic = topic;
            WomDocument.ResetTableOfContents();
            WomDocument.anchors = new string[25];
            _processBehaviorToText = true;
            _behaviorTopic = topic;

            //Normally get data from the cache, but when debugging Womdocument need to avoid using cached data
            //string wom = _mgr.GetTopicProperty(topic.AsUnqualifiedTopicRevision(), "_Wom").LastValue;
            string wom = "";

            if (!String.IsNullOrEmpty(wom))
            {
                WomDocument xmldoc = new WomDocument(null);
                wom = findWikiBehavior.Replace(wom, new MatchEvaluator(wikiBehaviorMatch));
                xmldoc.ParsedDocument = findWikiBehavior.Replace(wom, new MatchEvaluator(wikiBehaviorMatch));

                if (findIncludedTopic.IsMatch(xmldoc.ParsedDocument))
                {
                    string included = xmldoc.ParsedDocument;
                    included = findIncludedTopic.Replace(included, new MatchEvaluator(wikiIncludedTopic));
                    xmldoc.ParsedDocument = included;
                }

                interpreted = xmldoc.ParsedDocument;
                xmldoc = null;
            }
            else
            {
                using (TextReader sr = _mgr.TextReaderForTopic(topic.AsUnqualifiedTopicRevision()))
                {
                    wikitext = sr.ReadToEnd() + "\r\n";
                }
                wikitext = escape(wikitext);
                int size = 0;
                while (String.IsNullOrEmpty(interpreted))
                {
                    try
                    {
                        string tempsize = _mgr.GetTopicProperty(topic.AsUnqualifiedTopicRevision(), "_ProcessTextSize").LastValue;
                        Int32.TryParse(tempsize, out size);
                        if (size == 0)
                        {
                            size = _chunk;
                        }
                        WomDocument xmldoc = ProcessText(wikitext, topic, _mgr, false, size);
                        string womdoc = xmldoc.ParsedDocument;
                        _mgr.SetWomCache(topic.AsUnqualifiedTopicRevision(), womdoc);
                        womdoc = findWikiBehavior.Replace(womdoc, new MatchEvaluator(wikiBehaviorMatch));
                        xmldoc.ParsedDocument = findWikiBehavior.Replace(womdoc, new MatchEvaluator(wikiBehaviorMatch));
                        
                        _behaviorTopic = null;

                        if (findIncludedTopic.IsMatch(xmldoc.ParsedDocument))
                        {
                            string included = xmldoc.ParsedDocument;
                            included = findIncludedTopic.Replace(included, new MatchEvaluator(wikiIncludedTopic));
                            xmldoc.ParsedDocument = included;
                        }

                        interpreted = xmldoc.ParsedDocument;
                        xmldoc = null;
                    }
                    catch (XmlException ex)
                    {
                        _mgr.SetProcessTextSize(topic.AsUnqualifiedTopicRevision(), size * (chunkcnt + 1));
                        string error = ex.ToString();
                    }
                }
            }
            _processBehaviorToText = false;
            return interpreted;
        }

        //this is the main body where the real parsing work is done < 30 lines of code
        public WomDocument ProcessText(string wikiInput, QualifiedTopicRevision topic, NamespaceManager mgr, bool fragment, int size)
        {
            //_stopwatch = Stopwatch.StartNew();
            ParserContext savedcontext;
            bool redo = false;
            wikiInput = wikiInput.Replace("\r\n\r\n", "\r\n");
            if (!fragment)
            {
                wikiInput = "\r\n" + wikiInput;
            }
            else
            {
                while (wikiInput.EndsWith("\r\n"))
                {
                    wikiInput = wikiInput.Remove(wikiInput.LastIndexOf("\r\n"));
                }
            }
            _womDocument = new WomDocument(null);
            _womDocument.Fed = _fed;
            _womDocument.Mgr = mgr;
            _womDocument.FragmentOnly = fragment;
            _womDocument.Begin();
            
            int chunk = _chunk;  //set the initial chunk size (for cache use)
            if (size > 0)
            {
                chunk = size;
            }
            chunkcnt = 1;
            while (_context.ParentRule != null)
            {
                if (_context.ParentRule.ParentContext != null)
                {
                    _context = _context.ParentRule.ParentContext;
                }
            }
            if (!String.IsNullOrEmpty(wikiInput))
            {
                StringBuilder source = new StringBuilder();
                StringBuilder temp = new StringBuilder();
                source.AppendLine(externalWikiRef.Replace(wikiInput, new MatchEvaluator(externalWikiRefMatch)));
                temp.Append(multilinePre.Replace(source.ToString(), new MatchEvaluator(multilinePreMatch)));
                source = temp;
                string savedtemp = temp.ToString();
                MatchCollection matches;
                while (source.Length > 0)
                {
                    string womElement = _context.WomElement;
                    //optimize here by passing in less than the full string when source is very long
                    //this gives a 5 times performance improvement
                    int matchcnt = 0;

                    if (source.Length > chunk * chunkcnt)
                    {
                        matches = _context.RegExp.Matches(source.ToString(0, chunk * chunkcnt));
                        matchcnt = matches.Count;
                    }
                    else
                    {
                        matches = _context.RegExp.Matches(source.ToString());
                        matchcnt = matches.Count;
                    }
                    if (matchcnt > 0)
                    {
                        if (matches[0].Index > 0)
                        {
                            _womDocument.SimpleAdd(source.ToString(0, matches[0].Index), womElement, "", _context.RuleList[0]);
                        }
                        int x = 1;
                        if (_context.RegExpStr.StartsWith("(?:") && !matches[0].Value.StartsWith("%"))
                        {
                            x = 0;
                        }
                        int cnt = matches[0].Groups.Count;
                        for (int i = x; i < cnt; i++)
                        {
                            if (matches[0].Groups[i].Success)
                            {
                                if (womElement != "WikiText")
                                {
                                    //we are in a child rule set with an end condition
                                    //whereas WikiText ends by running out of source or matches
                                    if (i == 1)
                                    {
                                        i = 0;
                                    }
                                }
                                if (_womDocument.InTable && _womDocument.InItem && matches[0].Value.StartsWith("%"))
                                {
                                    i++;  //add one to the index here as we are in womText rules twice for this condition
                                }

                                ParserRule rule = _context.RuleList[i];
                                savedcontext = _context;
                                string addElement;
                                if (!String.IsNullOrEmpty(rule.WomElement))
                                {
                                    addElement = rule.WomElement;
                                }
                                else
                                {
                                    addElement = womElement;
                                }
                                _womDocument.SimpleAdd(matches[0].Value, addElement, rule.Jump, rule);
                                if (_womDocument.ParsedDocument.Contains("<<"))  //an error occurred
                                {
                                    chunkcnt++; //increase the chunk size and redo
                                    redo = true;
                                }
                                else
                                {
                                    if (addElement != "WikiStylingEnd")
                                    {
                                        _context = rule.Context;
                                    }
                                    //still in a line - only pop one context item
                                    else if (matches[0].Value == "%%" || matches[0].Value == "%" || matches[0].Value.Contains("{||"))  
                                    {
                                        _context = _context.ParentRule.ParentContext;
                                    }
                                    else //done with that line - pop all context back to start
                                    {
                                        while (_context.ParentRule != null)
                                        {
                                            if (_context.ParentRule.ParentContext != null)
                                            {
                                                _context = _context.ParentRule.ParentContext;
                                            }
                                        }
                                    }
                                }
                                break;
                            }                      
                        }
                        if (!redo) //an error occurred
                        {
                            bool modifyRemove = false;

                            if (womElement == "womListText" && matches[0].Value.Contains("{||")) //need to leave this bit in source
                            {
                                modifyRemove = true;
                            }
                            else if (womElement == "womWikiStyledText" && matches[0].Value == "%")
                            {
                                modifyRemove = true;
                            }

                            if (modifyRemove)
                            {
                                source.Remove(0, matches[0].Index);
                            }
                            else
                            {
                                source.Remove(0, matches[0].Index + matches[0].Length);
                            }
                        }
                        else
                        {
                            //reset and start over with larger chunk
                            source = new StringBuilder();
                            source.Append(savedtemp);
                            _womDocument = new WomDocument(null);
                            _womDocument.Fed = _fed;
                            _womDocument.Mgr = mgr;
                            _womDocument.FragmentOnly = fragment;
                            _womDocument.Begin();
                            redo = false;
                            while (_context.ParentRule != null)
                            {
                                if (_context.ParentRule.ParentContext != null)
                                {
                                    _context = _context.ParentRule.ParentContext;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (source.Length > chunk * chunkcnt) //no match in that chunk, increase size and retry
                        {
                            source = new StringBuilder();
                            source.Append(savedtemp);
                            _womDocument = new WomDocument(null);
                            _womDocument.Fed = _fed;
                            _womDocument.Mgr = mgr;
                            _womDocument.FragmentOnly = fragment;
                            _womDocument.Begin();
                            chunkcnt++;
                            while (_context.ParentRule != null)
                            {
                                if (_context.ParentRule.ParentContext != null)
                                {
                                    _context = _context.ParentRule.ParentContext;
                                }
                            }
                        }
                        else
                        {
                            _womDocument.SimpleAdd(source.ToString(), womElement, "", _context.RuleList[0]);
                            source.Length = 0;
                        }
                    }
                }
                source = null;
                temp = null;
            }
            _womDocument.End();
            if (((bool)_fed.Application["DisableWikiEmoticons"] == false)) //&& (_mgr.DisableNamespaceEmoticons == false))
            {
                MatchCollection emoticonMatches = findEmoticon.Matches(_womDocument.ParsedDocument);
                if (emoticonMatches.Count > 0)
                {
                    _womDocument.ConvertEmoticons(emoticonMatches);
                }
            }
            //_stopwatch.Stop();
            if (chunkcnt > 1)
            {
                mgr.SetProcessTextSize(topic.AsUnqualifiedTopicRevision(), chunk * chunkcnt);
            }
            
            return _womDocument;
        }

        public ParserContext EngineContext
        {
            get { return _context; }
        }
        public ParserEngine ParserApplication
        {
            get { return this; }
        }
        private string multilinePreMatch(Match match)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(@"
<PreformattedMultilineKeyed>");
            result.AppendLine(match.Groups["PreText"].Value);
            result.AppendLine(@"</PreformattedMultilineKeyed>");
            return result.ToString();
        }
        private string externalWikiRefMatch(Match match)
        {
            // match.Value may contain some additional leading and/or trailing white space that we don't want to lose.
            string result = match.Value;
            if (!emailAddress.IsMatch(result))
            {
                // Build a string containing the actual match.
                string noSpace = match.Groups["param"].Value + "@" + match.Groups["behavior"].Value;
                // Replace that in the whole match with the external wiki ref macro.
                result = result.Replace(noSpace, "@@InterWiki(\"$behavior\", \"$param\", \"$param\")@@");
                // Replace the parameters with the regex matches.
                result = result.Replace("$behavior", match.Groups["behavior"].Value).Replace("$param", match.Groups["param"].Value);
            }
            return result;
        }
        private string wikiBehaviorMatch(Match match)
        {
            string replacement = match.ToString();
            string linestart = match.Groups["LineStart"].Value;
            string lineend = match.Groups["LineEnd"].Value;

            bool doBehavior = false;
            if (insideTable.IsMatch(linestart))
            {
                doBehavior = true;
            }
            else if (!preBehavior.IsMatch(linestart))
            {
                doBehavior = true;
            }

            if (doBehavior)
            {
                string expr = match.Groups["Behavior"].Value;
                TopicContext tc = new TopicContext(_fed, _mgr, new TopicVersionInfo(_fed, _behaviorTopic));
                BehaviorInterpreter interpreter = new BehaviorInterpreter(_behaviorTopic == null ? "" : _behaviorTopic.DottedName, expr, _fed, _fed.WikiTalkVersion, this);
                if (!interpreter.Parse())
                {   //parse failed
                    replacement = ErrorMessage(null, interpreter.ErrorString);
                }
                else
                {   //parse succeeded
                    if (!interpreter.EvaluateToPresentation(tc, _mgr.ExternalReferences))
                    {   //eval failed
                        replacement = ErrorMessage(null, interpreter.ErrorString);
                        _processBehaviorToText = false;
                    }
                    else
                    {   //eval succeeded
                        WikiOutput nOut = new WomDocument(null);
                        interpreter.Value.OutputTo(nOut);
                        replacement = nOut.ToString();
                    }
                    
                }
                if (replacement.Contains("<script"))
                {
                    replacement = Regex.Replace(replacement, @"<script(.*?)>", @"&lt;script$1&gt;", RegexOptions.Singleline);
                    replacement = Regex.Replace(replacement, @"</script>", @"&lt;/script&gt;");
                }
                replacement = Regex.Replace(replacement, @"^\|\|", "||}", RegexOptions.Multiline);
                replacement = Regex.Replace(replacement, @" \|\|$| \|\|\r\n| \{\|\|\r\n|\|\|\r\n", " {||\r\n", RegexOptions.Multiline);
                replacement = Regex.Replace(replacement, @"(?<!\{|"""")\|\|(?!\}|"""")", @" {||}");
                if (_processBehaviorToText)
                {
                    replacement = replacement.Replace("        ", "\t");
                    replacement = escAmpersand.Replace(replacement, "&amp;"); //full escape causes some pages to fail
                    replacement = ProcessText(replacement, _behaviorTopic, _mgr, true, 350).ParsedDocument;
                }
            }
            replacement = linestart + replacement + lineend;
            return replacement;
        }
        private string wikiIncludedTopic(Match match)
        {
            string replacement = match.ToString();
            string linestart = match.Groups["LineStart"].Value;

            if (!preBehavior.IsMatch(linestart))
            {
                string topic = match.Groups["IncludedTopic"].Value;
                TopicRevision topicRevision = new TopicRevision(topic);
                int size = 0;

                if (_mgr.TopicExists(topicRevision, ImportPolicy.IncludeImports))
                {
                    string ns = _mgr.UnambiguousTopicNameFor(topicRevision.LocalName).Namespace;
                    NamespaceManager containingNamespaceManager = _fed.NamespaceManagerForNamespace(ns);
                    QualifiedTopicRevision abs = new QualifiedTopicRevision(topicRevision.LocalName, ns);
                    if (containingNamespaceManager.HasPermission(new UnqualifiedTopicName(abs.LocalName), TopicPermission.Read))
                    {
                        replacement = containingNamespaceManager.Read(abs.LocalName.TrimEnd());
                    }
                    _behaviorTopic = abs;
                    string tempsize = _mgr.GetTopicProperty(_behaviorTopic.AsUnqualifiedTopicRevision(), "_ProcessTextSize").LastValue;
                    Int32.TryParse(tempsize, out size);
                    if (size == 0)
                    {
                        size = _chunk;
                    }
                    replacement = findWikiBehavior.Replace(replacement, new MatchEvaluator(wikiBehaviorMatch));
                    replacement = replacement.Replace("        ", "\t");
                    replacement = escape(replacement);
                    if (_processBehaviorToText)
                    {
                        WomDocument xmldoc = ProcessText(replacement, abs, _mgr, true, size);
                        replacement = xmldoc.ParsedDocument;
                    }
                    else
                    {
                        _processBehaviorToText = true;
                    }
                    _behaviorTopic = null;
                }
            }
            return replacement + "\r\n";
        }
        public string ErrorMessage(string title, string body)
        {
            // We can't use the fancy ErrorString method -- it didn't exist in v0
            WikiOutput nOut = new WomDocument(null);
            nOut.WriteErrorMessage(title, body);
            return nOut.ToString();
        }
        public IWikiToPresentation WikiToPresentation(QualifiedTopicRevision topic, WikiOutput output,
           NamespaceManager namespaceManager, LinkMaker maker, ExternalReferencesMap external, int headingLevelBase, bool fragment)
        {
            //ArrayList lines = new ArrayList();
            //lines.Add(new StyledLine("", LineStyle.Unchanged));
            //return new Formatter(topic, lines, output, namespaceManager, maker, external, headingLevelBase);
            WomDocument doc = (WomDocument)output;
            doc.Mgr = namespaceManager;
            doc.Fed = _fed;
            doc.FragmentOnly = fragment;
            output.Begin();
            return this;
        }
        public string WikiToPresentation(XmlDocument doc)
        {
            //return NestedFormat(s, Output);
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter sw = new StringWriter(stringBuilder);
            XhtmlTextWriter writer = new XhtmlTextWriter(sw);
            _xslt.Transform(doc, null, writer);

            return writer.InnerWriter.ToString();
        }
        public string WikiToPresentation(string s)
        {
            return s;
        }
        static public string escape(string input)
        {
            if (input == null)
                return "";
            // replace HTML special characters with character entities
            // this has the side-effect of stripping all markup from text
            string str = input;
            str = escAmpersand.Replace(str, "&amp;");
            //str = Regex.Replace(str, "\"", "&quot;");
            str = escLeftAngle.Replace(str, "&lt;");
            str = escRightAngle.Replace(str, "&gt;");
            return str;
        }
    }
}
