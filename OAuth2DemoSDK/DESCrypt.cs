using OAuth2DemoDbContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OAuth2DemoSDK
{
    public static class DESCrypt
    {
        /// <summary>
        /// 加密用户信息
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <param name="clientid">商户标识</param>
        /// <returns>加密字符串</returns>
        public static string Encrypt(string user, string clientid)
        {
            using (var db = new OAuthDbContext())
            {
                var c = db.Clients.FirstOrDefault(o => o.ClientIdentifier == clientid);
                if (c == null)
                    throw new Exception("不受信任的商户");
                byte[] key = ToMD5(c.ClientSecret);
                byte[] buffer = Encoding.UTF8.GetBytes(user);
                byte[] cipher = Encrypt(buffer, key);
                return Convert.ToBase64String(cipher);
            }            
        }

        /// <summary>
        /// 解密用户信息
        /// </summary>
        /// <param name="userkey">加密字符串</param>
        /// <param name="clientSecret">商户密钥</param>
        /// <returns>用户信息</returns>
        public static string Decrypt(string userkey, string clientSecret)
        {
            byte[] key = ToMD5(clientSecret);
            byte[] cipher = Convert.FromBase64String(userkey);
            byte[] plainText = Decrypt(cipher, key);
            return Encoding.UTF8.GetString(plainText);
        }

        private static string Encrypt(string plainText, byte[] key)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(plainText);
            byte[] cipher = Encrypt(buffer, key);
            return Convert.ToBase64String(cipher);
        }

        private static string Decrypt(string cipherText, byte[] key)
        {
            byte[] cipher = Convert.FromBase64String(cipherText);
            byte[] plainText = Decrypt(cipher, key);
            return Encoding.UTF8.GetString(plainText);
        }

        private static byte[] Encrypt(byte[] buffer, byte[] key)
        {
            using (SymmetricAlgorithm crypto = CreateSymmetricAlgorithm(key))
            {
                using (var ms = new MemoryStream())
                {
                    var binaryWriter = new BinaryWriter(ms);
                    binaryWriter.Write((byte)1);
                    binaryWriter.Write(crypto.IV);
                    binaryWriter.Flush();

                    var cryptoStream = new CryptoStream(ms, crypto.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(buffer, 0, buffer.Length);
                    cryptoStream.FlushFinalBlock();

                    return ms.ToArray();
                }
            }
        }

        private static byte[] Decrypt(byte[] buffer, byte[] key)
        {
            using (SymmetricAlgorithm crypto = CreateSymmetricAlgorithm(key))
            {
                using (var ms = new MemoryStream(buffer))
                {
                    var binaryReader = new BinaryReader(ms);
                    int algorithmVersion = binaryReader.ReadByte();
                    crypto.IV = binaryReader.ReadBytes(crypto.IV.Length);

                    var decryptedBuffer = new byte[buffer.Length];
                    int actualDecryptedLength;

                    using (var cryptoStream = new CryptoStream(ms, crypto.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        actualDecryptedLength = cryptoStream.Read(decryptedBuffer, 0, decryptedBuffer.Length);
                    }

                    var finalDecryptedBuffer = new byte[actualDecryptedLength];
                    Array.Copy(decryptedBuffer, finalDecryptedBuffer, actualDecryptedLength);
                    return finalDecryptedBuffer;
                }
            }
        }

        private static SymmetricAlgorithm CreateSymmetricAlgorithm(byte[] key)
        {
            SymmetricAlgorithm result = null;
            try
            {
                result = SymmetricAlgorithm.Create();
                result.Mode = CipherMode.CBC;
                result.Key = key;
                return result;
            }
            catch
            {
                IDisposable disposableResult = result;
                if (disposableResult != null)
                {
                    disposableResult.Dispose();
                }

                throw;
            }
        }

        private static byte[] ToMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.Unicode.GetBytes(str);
            byte[] toData = md5.ComputeHash(fromData);//结果为长度为16的字节数组(128bit)
            return toData;
        }
    }
}
