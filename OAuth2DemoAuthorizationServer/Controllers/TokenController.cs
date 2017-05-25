using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using OAuth2DemoDbContext;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace OAuth2DemoAuthorizationServer.Controllers
{
    public class TokenController : ApiController
    {
        // POST /token
        public HttpResponseMessage AccessToken(HttpRequestMessage request)
        {
            var authServer = new AuthorizationServer(new OAuth2AuthorizationServer());
            var message = authServer.HandleTokenRequest(request);
            return message.AsHttpResponseMessage();
        }

    }
}
