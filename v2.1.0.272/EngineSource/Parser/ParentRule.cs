using System;

using FlexWiki;

namespace FlexWiki.Formatting
{
    public class ParentRule : ParserRule
    {
        public ParentRule(string womObject, string pattern, string end, string jump,
                            string optimization, string[] elements, ParserContext ruleContext, ParserContext context)
            : base(womObject, pattern, end, jump, optimization, elements, ruleContext, context)
        {
        }
    }
}