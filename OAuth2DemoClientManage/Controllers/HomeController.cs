using OAuth2DemoDbContext;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OAuth2DemoClientManage.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(string username, string password, string returnUrl)
        {
            if (username == "admin" && password == ConfigurationManager.AppSettings["admin"])
            {
                FormsAuthentication.SetAuthCookie(username, false);
                return Redirect(returnUrl);
            }
            else
            {
                using (var db = new OAuthDbContext())
                {
                    if (db.Clients.Any(o => o.AccountName == username && o.AccountPassword == password))
                    {
                        FormsAuthentication.SetAuthCookie(username, false);
                        return Redirect(returnUrl);
                    }
                }
            }
            ViewBag.LoginError = "用户名或密码有误！";
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
    }
}
