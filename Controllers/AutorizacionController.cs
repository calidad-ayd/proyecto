using System.Web.Mvc;
using System.Collections.Generic;
using ComunidadPractica.Handlers;

namespace ComunidadPractica.Controllers
{
    public  class AutorizacionController : Controller
    {
        public static bool tengoPermiso(System.Web.HttpContext contexto, SesionHandler.Permiso permiso)
        {
            if (contexto.Session["permisos"] != null) {
                var lista = (List<SesionHandler.Permiso>)contexto.Session["permisos"];
                foreach (var permisoIt in lista)
                {
                    if (permiso == permisoIt)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<SesionHandler.Permiso> obtenerListadoPermisos(SesionHandler.Rol rol, string correo)
        {
            var lista = new List<SesionHandler.Permiso>();
            if (CursoController.tengoCursosAprobados(correo))
            {
                lista.Add(SesionHandler.Permiso.CursoCrear);
            }
            switch (rol) {
                case SesionHandler.Rol.Externo:
                    lista.Add(SesionHandler.Permiso.CursoVer);
                    break;
                case SesionHandler.Rol.Activo:
                    lista.Add(SesionHandler.Permiso.MiembroEditar);
                    lista.Add(SesionHandler.Permiso.EncuestaVerListado);
                    lista.Add(SesionHandler.Permiso.EncuestaContestar);
                    lista.Add(SesionHandler.Permiso.CursoProponer);
                    lista.Add(SesionHandler.Permiso.CursoVer);
                    break;
                case SesionHandler.Rol.Nucleo:
                    lista.Add(SesionHandler.Permiso.MiembroEditar);
                    lista.Add(SesionHandler.Permiso.EncuestaVerListado);
                    lista.Add(SesionHandler.Permiso.EncuestaContestar);
                    lista.Add(SesionHandler.Permiso.EncuestaVerRespuestas);
                    lista.Add(SesionHandler.Permiso.CursoProponer);
                    lista.Add(SesionHandler.Permiso.CursoAprobar);
                    lista.Add(SesionHandler.Permiso.CursoVer);
                    break;
                case SesionHandler.Rol.Coordinador:
                    lista.Add(SesionHandler.Permiso.MiembroEditar);
                    lista.Add(SesionHandler.Permiso.EncuestaVerListado);
                    lista.Add(SesionHandler.Permiso.EncuestaVerRespuestas);
                    lista.Add(SesionHandler.Permiso.EncuestaCrear);
                    lista.Add(SesionHandler.Permiso.EncuestaContestar);
                    lista.Add(SesionHandler.Permiso.CursoProponer);
                    lista.Add(SesionHandler.Permiso.CursoAprobar);
                    lista.Add(SesionHandler.Permiso.CursoVer);
                    break;
            }
            return lista;
        }
    }
}