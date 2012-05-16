using System.Web.Mvc;

namespace Raven.WebConsole.Controllers
{
    [Authorize]
    public abstract class ContentController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.User = User.Identity.Name;
        }

        public virtual ActionResult Index()
        {
            return View();
        }
    }
}