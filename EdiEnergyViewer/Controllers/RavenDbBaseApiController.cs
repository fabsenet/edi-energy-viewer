using System;
using System.Web.Http;
using NLog;
using Polly;
using Raven.Client;
using Raven.Client.Documents;
using System.Configuration;

namespace Fabsenet.EdiEnergy.Controllers
{
    public abstract class RavenDbBaseApiController : ApiController
    {
        protected static readonly IDocumentStore DocumentStore = new DocumentStore()
        {
            Urls = new[] { ConfigurationManager.AppSettings["RavenDBUrl"] },
            Database = ConfigurationManager.AppSettings["RavenDBDatabase"]
        }.Initialize();

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
    }
}