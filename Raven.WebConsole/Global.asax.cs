using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Raven.Bundles.Authentication;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.WebConsole.Utils;
using Raven.WebConsole.ViewModels;

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
            SetupAutofac();

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private static void SetupAutofac()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacWebTypesModule());
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.Register<IDocumentStore>(cb =>
                                 {
                                     var store = CreateDocumentStore();
                                     CreateDefaultUser(store);
                                     return store;
                                 }).SingleInstance();

            builder.Register(cb => cb.Resolve<IDocumentStore>().OpenSession()).InstancePerLifetimeScope();
            builder.RegisterType(typeof (MyWebClient)).AsImplementedInterfaces().InstancePerLifetimeScope();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private static void CreateDefaultUser(IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                if (!session.Advanced.LoadStartingWith<AuthenticationUser>(Keys.Database.AUTH_USERS_PREFIX).Any())
                {
                    session.Store(new AuthenticationUser
                                      {
                                          Id = Keys.Database.AUTH_USERS_PREFIX + "admin",
                                          Admin = true,
                                          AllowedDatabases = new []{"*"},
                                          Name = "Admin"
                                      }.SetPassword("12345"));
                }
                session.SaveChanges();
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