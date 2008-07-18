using System;
using System.Collections.Generic;
using System.Text;

namespace WebTestConsole
{
    internal class ParseOptionsException : Exception
    {
        internal ParseOptionsException(string message)
            : base(message)
        {
        }
    }
}
