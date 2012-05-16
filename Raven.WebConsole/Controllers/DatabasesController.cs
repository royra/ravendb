using System;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Connection;
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
            var databaseCommands = store.DatabaseCommands;

            var baseUrl = store.Url;

            var databases = databaseCommands.GetDatabaseNames(1000)
                .Select(delegate(string dbName)
                            {
                                var dbUrl = string.Format("{0}/databases/{1}", baseUrl, dbName);
                                var dbSizeBytes = (long)
                                    webClient.GetDynamicJson(string.Format("{0}/database/size", dbUrl)).DatabaseSize;

                                var backupStatus = webClient.GetJson<BackupStatus>(string.Format("{0}/docs/Raven/Backup/Status", dbUrl));

                                return new DatabasesViewModel.Database(
                                    dbName,
                                    DecimalExtensions.GetTruncatedMbytes(dbSizeBytes),
                                    new[] {"Replication", "Versioning"},
                                    backupStatus != null ? (DateTime?)backupStatus.Started : null);
                            });

            return View(new DatabasesViewModel
                            {
                                BaseUrl = baseUrl,
                                Databases = databases.Concat(new[]
                                                {
                                                    new DatabasesViewModel.Database("thisandthat", (decimal)101.2,
                                                                                new[] {"Replication", "Versioning"},
                                                                                new DateTime(2011, 2, 1)),
                                                    new DatabasesViewModel.Database("joes", (decimal)11.22,
                                                                                new[] {"Encryption", "Versioning"},
                                                                                DateTime.Now),
                                                })
                            });
        }

        public ActionResult New()
        {
            return View();
        }
    }
}