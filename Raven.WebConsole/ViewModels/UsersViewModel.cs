using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Bundles.Authentication;

namespace Raven.WebConsole.ViewModels
{
    public class UsersViewModel
    {
        public IEnumerable<User> Users { get; set; }

        public class User
        {
            public string Name;
            public bool IsAdmin;
            public bool AccessToAllDatabases;
            public IEnumerable<DatabaseAccessViewModel> Databases;

            public User()
            {
            }

            public User(AuthenticationUser user)
            {
                if (user == null) throw new ArgumentNullException("user");

                Name = user.Name;
                IsAdmin = user.Admin;
                AccessToAllDatabases = user.AllowedDatabases.Contains("*");
                Databases = user.ToViewModel();
            }
        }
    }
}