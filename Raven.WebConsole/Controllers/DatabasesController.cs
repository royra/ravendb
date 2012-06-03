using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Newtonsoft.Json;
using Raven.Abstractions.Data;
using Raven.Bundles.Authentication;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Extensions;
using Raven.Database.Exceptions;
using Raven.WebConsole.Utils;
using Raven.WebConsole.ViewModels;

namespace Raven.WebConsole.Controllers
{
    public class DatabasesController : ContentController
    {
        private readonly IDocumentStore store;
        private readonly IWebClient webClient;
        private readonly IDocumentSession session;
        private int MAX_DATABASES = 1000;

        public DatabasesController(IDocumentStore store, IWebClient webClient, IDocumentSession session)
            : base(session)
        {
            this.store = store;
            this.webClient = webClient;
            this.session = session;
        }

        public override ActionResult Index()
        {
            var baseUrl = store.Url;

            var databases = DatabaseCommands.GetDatabaseNames(MAX_DATABASES)
                .OrderBy(n => n)
                .Select(delegate(string dbName)
                            {
                                var dbUrl = string.Format("{0}/databases/{1}", baseUrl, dbName);
                                var dbSizeBytes = (long)
                                    webClient.GetDynamicJson(string.Format("{0}/database/size", dbUrl)).DatabaseSize;

                                BackupStatus backupStatus;
                                using (var ses = store.OpenSession(dbName))
                                    backupStatus = ses.Load<BackupStatus>("Raven/Backup/Status");
                                
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

        private static readonly Regex filterStackTrace = new Regex(@"\n\s+at .*", RegexOptions.Multiline);

        public ActionResult New(string name)
        {
            if (!ValidateName(name).IsValid)
                return RedirectToAction("Index");

            string errorMessage = null;
            try
            {
                DatabaseCommands.EnsureDatabaseExists(name);
            }
            catch(Exception e)
            {
                errorMessage = filterStackTrace.Replace(e.Message, "");
            }

            if (errorMessage != null)
                SetMessage(string.Format("There was an error while creating the database '{0}': {1}", name, errorMessage), MessageLevel.Warning);
            else
                SetMessage(string.Format("Created database '{0}'", name));

            return RedirectToAction("Index");
        }

        public JQueryValidateRemoteResult ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new JQueryValidateRemoteResult("*");

            name = name.Trim();

            if ("default".Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return new JQueryValidateRemoteResult("The name 'default' is reserved for the system default database");

            var existingNames = DatabaseCommands.GetDatabaseNames(MAX_DATABASES);

            if (existingNames.Any(n => name.Equals(n, StringComparison.InvariantCultureIgnoreCase)))
                return new JQueryValidateRemoteResult("Already exists");

            return new JQueryValidateRemoteResult();
        }

        [HttpPost]
        public ActionResult Delete(string name)
        {
            DatabaseCommands.Delete(string.Format("Raven/Databases/{0}", name), null);
            SetMessage(string.Format("Database '{0}' was deleted", name));
            return RedirectToAction("Index");
        }

        private IDatabaseCommands DatabaseCommands
        {
            get { return store.DatabaseCommands; }
        }

        [HttpPost]
        public ActionResult Backup(string name, string path)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (path == null) throw new ArgumentNullException("path");

            var validation = BackupPathOk(path);
            if (!validation.IsValid)
                throw new ClientVisibleException {ClientVisibleMessage = validation.ErrorMessage};
            
            var url = string.Format("{0}/databases/{1}/admin/backup", store.Url, name);
            var json = new {BackupLocation = path};
            webClient.PostJson(url, json);

            return new EmptyResult();
        }

        public JQueryValidateRemoteResult BackupPathOk(string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            if (!Path.IsPathRooted(path))
                return new JQueryValidateRemoteResult("Enter an absolute path");

            return new JQueryValidateRemoteResult();
        }

        public JsonResult GetDatabases()
        {
            return new JsonResult
                       {
                           JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                           Data = DatabaseCommands.GetDatabaseNames(MAX_DATABASES)
                       };
        }
    }
}