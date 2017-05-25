using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OAuth2DemoDbContext
{
    public class OAuth_SymmetricCryptoKey
    {
        [Key, Column(Order = 0)]
        public string Bucket { get; set; }

        [Key, Column(Order = 1)]
        public string Handle { get; set; }

        public DateTime ExpiresUtc { get; set; }

        public byte[] Secret { get; set; }
    }
}