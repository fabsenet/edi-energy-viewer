using Fabsenet.EdiEnergyViewer.Models;
using Fabsenet.EdiEnergyViewer.Util;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Fabsenet.EdiEnergyViewer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CheckIdentifierController(IDocumentStore store) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        using var session = store.OpenSession();
        var query = session.Query<EdiDocument>()
            .Where(doc => doc.CheckIdentifier != null)
            .Select(doc => new { EdiDocId = doc.Id, doc.CheckIdentifier })
            .ToList() //execute query here!
            .Select(doc => new
            {
                doc.EdiDocId,
                SizeOfLargestPageBlockByCheckIdentifier = doc.CheckIdentifier.ToDictionary(kvp => kvp.Key,
                            kvp => kvp.Value.InverseSelectMany((lastPage, currentPage) => 2 >= currentPage - lastPage)
                        .Select(ps =>
                        {
                            var l = ps.ToList();
                            return l.Max() - l.Min() + 1;
                        })
                        .Max())
            }

            )
            .SelectMany(g => g.SizeOfLargestPageBlockByCheckIdentifier.Select(b => new { g.EdiDocId, CheckIdentifier = b.Key, LargestPageBlock = b.Value }))
            .GroupBy(g => g.CheckIdentifier)
            .ToDictionary(g => g.Key, g => g.ToDictionary(g2 => g2.EdiDocId, g2 => g2.LargestPageBlock));

        return Ok(query);
    }
}