using System.Linq;
using Raven.Bundles.Authentication;
using Raven.Client;
using Raven.WebConsole.ViewModels;

namespace Raven.WebConsole.Controllers
{
    public class UsersController : ContentController
    {
        private readonly IDocumentSession session;

        public UsersController(IDocumentSession session)
        {
            this.session = session;
        }

        public override System.Web.Mvc.ActionResult Index()
        {
            var users = session.Query<AuthenticationUser>().ToList();

            return View(new UsersViewModel()
                            {
                                Users = users.Select(u => new UsersViewModel.User(u.Name, u.Admin)),
                            });
        }
    }
}