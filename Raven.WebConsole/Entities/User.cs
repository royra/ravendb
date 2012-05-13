using System.Collections.Generic;
using System.Web;

namespace Raven.WebConsole.Entities
{
    public class User : RootAggregate
    {
        public string Name { get; set; }
        public Password Password { get; set; }
    }
}