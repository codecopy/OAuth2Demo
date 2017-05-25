using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using OAuth2DemoDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OAuth2DemoCertification
{
    public class MappingAccessTokenAnalyzer : IAccessTokenAnalyzer
    {
        public AccessToken DeserializeAccessToken(IDirectedProtocolMessage message, string accessToken)
        {
            //此处考虑到性能，可加cache
            return GetAccessToken(accessToken);
        }

        private AccessToken GetAccessToken(string accessToken)
        {
            using (var db = new OAuthDbContext())
            {
                var query = from auth in db.ClientAuthorizations
                            from client in db.Clients
                            where auth.ClientId == client.ClientId && auth.Token == accessToken
                            select new
                            {
                                client.ClientIdentifier,
                                auth.UserId,
                                auth.Scope,
                                auth.ExpirationDateUtc,
                                auth.CreatedOnUtc
                            };
                var clientAuth = query.FirstOrDefault();
                if (clientAuth == null)
                    throw new Exception("当前AccessToken无效，请重新认证！");

                else if (clientAuth.ExpirationDateUtc.HasValue && clientAuth.ExpirationDateUtc < DateTime.UtcNow)
                    throw new Exception("当前AccessToken已过期！");

                //token.UtcIssued和token.Lifetime此处可以不赋值（后续并没有用到）
                var token = new AccessToken
                {
                    ClientIdentifier = clientAuth.ClientIdentifier,
                    User = clientAuth.UserId
                };

                var scopes = OAuthUtilities.SplitScopes(clientAuth.Scope);
                if (scopes.Count > 0) token.Scope.AddRange(scopes);

                return token;
            }
        }
    }
}
