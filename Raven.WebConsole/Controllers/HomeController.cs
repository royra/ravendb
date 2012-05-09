using System.Web.Mvc;

namespace Raven.WebConsole.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
