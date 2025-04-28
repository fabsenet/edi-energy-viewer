using System.Text.RegularExpressions;
using Fabsenet.EdiEnergyViewer.Models;
using Fabsenet.EdiEnergyViewer.Util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Fabsenet.EdiEnergyViewer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EdiDocumentsController(IDocumentStore store, ILogger<EdiDocumentsController> log) : ControllerBase
{
    private readonly ILogger<EdiDocumentsController> _log = log ?? throw new ArgumentNullException(nameof(log));

    [HttpGet]
    public IEnumerable<EdiDocumentSlim> GetAllEdiDocuments()
    {
        using var session = store.OpenSession();

        var xmlDocuments = session.Query<EdiXmlDocument>().ToList();

        var statsByCheckidentifierAndDocumentId = session.Query<EdiDocument>()
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

        var validFromBoundary = DateTime.Now < new DateTime(2025, 6, 7) ? new DateTime(2025, 9, 30) : DateTime.MaxValue;
        var ediDocs = session.Query<EdiDocument>()
            .Take(1024)
            .Where(d => d.ValidFrom <= validFromBoundary)
            //.Where(d => d.MessageTypeVersion == "G1.0a" || !d.IsMig || d.ContainedMessageTypes == null || !d.ContainedMessageTypes.Contains("UTILMD"))
            .ToList() //force db query
            .Select(ediDoc =>
            {
                var xmlDocs = new List<XmlDocumentSlim>();
                if (ediDoc.ContainedMessageTypes != null)
                {
                    //this matches "außerordentliche veröffentlichen stand 12.04.2024" and so on
                    xmlDocs.AddRange(
                        xmlDocuments
                               .Where(x => ediDoc.ContainedMessageTypes.Contains(x.MessageType)
                                               && ediDoc.IsMig == x.IsMIG
                                               && ediDoc.IsAhb == x.IsAHB
                                               && ediDoc.MessageTypeVersion == x.MessageVersion
                                               && x.OriginalTitle.Contains(ediDoc.DocumentDate.ToString("dd.MM.yyyy"))
                                               )
                               .Select(x => new XmlDocumentSlim { Name = x.CleanedTitle, DocumentUri = x.Id })
                               );

                    if (xmlDocs.Count == 0)
                    {
                        //this case matches the first release of a message version
                        xmlDocs.AddRange(
                            xmlDocuments
                                   .Where(x => ediDoc.ContainedMessageTypes.Contains(x.MessageType)
                                                   && ediDoc.IsMig == x.IsMIG
                                                   && ediDoc.IsAhb == x.IsAHB
                                                   && ediDoc.MessageTypeVersion == x.MessageVersion
                                                   && !Regex.IsMatch(x.OriginalTitle, @"\b\d{2}\.\d{2}\.\d{4}\b") //=no date in the title
                                                   )
                                   .Select(x => new XmlDocumentSlim { Name = x.CleanedTitle, DocumentUri = x.Id })
                                   );
                    }
                }

                return new
                {
                    EdiDocumentSlim = new EdiDocumentSlim()
                    {
                        BdewProcess = ediDoc.BdewProcess,
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
                        IsStrom = ediDoc.IsStrom,
                        IsGas = ediDoc.IsGas,
                        IsStromUndOderGas = ediDoc.IsStromUndOderGas,
                        Filename = ediDoc.Filename,
                        IsMig = ediDoc.IsMig,
                        MessageTypeVersion = ediDoc.MessageTypeVersion,
                        ValidFrom = ediDoc.ValidFrom,
                        ValidTo = ediDoc.ValidTo,
                        IsHot = false,
                        XmlDocuments = xmlDocs,
                    },
                    CheckIdentifier = ediDoc.CheckIdentifier == null ? null : ediDoc.CheckIdentifier.Select(kvp => kvp.Key).OrderBy(id => id).ToList(),
                };
            })
            .Select(a =>
            {
                if (a.CheckIdentifier != null)
                {
                    a.EdiDocumentSlim.CheckIdentifiersWithStats =
                    [
                        .. a.CheckIdentifier
                        .Select(ci =>
                        {
                            if (statsByCheckidentifierAndDocumentId.TryGetValue(ci, out var stats))
                            {
                                if (stats.TryGetValue(a.EdiDocumentSlim.Id, out var largestPageBlock))
                                {
                                    var ciws = new CheckidentiferWithStats
                                    {
                                        CheckIdentifier = ci,
                                        LargestPageBlock = largestPageBlock
                                    };
                                    return ciws;
                                }
                            }
                            throw new Exception("Something is wrong with the statsByCheckidentifierAndDocumentId dictionary!");
                        })
                        .OrderBy(c => c.CheckIdentifier)
                    ];
                }
                return a.EdiDocumentSlim;
            })
            .OrderBy(d => d.ContainedMessageTypes == null ? d.DocumentName : d.ContainedMessageTypes[0])
            .ThenByDescending(d => d.DocumentDate)
            .ToList();

        if (ediDocs.Count > 0)
        {
            var latestDocumentDate = ediDocs.Max(e => e.DocumentDate);
            var boundaryDate = latestDocumentDate.AddDays(-20);
            foreach (var ediDoc in ediDocs.Where(e => e.DocumentDate >= boundaryDate))
            {
                ediDoc.IsHot = true;
            }
        }

        return ediDocs;
    }

    [HttpGet("ClearCache")]
    public async Task<ActionResult<string>> ClearCache()
    {
        var count = 0;
        using (var session = store.OpenAsyncSession())
        {
            var ediDocs = await session.Query<EdiDocument>().ToListAsync();
            foreach (var ediDoc in ediDocs)
            {
                var attachments = session.Advanced.Attachments.GetNames(ediDoc)
                    .Where(n => n.Name.StartsWith("pdf-"))
                    .ToList();


                foreach (var attachment in attachments)
                {
                    session.Advanced.Attachments.Delete(ediDoc, attachment.Name);
                    count++;
                }
            }
            await session.SaveChangesAsync();
        }
        return Ok($"removed {count} attachments from cache!");
    }

    [HttpGet("{id}/part/{checkIdentifier}")]
    public async Task<IActionResult> GetEdiDocumentPart(string id, int checkIdentifier)
    {
        id = "EdiDocuments/" + id;
        var cachedPartName = $"pdf-{checkIdentifier}";

        using (var session = store.OpenAsyncSession())
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
                .InverseSelectMany((lastPage, currentPage) => 2 >= currentPage - lastPage)
                .Select(ps => ps.ToList())
                .OrderByDescending(ps => ps.Count())
                .FirstOrDefault();

            if (consecutivePages == null || consecutivePages.Count == 0)
            {
                return BadRequest("unknown error");
            }

            var reader = new PdfReader(fullPdf.Stream);
            int firstPage = consecutivePages.Min();
            if (firstPage < 0)
            {
                return BadRequest($"Error! The starting page {firstPage} is less than 0.");
            }

            int lastPage = consecutivePages.Max();
            if (lastPage > reader.NumberOfPages)
            {
                return base.BadRequest($"Error! The end page {lastPage} is behind the last page {reader.NumberOfPages}.");
            }

            var memoryStream = new MemoryStream();
            Document strippedDocument = new Document();
            PdfWriter w = PdfWriter.GetInstance(strippedDocument, memoryStream);
            strippedDocument.Open();
            for (int pageNum = firstPage; pageNum <= lastPage; pageNum++)
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

            using var session = store.OpenSession();
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
        catch (Exception ex)
        {
            _log.LogCritical(ex, "GetEdiDocumentFull failed for document id: {DocumentId}", id);
            return BadRequest(ex);
        }
    }

    [HttpGet("{id}")]
    public ActionResult<EdiDocument> GetEdiDocument(string id)
    {
        try
        {
            id = "EdiDocuments/" + id;

            using var session = store.OpenSession();
            //return the metadata document only
            var ediDocument = session.Load<EdiDocument>(id);
            if (ediDocument == null) return NotFound();

            return Ok(ediDocument);
        }
        catch (Exception ex)
        {
            _log.LogCritical(ex, "GetEdiDocument failed for document id: {DocumentId}", id);
            return BadRequest(ex);
        }
    }
}
