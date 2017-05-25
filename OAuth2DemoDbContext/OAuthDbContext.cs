using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OAuth2DemoDbContext
{
    public class OAuthDbContext : System.Data.Entity.DbContext
    {
        public DbSet<OAuth_Nonce> Nonces { get; set; }

        public DbSet<OAuth_SymmetricCryptoKey> SymmetricCryptoKeys { get; set; }

        public DbSet<OAuth_ClientOpenApi> MerchantOpenApis { get; set; }

        public DbSet<OAuth_Client> Clients { get; set; }

        public DbSet<OAuth_ClientAuthorization> ClientAuthorizations { get; set; }

        public DbSet<OAuth_ClientOpenApi> ClientOpenApis { get; set; }
    }
}