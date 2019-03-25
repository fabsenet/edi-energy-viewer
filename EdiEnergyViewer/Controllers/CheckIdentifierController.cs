using System.Collections.Generic;
using System.Linq;
using Fabsenet.EdiEnergy.Util;
using Raven.Client.Documents;

namespace Fabsenet.EdiEnergy.Controllers
{
    public class CheckIdentifierController : RavenDbBaseApiController
    {
        public List<CheckIdentifier> GetAll()
        {
            using (var session = DocumentStore.OpenSession())
            {
                var query = session.Query<EdiDocument, EdiDocuments_CheckIdentifiers>()
                    .Customize(c => c.WaitForNonStaleResults())
                    .ProjectInto<CheckIdentifier>();

                var result = query.Take(1024).ToList();
                return result;

            }
        }
    }
}