using System;
using System.Linq;
using System.Web.Mvc;
using Raven.Bundles.Authentication;
using Raven.Client;

namespace Raven.WebConsole.Controllers
{
    public class BaseController : Controller
    {
        private readonly IDocumentSession session;

        public BaseController(IDocumentSession session)
        {
            this.session = session;
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            using (session)
            {
                if (filterContext.Exception != null)
                    return;

                if (session != null)
                    session.SaveChanges();
            }
        }

        protected AuthenticationUser GetUser(string name, bool throwIfNotExists = true)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("user should not be empty");

            var id = Keys.Database.AUTH_USERS_PREFIX + name;
            var user = session.Load<AuthenticationUser>(id);

            if (throwIfNotExists && user == null)
                throw new Exception(string.Format("No such user '{0}'", name));

            return user;
        }
    }
}