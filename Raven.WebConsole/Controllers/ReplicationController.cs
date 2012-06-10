using Raven.Client;

namespace Raven.WebConsole.Controllers
{
    public class ReplicationController : ContentController
    {
        public ReplicationController(IDocumentSession session) : base(session)
        {
        }
    }
}