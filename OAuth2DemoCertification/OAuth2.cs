using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using DotNetOpenAuth.OAuth2;
using System.Security.Cryptography;
using System.ServiceModel.Channels;
using System.ServiceModel;
using OAuth2DemoDbContext;

namespace OAuth2DemoCertification
{
    /// <summary>
    /// OAuth2认证服务
    /// </summary>
    public class AuthenticationOAuth2
    {
        /// <summary>
        /// 实现认证接口
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public SiteUser Authorize(OperationContext operationContext)
        {
            var httpDetails = operationContext.RequestContext.RequestMessage.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            var requestUri = operationContext.RequestContext.RequestMessage.Properties.Via;

            try
            {
                var principal = this.VerifyOAuth2Async(
                    httpDetails,
                    requestUri);
                //operationContext.IncomingMessageHeaders.Action ?? operationContext.IncomingMessageHeaders.To.AbsolutePath
                //若requiredScopes不在已授权scope范围内，则将抛出ProtocolFaultResponseException异常                
                if (principal != null)
                {
                    int userid = 0;
                    if (int.TryParse(principal.Identity.Name, out userid))
                    {
                        if (userid != 999)
                            throw new Exception("不存在相关用户对此应用授权");
                        return new SiteUser { Name = "admin", ID = userid };
                    }
                    else
                        throw new Exception("用户标示不合法.");
                    return null;
                }
                else
                    throw new Exception("OAuth认证失败.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IPrincipal VerifyOAuth2Async(HttpRequestMessageProperty httpDetails, Uri requestUri, params string[] requiredScopes)
        {
            var resourceServer = new ResourceServer(new MappingAccessTokenAnalyzer());//signing, encrypting
            return resourceServer.GetPrincipal(httpDetails, requestUri, requiredScopes: requiredScopes);
        }
    }
}
