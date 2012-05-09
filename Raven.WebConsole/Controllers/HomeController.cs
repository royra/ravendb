using System.Dynamic;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Raven.WebConsole.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string user, string password, string persistent = null, string returnurl = null)
        {
            user = user ?? "";
            user = user.Trim().ToLowerInvariant();

            var success = user == "admin" && password == "12345";

            if (success)
            {
                var cookie = FormsAuthentication.GetAuthCookie(user, persistent != null);
                Response.Cookies.Set(cookie);

                return string.IsNullOrWhiteSpace(returnurl)
                           ? (ActionResult)RedirectToAction("Index")
                           : new RedirectResult(returnurl);
            }

            ViewBag.Error = true;
            ViewBag.User = user;
            ViewBag.Persistent = persistent != null;

            return View();
        }
    }
}
