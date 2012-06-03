using Raven.Client;

namespace Raven.WebConsole.Controllers
{
    public class BundlesController : ContentController
    {
        public BundlesController(IDocumentSession session) : base(session)
        {
        }
    }
}