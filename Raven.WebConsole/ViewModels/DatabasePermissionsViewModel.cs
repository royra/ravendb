using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Bundles.Authentication;

namespace Raven.WebConsole.ViewModels
{
    public class DatabasePermissionsViewModel
    {
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public static class DatabasePermissionsViewModelHelpers
    {
        public static void SetInAuthenticationUser(this IEnumerable<DatabasePermissionsViewModel> perms, AuthenticationUser user)
        {
            if (user == null) throw new ArgumentNullException("user");

            user.AllowedDatabases = new string[0];
            user.Databases = perms
                .Select(a => new UserDatabaseAccess {Name = a.Name, Admin = a.IsAdmin, ReadOnly = a.IsReadOnly})
                .ToArray();
        }

        public static IList<DatabasePermissionsViewModel> ToViewModel(this AuthenticationUser user)
        {
            if (user == null) throw new ArgumentNullException("user");

            var first = user.AllowedDatabases != null
                            ? user.AllowedDatabases
                                .Where(n => n != "*")
                                .Select(n => new DatabasePermissionsViewModel {Name = n})
                            : Enumerable.Empty<DatabasePermissionsViewModel>();

            var second = user.Databases != null
                ? user.Databases.Select(d => new DatabasePermissionsViewModel {Name = d.Name, IsAdmin = d.Admin, IsReadOnly = d.ReadOnly})
                : Enumerable.Empty<DatabasePermissionsViewModel>();
            
            return first.Concat(second).ToList();
        }
    }
}