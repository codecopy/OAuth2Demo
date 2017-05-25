using DotNetOpenAuth.Messaging.Bindings;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OAuth2.ChannelElements;
using DotNetOpenAuth.OAuth2.Messages;
using OAuth2DemoDbContext;
using OAuth2DemoAuthorizationServer.Models;
using System;
using System.Configuration;
using System.Linq;

namespace OAuth2DemoAuthorizationServer
{
    internal class OAuth2AuthorizationServer : IAuthorizationServerHost
    {
        #region Implementation of IAuthorizationServerHost

        public ICryptoKeyStore CryptoKeyStore
        {
            get { return MvcApplication.KeyNonceStore; }
        }

        public INonceStore NonceStore
        {
            get { return MvcApplication.KeyNonceStore; }
        }

        public AccessTokenResult CreateAccessToken(IAccessTokenRequest accessTokenRequestMessage)
        {
            var accessToken = new MappingAccessToken();

            int minutes;
            string setting = ConfigurationManager.AppSettings["AccessTokenLifeTime"];
            if (!int.TryParse(setting, out minutes))
            {
                minutes = 30;
            }
            accessToken.Lifetime = TimeSpan.FromMinutes(minutes);

            //accessToken.ResourceServerEncryptionKey = new RSACryptoServiceProvider();
            //accessToken.ResourceServerEncryptionKey.ImportParameters(ResourceServerEncryptionPublicKey);
            //accessToken.AccessTokenSigningKey = CreateRSA();

            var result = new AccessTokenResult(accessToken);
            return result;
        }

        public IClientDescription GetClient(string clientIdentifier)
        {
            using (var db = new OAuthDbContext())
            {
                var consumerRow = db.Clients.SingleOrDefault(
                    consumerCandidate => consumerCandidate.ClientIdentifier == clientIdentifier);
                if (consumerRow == null)
                {
                    throw new ArgumentOutOfRangeException("clientIdentifier");
                }

                return new OAuthClientDescription(consumerRow);
            }
        }

        public bool IsAuthorizationValid(IAuthorizationDescription authorization)
        {
            //这里可以改进
            //只要到达这步，说明authorization.Scope已经获得认证了，不需要再重新校验
            //return this.IsAuthorizationValid(authorization.Scope, authorization.ClientIdentifier, authorization.UtcIssued, authorization.User);
            return true;
        }

        public AutomatedUserAuthorizationCheckResponse CheckAuthorizeResourceOwnerCredentialGrant(string userName, string password, IAccessTokenRequest accessRequest)
        {
            try
            {
                if (userName == "admin" && password == "testpwd")
                {
                    var userid = "999";
                    return new AutomatedUserAuthorizationCheckResponse(accessRequest, true, userid);
                }
                else
                {
                    return new AutomatedUserAuthorizationCheckResponse(accessRequest, false, null);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Determines whether an access token request given a client credential grant should be authorized
        /// and if so records an authorization entry such that subsequent calls to <see cref="IsAuthorizationValid" /> would
        /// return <c>true</c>.
        /// </summary>
        /// <param name="accessRequest">The access request the credentials came with.
        /// This may be useful if the authorization server wishes to apply some policy based on the client that is making the request.</param>
        /// <returns>
        /// A value that describes the result of the authorization check.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public AutomatedAuthorizationCheckResponse CheckAuthorizeClientCredentialsGrant(IAccessTokenRequest accessRequest)
        {
            //客户密码已经在框架内部校验，因此这里不需要重复校验（且密码也没有被框架带过来）
            //AuthenticatedClientRequestBase request = accessRequest as AuthenticatedClientRequestBase;
            //using (var db = new OAuthDbContext())
            //{
            //    var consumerRow = db.Clients.SingleOrDefault(
            //        cc => cc.ClientIdentifier == request.ClientIdentifier && cc.ClientSecret == request.ClientSecret);
            //    if (consumerRow == null)
            //    {
            //        return new AutomatedUserAuthorizationCheckResponse(accessRequest, false, null);
            //    }

            //    return new AutomatedUserAuthorizationCheckResponse(accessRequest, true, null);
            //}

            //校验通过后，ClientAuthenticated为true
            if (accessRequest.ClientAuthenticated)
            {
                // Before returning a positive response, be *very careful* to validate the requested access scope
                // to make sure it is appropriate for the requesting client.
                return new AutomatedAuthorizationCheckResponse(accessRequest, true);
            }
            else
            {
                // Only authenticated clients should be given access.
                return new AutomatedAuthorizationCheckResponse(accessRequest, false);
            }
        }

        #endregion
    }
}