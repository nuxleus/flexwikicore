using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using FlexWiki.Collections;
using FlexWiki.Web;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class ApplicationConfigurationTests
    {
        private MockWikiApplication _application;
        private FlexWikiWebApplicationConfiguration _applicationConfiguration;

        [SetUp]
        public void Setup()
        {
            _application = new MockWikiApplication(
                new LinkMaker("test://ApplicationConfigurationTests/"), OutputFormat.HTML);
            _applicationConfiguration = _application.ApplicationConfiguration;

        }

        [Test]
        public void GetContentUploadPath()
        {
            //ContentUploadPath is option, but defined as "content\upload" for testing
            Assert.AreEqual(_applicationConfiguration.ContentUploadPath, "content\\upload");
        }
    }
}
