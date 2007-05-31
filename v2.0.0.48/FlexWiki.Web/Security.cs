using System;
using System.IO;
using System.Text;
using System.Security;
using System.Globalization;
using System.Security.Cryptography;

namespace FlexWiki.Web
{
    public class Security
    {
        public static string Decrypt(string ciphertext, string masterKey)
        {
            byte[] key = HexStringToByteArray(masterKey);
            byte[] rgCiphertext = HexStringToByteArray(ciphertext);
            return Decrypt(rgCiphertext, key);
        }
        public static string Encrypt(string plaintext, string masterKey)
        {
            byte[] key = HexStringToByteArray(masterKey);
            byte[] ciphertext = Encrypt(plaintext, key);
            string ct = ByteArrayToHexString(ciphertext);
            return ct;
        }
        
        internal static string ByteArrayToHexString(byte[] arr)
        {
            StringBuilder sb = new StringBuilder(arr.Length);
            for (int i = 0; i < arr.Length; i++)
                sb.AppendFormat("{0:X2}", arr[i]);
            return sb.ToString();
        }
        internal static string Decrypt(byte[] ciphertext, byte[] masterKey)
        {
            RC2CryptoServiceProvider alg = new RC2CryptoServiceProvider();
            alg.Mode = CipherMode.ECB;
            alg.Key = masterKey;
            MemoryStream media = new MemoryStream(ciphertext);
            CryptoStream cryptoStream = new CryptoStream(media, alg.CreateDecryptor(), CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cryptoStream, Encoding.UTF8);
            string password = reader.ReadToEnd();
            reader.Close();
            return password;
        }
        internal static byte[] Encrypt(string plaintext, byte[] masterKey)
        {
            RC2CryptoServiceProvider alg = new RC2CryptoServiceProvider();
            alg.Mode = CipherMode.ECB;
            alg.Key = masterKey;
            MemoryStream media = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(media, alg.CreateEncryptor(), CryptoStreamMode.Write);
            StreamWriter writer = new StreamWriter(cryptoStream, Encoding.UTF8);
            writer.Write(plaintext);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            byte[] ciphertext = new byte[media.Length];
            writer.Close();
            Array.Copy(media.GetBuffer(), 0, ciphertext, 0, ciphertext.Length);
            return ciphertext;
        }
        internal static byte[] Encrypt(byte[] plaintext, byte[] masterKey)
        {
            RC2CryptoServiceProvider alg = new RC2CryptoServiceProvider();
            alg.Mode = CipherMode.ECB;
            alg.Key = masterKey;
            MemoryStream media = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(media, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(plaintext, 0, plaintext.Length);
            cryptoStream.FlushFinalBlock();
            byte[] ciphertext = new byte[media.Length];
            cryptoStream.Close();
            Array.Copy(media.GetBuffer(), 0, ciphertext, 0, ciphertext.Length);
            return ciphertext;
        }
        internal static byte[] HexStringToByteArray(string s)
        {
            byte[] ret = new byte[s.Length / 2];
            int retIdx = 0;
            for (int i = 0; i < s.Length; i += 2)
                ret[retIdx++] = byte.Parse(s[i].ToString() + s[i + 1].ToString(), NumberStyles.HexNumber);
            return ret;
        }

        internal static string Decrypt(string encryptedCode, object CaptchaKey)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}