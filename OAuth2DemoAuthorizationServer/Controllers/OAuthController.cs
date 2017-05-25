using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using OAuth2DemoAuthorizationServer.Models;
using OAuth2DemoDbContext;
using DotNetOpenAuth.OAuth2.Messages;
using System.Configuration;
using System.Text;
using System.IO;
using OAuth2DemoSDK;

namespace OAuth2DemoAuthorizationServer.Controllers
{
    public class OAuthController : Controller
    {
        private static string _authorizeUrl = ConfigurationManager.AppSettings["AuthorizeUrl"];
        private static string[] _queryParameters = new string[] { "client_id", "redirect_uri", "state", "response_type", "scope" };
        private readonly AuthorizationServer _authorizationServer = new AuthorizationServer(new OAuth2AuthorizationServer());

        /// <summary>
        /// Prompts the user to authorize a client to access the user's private data.
        /// </summary>
        /// <returns>The browser HTML response that prompts the user to authorize the client.</returns>
        [AcceptVerbs(HttpVerbs.Get)]//HttpVerbs.Post
        //[Authorize]
        //[HttpHeader("x-frame-options", "SAMEORIGIN")] // mitigates clickjacking
        public ActionResult Authorize(string userkey)
        {
            var pendingRequest = this._authorizationServer.ReadAuthorizationRequest(Request);
            if (pendingRequest == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
            }

            if (string.IsNullOrEmpty(userkey))
            {
                string url = _authorizeUrl, callback = Request.Url.GetLeftPart(UriPartial.Path);
                StringBuilder querystring = new StringBuilder(string.Format("client_id={0}&", HttpUtility.UrlEncode(this.Request.QueryString["client_id"]))), callbackQuery = new StringBuilder();
                foreach (string key in this.Request.QueryString.Keys)
                {
                    if (!_queryParameters.Contains(key))
                        querystring.Append(string.Format("{0}={1}&", key, HttpUtility.UrlEncode(this.Request.QueryString[key])));
                    else
                        callbackQuery.Append(string.Format("{0}={1}&", key, HttpUtility.UrlEncode(this.Request.QueryString[key])));
                }
                if (callbackQuery.Length > 0)
                {
                    callback += ("?" + callbackQuery.ToString().TrimEnd('&'));
                    querystring.Append(string.Format("callback={0}&", HttpUtility.UrlEncode(callback)));
                }
                if (querystring.Length > 0)
                {
                    url += ("?" + querystring.ToString().TrimEnd('&'));
                }
                return Redirect(url);
            }
            else
            {
                using (var db = new OAuthDbContext())
                {
                    var client = db.Clients.FirstOrDefault(o => o.ClientIdentifier == pendingRequest.ClientIdentifier);
                    if (client == null)
                        throw new Exception("不受信任的商户");
                    else
                    {
                        var user = DESCrypt.Decrypt(userkey, client.ClientSecret);
                        var approval = this._authorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, user);
                        var response = this._authorizationServer.Channel.PrepareResponse(approval);
                        return response.AsActionResult();
                    }
                }
            }
        }
    }
}
