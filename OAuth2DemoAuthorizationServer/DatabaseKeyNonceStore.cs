using DotNetOpenAuth.Messaging.Bindings;
using OAuth2DemoDbContext;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace OAuth2DemoAuthorizationServer
{
    public class DatabaseKeyNonceStore : INonceStore, ICryptoKeyStore
    {
        private Dictionary<string, string> _dicContext = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseKeyNonceStore"/> class.
        /// </summary>
        public DatabaseKeyNonceStore()
        {
        }

        #region INonceStore Members

        public bool StoreNonce(string context, string nonce, DateTime timestampUtc)
        {
            if (!_dicContext.ContainsKey(context))
                _dicContext.Add(context, this.ToMD5String(context));

            using (var db = new OAuthDbContext())
            {
                db.Nonces.Add(new OAuth_Nonce { Context = _dicContext[context], Code = nonce, Timestamp = timestampUtc });
                try
                {
                    db.SaveChanges();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        #endregion

        private string ToMD5String(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.Unicode.GetBytes(str);
            byte[] toData = md5.ComputeHash(fromData);
            string byteStr = null;
            for (int i = 0; i < toData.Length; i++)
            {
                byteStr += toData[i].ToString("x");
            }
            return byteStr;
        }

        #region ICryptoKeyStore Members

        public CryptoKey GetKey(string bucket, string handle)
        {
            using (var db = new OAuthDbContext())
            {
                // It is critical that this lookup be case-sensitive, which can only be configured at the database.
                var matches = from key in db.SymmetricCryptoKeys
                              where key.Bucket == bucket && key.Handle == handle
                              select key;

                var k = matches.FirstOrDefault();
                return new CryptoKey(k.Secret, this.AsUtc(k.ExpiresUtc));
            }
        }

        public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket)
        {
            using (var db = new OAuthDbContext())
            {
                var query = from key in db.SymmetricCryptoKeys
                            where key.Bucket == bucket
                            orderby key.ExpiresUtc descending
                            select key;
                var keys = query.ToList();
                return keys.Select(k => new KeyValuePair<string, CryptoKey>(k.Handle, new CryptoKey(k.Secret, this.AsUtc(k.ExpiresUtc))));
            }
        }

        public void StoreKey(string bucket, string handle, CryptoKey key)
        {
            var keyRow = new OAuth_SymmetricCryptoKey()
            {
                Bucket = bucket,
                Handle = handle,
                Secret = key.Key,
                ExpiresUtc = key.ExpiresUtc,
            };
            using (var db = new OAuthDbContext())
            {
                db.SymmetricCryptoKeys.Add(keyRow);
                db.SaveChanges();
            }
        }

        public void RemoveKey(string bucket, string handle)
        {
            using (var db = new OAuthDbContext())
            {
                var match = db.SymmetricCryptoKeys.FirstOrDefault(k => k.Bucket == bucket && k.Handle == handle);
                if (match != null)
                {
                    db.SymmetricCryptoKeys.Remove(match);
                    db.SaveChanges();
                }
            }
        }

        #endregion

        /// <summary>
        /// Ensures that local times are converted to UTC times.  Unspecified kinds are recast to UTC with no conversion.
        /// </summary>
        /// <param name="value">The date-time to convert.</param>
        /// <returns>The date-time in UTC time.</returns>
        DateTime AsUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified)
            {
                return new DateTime(value.Ticks, DateTimeKind.Utc);
            }

            return value.ToUniversalTime();
        }
    }
}