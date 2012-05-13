using System.Web.Mvc;
using Raven.Client;

namespace Raven.WebConsole.Controllers
{
    public class BaseController : Controller
    {
        protected IDocumentSession RavenSession
        {
            get { return DependencyResolver.Current.GetService<IDocumentSession>(); }
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            using (RavenSession)
            {
                if (filterContext.Exception != null)
                    return;

                if (RavenSession != null)
                    RavenSession.SaveChanges();
            }
        }
    }
}