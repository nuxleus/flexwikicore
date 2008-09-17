using System;

using FlexWiki;

namespace FlexWiki.Formatting
{
    public interface IParserEngine
    {
        GrammarDocument Grammar { get; }
        ParserEngine Parser { get; }
    }
}