using DotNetOpenAuth.OAuth2;
using OAuth2DemoDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace OAuth2DemoAuthorizationServer
{
    public class MappingAccessToken : AuthorizationServerAccessToken
    {
        protected override string Serialize()
        {
            DateTime? expirationDateUtc = null;
            if (this.Lifetime.HasValue)
            {
                DateTime expirationDate = this.UtcIssued + this.Lifetime.Value;
                expirationDateUtc = expirationDate.ToLocalTime();
            }
            var token = this.SaveClientAuthorization(this.ClientIdentifier, this.User, OAuthUtilities.JoinScopes(this.Scope), expirationDateUtc);
            return token;
        }

        private string SaveClientAuthorization(string clientIdentifier, string userid, string scope, DateTime? expirationDateUtc)
        {
            string token = Guid.NewGuid().ToString().ToUpper();

            using (var db = new OAuthDbContext())
            {
                var query = from auth in db.ClientAuthorizations
                            from client in db.Clients
                            where
                                auth.ClientId == client.ClientId && client.ClientIdentifier == clientIdentifier
                                && auth.UserId == userid
                            select auth;
                var clientAuth = query.FirstOrDefault();
                if (clientAuth == null)
                {
                    var client = db.Clients.FirstOrDefault(o => o.ClientIdentifier == clientIdentifier);
                    if (client == null)
                        throw new Exception("不受信任的商户！");

                    clientAuth = new OAuth_ClientAuthorization
                    {
                        ClientId = client.ClientId,
                        CreatedOnUtc = DateTime.Now,
                        Scope = scope,
                        UserId = userid,
                        Token = token,
                        ExpirationDateUtc = expirationDateUtc
                    };
                    db.ClientAuthorizations.Add(clientAuth);
                }
                else
                {
                    clientAuth.CreatedOnUtc = DateTime.Now;
                    clientAuth.Scope = scope;
                    clientAuth.Token = token;
                    clientAuth.ExpirationDateUtc = expirationDateUtc;
                }
                db.SaveChanges();
            }

            return token;
        }
    }
}