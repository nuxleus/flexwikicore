using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using FlexWiki;

namespace FlexWiki.Formatting
{
    public class WomDocument : WikiOutput
    {
        LineStyle _CurrentStyle = LineStyle.Unchanged;
        private StringBuilder _intermediate;
        private StringBuilder _templist;
        private bool _initem;
        private bool _intable;
        private bool _inpre;
        private string _lastWom = "";

        private int _lastitemlevel;
        private int _currentitemlevel;

        private string urlKeys = "https|http|ftp|mailto|ms-help|ldaps|ldap|file|gopher|news|nntp|telnet|wais|prospero";
        private string urlRegex = "://[-_.+$a-z0-9!*'(){}|\\\\^~\\[\\]<>#%\\\";/?:@&=]*[a-z0-9/]";
        private Regex urlRegExp;
        private char[] lineEnds = {'\r','\n'};
        private XmlDocument _xmldoc;
        private Random _rn;
        private string _defaultns;

        private static string textileRegexString = @"('''[^']+''')|(''[^']+'')|(\?\?[^\?]+(?<! )\?\?)|( @[^@]+(?<! )@ )|((?<= )-(?! )[^-]+(?<! )-)|((?<= )_(?! )[^_]+(?<! )_(?= ))|(``(?! )[^`]+(?<! )``)|((?<= )\+(?! )[^\+]+(?<! )\+)|((?<= )~(?! )[^~]+(?<! )~)|((?<= )\^(?! )[^\^]+(?<! )\^)|(\*(?! )[^\*\r\n]+\*)";
        private static Regex textileRegex = new Regex(textileRegexString, RegexOptions.Compiled);

        private static Regex escapedText = new Regex(@"""""[^"":]+""""", RegexOptions.Compiled);

        private static Regex tooltipRegex = new Regex(@"(?<LinkText>[^\(""]+)[ ]*\((?<TipText>[^\)]+)\)", RegexOptions.Compiled);

        private static Regex attributeRegex = new Regex(@"(?<AttributeName>[^ \r\n=]+)=['""]{1}(?<AttributeValue>[^'""]+)['""]{1}", RegexOptions.Compiled);

        private string[,] emoticons = new string[42,2];
        public static string[] anchors;

        private static string badControlCharString = @"("""")?(&#1;|&#2;|&#3;|&#4;|&#5;|&#6;|&#7;|&#8;|&#11;|&#12;|&#14;|&#15;|&#16;|&#17;|&#18;|&#19;|&#20;|&#21;|&#22;|&#23;|&#24;|&#25;|&#26;|&#27;|&#28;|&#29;|&#30;|&#31;|&#32;|&#x1;|&#x2;|&#x3;|&#x4;|&#x5;|&#x6;|&#x7;|&#x8;|&#xB;|&#xC;|&#xE;|&#xF;|&#x01;|&#x02;|&#x03;|&#x04;|&#x05;|&#x06;|&#x07;|&#x08;|&#x0B;|&#x0C;|&#x0E;|&#x0F;|&#x11;|&#x12;|&#x13;|&#x14;|&#x15;|&#x16;|&#x17;|&#x18;|&#x19;|&#x1A;|&#x1B;|&#x1C;|&#x1D;|&#x1E;|&#x1F;)("""")?";
        private static Regex badControlChar = new Regex(badControlCharString, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        private static int anchorid;
        private static int tableofcontents;
        private static Regex invalidChar = new Regex(@"[\p{Pe}\p{Pi}\p{Pf}\p{Po}\p{Ps}\p{S}]", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private string _topic;
        private Federation _fed;
        private NamespaceManager _mgr;

        private bool _fragmentOnly;
        private string _unclosedTableElements;

        public WomDocument(WikiOutput parent): base(parent)
        {
            //should use an XmlDocument instead of a StringBuilder in production version ??
            //_filename = filename;
            _intermediate = new StringBuilder(10000);
            urlRegExp = new Regex(urlKeys + urlRegex, RegexOptions.IgnoreCase);
            _templist = new StringBuilder();
            _lastitemlevel = 0;
            _rn = new Random();
            anchorid = 1;
            //tableofcontents = 1;
            //Begin();

            
        }
        override public LineStyle Style
        {
            get
            {
                return _CurrentStyle;
            }
            set
            {
                _CurrentStyle = value;
            }
        }
        override public OutputFormat Format
        {
            get
            {
                return OutputFormat.WikiObjectModel;
            }
        }
        public Federation Fed
        {
            get { return _fed; }
            set { _fed = value; }
        }
        public bool FragmentOnly
        {
            get { return _fragmentOnly; }
            set { _fragmentOnly = value; }
        }
        public NamespaceManager Mgr
        {
            get { return _mgr; }
            set { _mgr = value; }
        }
        public string Topic
        {
            get { return _topic; }
            set { _topic = value; }
        }
        public override void Begin()
        {
            if (((bool)_fed.Application["DisableWikiEmoticons"] == false) && (_mgr.DisableNamespaceEmoticons == false))
            {
                emoticons[0, 0] = @"(\(A\)|\(a\))";
                emoticons[0, 1] = @"emoticons/angel_smile.gif";
                emoticons[1, 0] = @"(\(B\)|\(b\))";
                emoticons[1, 1] = @"emoticons/beer_yum.gif";
                emoticons[2, 0] = @"(\(C\)|\(c\))";
                emoticons[2, 1] = @"emoticons/coffee.gif";
                emoticons[3, 0] = @"(\(D\)|\(d\))";
                emoticons[3, 1] = @"emoticons/martini_shaken.gif";
                emoticons[4, 0] = @"(\(E\)|\(e\))";
                emoticons[4, 1] = @"emoticons/envelope.gif";
                emoticons[5, 0] = @"(\(F\)|\(f\))";
                emoticons[5, 1] = @"emoticons/rose.gif";
                emoticons[6, 0] = @"(\(G\)|\(g\))";
                emoticons[6, 1] = @"emoticons/present.gif";
                emoticons[7, 0] = @"(\(H\)|\(h\))";
                emoticons[7, 1] = @"emoticons/shades_smile.gif";
                emoticons[8, 0] = @"(\(I\)|\(i\))";
                emoticons[8, 1] = @"emoticons/lightbulb.gif";
                emoticons[9, 0] = @"(\(K\)|\(k\))";
                emoticons[9, 1] = @"emoticons/kiss.gif";
                emoticons[10, 0] = @"(\(L\)|\(l\))";
                emoticons[10, 1] = @"emoticons/heart.gif";
                emoticons[11, 0] = @"(\(M\)|\(m\))";
                emoticons[11, 1] = @"emoticons/messenger.gif";
                emoticons[12, 0] = @"(\(N\)|\(n\))";
                emoticons[12, 1] = @"emoticons/thumbs_down.gif";
                emoticons[13, 0] = @"(\(O\)|\(o\))";
                emoticons[13, 1] = @"emoticons/clock.gif";
                emoticons[14, 0] = @"(\(P\)|\(p\))";
                emoticons[14, 1] = @"emoticons/camera.gif";
                emoticons[15, 0] = @"(\(S\))";
                emoticons[15, 1] = @"emoticons/moon.gif";
                emoticons[16, 0] = @"(\(T\)|\(t\))";
                emoticons[16, 1] = @"emoticons/phone.gif";
                emoticons[17, 0] = @"(\(U\)|\(u\))";
                emoticons[17, 1] = @"emoticons/broken_heart.gif";
                emoticons[18, 0] = @"(\(W\)|\(w\))";
                emoticons[18, 1] = @"emoticons/wilted_rose.gif";
                emoticons[19, 0] = @"(\(X\)|\(x\))";
                emoticons[19, 1] = @"emoticons/girl_handsacrossamerica.gif";
                emoticons[20, 0] = @"(\(Y\)|\(y\))";
                emoticons[20, 1] = @"emoticons/thumbs_up.gif";
                emoticons[21, 0] = @"(\(Z\)|\(z\))";
                emoticons[21, 1] = @"emoticons/guy_handsacrossamerica.gif";
                emoticons[22, 0] = @"(\(6\))";
                emoticons[22, 1] = @"emoticons/devil_smile.gif";
                emoticons[23, 0] = @"(\(8\))";
                emoticons[23, 1] = @"emoticons/musical_note.gif";
                emoticons[24, 0] = @"(\({\))";
                emoticons[24, 1] = @"emoticons/dude_hug.gif";
                emoticons[25, 0] = @"(\(}\))";
                emoticons[25, 1] = @"emoticons/girl_hug.gif";
                emoticons[26, 0] = @"(\(~\))";
                emoticons[26, 1] = @"emoticons/film.gif";
                emoticons[27, 0] = @"(\(@\))";
                emoticons[27, 1] = @"emoticons/kittykay.gif";
                emoticons[28, 0] = @"(\(\*\))";
                emoticons[28, 1] = @"emoticons/star.gif";
                emoticons[29, 0] = @"(\(\^\))";
                emoticons[29, 1] = @"emoticons/cake.gif";
                emoticons[30, 0] = @"(:-\[)";
                emoticons[30, 1] = @"emoticons/bat.gif";
                emoticons[31, 0] = @"(:-\)|:\))";
                emoticons[31, 1] = @"emoticons/regular_smile.gif";
                emoticons[32, 0] = @"(:-D)";
                emoticons[32, 1] = @"emoticons/teeth_smile.gif";
                emoticons[33, 0] = @"(:-O)";
                emoticons[33, 1] = @"emoticons/omg_smile.gif";
                emoticons[34, 0] = @"(:-P)";
                emoticons[34, 1] = @"emoticons/tounge_smile.gif";
                emoticons[35, 0] = @"(:-\(|:\()";
                emoticons[35, 1] = @"emoticons/sad_smile.gif";
                emoticons[36, 0] = @"(:-S)";
                emoticons[36, 1] = @"emoticons/confused_smile.gif";
                emoticons[37, 0] = @"(:-\||:\|)";
                emoticons[37, 1] = @"emoticons/whatchutalkingabout_smile.gif";
                emoticons[38, 0] = @"(:'\()";
                emoticons[38, 1] = @"emoticons/cry_smile.gif";
                emoticons[39, 0] = @"(:$|:-$)";
                emoticons[39, 1] = @"emoticons/embaressed_smile.gif";
                emoticons[40, 0] = @"(:-@|:@)";
                emoticons[40, 1] = @"emoticons/angry_smile.gif";
                emoticons[41, 0] = @"(;-\)|;\))";
                emoticons[41, 1] = @"emoticons/wink_smile.gif";
            }
            string siteUrl = _fed.ExposedLinkMaker.SiteURL;
            if (_fragmentOnly == false)
            {
                _intermediate.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                _intermediate.AppendLine(@"<!DOCTYPE message [");
                _intermediate.AppendLine("<!ENTITY nbsp \"&#160;\"> ]>");
                _intermediate.AppendLine(@"<WomDocument>");
                _intermediate.AppendLine(@"<BaseTopic>" + siteUrl + @"default.aspx/</BaseTopic>");
                _intermediate.AppendLine(@"<BaseEdit>" + siteUrl + @"WikiEdit.aspx?topic=</BaseEdit>");
                _intermediate.AppendLine(@"<BaseImage>" + siteUrl + @"images/</BaseImage>");
                _intermediate.AppendLine(@"<SiteUrl>" + siteUrl + @"</SiteUrl>");
                _intermediate.AppendLine(@"<InputDoc>" + _topic + "</InputDoc>");
            }
            _defaultns = _mgr.Namespace;
        }
        public static void ResetTableOfContents()
        {
            tableofcontents = 1;
        }
        public void SimpleAdd(string text, string womElement, string jump, ParserRule rule)
        {
            MatchCollection matches;
            StringBuilder temp = new StringBuilder();
            string savedtext = text;
            string ns = "";
            string topic = "";
            if (_inpre && !womElement.Contains("EmptyLine"))
            {
                if (!womElement.Contains("PreformattedSingleLine"))
                {
                    _intermediate.AppendLine("</PreformattedSingleLine>");
                    _inpre = false;
                }
            }
            if (_intable && _lastWom.Contains("TableRowEnd") && (!womElement.Contains("TableRow")))
            {
                if (_lastitemlevel > 0 && !_initem && !womElement.Contains("rderedList"))
                {
                    int indx = _templist.ToString().LastIndexOf("</item>");
                    int len = _templist.Length;
                    _templist.Remove(indx, len - indx);
                    for (int x = _lastitemlevel; x > 0; x--)
                    {
                        _templist.AppendLine("</item></list>");
                    }
                    _lastitemlevel = 0;
                    _intermediate.Append(_templist.ToString());
                    _templist = new StringBuilder();
                }
                if (!String.IsNullOrEmpty(_unclosedTableElements))
                {
                    temp = new StringBuilder();
                    string[] _tempElements = _unclosedTableElements.Split(';');
                    for (int i = _tempElements.Length - 2; i > -1; i--)
                    {
                        temp.AppendFormat("</{0}>", _tempElements[i]); 
                    }
                    _intermediate.AppendLine(temp.ToString());
                    _unclosedTableElements = "";
                }
                _intable = false;
            }
            if (_lastitemlevel > 0 && !_initem && !womElement.Contains("rderedList"))
            {
                int indx = _templist.ToString().LastIndexOf("</item>");
                int len = _templist.Length;
                _templist.Remove(indx, len - indx);
                for (int x = _lastitemlevel; x > 0; x--)
                {
                    _templist.AppendLine("</item></list>");
                }
                _lastitemlevel = 0;
                _intermediate.Append(_templist.ToString());
                _templist = new StringBuilder();
            }
            if (text == "\r\n" || String.IsNullOrEmpty(text))
            {
                if (womElement.Contains("rderedList"))
                {
                    _templist.AppendLine("</item>");
                    _initem = false;
                }
                else if (womElement == "WikiStylingEnd")
                {
                    string originalElement;
                    while (rule.ParentContext.ParentRule != null)
                    {
                        rule = rule.ParentContext.ParentRule;
                        //originalElement = rule.WomElement;
                    }
                    originalElement = rule.WomElement;
                    if (_initem)
                    {
                        if (originalElement == "MultilineTableRow")
                        {
                            _templist.AppendLine("</WikiStyling></item><Break />\r\n");
                        }
                        else
                        {
                            _templist.AppendLine("</WikiStyling></item>\r\n");
                        }
                    }
                    else if (originalElement == "MultilineTableRow")
                    {
                        WriteWom("</WikiStyling><Break />\r\n");
                    }
                    else
                    {
                        WriteWom("</WikiStyling></" + originalElement + ">\r\n");
                    }
                    _initem = false;
                }
                else if (womElement == "ParaEnd")
                {
                    WriteWom("</" + womElement.Substring(0, womElement.Length - 3) + ">\r\n");
                }
                else if (womElement == "HiddenWikiTalkMethodEnd")
                {
                    WriteWom("</HiddenWikiTalkMethod>\r\n");
                }
                else if (womElement.EndsWith("End"))
                {
                    WriteWom("</" + womElement.Substring(0, womElement.Length - 3) + ">");
                }
                else
                {
                    if (_lastWom.Contains("PreformattedSingleLine"))
                    {
                        WriteWom("<Para>");
                        //womElement = "Para";
                    }
                    else
                    {
                        if (womElement == "EmptyLine")
                        {
                            WriteWom("<EmptyLine />");
                        }
                        else
                        {
                            WriteWom("<Para>");
                        }
                    }
                }
            }
            else if (!String.IsNullOrEmpty(jump))
            {
                switch (womElement)
                {
                    case "UnorderedList":
                        if (text.StartsWith("\r\n"))
                        {
                            _currentitemlevel = text.Length - 3;
                        }
                        else
                        {
                            _currentitemlevel = text.Length - 1;
                        }
                        if (_lastitemlevel < _currentitemlevel)
                        {
                            if (_lastitemlevel > 0)
                            {
                                _templist.Remove(_templist.Length - 9, 9);
                            }
                            _templist.AppendLine("<list type=\"unordered\"><item>");
                        }
                        else if (_lastitemlevel > _currentitemlevel)
                        {
                            //_templist.Remove(_templist.Length - 9, 9);
                            for (int x = _lastitemlevel; x > _currentitemlevel; x--)
                            {
                                _templist.AppendLine("</list></item>");
                            }
                            _templist.AppendLine("<item>");
                        }
                        else  // (_lastitemlevel == text.Length - 1)
                        {
                            _templist.AppendLine("<item>");
                        }
                        _lastitemlevel = _currentitemlevel;
                        _initem = true;
                        break;
                    case "OrderedList":
                        if (text.StartsWith("\r\n"))
                        {
                            _currentitemlevel = text.Length - 4;
                        }
                        else
                        {
                            _currentitemlevel = text.Length - 2;
                        }
                        if (_lastitemlevel < _currentitemlevel)
                        {
                            if (_lastitemlevel > 0)
                            {
                                _templist.Remove(_templist.Length - 9, 9);
                            }
                            _templist.AppendLine("<list type=\"ordered\"><item>");
                        }
                        else if (_lastitemlevel > _currentitemlevel)
                        {
                            //_templist.Remove(_templist.Length - 9, 9);
                            for (int x = _lastitemlevel; x > _currentitemlevel; x--)
                            {
                                _templist.AppendLine("</list></item>");
                            }
                            //_currentitemlevel--;
                            //_lastitemlevel = _currentitemlevel;
                            _templist.AppendLine("<item>");
                        }
                        else  // (_lastitemlevel == text.Length - 1)
                        {
                            _templist.AppendLine("<item>");
                        }
                        _lastitemlevel = _currentitemlevel;
                        _initem = true;
                        break;
                    case "IncludeTopic":
                        _intermediate.AppendFormat("<{0}><Level>{1}</Level>\r\n", womElement, text.Length - 2);
                        break;
                    case "Header" :
                        text = text.Replace("\r\n", "");
                        _intermediate.AppendFormat("<{0} level=\"{1}\">\r\n", womElement, text.Length);
                        break;
                    case "WikiStyling" :
                        matches = rule.OptRegExp.Matches(text);

                        //_intermediate.AppendFormat("<{0}><{1}>{2}</{1}>", womElement, rule.OptRegExp.GroupNameFromNumber(1), matches[0].Groups[1]);
                        temp.AppendFormat("<{0}>", womElement);
                        foreach (Match match in matches)
                        {
                            for (int x = 1; x < match.Groups.Count; x++)
                            {
                                if (match.Groups[x].Success)
                                {
                                    if (x == 3)
                                    {
                                        temp.AppendFormat("<{0}>{1}</{0}>", rule.OptRegExp.GroupNameFromNumber(x), match.Groups[x]);
                                    }
                                    else
                                    {
                                        temp.AppendFormat("<{0}/>", rule.OptRegExp.GroupNameFromNumber(x));
                                    }
                                }
                            }
                        }
                        WriteWom(temp.ToString());
                        break;
                    case "HiddenSinglelineProperty":
                    case "HiddenWikiTalkMethod":
                        matches = rule.OptRegExp.Matches(text);
                        _intermediate.AppendFormat("<{0}><{1}>{2}</{1}>\r\n", womElement, rule.OptRegExp.GroupNameFromNumber(1), matches[0].Groups[1]);
                        break;
                    case "WikiTalkMethod":
                    case "MultilineProperty":
                    case "SinglelineProperty":
                        text = text.Trim();
                        string temptext = text;
                     
                        if (escapedText.IsMatch(text))
                        {
                            _intermediate.AppendFormat("\r\n<{0}><{1}>{2}</{1}>", womElement, "Name", text.Substring(text.IndexOf(@"""""") + 2, text.Length - 7));
                        }
                        else
                        {
                            matches = rule.OptRegExp.Matches(text);
                            string rulename = rule.OptRegExp.GroupNameFromNumber(1).ToString();
                            text = matches[0].Groups[1].Value;
                            string oneCap = @"(^[\p{Lu}]{1}|^[\p{Lu}]{1})[\p{Ll}\p{Nd}\P{Lu}]+:";
                            if (Regex.IsMatch(temptext, oneCap))
                            {
                                _intermediate.AppendFormat("\r\n<{0}><{1}>{2}</{1}>", womElement, rulename, text);
                            }
                            else if (_mgr.TopicExists(text, ImportPolicy.DoNotIncludeImports))
                            {
                                ns = _mgr.Namespace;
                                if (_fed.HasPermission(new QualifiedTopicRevision(text, ns), TopicPermission.Read))
                                {
                                    string tipid = NewUniqueIdentifier();
                                    string tiptext = "";
                                    string tipstat = "";
                                    if (_mgr.GetTopicLastModificationTime(text) != null)
                                    {
                                        tipstat = _mgr.GetTopicLastModificationTime(text).ToString();
                                    }
                                    try
                                    {
                                        tipstat = tipstat + " - " + _mgr.GetTopicLastAuthor(text);
                                    }
                                    catch (FlexWiki.Security.FlexWikiAuthorizationException ex)
                                    {
                                        string stuff = ex.ToString();
                                    }
                                    _intermediate.AppendFormat("\r\n<{0}><{1}>{2}</{1}><{3}><{4}>{5}</{4}><{6}>{7}</{6}><{8}>{9}</{8}>", womElement, rulename, text, "TopicExists", "Namespace", _defaultns, "Topic", text, "TipId", tipid);
                                    _intermediate.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExists", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);

                                }
                                else
                                {
                                    _intermediate.AppendFormat("\r\n<{0}><{1}>{2}</{1}><{7}><{3}>{4}</{3}><{5}>{6}</{5}></{7}>", womElement, rulename, text, womElement, text, "Namespace", _defaultns, "CreateNewTopic");
                                }
                            }
                            else
                            {
                                _intermediate.AppendFormat("\r\n<{0}><{1}>{2}</{1}><{7}><{3}>{4}</{3}><{5}>{6}</{5}></{7}>", womElement, rulename, text, womElement, text, "Namespace", _defaultns, "CreateNewTopic");
                            }
                        }
                        break;
                    case "TableRow":
                        if (!_intable)
                        {
                            _intermediate.AppendLine("<Table><TableRow><womCellText>");
                            _intable = true;
                            _unclosedTableElements = "Table;TableRow;womCellText;";
                        }
                        else
                        {
                            _intermediate.AppendLine("<TableRow><womCellText>");
                            _unclosedTableElements = _unclosedTableElements + "TableRow;womCellText;";
                        }
                        break;
                    case "MultilineTableRow":
                        if (!_intable)
                        {
                            _intermediate.AppendLine("<Table><TableRow><womMultilineCellText>");
                            _unclosedTableElements = "Table;TableRow;womMultilineCellText;";
                            _intable = true;
                        }
                        else
                        {
                            _intermediate.AppendLine("<TableRow><womMultilineCellText>");
                            _unclosedTableElements = _unclosedTableElements + "TableRow;womMultilineCellText;";
                        }
                        break;
                    case "Para":
                    case "TableStyle":
                        _intermediate.AppendFormat("<{0}>", womElement);
                        break;
                    case "ExtendedCode":
                    case "PreformattedMultilineKeyed":
                    case "PreformattedMultiline":
                        _intermediate.AppendFormat("<{0}>", womElement);
                        break;
                    case "TextileStrong":
                        WriteWom("<TextileStrong>");
                        break;
                    default:
                        _intermediate.AppendFormat("<ParseError>{0}; {1}: {2}</ParseError>\r\n", womElement, text, jump);
                        break;
                }
            }
            else if (womElement == "womCellText")
            {
                _intermediate.AppendLine("</womCellText><womCellText>");
            }
            else if (womElement == "womMultilineCellText")
            {
                _intermediate.AppendLine("</womMultilineCellText><womMultilineCellText>");
            }
            else if (womElement.EndsWith("End"))
            {
                if (womElement == "TableRowEnd")
                {
                    
                    _intermediate.AppendLine("</womCellText></TableRow>");
                    int indx = _unclosedTableElements.LastIndexOf("womCellText");
                    if (_unclosedTableElements.Length > indx + 12)
                    {
                        _unclosedTableElements = _unclosedTableElements.Substring(0, indx) + _unclosedTableElements.Substring(indx + 12);
                    }
                    else
                    {
                        _unclosedTableElements = _unclosedTableElements.Substring(0, indx);
                    }
                    indx = _unclosedTableElements.LastIndexOf("TableRow");
                    if (_unclosedTableElements.Length > indx + 9)
                    {
                        _unclosedTableElements = _unclosedTableElements.Substring(0, indx) + _unclosedTableElements.Substring(indx + 9);
                    }
                    else
                    {
                        _unclosedTableElements = _unclosedTableElements.Substring(0, indx);
                    }
                }
                else if (womElement == "MultilineTableRowEnd")
                {
                    _intermediate.AppendLine("</womMultilineCellText></TableRow>");
                    int indx = _unclosedTableElements.LastIndexOf("womMultilineCellText");
                    if (_unclosedTableElements.Length > indx + 21)
                    {
                        _unclosedTableElements = _unclosedTableElements.Substring(0, indx) + _unclosedTableElements.Substring(indx + 21);
                    }
                    else
                    {
                        _unclosedTableElements = _unclosedTableElements.Substring(0, indx);
                    }
                    indx = _unclosedTableElements.LastIndexOf("TableRow");
                    if (_unclosedTableElements.Length > indx + 9)
                    {
                        _unclosedTableElements = _unclosedTableElements.Substring(0, indx) + _unclosedTableElements.Substring(indx + 9);
                    }
                    else
                    {
                        _unclosedTableElements = _unclosedTableElements.Substring(0, indx);
                    }
                }
                else if (womElement == "PreformattedMultilineEnd")
                {
                    if (text.Length > 4)
                    {
                        _intermediate.AppendFormat("{0}</{1}>\r\n", text.Substring(2, text.Length - 4), womElement.Substring(0, womElement.Length - 3));
                    }
                    else
                    {
                        WriteWom("</" + womElement.Substring(0, womElement.Length - 3) + ">");
                    }
                }
                else if (womElement.Contains("rderedList"))
                {
                    if (_currentitemlevel > _lastitemlevel)
                    {
                        for (int x = _currentitemlevel; x > _lastitemlevel; x--)
                        {
                            _templist.AppendLine("</item></list>");
                        }
                    }
                    _templist.AppendLine("</item>");
                    _initem = false;
                }
                else if (womElement == "HiddenWikiTalkMethodEnd")
                {
                    WriteWom("</HiddenWikiTalkMethod>\r\n");
                }
                else
                {
                    WriteWom("</" + womElement.Substring(0, womElement.Length - 3) + ">");
                }
            }
            else if (womElement.EndsWith("PreformattedSingleLine"))
            {
                if (_inpre)
                {
                    if (text != "\r\n")
                    {
                        _intermediate.Append(text);
                    }
                }
                else
                {
                    if (text.StartsWith(" \r\n"))
                    {
                        _intermediate.AppendFormat("\r\n<{0}>{1}", "PreformattedSingleLine", text.Substring(3));
                    }
                    else
                    {
                        _intermediate.AppendFormat("\r\n<{0}>{1}", "PreformattedSingleLine", text);
                    }
                    _inpre = true;
                }
            }
            else if (text.EndsWith("\r\n"))
            {
                switch (womElement)
                {
                    case "PageRule":
                        _intermediate.AppendLine("<PageRule/>");
                        break;
                    case "womMultilineCode":
                        if (text.StartsWith("\r\n\r\n"))
                        {
                            _intermediate.AppendFormat("<{0}>{1}</{0}>\r\n", womElement, text.Substring(2, text.Length - 4));
                        }
                        else
                        {
                            _intermediate.AppendFormat("<{0}>{1}</{0}>\r\n", womElement, text);
                        }
                        break;
                    case "womMultilineCell":
                        text = text.TrimEnd(lineEnds);
                        WriteWom("<womMultilineCell>" + text + "</womMultilineCell><Break />");
                        break;
                    case "Containerdiv":
                    case "Containerspan":
                    case "ContainerEnddiv":
                    case "ContainerEndspan":
                        if (_initem)
                        {
                            _templist.AppendFormat("{0}", text);
                        }
                        else
                        {
                            _intermediate.AppendFormat("{0}", text);
                        }
                        break;
                    default:
                        text = text.TrimEnd(lineEnds);
                        _intermediate.AppendFormat("<{0}>{1}</{0}>\r\n", womElement, text);
                        break;
                }
            }
            else
            {
                switch (womElement)
                {
                    case "FreeLinkToHttpImageDisplayGif":
                    case "FreeLinkToHttpsImageDisplayGif":
                    case "FreeLinkToHttpImageDisplayJpg":
                    case "FreeLinkToHttpsImageDisplayJpg":
                    case "FreeLinkToHttpImageDisplayJpeg":
                    case "FreeLinkToHttpsImageDisplayJpeg":
                    case "FreeLinkToHttpImageDisplayPng":
                    case "FreeLinkToHttpsImageDisplayPng":
                    case "FreeLinkToHttpLink":
                    case "FreeLinkToHttpsLink":
                    case "FreeLinkToMailto":
                        matches = rule.OptRegExp.Matches(text);
                        string firstGroupName = rule.OptRegExp.GroupNameFromNumber(1);
                        string freetext = matches[0].Groups[1].Value;
                        string secondGroupName = rule.OptRegExp.GroupNameFromNumber(2);
                        string link = matches[0].Groups[2].Value;
                        string tooltip = "";
                        if (tooltipRegex.IsMatch(freetext))
                        {
                            matches = tooltipRegex.Matches(freetext);
                            freetext = matches[0].Groups[1].Value;
                            if (matches[0].Groups[2].Success)
                            {
                                tooltip = matches[0].Groups[2].Value;
                            }
                        }
                        while (textileRegex.IsMatch(freetext))
                        {
                            freetext = textileMatch(freetext);                     
                        }
                        if (!String.IsNullOrEmpty(tooltip))
                        {
                            temp.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}></{0}>", womElement, firstGroupName, freetext, secondGroupName, link, "Tip", tooltip);
                        }
                        else
                        {
                            temp.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}></{0}>", womElement, firstGroupName, freetext, secondGroupName, link);
                        }
                        WriteWom(temp.ToString());
                        break;
                    case "NamespaceTopic":
                    case "NamespaceMulticapsTopic":
                        string[] testitem = text.Split('.');
                        if (_fed.NamespaceManagerForNamespace(testitem[0]) != null)
                        {
                            if (_mgr.TopicExists(new UnqualifiedTopicName(testitem[1]), ImportPolicy.IncludeImports))
                            {
                                ns = testitem[0];
                                topic = testitem[1];
                                if (_fed.HasPermission(new QualifiedTopicRevision(topic, ns), TopicPermission.Read))
                                {
                                    string tipid = NewUniqueIdentifier();
                                    string tiptext = "";
                                    if (_mgr.GetTopicProperty(topic, "Summary") != null)
                                    {
                                        tiptext = _mgr.GetTopicProperty(topic, "Summary").LastValue;
                                    }
                                    string tipstat = _mgr.GetTopicLastModificationTime(topic) + " - " + _mgr.GetTopicLastAuthor(topic);
                                    if (_initem)
                                    {
                                        _templist.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "TopicExists", "Namespace", ns, "Topic", topic, "TipId", tipid, "DisplayText", topic);
                                        _templist.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExists", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                                    }
                                    else
                                    {
                                        _intermediate.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "TopicExists", "Namespace", ns, "Topic", topic, "TipId", tipid, "DisplayText", topic);
                                        _intermediate.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExists", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                                    }

                                }
                                else
                                {
                                    temp.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}></{4}>", womElement, savedtext, "CreateTopic", topic, "CreateNamespaceTopic");
                                }
                            }
                        }
                        else
                        {
                            temp.AppendFormat("<{0}>{1}</{0}>", "paraText", savedtext);
                        }
                        WriteWom(temp.ToString());
                        break;
                    case "StartsWithOneCap":
                    case "StartsWithMulticaps":
                    case "MalformedTopic":
                        
                        if (womElement == "MalformedTopic")
                        {
                            matches = rule.OptRegExp.Matches(text);
                            if (matches.Count > 0)
                            {
                                text = matches[0].Groups[1].Value;
                            }
                        }
                        topic = text.Trim();
                        if (_mgr.TopicExists(topic, ImportPolicy.IncludeImports))
                        {
                            ns = _mgr.Namespace;
                            if (_fed.HasPermission(new QualifiedTopicRevision(topic, ns), TopicPermission.Read))
                            {
                                string tipid = NewUniqueIdentifier();
                                string tiptext = "";
                                string tipstat = "";
                                if (_mgr.GetTopicLastModificationTime(topic) != null)
                                {
                                    tipstat = _mgr.GetTopicLastModificationTime(topic).ToString();
                                }
                                try
                                {
                                    tipstat = tipstat + " - " + _mgr.GetTopicLastAuthor(topic);
                                }
                                catch (FlexWiki.Security.FlexWikiAuthorizationException ex)
                                {
                                    string stuff = ex.ToString();
                                }
                                if (_initem)
                                {
                                    _templist.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "TopicExists", "Namespace", ns, "Topic", topic, "TipId", tipid, "DisplayText", topic);
                                    _templist.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExists", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                                }
                                else
                                {
                                    _intermediate.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "TopicExists", "Namespace", ns, "Topic", topic, "TipId", tipid, "DisplayText", topic);
                                    _intermediate.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExists", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                                }
                            }
                            else
                            {
                                if (_initem)
                                {
                                    _templist.AppendFormat("<{0}>{1}</{0}>", "paraText", savedtext);
                                }
                                else
                                {
                                    _intermediate.AppendFormat("<{0}>{1}</{0}>", "paraText", savedtext);
                                }
                            }
                        }
                        else
                        {
                            if (_initem)
                            {
                                _templist.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}></{4}>", womElement, topic, "Namespace", _defaultns, "CreateNewTopic");
                            }
                            else
                            {
                                _intermediate.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}></{4}>", womElement, topic, "Namespace", _defaultns, "CreateNewTopic");
                            }
                        }
                        break;
                    case "LinkToAnchor":
                        if (rule.OptRegExp.IsMatch(text))
                        {
                            matches = rule.OptRegExp.Matches(text);
                            text = matches[0].Groups[1].Value;
                            if (_mgr.TopicExists(text, ImportPolicy.DoNotIncludeImports))
                            {
                                if (_fed.HasPermission(new QualifiedTopicRevision(text, _defaultns), TopicPermission.Read))
                                {
                                    string tipid = NewUniqueIdentifier();
                                    string tiptext = "";
                                    string tipstat = "";
                                    if (_mgr.GetTopicProperty(text, "Summary") != null)
                                    {
                                        tiptext = _mgr.GetTopicProperty(text, "Summary").LastValue;
                                    }
                                    if (_mgr.GetTopicLastModificationTime(text) != null)
                                    {
                                        tipstat = _mgr.GetTopicLastModificationTime(text).ToString();
                                    }
                                    try
                                    {
                                        tipstat = tipstat + " - " + _mgr.GetTopicLastAuthor(text);
                                    }
                                    catch (FlexWiki.Security.FlexWikiAuthorizationException ex)
                                    {
                                        string stuff = ex.ToString();
                                    }
                                    if (_initem)
                                    {
                                        _templist.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}><{9}>{10}</{9}>", "TopicExistsAnchor", "Namespace", _defaultns, "Topic", text, "Anchor", matches[0].Groups[2], "TipId", tipid, "DisplayText", matches[0].Groups[1]);
                                        _templist.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExistsAnchor", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                                    }
                                    else
                                    {
                                        _intermediate.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}><{9}>{10}</{9}>", "TopicExistsAnchor", "Namespace", _defaultns, "Topic", text, "Anchor", matches[0].Groups[2], "TipId", tipid, "DisplayText", matches[0].Groups[1]);
                                        _intermediate.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExistsAnchor", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                                    }
                                }
                                else
                                {
                                    if (_initem)
                                    {
                                        _templist.AppendFormat("<{0}>{1}</{0}>", "paraText", savedtext);
                                    }
                                    else
                                    {
                                        _intermediate.AppendFormat("<{0}>{1}</{0}>", "paraText", savedtext);
                                    }
                                }
                            }
                            else
                            {
                                if (_initem)
                                {
                                    _templist.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}></{4}>", "Topic", text, "Namespace", _defaultns, "CreateNewTopic");
                                }
                                else
                                {
                                    _intermediate.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}></{4}>", "Topic", text, "Namespace", _defaultns, "CreateNewTopic");
                                }
                            }
                        }
                        break;
                    case "FreeLinkToNamespaceTopic":
                    case "FreeLinkToNamespaceMalformedTopic":
                        try
                        {
                            matches = rule.OptRegExp.Matches(text);
                            ns = matches[0].Groups[2].Value;
                            matches = rule.OptRegExp.Matches(text);
                            text = matches[0].Groups[3].Value;
                            if (_fed.NamespaceManagerForNamespace(ns) != null)
                            {
                                if (_mgr.TopicExists(new UnqualifiedTopicName(text), ImportPolicy.IncludeImports))
                                {
                                    if (_fed.HasPermission(new QualifiedTopicRevision(text, ns), TopicPermission.Read))
                                    {
                                        string tipid = NewUniqueIdentifier();
                                        string tiptext = "";
                                        if (_mgr.GetTopicProperty(text, "Summary") != null)
                                        {
                                            tiptext = _mgr.GetTopicProperty(text, "Summary").LastValue;
                                        }
                                        string tipstat = _mgr.GetTopicLastModificationTime(text) + " - " + _mgr.GetTopicLastAuthor(text);
                                        if (_initem)
                                        {
                                            _templist.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "TopicExists", "Namespace", ns, "Topic", text, "TipId", tipid, "DisplayText", matches[0].Groups[1]);
                                            _templist.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExists", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                                        }
                                        else
                                        {
                                            _intermediate.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "TopicExists", "Namespace", ns, "Topic", text, "TipId", tipid, "DisplayText", matches[0].Groups[1]);
                                            _intermediate.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExists", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                                        }
                                    }
                                    else
                                    {
                                        if (_initem)
                                        {
                                            _templist.AppendFormat("<{0}>{1}</{0}>", "paraText", savedtext);
                                        }
                                        else
                                        {
                                            _intermediate.AppendFormat("<{0}>{1}</{0}>", "paraText", savedtext);
                                        }
                                    }
                                }
                                else
                                {
                                    if (_initem)
                                    {
                                        _templist.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}><{5}>{6}</{5}></{4}>", "Topic", text, "Namespace", ns, "CreateNewTopic", "DisplayText", matches[0].Groups[1]);
                                    }
                                    else
                                    {
                                        _intermediate.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}><{5}>{6}</{5}></{4}>", "Topic", text, "Namespace", ns, "CreateNewTopic", "DisplayText", matches[0].Groups[1]);
                                    }
                                }
                            }
                            else
                            {
                                if (_initem)
                                {
                                    _templist.AppendFormat("<{0}>{1}</{0}>", "paraText", savedtext);
                                }
                                else
                                {
                                    _intermediate.AppendFormat("<{0}>{1}</{0}>", "paraText", savedtext);
                                }
                            }
                        }
                        catch (FlexWikiException ex)
                        {
                            string error = ex.ToString();
                        }
                        break;
                    case "FreeLinkToTopic":
                    case "FreeLinkToMultiCapsTopic":
                    case "FreeLinkToMalformedTopic":
                        matches = rule.OptRegExp.Matches(text);
                        text = matches[0].Groups[2].Value;
                        if (_mgr.TopicExists(text, ImportPolicy.IncludeImports))
                        {

                            string tipid = NewUniqueIdentifier();
                            string tiptext = "";
                            string tipstat = "";
                            if (_mgr.GetTopicProperty(text, "Summary") != null)
                            {
                                tiptext = _mgr.GetTopicProperty(text, "Summary").LastValue;
                            }
                            if (_mgr.GetTopicLastModificationTime(text) != null)
                            {
                                tipstat = _mgr.GetTopicLastModificationTime(text).ToString();
                            }
                            try
                            {
                                tipstat = tipstat + " - " + _mgr.GetTopicLastAuthor(text);
                            }
                            catch (FlexWiki.Security.FlexWikiAuthorizationException ex)
                            {
                                string stuff = ex.ToString();
                            }
                            if (_initem)
                            {
                                _templist.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "TopicExists", "Namespace", _defaultns, "Topic", text, "TipId", tipid, "DisplayText", matches[0].Groups[1]);
                                _templist.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExists", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                            }
                            else
                            {
                                _intermediate.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "TopicExists", "Namespace", _defaultns, "Topic", text, "TipId", tipid, "DisplayText", matches[0].Groups[1]);
                                _intermediate.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExists", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                            }
                        }
                        else
                        {
                            if (_initem)
                            {
                                _templist.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}><{5}>{6}</{5}></{4}>", "Topic", text, "Namespace", _defaultns, "CreateNewTopic", "DisplayText", matches[0].Groups[1]);
                            }
                            else
                            {
                                _intermediate.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}><{5}>{6}</{5}></{4}>", "Topic", text, "Namespace", _defaultns, "CreateNewTopic", "DisplayText", matches[0].Groups[1]);
                            }
                        }
                        break;
                    case "HiddenMultilineProperty":
                        //matches = rule.OptRegExp.Matches(text);
                        _intermediate.AppendFormat("<{0}>{1}</{0}>\r\n", womElement, text);
                        break;
                    case "FreeLinkToAnchor":
                        matches = rule.OptRegExp.Matches(text);
                        text = matches[0].Groups[2].Value;
                        if (_mgr.TopicExists(text, ImportPolicy.DoNotIncludeImports))
                        {

                            string tipid = NewUniqueIdentifier();
                            string tiptext = _mgr.GetTopicProperty(text, "Summary").LastValue;
                            string tipstat = _mgr.GetTopicLastModificationTime(text) + " - " + _mgr.GetTopicLastAuthor(text);
                            if (_initem)
                            {
                                _templist.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}><{9}>{10}</{9}>", "TopicExistsAnchor", "Namespace", _defaultns, "Topic", text, "Anchor", matches[0].Groups[3], "TipId", tipid, "DisplayText", matches[0].Groups[1]);
                                _templist.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExistsAnchor", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                            }
                            else
                            {
                                _intermediate.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}><{9}>{10}</{9}>", "TopicExistsAnchor", "Namespace", _defaultns, "Topic", text, "Anchor", matches[0].Groups[3], "TipId", tipid, "DisplayText", matches[0].Groups[1]);
                                _intermediate.AppendFormat("<{1}><{2}>{3}</{2}><{4}>{5}</{4}><{6}>{7}</{6}></{1}></{0}>", "TopicExistsAnchor", "TipData", "TipIdData", tipid, "TipText", ParserEngine.escape(tiptext), "TipStat", tipstat);
                            }
                        }
                        else
                        {
                            if (_initem)
                            {
                                _templist.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}><{5}>{6}</{5}></{4}>", "Topic", text, "Namespace", _defaultns, "CreateNewTopic", "DisplayText", matches[0].Groups[1]);
                            }
                            else
                            {
                                _intermediate.AppendFormat("<{4}><{0}>{1}</{0}><{2}>{3}</{2}><{5}>{6}</{5}></{4}>", "Topic", text, "Namespace", _defaultns, "CreateNewTopic", "DisplayText", matches[0].Groups[1]);
                            }
                        }
                        break;
                    case "TextileCodeLineInLine":
                    case "TextileEmphasisInLine":
                    case "TextileDeemphasisInLine":
                    case "TextileDeletionInLine":
                    case "TextileInsertedInLine":
                    case "TextileSubscriptInLine":
                    case "TextileSuperscriptInLine":
                    case "TextileCodeLineLineStart":
                    case "TextileEmphasisLineStart":
                    case "TextileDeemphasisLineStart":
                    case "TextileDeletionLineStart":
                    case "TextileInsertedLineStart":
                    case "TextileSubscriptLineStart":
                    case "TextileSuperscriptLineStart":
                    case "TextileCitationInLine":
                    case "TextileCitationLineStart":
                    case "TextileStrongInLine":
                    case "TextileStrongLineStart":
                    case "Italics":
                    case "Strong":
                    case "EscapedNoFormatText":
                        matches = rule.OptRegExp.Matches(text);
                        if (matches.Count > 0)
                        {
                            text = matches[0].Groups[1].Value;
                        }
                        if (textileRegex.Matches(text).Count > 0)
                        {
                            while (textileRegex.IsMatch(text))
                            {
                                text = textileMatch(text);
                            }
                        }
                        //else
                        //{
                        //    matches = rule.OptRegExp.Matches(text);
                        //    int lencontrol = 1;
                        //    if (womElement == "Strong")
                        //    {
                        //        lencontrol = 3;
                        //    }
                        //    else if (womElement.Contains("Citation") || womElement.Contains("Deemphasis") || womElement == "EscapedNoFormatText" || womElement == "Italics")
                        //    {
                        //        lencontrol = 2;
                        //    }
                        //    text = text.Substring(0, matches[0].Groups[1].Index - lencontrol) + matches[0].Groups[1].Value + text.Substring(matches[0].Groups[1].Index + matches[0].Groups[1].Length + lencontrol);
                        //}
                        if (_initem)
                        {
                            _templist.AppendFormat("<{0}>{1}</{0}>", womElement, text);
                        }
                        else
                        {

                            _intermediate.AppendFormat("<{0}>{1}</{0}>", womElement, text);
                        }
                        break;
                    case "CellStyleColor":
                    case "StyleHexColor":
                    case "StyleHexTextColor":
                    case "CellTextColor":
                        if (_initem)
                        {
                            _templist.AppendFormat("<{0}>{1}</{0}>", womElement, text.Substring(1, text.Length - 2));
                        }
                        else
                        {
                            _intermediate.AppendFormat("<{0}>{1}</{0}>", womElement, text.Substring(1, text.Length - 2));
                        }
                        break;
                    case "AltFileLink":
                        matches = rule.OptRegExp.Matches(text);
                        if (_initem)
                        {
                            _templist.AppendFormat("<{0}>{1}</{0}>", womElement, matches[0].Groups[1]);
                        }
                        else
                        {
                            _intermediate.AppendFormat("<{0}>{1}</{0}>", womElement, matches[0].Groups[1]);
                        }
                        break;
                    case "womHeaderText":
                        string anchor = CreateAnchor(text);
                        _intermediate.AppendFormat("<{0}>{1}</{0}><{2}>{3}</{2}>", womElement, text, "AnchorText", anchor);
                        break;
                    case "FreeLinkToHttpDisplayImage":
                    case "WikiTalkLink":
                    case "WikiForm":
                    case "HttpDisplayImage":
                    case "Containerdiv":
                    case "Containerspan":
                    case "ContainerEnddiv":
                    case "ContainerEndspan":
                    case "ErrorMessage":
                        if (_initem)
                        {
                            _templist.AppendFormat("{0}", text);
                        }
                        else
                        {
                            _intermediate.AppendFormat("{0}\r\n", text);
                        }
                        break;
                    case "womMultilineCell":
                        string replacement = Regex.Replace(text, "\r\n", "<Break />");
                        WriteWom("<womMultilineCell>" + replacement + "</womMultilineCell>");
                        break;
                    case "womStyledCode":
                        string fixwhitespace = Regex.Replace(text, " ", "&nbsp;");
                        fixwhitespace = Regex.Replace(fixwhitespace, "\r\n", "<Break />");
                        //{
                        //    text = "<Break />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                        //}
                        WriteWom("<womStyledCode>" + fixwhitespace + "</womStyledCode>");
                        break;
                    default:
                        if (_initem)
                        {
                            _templist.AppendFormat("<{0}>{1}</{0}>", womElement, text);
                        }
                        else
                        {
                            _intermediate.AppendFormat("<{0}>{1}</{0}>", womElement, text);
                        }
                        break;
                }
            }
            _lastWom = womElement;
        }
        public bool InItem
        {
            get { return _initem; }
        }
        public bool InTable
        {
            get { return _intable; }
        }
        public string ParsedDocument
        {
            get { return _intermediate.ToString(); }
            set
            {
                _intermediate = new StringBuilder();
                _intermediate.Append(value);
            }
        }
        public override void End()
        {
            if (_inpre)
            {
                _intermediate.AppendLine("</PreformattedSingleLine>");
                _inpre = false;
            }
            if (_lastitemlevel > 0)
            {
                int indx = _templist.ToString().LastIndexOf("</item>");
                int len = _templist.Length;
                _templist.Remove(indx, len - indx);
                for (int x = _lastitemlevel; x > 0; x--)
                {
                    _templist.AppendLine("</item></list>");
                }
                _lastitemlevel = 0;
                _intermediate.Append(_templist.ToString());
            }
            if (_lastWom == "WikiText")
            {
                if (_intermediate.ToString().EndsWith("<Para>"))
                {
                    _intermediate.Remove(_intermediate.ToString().LastIndexOf("<Para>"), 6);
                }
                //else
                //{
                //    _intermediate.Append("</Para>");
                //}
            }
            if (_intable)
            {
                string[] _tempElements = _unclosedTableElements.Split(';');
                for (int i = _tempElements.Length - 2; i > -1; i--)
                {
                    _intermediate.AppendFormat("</{0}>", _tempElements[i]);
                }
                _unclosedTableElements = "";
                _intable = false;
            }
            if (!_fragmentOnly)
            {
                _intermediate.AppendLine(@"<TipHolder></TipHolder>");
                _intermediate.AppendLine(@"</WomDocument>");
            }
        }
        public XmlDocument XmlDoc
        {
            get
            {
                if (_xmldoc == null)
                {
                    string temp = badControlChar.Replace(_intermediate.ToString(), "");
                    _xmldoc = new XmlDocument();
                    try
                    {
                        _xmldoc.LoadXml(temp);
                    }
                    catch (SystemException ex)
                    {
                        StringBuilder error = new StringBuilder();
                        error.AppendLine(@"<div><div id=""ErrorMessage"" class=""ErrorMessage"">An error occured during conversion to XML of this section<br/>");
                        error.AppendLine(ex.Message + "</div></div>");
                        _xmldoc.LoadXml(error.ToString());
                    }
                }
                return _xmldoc;
            }
        }
        public string CreateAnchor(string text)
        {
            //This will cause existing anchors to break
            //   those anchors generated by TableOfContents will continue to work
            string temp = "";
            if (anchors == null)
            {
                anchors = new string[25];
            }

            if (!String.IsNullOrEmpty(anchors[anchorid]))
            {
                temp = anchors[anchorid];
            }
            else
            {
                temp = invalidChar.Replace(text, "");
                temp = Regex.Replace(temp, " ", "_");
                temp = "_" + anchorid.ToString() + "_" + temp;
            }
            anchorid++;

            return temp;
        }
        private string GetAttributeString(string attr)
        {
            StringBuilder bldr = new StringBuilder();
            MatchCollection matches = attributeRegex.Matches(attr);

            foreach (Match match in matches)
            {
                bldr.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}></{0}>", "Attribute", "AttributeName", match.Groups["AttributeName"], "AttributeValue", match.Groups["AttributeValue"]);
            }
            return bldr.ToString();
        }
        //simulate a NamespaceExists function
        //public bool NamespaceExists(string ns)
        //{
        //    bool exists = false;
        //    switch (ns)
        //    {
        //        case "OdsWiki":
        //            exists = true;
        //            break;
        //    }
        //    return exists;
        //}
        //simulate a TopicExists function
        //the topics here need to be created in the test environment for unit tests 
        //public bool TopicExists(string name)
        //{
        //    bool exists = false;
        //    switch (name)
        //    {
        //        case "GoodTopic":
        //        case "AnotherGoodTopic":
        //        case "AaTestNewParser":
        //        case "goodtopic":
        //        case "MULTIcapsGoodTopic":
        //        case "TopicForMethodExists":
        //            exists = true;
        //            break;
        //        case "UnknownTopic":
        //        case "BadTopic":
        //        case "badtopic":
        //        case "MULTIcapsBadTopic":
        //        case "TopicForMethodCreate":
        //            exists = false;
        //            break;
        //        default:
        //            exists = (_rn.Next(10) > 5);
        //            break;
        //    }
        //    return exists;
        //}
        private static int unique = 0;
        private string NewUniqueIdentifier()
        {
            unique++;
            if (unique > 100000)		// wrap ever 100000; nothing magic about this number
            {
                unique = 0;
            }
            return "id" + unique.ToString();
        }
        //Needed to have consistent test results as id strings are in test evaluations
        public static void ResetUniqueIdentifier()
        {
            unique = 0;
        }
        private string textileMatch(string text)
        {
            StringBuilder result = new StringBuilder();
            string temp;
            MatchCollection matches = textileRegex.Matches(text);
            for (int i = 1; i < matches[0].Groups.Count; i++)
            {
                if (matches[0].Groups[i].Success)
                {
                    result.Append(text.Substring(0, matches[0].Index));
                    temp = matches[0].Groups[i].Value;
                    switch ((TextileFormat)i)
                    {
                        case TextileFormat.Italics:
                            break;
                        case TextileFormat.Strong:
                            result.AppendFormat("<{0}>{1}</{0}>", "Strong", temp.Substring(3, temp.Length - 6));
                            break;
                        case TextileFormat.TextileCitationInLine:
                            result.AppendFormat("<{0}>{1}</{0}>", "TextileCitationInLine", temp.Substring(2, temp.Length - 4));
                            break;
                        case TextileFormat.TextileCodeLineInLine:
                            result.AppendFormat("<{0}>{1}</{0}>", "TextileCodeLineInLine", temp.Substring(1, temp.Length - 2));
                            break;
                        case TextileFormat.TextileDeletionInLine:
                            result.AppendFormat("<{0}>{1}</{0}>", "TextileDeletionInLine", temp.Substring(1, temp.Length - 2));
                            break;
                        case TextileFormat.TextileEmphasisInLine:
                            result.AppendFormat("<{0}>{1}</{0}>", "TextileEmphasisInLine", temp.Substring(1, temp.Length - 2));
                            break;
                        case TextileFormat.TextileDeemphasisInLine:
                            result.AppendFormat("<{0}>{1}</{0}>", "TextileDeemphasisInLine", temp.Substring(2, temp.Length - 4));
                            break;
                        case TextileFormat.TextileInsertedInLine:
                            result.AppendFormat("<{0}>{1}</{0}>", "TextileInsertedInLine", temp.Substring(1, temp.Length - 2));
                            break;
                        case TextileFormat.TextileSubscriptInLine:
                            result.AppendFormat("<{0}>{1}</{0}>", "TextileSubscriptInLine", temp.Substring(1, temp.Length - 2));
                            break;
                        case TextileFormat.TextileSuperscriptInLine:
                            result.AppendFormat("<{0}>{1}</{0}>", "TextileSuperscriptInLine", temp.Substring(1, temp.Length - 2));
                            break;
                        case TextileFormat.TextileStrongInLine:
                            result.AppendFormat("<{0}>{1}</{0}>", "Strong", temp.Substring(1, temp.Length - 2));
                            break;
                        default:
                            break;
                    }
                }
            }
            result.Append(text.Substring(matches[0].Index + matches[0].Length));
            return result.ToString();
        }
        public void ConvertEmoticons(MatchCollection matches)
        {
            string temp = _intermediate.ToString();
            int cnt = matches.Count;
            for (int y = 0; y < cnt; y++)
            {
                int cntgrp = matches[y].Groups.Count;
                for (int i = 1; i < cntgrp; i++)
                {
                    if (matches[y].Groups[i].Success)
                    {
                        temp = Regex.Replace(temp, @"(?<!<PreformattedSingleLine>[\p{L}\p{N}\p{P}\p{S}\p{Z}\|']*|<EscapedNoFormatText>|&amp|&lt|&gt)" + emoticons[i - 1, 0] , "<Emoticon>" + emoticons[i - 1, 1] + "</Emoticon>");
                    }
                }
            }
            _intermediate = new StringBuilder();
            _intermediate.Append(temp);
        }
        public override void ContainerStart(string type, string id, string style)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("<{0}>", "Container" + type.ToLower());
            if (!String.IsNullOrEmpty(id))
            {
                builder.AppendFormat("<{0}>{1}</{0}>", "Id", id);
            }
            if (!String.IsNullOrEmpty(style))
            {
                builder.AppendFormat("<{0}>{1}</{0}>", "Style", style);
            }

            WriteWom(builder.ToString());
       }

        public override void ContainerEnd(string type)
        {
            if (_initem)
            {
                _templist.AppendFormat("</{0}>\r\n", "Container" + type.ToLower());
            }
            else
            {
                _intermediate.AppendFormat("</{0}>\r\n", "Container" + type.ToLower());
            }
        }
        override public void FormStart(string method, string URI, string attributes)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}>", "WikiForm", "FormAction", URI, "FormMethod", method);
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            WriteWom(builder.ToString());
       }
        override public void FormEnd()
        {
            WriteWom("</WikiForm>");
        }
        override public void FormCheckbox(string fieldName, string fieldValue, bool isChecked, string attributes)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "FormCheckbox", "Type", "checkbox", "Name", fieldName, "Id", fieldName, "Value", Formatter.EscapeHTML(fieldValue));
            if (isChecked)
            {
                builder.AppendFormat("<{0}>{1}</{0}>", "Checked", "true");
            }
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            builder.AppendFormat("</{0}>", "FormCheckbox");

            WriteWom(builder.ToString());
        }
        override public void FormHiddenField(string FieldName, string fieldValue, string attributes)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}>", "FormHiddenField", "Type", "hidden", "Name", FieldName, "Value", Formatter.EscapeHTML(fieldValue));
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            builder.AppendFormat("</{0}>", "FormHiddenField");
            WriteWom(builder.ToString());
        }
        override public void FormImageButton(string FieldName, string ImageURI, string TipString, string attributes)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}>", "FormImageButton", "Type", "image", "Source", ImageURI, "Title", Formatter.EscapeHTML(TipString));
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            builder.AppendFormat("</{0}>", "FormImageButton");
            WriteWom(builder.ToString());
        }
        override public void FormInputBox(string FieldName, string fieldValue, int fieldLength, string attributes)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "FormInputBox", "Type", "text", "Name", FieldName, "Id", FieldName, "Value", Formatter.EscapeHTML(fieldValue));
            if (fieldLength > 0)
            {
                builder.AppendFormat("<{0}>{1}</{0}>", "Size", fieldLength);
            }
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            builder.AppendFormat("</{0}>", "FormInputBox");
            WriteWom(builder.ToString());
        }
        override public void FormRadio(string fieldName, string fieldValue, bool isChecked, string attributes)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "FormRadio", "Type", "radio", "Name", fieldName, "Id", fieldName, "Value", Formatter.EscapeHTML(fieldValue));
            if (isChecked)
            {
                builder.AppendFormat("<{0}>{1}</{0}>", "Checked", "true");
            }
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            builder.AppendFormat("</{0}>", "FormRadio");

            WriteWom(builder.ToString());
        }
        override public void FormResetButton(string FieldName, string label, string attributes)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "FormResetButton", "Type", "reset", "Name", FieldName, "Id", FieldName, "Value", Formatter.EscapeHTML(label));
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            builder.AppendFormat("</{0}>", "FormResetButton");

            WriteWom(builder.ToString());
        }
        public override void FormSelectField(string fieldName, int size, bool multiple, ArrayList options, string selectedOption, ArrayList values, object selectedValue, string attributes)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}>", "FormSelectField", "Name", fieldName, "Id", fieldName, "Size", size);
            if (multiple)
            {
                builder.AppendFormat("<{0}>{1}</{0}>", "Multiple", "multiple");
            }
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            if (values != null)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}>", "SelectOption", "Value", values[i].ToString(), "OptionString", options[i].ToString());

                    if ((null != selectedValue) && (selectedValue.ToString().Length > 0) && (selectedValue.ToString() == values[i].ToString()))
                    {
                        builder.AppendFormat("<{0}>{1}</{0}>", "Selected", "selected");
                    }
                    builder.AppendFormat("</{0}>", "SelectOption");
                }
            }
            else
            {
                foreach (object option in options)
                {
                    builder.AppendFormat("<{0}>", "SelectOption");
                    if ((null != selectedOption) && (selectedOption.Length > 0) && (selectedOption == option.ToString()))
                    {
                        builder.AppendFormat("<{0}>{1}</{0}>", "Selected", "selected");
                    }
                    builder.AppendFormat("<{0}>{1}</{0}>", "OptionString", option.ToString());
                    builder.AppendFormat("</{0}>", "SelectOption");
                }
            }
            builder.AppendFormat("</{0}>", "FormSelectField");

            WriteWom(builder.ToString());
        }
        override public void FormSubmitButton(string FieldName, string label, string attributes)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "FormSubmitButton", "Type", "submit", "Name", FieldName, "Id", FieldName, "Value", Formatter.EscapeHTML(label));
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            builder.AppendFormat("</{0}>", "FormSubmitButton");

            WriteWom(builder.ToString());
        }
        override public void FormTextarea(string FieldName, string fieldValue, int rows, int cols, string attributes)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}><{7}>{8}</{7}>", "FormTextarea", "Name", FieldName, "Id", FieldName, "Value", Formatter.EscapeHTML(fieldValue));
            if (rows > 0)
            {
                builder.AppendFormat("<{0}>{1}</{0}>", "Rows", rows.ToString());
            }
            if (cols > 0)
            {
                builder.AppendFormat("<{0}>{1}</{0}>", "Cols", cols);
            }
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            builder.AppendFormat("</{0}>", "FormTextarea");

            WriteWom(builder.ToString());
        }
        public override string ToString()
        {
            return _intermediate.ToString();
        }
        public override void WriteImage(string title, string URL, string linkToURL, string height, string width, string attributes)
        {
            StringBuilder builder = new StringBuilder();
            if (!String.IsNullOrEmpty(linkToURL))
            {
                builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}>", "FreeLinkToHttpDisplayImage", "LinkUrl", linkToURL, "ImageLink", URL);
                if (!String.IsNullOrEmpty(title))
                {
                    builder.AppendFormat("<{0}>{1}</{0}>", "Title", Formatter.EscapeHTML(title));
                }
                if (!String.IsNullOrEmpty(width))
                {
                    builder.AppendFormat("<{0}>{1}</{0}>", "ImageWidth", width);
                }
                if (!String.IsNullOrEmpty(height))
                {
                    builder.AppendFormat("<{0}>{1}</{0}>", "ImageHeight", height);
                }
                if (!String.IsNullOrEmpty(attributes))
                {
                    string temp = GetAttributeString(attributes);
                    if (!String.IsNullOrEmpty(temp))
                    {
                        builder.Append(temp);
                    }
                }
                builder.AppendFormat("</{0}>", "FreeLinkToHttpDisplayImage");
            }
            else
            {
                builder.AppendFormat("<{0}><{1}>{2}</{1}>", "HttpDisplayImage", "DisplayLink", URL);
                if (!String.IsNullOrEmpty(title))
                {
                    builder.AppendFormat("<{0}>{1}</{0}>", "Title", Formatter.EscapeHTML(title));
                }
                if (!String.IsNullOrEmpty(width))
                {
                    builder.AppendFormat("<{0}>{1}</{0}>", "ImageWidth", width);
                }
                if (!String.IsNullOrEmpty(height))
                {
                    builder.AppendFormat("<{0}>{1}</{0}>", "ImageHeight", height);
                }
                if (!String.IsNullOrEmpty(attributes))
                {
                    string temp = GetAttributeString(attributes);
                    if (!String.IsNullOrEmpty(temp))
                    {
                        builder.Append(temp);
                    }
                }
                builder.AppendFormat("</{0}>", "HttpDisplayImage");
            }
            WriteWom(builder.ToString());
        }
        override public void WriteLabel(string forId, string text, string attributes)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}>", "FormLabel", "ForId", forId, "Value", Formatter.EscapeHTML(text));
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            builder.AppendFormat("</{0}>", "FormLabel");

            WriteWom(builder.ToString());
        }
        public override void WriteLink(string URL, string tip, string content, string attributes)
        {
            StringBuilder builder = new StringBuilder();
            if (URL.Contains("#"))
            {
                string temp = invalidChar.Replace(URL, "");
                temp = Regex.Replace(temp, " ", "_");
                URL = "#_" + tableofcontents.ToString() + "_" + temp;
                anchors[tableofcontents] = URL.Substring(1);
                tableofcontents++;
            }
            builder.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}><{5}>{6}</{5}>", "WikiTalkLink", "LinkUrl", URL, "Title", Formatter.EscapeHTML(tip), "TextValue", Formatter.EscapeHTML(content));
            if (!String.IsNullOrEmpty(attributes))
            {
                string temp = GetAttributeString(attributes);
                if (!String.IsNullOrEmpty(temp))
                {
                    builder.Append(temp);
                }
            }
            builder.AppendFormat("</{0}>", "WikiTalkLink");

            WriteWom(builder.ToString());
        }
        public override void WriteErrorMessage(string title, string body)
        {
            if (_initem)
            {
                _templist.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}></{0}>", "ErrorMessage", "ErrorMessageTitle", title, "ErrorMessageBody", body);
            }
            else
            {
                _intermediate.AppendFormat("<{0}><{1}>{2}</{1}><{3}>{4}</{3}></{0}>", "ErrorMessage", "ErrorMessageTitle", title, "ErrorMessageBody", body);
            }
        }

        //The following methods are all no-ops as they are only required to fulfill the 
        //  WikiOutput contract
        public override void NonBreakingSpace()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void AddToFooter(string s)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void Write(string s)
        {

            _intermediate.Append(s);
        }
        public override void WriteOpenAnchor(string name)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteCloseAnchor()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteOpenOrderedList()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteCloseOrderedList()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteOpenPara()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteClosePara()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteOpenPreformatted()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteClosePreformatted()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteOpenProperty(string name)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteCloseProperty()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteOpenTable(AlignOption alignment, bool hasBorder, int Width)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteCloseTable()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteOpenTableRow()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteCloseTableRow()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteOpenUnorderedList()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteCloseUnorderedList()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteEndLine()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteHeading(string anchor, string text, int level)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteListItem(string each)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteRule()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteSingleLine(string each)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void WriteTableCell(string s, bool isHighlighted, 
            AlignOption alignment, int colSpan, int RowSpan, bool hasBorder, 
            bool allowBreaks, int Width, string bgcolor)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        private void WriteWom(string womData)
        {
            if (_initem)
            {
                _templist.Append(womData);
            }
            else
            {
                _intermediate.Append(womData);
            }
        }
    }
}