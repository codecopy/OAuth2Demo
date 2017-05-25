using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace OAuth2DemoMVCClient.Controllers
{
    public class PasswordCredentialController : Controller
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
        static PasswordCredentialController()
        {
            _client = new WebServerClient(_authServerDescription, "sampleconsumer", "samplesecret");
        }

        public ActionResult Index()
        {
            return View(Authorization);
        }

        [HttpPost]
        public ActionResult Index(string username, string password)
        {
            //var user = UserClient.Instance.ValidateUser(username, password, Shumi.Users.PasswordFormat.Cleartext);
            //if (user != null)
            //{
            //    FormsAuthentication.SetAuthCookie(user.Name, false);
            //    Authorization = _client.ExchangeUserCredentialForToken(username, password);
            //}
            if (Authorization == null)
            {
                try
                {
                    Authorization = _client.ExchangeUserCredentialForToken(username, password);
                }
                catch (ProtocolException ex)
                {
                    ViewBag.LoginError = "请求被拒绝，请检查用户名或密码是否有误！";
                }
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
