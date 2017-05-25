using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Web.Security;
using System.Net;
using Newtonsoft.Json;

namespace OAuth2DemoMVCClient.Controllers
{
    public class HomeController : Controller
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
            AuthorizationEndpoint = new Uri(MvcApplication.AuthorizationEndpoint),

            //TokenEndpoint = new Uri("http://localhost:4182/api/token"),
            //AuthorizationEndpoint = new Uri("http://localhost:4182/OAuth/Authorize")
        };

        private IAuthorizationState Authorization
        {
            get { return (AuthorizationState)HttpContext.Session["Authorization"]; }
            set { HttpContext.Session["Authorization"] = value; }
        }

        /// <summary>
        /// Initializes static members of the <see cref="SampleWcf2"/> class.
        /// </summary>
        static HomeController()
        {
            _client = new WebServerClient(_authServerDescription, "sampleconsumer", "samplesecret");
        }

        public ActionResult Index(string code, string state)
        {
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(state))
            {
                var authorization = _client.ProcessUserAuthorization(Request);
                Authorization = authorization;
                return View(authorization);
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(bool flag)
        {
            if (Authorization == null)
            {
                return _client.PrepareRequestUserAuthorization().AsActionResult();
            }
            else
            {
                var request = new HttpRequestMessage(new HttpMethod("GET"), "http://localhost:57036/api/values");
                using (var httpClient = new HttpClient(_client.CreateAuthorizingHandler(Authorization)))
                {
                    using (var resourceResponse = httpClient.SendAsync(request))
                    {
                        ViewBag.Result = resourceResponse.Result.Content.ReadAsStringAsync().Result;
                    }
                }
                return View(Authorization);
            }
        }

        public ActionResult Invoke()
        {
            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://demo.openapi.cn/bookcates");
            using (var httpClient = new HttpClient(_client.CreateAuthorizingHandler(Authorization)))
            {
                using (var resourceResponse = httpClient.SendAsync(request))
                {
                    ViewBag.Result = resourceResponse.Result.Content.ReadAsStringAsync().Result;
                }
            }
            return View(Authorization);
        }

        private static Random _random = new Random();

        private const string XsrfCookieName = "DotNetOpenAuth.WebServerClient.XSRF-Session";

        public ActionResult Pure(string code, string state)
        {
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(state))
            {
                var httpClient = new HttpClient();
                var httpContent = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"code", code},
                {"redirect_uri", "http://localhost:22187/"},
                {"grant_type","authorization_code"}
            });
                string concat = _client.ClientIdentifier + ":samplesecret";
                byte[] bits = Encoding.UTF8.GetBytes(concat);
                string base64 = Convert.ToBase64String(bits);
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64);
                var response = httpClient.PostAsync(MvcApplication.TokenEndpoint, httpContent).Result;
                Authorization = response.Content.ReadAsAsync<AuthorizationState>().Result;
                return View(Authorization);
            }
            return View();
        }

        [HttpPost]
        public ActionResult PureInvoke()
        {
            var httpClient = new HttpClient();

            if (Authorization == null)
            {
                //var cookies = new List<CookieHeaderValue>();
                string xsrfKey = this.GetNonCryptoRandomDataAsBase64(16);//生成随机数
                string url = MvcApplication.AuthorizationEndpoint + "?" + string.Format("client_id={0}&redirect_uri={1}&response_type={2}&state={3}",//&scope=
                     _client.ClientIdentifier, "http://localhost:22187/", "code", xsrfKey);

                HttpCookie xsrfKeyCookie = new HttpCookie(XsrfCookieName, xsrfKey);
                xsrfKeyCookie.HttpOnly = true;
                xsrfKeyCookie.Secure = FormsAuthentication.RequireSSL;
                Response.Cookies.Add(xsrfKeyCookie);

                return Redirect(url);
            }
            else
            {
                if (this.Authorization.AccessTokenExpirationUtc.HasValue && this.Authorization.AccessTokenExpirationUtc.Value < DateTime.UtcNow)
                {
                    var httpContent = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"refresh_token", Authorization.RefreshToken},
                {"grant_type","refresh_token"}
            });
                    string concat = _client.ClientIdentifier + ":samplesecret";
                    byte[] bits = Encoding.UTF8.GetBytes(concat);
                    string base64 = Convert.ToBase64String(bits);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);
                    var response = httpClient.PostAsync(MvcApplication.TokenEndpoint, httpContent).Result;
                    Authorization = response.Content.ReadAsAsync<AuthorizationState>().Result;
                }
                var bearerToken = this.Authorization.AccessToken;

                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                var request = new HttpRequestMessage(new HttpMethod("GET"), "http://demo.openapi.cn/bookcates");
                using (var resourceResponse = httpClient.SendAsync(request))
                {
                    ViewBag.Result = resourceResponse.Result.Content.ReadAsStringAsync().Result;
                }
                return View(Authorization);
            }
        }

        private string GetNonCryptoRandomDataAsBase64(int binaryLength)
        {
            byte[] buffer = new byte[binaryLength];
            _random.NextBytes(buffer);
            string uniq = Convert.ToBase64String(buffer);
            return uniq;
        }

        public ActionResult DemoRequestCode()
        {
            string xsrfKey = this.GetNonCryptoRandomDataAsBase64(16);//生成随机数
            string url = MvcApplication.AuthorizationEndpoint + "?" +
                string.Format("client_id={0}&redirect_uri={1}&response_type={2}&state={3}",
                "democlient", "http://localhost:22187/", "code", xsrfKey);
            HttpCookie xsrfKeyCookie = new HttpCookie(XsrfCookieName, xsrfKey);
            xsrfKeyCookie.HttpOnly = true;
            xsrfKeyCookie.Secure = FormsAuthentication.RequireSSL;
            Response.Cookies.Add(xsrfKeyCookie);

            return Redirect(url);
        }

        private bool VerifyState(string state)
        {
            var cookie = Request.Cookies[XsrfCookieName];
            if (cookie == null)
                return false;

            var xsrfCookieValue = cookie.Value;
            return xsrfCookieValue == state;
        }

        private AuthenticationHeaderValue SetAuthorizationHeader()
        {
            string concat = "democlient:samplesecret";
            byte[] bits = Encoding.UTF8.GetBytes(concat);
            string base64 = Convert.ToBase64String(bits);
            return new AuthenticationHeaderValue("Basic", base64);
        }

        public ActionResult Demo(string code, string state)
        {
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(state) && VerifyState(state))
            {
                var httpClient = new HttpClient();
                var httpContent = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"code", code},
                {"redirect_uri", "http://localhost:22187/"},
                {"grant_type","authorization_code"}
            });
                httpClient.DefaultRequestHeaders.Authorization = this.SetAuthorizationHeader();

                var response = httpClient.PostAsync(MvcApplication.TokenEndpoint, httpContent).Result;
                Authorization = response.Content.ReadAsAsync<AuthorizationState>().Result;
                return View(Authorization);
            }
            return View();
        }

        private void RefreshAccessToken()
        {
            var httpClient = new HttpClient();
            var httpContent = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"refresh_token", Authorization.RefreshToken},
                {"grant_type","refresh_token"}
            });
            httpClient.DefaultRequestHeaders.Authorization = this.SetAuthorizationHeader();

            var response = httpClient.PostAsync(MvcApplication.TokenEndpoint, httpContent).Result;
            Authorization = response.Content.ReadAsAsync<AuthorizationState>().Result;
        }

        public ActionResult DemoInvoke()
        {
            var httpClient = new HttpClient();
            if (this.Authorization.AccessTokenExpirationUtc.HasValue && this.Authorization.AccessTokenExpirationUtc.Value < DateTime.UtcNow)
            {
                this.RefreshAccessToken();
            }
            var bearerToken = this.Authorization.AccessToken;

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://demo.openapi.cn/bookcates");
            using (var resourceResponse = httpClient.SendAsync(request))
            {
                ViewBag.Result = resourceResponse.Result.Content.ReadAsStringAsync().Result;
            }
            return View(Authorization);
        }
    }
}
