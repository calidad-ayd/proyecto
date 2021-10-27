using System.Collections.Generic;
using ComunidadPractica.Models;
using System.Text;
using System.Web.Script.Serialization;
using System;
using System.IO;
using System.Web;
using System.Linq;

namespace ComunidadPractica.Handlers
{
    public class NoticiaHandler : BaseHandler
    {
        public NoticiaModel obtenerNoticia(string nombreArchivo)
        {
            string noticiaSerializada;
            NoticiaModel noticiaDeserializada = null;

            try
            {
                noticiaSerializada = System.IO.File.ReadAllText(nombreArchivo, Encoding.UTF8);
                noticiaDeserializada = new JavaScriptSerializer().Deserialize<NoticiaModel>(noticiaSerializada);
            }
            catch (Exception e) 
            {
            }
            
            return noticiaDeserializada;
        }

        public List<NoticiaModel> obtenerNoticiasPorPagina(List<string> ubicaciones, int pagina, int tuplasPorPagina, bool soloNoticiasAceptadas)
        {
            List<NoticiaModel> noticias = new List<NoticiaModel>();
            List<NoticiaModel> noticiasFiltradas;

            if (soloNoticiasAceptadas)
            {
                noticiasFiltradas = new List<NoticiaModel>();
                foreach (string ubicacion in ubicaciones)
                {
                    NoticiaModel noticia = obtenerNoticia(ubicacion);
                    if (noticia.aceptada) {
                        noticiasFiltradas.Add(noticia);
                    }
                }
            }
            else {
                noticiasFiltradas = noticias;
            }

            int inicioLista = pagina * tuplasPorPagina;
            int finLista = Math.Min(inicioLista  + tuplasPorPagina, noticiasFiltradas.Count);

            noticiasFiltradas = noticiasFiltradas.GetRange(inicioLista, finLista);

            return noticiasFiltradas;
        }

        public List<string> obtenerUbicacionesNoticias()
        {
            List<string> noticias = new List<string>();
            noticias = Directory.GetFiles(HttpContext.Current.Server.MapPath("~/App_Data/Noticias/Metadatos/")).ToList();
            return noticias;
        }
    }
}