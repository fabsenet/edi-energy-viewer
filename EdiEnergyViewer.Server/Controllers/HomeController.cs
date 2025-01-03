using Fabsenet.EdiEnergyViewer.Models;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Fabsenet.EdiEnergyViewer.Controllers;


public class HomeController(IDocumentStore store) : ControllerBase
{
    public List<EdiDocument> Documents()
    {
        List<EdiDocument> ediDocs;
        using (var session = store.OpenSession())
        {
            ediDocs = [.. session.Query<EdiDocument>()];
        }

        return ediDocs;
    }
}