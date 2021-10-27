using ComunidadPractica.Handlers;
using ComunidadPractica.Models;
using System.Threading;
using System.Web.Mvc;

namespace ComunidadPractica.Controllers
{
    public class SesionController : Controller
    {

        [HttpPost]
        public JsonResult iniciarSesion(string correo, string password)
        {
            bool exito = false;

            SesionHandler handler = new SesionHandler();
            SesionHandler.Rol rol = handler.autentificarUsuario(correo, password);

            if (rol != SesionHandler.Rol.Null) {
                HttpContext.Session["permisos"] = new AutorizacionController().obtenerListadoPermisos(rol,correo);
                HttpContext.Session["usuario"] = new UsuarioHandler().obtenerUsuario(correo);
                HttpContext.Session["miembro"] = new MiembroHandler().obtenerMiembro(correo);
                exito = true;
            }
            return Json(new { returnvalue = exito });
        }

        [HttpGet]
        public ActionResult finalizarSesion()
        {
            Session.Clear();
            return RedirectToAction("Portada", "Comunidad");
        }

        public static bool existeSesion(System.Web.HttpContext contexto) {
            return (contexto.Session != null && (contexto.Session["miembro"] != null || contexto.Session["usuario"] != null));
        }

        public static MiembroModel obtenerModeloSesion(System.Web.HttpContext contexto)
        {
            return (MiembroModel)contexto.Session["miembro"];
        }

        public static UsuarioModel obtenerModeloSesionUsuario(System.Web.HttpContext contexto)
        {
            return (UsuarioModel)contexto.Session["usuario"];
        }

    }
}