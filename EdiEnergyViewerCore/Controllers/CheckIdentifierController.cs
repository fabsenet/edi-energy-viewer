using System;
using System.Collections.Generic;
using System.Linq;
using Fabsenet.EdiEnergy.Util;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Fabsenet.EdiEnergy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckIdentifierController : ControllerBase
    {
        private readonly IDocumentStore _store;

        public CheckIdentifierController(IDocumentStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }
        public List<CheckIdentifier> GetAll()
        {
            using (var session = _store.OpenSession())
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