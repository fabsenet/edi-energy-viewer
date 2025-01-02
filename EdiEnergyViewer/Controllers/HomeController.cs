using Fabsenet.EdiEnergyViewer.Models;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using System.Collections.Generic;

namespace Fabsenet.EdiEnergyViewer.Controllers;


public class HomeController(IDocumentStore store) : Controller
{

    public ActionResult Index()
    {
        return View();
    }

    public ActionResult Documents()
    {
        List<EdiDocument> ediDocs;
        using (var session = store.OpenSession())
        {
            ediDocs = [.. session.Query<EdiDocument>()];
        }

        return Json(ediDocs);
    }
}