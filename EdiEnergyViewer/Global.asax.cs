using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using System.Configuration;

namespace Fabsenet.EdiEnergy
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.EnsureInitialized();


            IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), new DocumentStore()
            {
                Urls = new[] { ConfigurationManager.AppSettings["RavenDBUrl"] },
                Database = ConfigurationManager.AppSettings["RavenDBDatabase"]
            }.Initialize());

        }
    }
}
