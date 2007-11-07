using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public class NestedContextUnexpectedException : Exception
    {
        public NestedContextUnexpectedException()
            : base("Nested request contexts are not supported unless RequestContextOptions.AllowNesting is specified. Use this option with extreme caution, as misuse can result in cache corruption.")
        {
        }
    }
}
