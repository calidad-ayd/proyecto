#nullable enable
using System.Web.Script.Serialization;
using System.Web.Mvc;
using System.Text;
using ComunidadPractica.Models;

namespace ComunidadPractica.Controllers
{
    public class ComunidadController : Controller
    {
        public static string rutaGuardado = "~/App_Data/Comunidad/Portada.json";

        public ActionResult Portada()
        {
            ComunidadModel comunidad = obtenerComunidad();
            ViewBag.Comunidad = comunidad;
            return View();
        }

         public ComunidadModel obtenerComunidad() {
            string comunidadSerializada = System.IO.File.ReadAllText(Server.MapPath(rutaGuardado), Encoding.UTF8);
            ComunidadModel modeloDeserializado = new JavaScriptSerializer().Deserialize<ComunidadModel>(comunidadSerializada);
            return modeloDeserializado;
         }

         public void guardarComunidad(ComunidadModel modelo) {
            string comunidadSerializada = new JavaScriptSerializer().Serialize(modelo);
            System.IO.File.WriteAllText(Server.MapPath(rutaGuardado), comunidadSerializada, Encoding.UTF8);
         }

         public string obtenerCorreoDuennoProducto()
         {
            return obtenerComunidad().correo;
         }

    }
}