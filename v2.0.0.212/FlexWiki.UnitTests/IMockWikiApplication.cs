using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.UnitTests
{
    /// <summary>
    /// This interface is to allow access when testing to Application properties
    ///  that are normally read-only
    /// </summary>
    /// <remarks>
    /// Usage IMockWikiApplication appTest = (IMockWikiApplication) Federation.Application;
    /// </remarks>
    public interface IMockWikiApplication
    {
        void SetApplicationProperty(string key, bool value);
    }
}
