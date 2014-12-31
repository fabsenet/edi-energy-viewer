using System.Linq;
using System.Web.Mvc;

namespace Fabsenet.EdiEnergy.Controllers
{
    public class HomeController : RavenDbBaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Documents()
        {
            var ediDocs = RavenSession.Query<EdiDocument>().ToList();

            return new JsonResult(){Data=ediDocs,JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}