using System.Web.Mvc;

namespace ComunidadPractica.Controllers
{
    public class ErroresController : Controller
    {
        public ActionResult e404() {
            return View();
        }
    }
}