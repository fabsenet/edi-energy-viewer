using System.Web.Mvc;
using Raven.Client;
using Raven.Client.Document;

namespace Fabsenet.EdiEnergy.Controllers
{
    public abstract class RavenDbBaseController : Controller
    {
        private static readonly IDocumentStore _documentStore = new DocumentStore() { ConnectionStringName = "RavenDB" }.Initialize();

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