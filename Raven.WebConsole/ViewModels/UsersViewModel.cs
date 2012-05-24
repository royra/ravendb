using System.Collections.Generic;
using System.Linq;

namespace Raven.WebConsole.ViewModels
{
    public class UsersViewModel
    {
        public IEnumerable<User> Users { get; set; }

        public class User
        {
            public string Name;
            public bool IsAdmin;
            public IEnumerable<string> AllowedDatabases;

            public User(string name, bool isAdmin, IEnumerable<string> allowedDatabases)
            {
                Name = name;
                IsAdmin = isAdmin;
                AllowedDatabases = allowedDatabases;
                if (AllowedDatabases.Contains("*"))
                    AllowedDatabases = new[] {"(all)"};
            }
        }
    }
}