using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using FlexWiki.Collections;
using FlexWiki.Web;

namespace FlexWiki.UnitTests.Security
{
    [TestFixture]
    public class CrytpographicSecurityTests
    {
        [Test]
        public void EncryptAndDecrypt()
        {
            string testKey = "0123456789ABCDEF";
            string testCode = "98765";
            string encryptedVal = FlexWiki.Web.Security.Encrypt(testCode, testKey);
            Assert.AreEqual(32, encryptedVal.Length, "Ensure the cypher text returned by Encrypt is 32 bytes in length.");
            Assert.AreEqual("D11BC1D249D7EDFF82EC047F1FD0FFA1", encryptedVal, "Ensure encrypted value is equal to the specified cyphertext");
            string decryptedVal = FlexWiki.Web.Security.Decrypt(encryptedVal, testKey);
            Assert.AreEqual(decryptedVal, testCode, "Ensure decrypted value is equal to original value");

        }

    }
}
