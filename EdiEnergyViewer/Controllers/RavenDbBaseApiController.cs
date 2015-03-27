using System;
using System.Web.Http;
using NLog;
using Polly;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.FileSystem;

namespace Fabsenet.EdiEnergy.Controllers
{
    public abstract class RavenDbBaseApiController : ApiController
    {
        protected static readonly IDocumentStore DocumentStore = new DocumentStore() { ConnectionStringName = "RavenDB" }.Initialize();
        protected static readonly IFilesStore FilesStore = new FilesStore() { ConnectionStringName = "RavenFS" }.Initialize();

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        protected static readonly Policy RetryPolicy =
            Policy.Handle<Exception>().WaitAndRetry(
                new[]
                {
                    TimeSpan.FromMilliseconds(0),
                    TimeSpan.FromMilliseconds(0),
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(1000),
                    TimeSpan.FromMilliseconds(1000)
                },
                (ex, ts) => _log.Warn(ex)
                );

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