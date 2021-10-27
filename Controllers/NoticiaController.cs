#nullable enable
using System;
using System.Collections.Generic;
using ComunidadPractica.Handlers;
using System.Web.Mvc;
using ComunidadPractica.Models;

namespace ComunidadPractica.Controllers
{
    public class NoticiaController : Controller
    {

        public const bool noticias_no_aprobadas = false;
        public const bool noticias_aprobadas = true;

        [HttpGet]
        public ActionResult listadoDeNoticias(int pagina = 0)
        {
            NoticiaHandler handler = new NoticiaHandler();
            List<string> ubicaciones = handler.obtenerUbicacionesNoticias();

            List<NoticiaModel> noticias = handler.obtenerNoticiasPorPagina(ubicaciones, pagina, 10, noticias_aprobadas);

            int cantidadNoticias = noticias.Count;
            int cantidadPaginas = (int)Math.Ceiling((double)cantidadNoticias / 10);

            ViewBag.ListadoNoticias = noticias;
            ViewBag.Pagina = pagina;
            ViewBag.CantidadPaginas = cantidadPaginas;

            return View();
        }

        [HttpGet]
        public ActionResult verNoticia(string noticiaID)
        {
            if (noticiaID != null && noticiaID.Length > 0) {
                noticiaID = Tokenizador.DetokenizarHilera(noticiaID);
                if (noticiaID.Length > 12) {
                    NoticiaHandler handler = new NoticiaHandler();
                    NoticiaModel modeloNoticia = handler.obtenerNoticia(Server.MapPath("~/App_Data/Noticias/Metadatos/Noticia." + noticiaID + ".json"));

                    if (modeloNoticia != null)
                    {
                        ViewBag.Noticia = modeloNoticia;
                        return View();
                    }
                }
                
            }
            return RedirectToAction("Errores", "e404");
        }
      
    }
}