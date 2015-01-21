using System.Collections.Generic;
using System.Linq;
using Raven.Client.Document;

namespace Fabsenet.EdiEnergy.Controllers
{
    public class CheckIdentifierController : RavenDbBaseApiController
    {
        public IList<CheckIdentifier> GetAll()
        {
            using (var session = _documentStore.OpenSession())
            {
                var query = session.Query<CheckIdentifier>();

                var result = query.Take(1024).ToList();

                return result;

            }
        }
    }
}