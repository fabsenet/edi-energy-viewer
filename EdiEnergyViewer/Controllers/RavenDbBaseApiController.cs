using System.Web.Http;
using System.Web.Mvc;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.FileSystem;

namespace Fabsenet.EdiEnergy.Controllers
{
    public abstract class RavenDbBaseApiController : ApiController
    {
        protected static readonly IDocumentStore DocumentStore = new DocumentStore() { ConnectionStringName = "RavenDB" }.Initialize();
        protected static readonly IFilesStore FilesStore = new FilesStore() { ConnectionStringName = "RavenFS" }.Initialize();

        //protected IDocumentSession RavenSession;

        //override 
        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    RavenSession = _documentStore.OpenSession();
        //    base.OnActionExecuting(filterContext);
        //}

        //protected override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    RavenSession.SaveChanges();
        //    RavenSession.Dispose();

        //    base.OnActionExecuted(filterContext);
        //}

    }
}