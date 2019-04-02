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
        public IActionResult GetAll()
        {
            using (var session = _store.OpenSession())
            {
                var query = session.Query<EdiDocument>()
                    .Where(doc => doc.CheckIdentifier != null)
                    .Select(doc => new {EdiDocId=doc.Id, doc.CheckIdentifier})
                    .ToList() //execute query here!
                    .Select(doc => new
                        {
                            doc.EdiDocId,
                            SizeOfLargestPageBlockByCheckIdentifier = doc.CheckIdentifier.ToDictionary(kvp => kvp.Key,
                                    kvp => kvp.Value.InverseSelectMany((lastPage, currentPage) => lastPage + 1 == currentPage)
                                .Select(ps => ps.Count())
                                .Max())
                    }

                    )
                    .SelectMany(g => g.SizeOfLargestPageBlockByCheckIdentifier.Select(b => new {g.EdiDocId, CheckIdentifier=b.Key, LargestPageBlock = b.Value}))
                    .GroupBy(g => g.CheckIdentifier)
                    .ToDictionary(g => g.Key, g => g.ToDictionary(g2 => g2.EdiDocId, g2 => g2.LargestPageBlock));

                return Ok(query);

            }
        }
    }
}