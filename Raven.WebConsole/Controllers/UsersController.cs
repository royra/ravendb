using System;
using System.Linq;
using System.Web.Mvc;
using Raven.Bundles.Authentication;
using Raven.Client;
using Raven.Client.Connection;
using Raven.WebConsole.ViewModels;

namespace Raven.WebConsole.Controllers
{
    public class UsersController : ContentController
    {
        private readonly IDocumentStore store;
        private readonly IDocumentSession session;

        public UsersController(IDocumentSession session, IDocumentStore store)
        {
            this.session = session;
            this.store = store;
        }

        public override ActionResult Index()
        {
            var users = session.Query<AuthenticationUser>().ToList();

            return View(new UsersViewModel()
                            {
                                Users = users.Select(u => new UsersViewModel.User(u.Name, u.Admin)),
                            });
        }

        [HttpPost]
        public ActionResult Delete(string name)
        {
            var ravenUser = RavenSession.Query<AuthenticationUser>()
                .FirstOrDefault(u => u.Name == name);

            if (ravenUser != null)
                session.Delete(ravenUser);

            return RedirectToAction("Index");
        }

        public ActionResult ValidateName(string name)
        {
            return new JsonResult { Data = ValidateNameJson(name), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private object ValidateNameJson(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "*";

            var existingUser = RavenSession.Query<AuthenticationUser>().FirstOrDefault(u => u.Name == name || u.Id == name);

            if (existingUser != null)
                return "Already exists";

            return true;
        }

        public class NewUserModel
        {
            public string Name { get; set; }
            public bool IsAdmin { get; set; }
            public string Password { get; set; }
        }

        public ActionResult New(NewUserModel model)
        {
            if (!ValidateNameJson(model.Name).Equals(true))
                return RedirectToAction("Index");

            session.Store(new AuthenticationUser
            {
                Id = string.Format("Users/{0}", model.Name),
                Admin = model.IsAdmin,
                AllowedDatabases = new[] { "*" },
                Name = model.Name,
            }.SetPassword(model.Password));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SetPassword(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("user should not be empty");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("password should not be empty");

            if (password.Length < 5)
                throw new ArgumentException("password should be 5 or more characters");

            var user = RavenSession.Query<AuthenticationUser>().SingleOrDefault(u => u.Name == name || u.Id == name);

            if (user == null)
                throw new Exception(string.Format("No such user '{0}'", name));

            user.SetPassword(password);

            return new EmptyResult();
        }
    }
}