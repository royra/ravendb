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
using Raven.Database;
using Raven.Database.Config;
using Raven.Database.Exceptions;
using Raven.Json.Linq;
using Raven.WebConsole.Utils;
using Raven.WebConsole.ViewModels;

namespace Raven.WebConsole.Controllers
{
    public class DatabasesController : ContentController
    {
        private readonly IDocumentStore store;
        private readonly IDocumentSession session;
        private const int MAX_DATABASES = 1000;
        private const string DB_KEY_PREFIX = "Raven/Databases/";

        public DatabasesController(IDocumentStore store, IDocumentSession session)
            : base(session)
        {
            this.store = store;
            this.session = session;
        }

        public ActionResult Index(string query = "", int pageSize = 10, int start = 0)
        {
            var baseUrl = store.Url;

            var keyStartsWith = DB_KEY_PREFIX + query;
            var dbs = DatabaseCommands
                .StartsWith(keyStartsWith, start, pageSize + 1)
                .Select(x => new
                                 {
                                     Name = x.Metadata.Value<string>("@id").Replace(DB_KEY_PREFIX, ""),
                                     Data = x.DataAsJson
                                 })
                .OrderBy(d => d.Name)
                .Select(d =>
                            {
                                var dbName = d.Name;
                                var dbSizeBytes = DatabaseCommands.ForDatabase(dbName).GetSize();

                                BackupStatus backupStatus;
                                using (var ses = store.OpenSession(dbName))
                                    backupStatus = ses.Load<BackupStatus>("Raven/Backup/Status");
                                
                                return new DatabasesViewModel.Database(
                                    dbName,
                                    dbSizeBytes.GetTruncatedMbytes(),
                                    d.Data.Value<string[]>("Bundles") ?? Enumerable.Empty<string>(),
                                    backupStatus != null ? (DateTime?)backupStatus.Started : null);
                            })
                .ToArray();

            var model = new DatabasesViewModel
            {
                BaseUrl = baseUrl,
                Databases = dbs.Take(pageSize),
                More = dbs.Length > pageSize,
            };

            if ("json".Equals(Request.QueryString["output"], StringComparison.InvariantCultureIgnoreCase))
                return new JsonResult {Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
            
            return View(model);
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

            var existingName = session.Load<dynamic>(DB_KEY_PREFIX + name);

            if (existingName != null)
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

            var incremental = Request.Form["incremental"] != null;

            var validation = BackupPathOk(path);
            if (!validation.IsValid)
                throw new ClientVisibleException {ClientVisibleMessage = validation.ErrorMessage};

            DatabaseCommands.ForDatabase(name).StartBackup(path, incremental);

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
                           Data = DatabaseCommands.GetDatabaseNames(MAX_DATABASES, 0)
                       };
        }

        public ActionResult Restore(string name, string from)
        {
            // TODO: get the configuration from somewhere
            //DocumentDatabase.Restore(new RavenConfiguration(), from, DatabaseCommands.ForDatabase(name).GetStatistics());
            return new EmptyResult();
        }
    }
}