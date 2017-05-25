using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace OAuth2DemoMVCClient.Controllers
{
    public class ClientCredentialController : Controller
    {
        /// <summary>
        /// The OAuth 2.0 client object to use to obtain authorization and authorize outgoing HTTP requests.
        /// </summary>
        private static readonly WebServerClient _client;

        /// <summary>
        /// The details about the sample OAuth-enabled WCF service that this sample client calls into.
        /// </summary>
        private static AuthorizationServerDescription _authServerDescription = new AuthorizationServerDescription
        {
            TokenEndpoint = new Uri(MvcApplication.TokenEndpoint),
            AuthorizationEndpoint = new Uri(MvcApplication.AuthorizationEndpoint)
        };

        private IAuthorizationState Authorization
        {
            get { return (AuthorizationState)HttpContext.Session["Authorization"]; }
            set { HttpContext.Session["Authorization"] = value; }
        }

        /// <summary>
        /// Initializes static members of the <see cref="SampleWcf2"/> class.
        /// </summary>
        static ClientCredentialController()
        {
            _client = new WebServerClient(_authServerDescription, "sampleconsumer", "samplesecret");
        }

        public ActionResult Index()
        {
            return View(Authorization);
        }

        [HttpPost]
        public ActionResult Index(bool flag)
        {
            if (Authorization == null)
            {
                Authorization = _client.GetClientAccessToken();
            }
            else if (Authorization.AccessTokenExpirationUtc < DateTime.UtcNow)
            {
                return View();
            }
            else
            {
                var request = new HttpRequestMessage(new HttpMethod("GET"), "http://demo.openapi.cn/bookcates");
                using (var httpClient = new HttpClient(_client.CreateAuthorizingHandler(Authorization)))
                {
                    using (var resourceResponse = httpClient.SendAsync(request))
                    {
                        ViewBag.Result = resourceResponse.Result.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            return View(Authorization);
        }
    }
}
