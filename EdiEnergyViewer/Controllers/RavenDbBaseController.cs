using System.Web.Mvc;
using Raven.Client;
using Raven.Client.Documents;
using System.Configuration;
using Raven.Client.Documents.Session;

namespace Fabsenet.EdiEnergy.Controllers
{
    public abstract class RavenDbBaseController : Controller
    {
        private static readonly IDocumentStore _documentStore = new DocumentStore()
        {
            Urls = new[] { ConfigurationManager.AppSettings["RavenDBUrl"] },
            Database = ConfigurationManager.AppSettings["RavenDBDatabase"]
        }.Initialize();

        protected IDocumentSession RavenSession;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            RavenSession = _documentStore.OpenSession();
            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            RavenSession.SaveChanges();
            RavenSession.Dispose();

            base.OnActionExecuted(filterContext);
        }
    }
}