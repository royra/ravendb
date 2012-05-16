using System.Collections.Generic;

namespace Raven.WebConsole.ViewModels
{
    public class UsersViewModel
    {
        public IEnumerable<User> Users { get; set; }

        public class User
        {
            public string Name;
            public bool IsAdmin;

            public User(string name, bool isAdmin)
            {
                Name = name;
                IsAdmin = isAdmin;
            }
        }
    }
}