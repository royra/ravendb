using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Raven.Bundles.Authentication;
using Raven.Client;
using Raven.Client.Connection;
using Raven.WebConsole.ViewModels;

namespace Raven.WebConsole.Controllers
{
    public class UsersController : ContentController
    {
        private readonly IDocumentSession session;

        public UsersController(IDocumentSession session)
            :base(session)
        {
            this.session = session;
        }

        public ActionResult Index()
        {
            var users = session.Advanced.LoadStartingWith<AuthenticationUser>(Keys.Database.AUTH_USERS_PREFIX).ToList();

            return View(new UsersViewModel
                            {
                                Users = users.Select(u => new UsersViewModel.User(u)),
                            });
        }

        [HttpPost]
        public ActionResult Delete(string name)
        {
            var ravenUser = GetUser(name, false);

            if (ravenUser != null)
            {
                var ravenUserCount = session.Advanced.LoadStartingWith<AuthenticationUser>(Keys.Database.AUTH_USERS_PREFIX, pageSize: 2).Count();
                
                if (ravenUserCount == 1)
                    SetMessage("Cannot delete the only user", MessageLevel.Note);
                else
                {
                    session.Delete(ravenUser);
                    SetMessage(string.Format("Deleted user '{0}'", name));
                }
            }
            else 
                SetMessage(string.Format("Could not find user '{0}'", name), MessageLevel.Note);

            return RedirectToAction("Index");
        }

        public JQueryValidateRemoteResult ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new JQueryValidateRemoteResult("*");

            var existingUser = GetUser(name, false);

            if (existingUser != null)
                return new JQueryValidateRemoteResult("Already exists");

            return new JQueryValidateRemoteResult();
        }

        public class NewUserModel
        {
            public string Name { get; set; }
            public bool IsAdmin { get; set; }
            public string Password { get; set; }

            public const int MIN_PASSWORD_LEN = 5;
        }

        public ActionResult New(NewUserModel model)
        {
            var validationResult = ValidateName(model.Name);

            if (!validationResult.IsValid)
            {
                SetMessage(string.Format("Failed to create the new user: {0}", validationResult.ErrorMessage), MessageLevel.Note);
                return RedirectToAction("Index");
            }

            model.IsAdmin = Request.Form["isAdmin"] != null;

            session.Store(new AuthenticationUser
            {
                Id = Keys.Database.AUTH_USERS_PREFIX + model.Name,
                Admin = model.IsAdmin,
                AllowedDatabases = new[] { "*" },
                Name = model.Name,
            }.SetPassword(model.Password));

            SetMessage(string.Format("Created user '{0}'", model.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SetPassword(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("password should not be empty");

            if (password.Length < 5)
                throw new ArgumentException("password should be 5 or more characters");

            var user = GetUser(name);

            user.SetPassword(password);

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult SetAdmin(string name, bool isAdmin)
        {
            var user = GetUser(name);

            user.Admin = isAdmin;

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult SetDatabasePermissions(FormCollection form)
        {
            var userName = form["name"];
            
            if (userName == null) throw new ArgumentNullException("userName");

            var perms = GetPermsFromForm(form);

            var user = GetUser(userName);
            perms.SetInAuthenticationUser(user);

            SetMessage(string.Format("Database permissions saved for user {0}", userName));
            return RedirectToAction("Index");
        }

        private static List<DatabasePermissionsViewModel> GetPermsFromForm(FormCollection form)
        {
            var perms = new List<DatabasePermissionsViewModel>();
            var i = 0;
            string dbName;
            do
            {
                var basePrefix = string.Format("perms[{0}].", i);
                dbName = form[basePrefix + "Name"];
                if (dbName != null)
                {
                    perms.Add(new DatabasePermissionsViewModel
                                  {
                                      Name = dbName,
                                      IsAdmin = form[basePrefix + "IsAdmin"] != null,
                                      IsReadOnly = form[basePrefix + "IsReadonly"] != null,
                                  });
                }
                
                ++i;
            } while (dbName != null);

            return perms;
        }

        [HttpPost]
        public ActionResult SetDatabasePermissionsToAll([Bind(Prefix = "name")]string userName)
        {
            var user = GetUser(userName);
            user.Databases = new UserDatabaseAccess[0];
            user.AllowedDatabases = new[] { "*" };

            SetMessage(string.Format("Database permissions saved for user {0}", userName));
            return RedirectToAction("Index");
        }
    }
}