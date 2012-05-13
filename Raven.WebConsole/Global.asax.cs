using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.WebConsole.Entities;

namespace Raven.WebConsole
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Login", // Route name
                "Login", // URL with parameters
                new { controller = "Home", action = "Login" }
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            var store = CreateDocumentStore();
            SetupAutofac(store);

            using (var session = store.OpenSession())
            {
                SetupDefaultUser(session);
                session.SaveChanges();
            }

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private static void SetupAutofac(DocumentStore store)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacWebTypesModule());
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.Register(cb => store.OpenSession()).InstancePerLifetimeScope();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private void SetupDefaultUser(IDocumentSession session)
        {
            if (session.Query<User>().FirstOrDefault() == null)
            {
                session.Store(new User {Name = "admin", Password = new Password("12345")});
            }
        }

        private static DocumentStore CreateDocumentStore()
        {
            var store = new DocumentStore {ConnectionStringName = "RavenDB"};
            store.Initialize();
            IndexCreation.CreateIndexes(Assembly.GetCallingAssembly(), store);
            return store;
        }
    }
}