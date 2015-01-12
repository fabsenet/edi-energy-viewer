using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Fabsenet.EdiEnergy.Controllers
{
    public class EdiDocumentsController : RavenDbBaseApiController
    {
        public IQueryable<EdiDocument> GetAllEdiDocuments()
        {
            using (var session = _documentStore.OpenSession())
            {
                var ediDocs = session.Query<EdiDocument>().ToList().AsQueryable();
                return ediDocs;
            }
        }

        public IHttpActionResult GetEdiDocument(int id)
        {
            using (var session = _documentStore.OpenSession())
            {
                var ediDocs = session.Query<EdiDocument>().ToList().AsQueryable().First();
                return Ok(ediDocs);
            }
        }
    }
}
