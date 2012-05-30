using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Bundles.Authentication;

namespace Raven.WebConsole.ViewModels
{
    public class DatabaseAccessViewModel
    {
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public static class DatabaseAccessViewModelHelpers
    {
        public static void SetInAuthenticationUser(this IEnumerable<DatabaseAccessViewModel> accesses, AuthenticationUser user)
        {
            if (user == null) throw new ArgumentNullException("user");

            user.AllowedDatabases = new string[0];
            user.Databases = accesses
                .Select(a => new UserDatabaseAccess {Name = a.Name, Admin = a.IsAdmin, ReadOnly = a.IsReadOnly})
                .ToArray();
        }

        public static IList<DatabaseAccessViewModel> ToViewModel(this AuthenticationUser user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var first = user.AllowedDatabases != null
                            ? user.AllowedDatabases
                                .Where(n => n != "*")
                                .Select(n => new DatabaseAccessViewModel {Name = n})
                            : Enumerable.Empty<DatabaseAccessViewModel>();

            var second = user.Databases != null
                ? user.Databases.Select(d => new DatabaseAccessViewModel {Name = d.Name, IsAdmin = d.Admin, IsReadOnly = d.ReadOnly})
                : Enumerable.Empty<DatabaseAccessViewModel>();
            
            return first.Concat(second).ToList();
        }
    }
}