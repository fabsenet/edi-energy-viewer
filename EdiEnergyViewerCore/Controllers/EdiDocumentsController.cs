using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fabsenet.EdiEnergy.Util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;

namespace Fabsenet.EdiEnergy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EdiDocumentsController : ControllerBase
    {
        private readonly IDocumentStore _store;
        private readonly ILogger<EdiDocumentsController> _log;

        public EdiDocumentsController(IDocumentStore store, ILogger<EdiDocumentsController> log)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [HttpGet]
        public IEnumerable<EdiDocumentSlim> GetAllEdiDocuments()
        {
            using (var session = _store.OpenSession())
            {
                var ediDocs = session.Query<EdiDocument>()
                    .Take(1024)
                    .Select(ediDoc => new EdiDocumentSlim()
                    {
                        BdewProcess = ediDoc.BdewProcess,
                        CheckIdentifier = ediDoc.CheckIdentifier == null ? null : ediDoc.CheckIdentifier.Select(kvp => kvp.Key).OrderBy(id => id).ToList(),
                        ContainedMessageTypes = ediDoc.ContainedMessageTypes,
                        DocumentDate = ediDoc.DocumentDate,
                        DocumentName = ediDoc.DocumentName,
                        DocumentNameRaw = ediDoc.DocumentNameRaw,
                        DocumentUri = ediDoc.DocumentUri,
                        MirrorUri = ediDoc.MirrorUri,
                        Id = ediDoc.Id,
                        IsAhb = ediDoc.IsAhb,
                        IsGeneralDocument = ediDoc.IsGeneralDocument,
                        IsLatestVersion = ediDoc.IsLatestVersion,
                        Filename = ediDoc.Filename,
                        IsMig = ediDoc.IsMig,
                        MessageTypeVersion = ediDoc.MessageTypeVersion,
                        ValidFrom = ediDoc.ValidFrom,
                        ValidTo = ediDoc.ValidTo,
                    })
                    .ToList() //force db query
                    .OrderBy(d => d.ContainedMessageTypes == null ? d.DocumentName : d.ContainedMessageTypes[0])
                    .ThenByDescending(d => d.DocumentDate);

                return ediDocs;
            }
        }

        [HttpGet("{id}/part/{checkIdentifier}")]
        public async Task<IActionResult> GetEdiDocumentPart(string id, int checkIdentifier)
        {
            id = "EdiDocuments/" + id;
            var cachedPartName = $"pdf-{checkIdentifier}";

            using (var session = _store.OpenAsyncSession())
            {
                var cachedPart = await session.Advanced.Attachments.GetAsync(id, cachedPartName);
                if (cachedPart != null)
                {
                   return File(cachedPart.Stream, "application/pdf");
                }

                var doc = await session.LoadAsync<EdiDocument>(id);
                if (doc == null) return NotFound();


                var fullPdf = await session.Advanced.Attachments.GetAsync(id, "pdf");
                if (fullPdf?.Stream == null) throw new Exception("The fullPdf stream is null.");

                if (doc.CheckIdentifier == null || !doc.CheckIdentifier.TryGetValue(checkIdentifier, out var pages))
                {
                    return BadRequest("The edi document does not contain the requested check identifier!");
                }

                List<int> consecutivePages = pages
                    .InverseSelectMany((lastPage, currentPage) => lastPage + 1 == currentPage)
                    .Select(ps => ps.ToList())
                    .OrderByDescending(ps => ps.Count())
                    .FirstOrDefault();

                if (consecutivePages == null || consecutivePages.Count == 0)
                {
                    return BadRequest("unknown error");
                }

                var reader = new PdfReader(fullPdf.Stream);
                if (consecutivePages.Min() < 0)
                {
                    return BadRequest($"Error! The starting page {consecutivePages.Min()} is less than 0.");
                }
                if (consecutivePages.Max() > reader.NumberOfPages)
                {
                    return BadRequest($"Error! The end page {consecutivePages.Max()} is behind the last page {reader.NumberOfPages}.");
                }

                var memoryStream = new MemoryStream();
                Document strippedDocument = new Document();
                PdfWriter w = PdfWriter.GetInstance(strippedDocument, memoryStream);
                strippedDocument.Open();
                foreach (var pageNum in consecutivePages)
                {
                    strippedDocument.SetPageSize(reader.GetPageSize(pageNum));
                    strippedDocument.NewPage();

                    w.DirectContent.AddTemplate(w.GetImportedPage(reader, pageNum), 0, 0);
                }

                w.CloseStream = false;
                strippedDocument.Close();

                memoryStream.Position = 0;
                session.Advanced.Attachments.Store(id, cachedPartName, memoryStream);
                await session.SaveChangesAsync();

                memoryStream.Position = 0;
                return File(memoryStream, "application/pdf");
            }
        }

        [HttpGet("{id}/full")]
        public async Task<IActionResult> GetEdiDocumentFull(string id)
        {
            try
            {
                id = "EdiDocuments/" + id;

                using (var session = _store.OpenSession())
                {
                    var ediDocument = session.Load<EdiDocument>(id);
                    if (ediDocument == null) return NotFound();

                    //return the actual pdf document
                    var attachment = session.Advanced.Attachments.Get(ediDocument, "pdf");
                    if (attachment?.Stream == null) return NotFound();

                    var ms = new MemoryStream();
                    await attachment.Stream.CopyToAsync(ms);
                    ms.Position = 0;

                    if (ms.Length == 0)
                    {
                        return NotFound();
                    }

                    return File(ms, "application/pdf");

                }
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, $"GetEdiDocumentFull({id}) failed.");
                return this.BadRequest(ex);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetEdiDocument(string id)
        {
            try
            {
                id = "EdiDocuments/" + id;

                using (var session = _store.OpenSession())
                {
                        //return the metadata document only
                        var ediDocument = session.Load<EdiDocument>(id);
                        return Ok(ediDocument);
                }
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, $"GetEdiDocument({id}) failed.");
                return BadRequest(ex);
            }
        }
    }
}
