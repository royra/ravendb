using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Raven.Client.Linq;
using Raven.WebConsole.Entities;

namespace Raven.WebConsole.Controllers
{
    public class HomeController : BaseController
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

// ReSharper disable ReplaceWithSingleCallToFirstOrDefault
            var ravenUser = RavenSession.Query<User>().Where(u => u.Name == user).FirstOrDefault();
// ReSharper restore ReplaceWithSingleCallToFirstOrDefault

            var success = ravenUser != null && ravenUser.Password.Check(password);

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
