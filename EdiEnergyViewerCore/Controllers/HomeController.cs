using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Fabsenet.EdiEnergy.Controllers
{

    public class HomeController : Controller
    {
        private readonly IDocumentStore _store;

        public HomeController(IDocumentStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Documents()
        {
                List<EdiDocument> ediDocs;
            using (var session = _store.OpenSession())
            {
                ediDocs = session.Query<EdiDocument>().ToList();
            }

            return Json(ediDocs);
        }
    }
}