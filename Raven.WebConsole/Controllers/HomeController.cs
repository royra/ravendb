using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Raven.Bundles.Authentication;
using Raven.Client;
using Raven.Client.Linq;

namespace Raven.WebConsole.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IDocumentSession session) : base(session)
        {
        }

        public RedirectToRouteResult DefaultPage
        {
            get
            {
                return RedirectToAction("Index", "Databases");
            }
        }

        [Authorize]
        public ActionResult Index()
        {
            return DefaultPage;
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string user, string password, string persistent = null, string returnurl = null)
        {
            user = user ?? "";
            user = user.Trim();

            var ravenUser = GetUser(user, false);

            var success = ravenUser != null && ravenUser.ValidatePassword(password);

            if (success)
            {
                var cookie = FormsAuthentication.GetAuthCookie(ravenUser.Name, persistent != null);
                Response.Cookies.Set(cookie);

                return string.IsNullOrWhiteSpace(returnurl)
                           ? (ActionResult)DefaultPage
                           : new RedirectResult(returnurl);
            }

            ViewBag.Error = true;
            ViewBag.User = user;
            ViewBag.Persistent = persistent != null;

            return View();
        }

        public ActionResult Logout()
        {
            var cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null)
            {
                cookie.Expires = new DateTime(1970, 1, 1);
                Response.Cookies.Set(cookie);
            }

            return RedirectToAction("Index");
        }
    }
}
