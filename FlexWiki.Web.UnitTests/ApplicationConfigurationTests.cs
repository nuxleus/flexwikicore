using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using FlexWiki.Collections;
using FlexWiki.Web;

namespace FlexWiki.Web.UnitTests
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
        public void GetRequireCaptchaOnEdit()
        {
            Assert.AreEqual(_applicationConfiguration.RequireCaptchaOnEdit.ToString(), "Never");
        }

        [Test]
        public void GetCaptchaKey()
        {
            Assert.AreEqual(_applicationConfiguration.CaptchaKey.ToString(), "0123456789ABCDEF");
        }

        [Test]
        public void GetCaptchaLinkThreshold()
        {
            Assert.AreEqual(_applicationConfiguration.CaptchaLinkThreshold, 5);
        }

        [Test]
        public void GetContentUploadPath()
        {
            //ContentUploadPath is option, but defined as "content\upload" for testing
            Assert.AreEqual(_applicationConfiguration.ContentUploadPath, "content\\upload");
        }

        [Test]
        public void GetAttachmentIcons()
        {
            Assert.AreEqual(_applicationConfiguration.AttachmentIcons.GetUpperBound(0), 19);
            Assert.AreEqual(_applicationConfiguration.AttachmentIcons[0].Href, "page_white_picture.png");
            Assert.AreEqual(_applicationConfiguration.AttachmentIcons[1].IconKey, ".gif");
            Assert.AreEqual(_applicationConfiguration.AttachmentIcons[18].IconKey, ".divx");
            Assert.AreEqual(_applicationConfiguration.AttachmentIcons[19].Href, "film.png");
        }

        [Test]
        public void GetDisableRenameFixup()
        {
            Assert.IsFalse(_applicationConfiguration.DisableRenameFixup);
        }

        [Test]
        public void GetEditOnDoubleClick()
        {
            Assert.IsTrue(_applicationConfiguration.EditOnDoubleClick);
        }

        [Test]
        public void GetFederationConfiguration()
        {
            Assert.IsNotNull(_applicationConfiguration.FederationConfiguration);
        }

        [Test]
        public void GetLog4NetConfigPath()
        {
            Assert.IsNull(_applicationConfiguration.Log4NetConfigPath);
        }

        [Test]
        public void GetNewsletterConfiguration()
        {
            Assert.IsNotNull(_applicationConfiguration.NewsletterConfiguration);
            Assert.AreEqual(_applicationConfiguration.NewsletterConfiguration.AuthenticateAs, "anonymous");
        }
    }
}
