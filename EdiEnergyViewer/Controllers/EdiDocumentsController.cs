using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Fabsenet.EdiEnergy.Util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Polly;

namespace Fabsenet.EdiEnergy.Controllers
{
    public class EdiDocumentsController : RavenDbBaseApiController
    {
        public IEnumerable<EdiDocumentSlim> GetAllEdiDocuments()
        {
            return RetryPolicy.Execute(() =>
            {
                using (var session = DocumentStore.OpenSession())
                {
                    var ediDocs = session.Query<EdiDocument>().TransformWith<EdiDocumentsSlimTransformer, EdiDocumentSlim>()
                        .Take(1024)
                        .ToList() //force db query
                        .OrderBy(d => d.ContainedMessageTypes == null ? d.DocumentName : d.ContainedMessageTypes[0])
                        .ThenByDescending(d => d.DocumentDate);

                    return ediDocs;
                }
            });
        }

        public async Task<IHttpActionResult> GetEdiDocumentPart(string id, int checkIdentifier)
        {
            return await RetryPolicy.Execute<Task<IHttpActionResult>>(async () =>
            {
                id = "EdiDocuments/" + id;

                using (var session = DocumentStore.OpenAsyncSession())
                {
                    var doc = await session.LoadAsync<EdiDocument>(id);
                    if (doc == null) return NotFound();


                    var fullPdf = await session.Advanced.GetAttachmentAsync(doc, "pdf");
                    if (fullPdf == null || fullPdf.Stream == null) throw new Exception("The fullPdf stream is null.");

                    List<int> pages;
                    if (doc.CheckIdentifier == null || !doc.CheckIdentifier.TryGetValue(checkIdentifier, out pages))
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

                    using (var reader = new PdfReader(fullPdf.Stream))
                    {
                        if (consecutivePages.Min() < 0)
                        {
                            return BadRequest($"Error! The starting page {consecutivePages.Min()} is less than 0.");
                        }
                        if (consecutivePages.Max() > reader.NumberOfPages)
                        {
                            return BadRequest($"Error! The end page {consecutivePages.Max()} is behind the last page {reader.NumberOfPages}.");
                        }

                        using (var memoryStream = new MemoryStream())
                        using (Document strippedDocument = new Document())
                        using (PdfWriter w = PdfWriter.GetInstance(strippedDocument, memoryStream))
                        {
                            strippedDocument.Open();
                            foreach (var page in consecutivePages)
                            {
                                strippedDocument.SetPageSize(reader.GetPageSize(page));
                                strippedDocument.NewPage();
                                w.DirectContent.AddTemplate(w.GetImportedPage(reader, page), 0, 0);
                            }
                            strippedDocument.Close();

                            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(memoryStream.ToArray()) };
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                            return ResponseMessage(response);
                        }
                    }
                }
            });
        }

        public async Task<IHttpActionResult> GetEdiDocument(string id)
        {
            try
            {
                return await RetryPolicy.Execute<Task<IHttpActionResult>>(async () =>
                {
                    id = "EdiDocuments/" + id;

                    using (var session = DocumentStore.OpenSession())
                    {
                        if (id.EndsWith(".pdf") || id.EndsWith(".zip"))
                        {
                            id = id.Substring(0, id.Length - ".pdf".Length);
                            var ediDocument = session.Load<EdiDocument>(id);

                            //return the actual pdf document
                            var attachment = session.Advanced.GetAttachment(ediDocument, "pdf");
                            if (attachment == null || attachment.Stream == null) return NotFound();

                            var ms = new MemoryStream();
                            await attachment.Stream.CopyToAsync(ms);
                            ms.Position = 0;

                            if (ms.Length == 0)
                            {
                                return NotFound();
                            }

                            var result = new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StreamContent(ms)
                            };

                            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                            return ResponseMessage(result);
                        }
                        else
                        {
                            //return the metadata document only
                            var ediDocument = session.Load<EdiDocument>(id);
                            return Ok(ediDocument);
                        }
                        
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }
    }
}
