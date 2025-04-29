using Fabsenet.EdiEnergyViewer.Models;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Fabsenet.EdiEnergyViewer.Controllers;

[Route("api/[controller]")]
[ApiController]
[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
public class EdiXmlDocumentController(IDocumentStore store, ILogger<EdiDocumentsController> log) : ControllerBase
{

    [HttpGet("{id}")]
    public async Task<ActionResult<EdiDocument>> GetXmlDocumentContent(string id)
    {
        try
        {
            id = "EdiXmlDocument/" + id;

            using var session = store.OpenSession();
            //return the metadata document only
            var xmlDoc = session.Load<EdiXmlDocument>(id);
            if (xmlDoc == null) return NotFound();

            var attachment = session.Advanced.Attachments.Get(id, "xml");

            if (attachment?.Stream == null) return NotFound();

            var ms = new MemoryStream();
            await attachment.Stream.CopyToAsync(ms);
            ms.Position = 0;

            if (ms.Length == 0)
            {
                return NotFound();
            }

            return File(ms, "application/xml", xmlDoc.AttachmentFilename);
        }
        catch (Exception ex)
        {
            log.LogCritical(ex, "GetXmlDocumentContent failed for document id: {DocumentId}", id);
            return BadRequest(ex);
        }
    }
}
