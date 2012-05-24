using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Extensions;
using Raven.WebConsole.Utils;
using Raven.WebConsole.ViewModels;

namespace Raven.WebConsole.Controllers
{
    public class DatabasesController : ContentController
    {
        private readonly IDocumentStore store;
        private readonly IWebClient webClient;
        private readonly IDocumentSession session;

        public DatabasesController(IDocumentStore store, IWebClient webClient, IDocumentSession session)
        {
            this.store = store;
            this.webClient = webClient;
            this.session = session;
        }

        public override ActionResult Index()
        {
            var baseUrl = store.Url;

            var databases = DatabaseCommands.GetDatabaseNames(1000)
                .OrderBy(n => n)
                .Select(delegate(string dbName)
                            {
                                var dbUrl = string.Format("{0}/databases/{1}", baseUrl, dbName);
                                var dbSizeBytes = (long)
                                    webClient.GetDynamicJson(string.Format("{0}/database/size", dbUrl)).DatabaseSize;

                                BackupStatus backupStatus;
                                try
                                {
                                    backupStatus = webClient.GetJson<BackupStatus>(string.Format("{0}/docs/Raven/Backup/Status", dbUrl));    
                                }
                                catch(WebException we)
                                {
                                    var r = we.Response as HttpWebResponse;
                                    if (r != null && r.StatusCode == HttpStatusCode.NotFound)
                                        backupStatus = null;
                                    else
                                        throw;
                                }
                                
                                return new DatabasesViewModel.Database(
                                    dbName,
                                    dbSizeBytes.GetTruncatedMbytes(),
                                    new[] {"Replication", "Versioning"},
                                    backupStatus != null ? (DateTime?)backupStatus.Started : null);
                            });

            return View(new DatabasesViewModel
                            {
                                BaseUrl = baseUrl,
                                Databases = databases,
                            });
        }

        public ActionResult New(string name)
        {
            if (!ValidateNameJson(name).Equals(true))
                return RedirectToAction("Index");

            DatabaseCommands.EnsureDatabaseExists(name);

            return RedirectToAction("Index");
        }

        public ActionResult ValidateName(string name)
        {
            return new JsonResult {Data = ValidateNameJson(name), JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpPost]
        public ActionResult Delete(string name)
        {
            DatabaseCommands.Delete(string.Format("Raven/Databases/{0}", name), null);
            return RedirectToAction("Index");
        }

        private IDatabaseCommands DatabaseCommands
        {
            get { return store.DatabaseCommands; }
        }

        private object ValidateNameJson(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "*";

            var existingNames = DatabaseCommands.GetDatabaseNames(1000);

            if (existingNames.Any(n => name.Equals(n, StringComparison.InvariantCultureIgnoreCase)))
                return "Already exists";

            return true;
        }
    }
}