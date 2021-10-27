using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using ComunidadPractica.Handlers;
using ComunidadPractica.Models;
using System.Data;
using ClosedXML.Excel;
using System.IO;
using System.Threading;

namespace ComunidadPractica.Controllers
{
    public class EncuestaController : Controller
    {
        public static string correoMaestro = "introduccion.biologia.ucr @gmail.com";

        [HttpGet]
        public ActionResult listadoDeEncuestas()
        {
            ViewBag.Mensaje = "";
            ViewBag.Categorias = new EncuestaHandler().obtenerCategorias();
            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"].ToString();
                TempData.Remove("Message");
            }
            return View();
        }

        [HttpPost]
        public ActionResult listadoDeEncuestasParcial(string titulo, string categoria, string topico)
        {
            EncuestaHandler handler = new EncuestaHandler();
            List<EncuestaModel> lista = handler.obtenerEncuestasPorFiltros(titulo, categoria, topico);
            return PartialView(lista);
        }

        [HttpPost]
        public ActionResult listadoDeTopicosParcial(string categoria)
        {
            if (categoria.Length == 0)
            {
                categoria = "Todas";
            }
            EncuestaHandler handler = new EncuestaHandler();
            List<string> topicos = handler.obtenerTopicos(new List<string> { categoria });
            return PartialView("listadoDeTopicosParcial", topicos);
        }

        [HttpGet]
        public ActionResult crearEncuesta()
        {
            EncuestaModel encuesta = new EncuestaModel();
            ViewBag.Categorias = new EncuestaHandler().obtenerCategorias();
            ViewBag.Topicos = new List<string>();

            ViewBag.Error = "";
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"].ToString();
                TempData.Remove("Error");
            }

            return View(encuesta);
        }

        [HttpPost]
        public ActionResult crearEncuesta(EncuestaModel encuesta)
        {
            EncuestaHandler accesoDatos = new EncuestaHandler();
            int id = accesoDatos.crearEncuesta(encuesta);
            if (id >= 0)
            {
                return RedirectToAction("agregarItems", "Encuesta", new { encuesta = id });
            }
            else
            {
                TempData["Error"] = "No se pudo crear la encuesta";
                return RedirectToAction("crearEncuesta", "Encuesta");
            }
        }

        [HttpGet]
        public ActionResult contestarEncuesta(int? encuestaID)
        {
           
            if (encuestaID.HasValue)
            {

                if (SesionController.existeSesion(System.Web.HttpContext.Current)) {
                    if (AutorizacionController.tengoPermiso(System.Web.HttpContext.Current, SesionHandler.Permiso.EncuestaVerRespuestas))
                    {

                        var correoMiembro = SesionController.obtenerModeloSesionUsuario(System.Web.HttpContext.Current).correo;
                        var comprobacionTemporal = new EncuestaHandler().
                            consultarTabla("SELECT correoFK FROM dbo.Encuesta WHERE correoFK = '" + correoMiembro + "'" +
                            " AND encuestaIdPK = " + encuestaID + "");

                        if (comprobacionTemporal.Rows.Count > 0)
                        {
                            return RedirectToAction("obtenerRespuestas", "Encuesta", new { encuestaID });
                        }

                    }
                }
               
                EncuestaHandler accesoDatos = new EncuestaHandler();
                EncuestaModel modelo = accesoDatos.obtenerEncuesta((int)encuestaID);
                if (modelo != null)
                {
                    List<int> idsItems = new List<int>();
                    foreach (var item in modelo.items)
                    {
                        idsItems.Add(item.numItem);
                    }

                    ViewBag.IDsItems = idsItems;
                    ViewBag.Error = "";

                    if (TempData["Error"] != null) 
                    {
                        ViewBag.Error = TempData["Error"];
                        TempData.Remove("Error");
                    }

                    return View(modelo);
                }
                else
                {
                    return RedirectToAction("Portada", "Comunidad");
                }
            }
            else
            {
                return RedirectToAction("Portada", "Comunidad");
            }
        }
        
        [HttpPost]
        public ActionResult contestarEncuesta(int encuestaID, List<int> numItems, List<string> respuestas)
        {
            EncuestaHandler accesoDatos = new EncuestaHandler();

            int intento = accesoDatos.obtenerNumeroIntentos(SesionController.obtenerModeloSesion(System.Web.HttpContext.Current).correo, encuestaID);

            bool insertada = new EncuestaHandler().contestarEncuesta(encuestaID, numItems, respuestas, intento + 1);
            if (insertada)
            {
                TempData["Mensaje"] = "La encuesta se ha contestado correctamente.";
                return Json(Url.Action("listadoDeEncuestas", "Encuesta"));
            }
            else
            {
                TempData["Error"] = "Debe contestar todas los enunciados.";
                return Json(Url.Action("contestarEncuesta", "Encuesta", new { encuestaID }));
            }
        }

        [HttpPost]
        public ActionResult eliminarEncuesta(int encuestaID)
        {
            EncuestaHandler accesoDatos = new EncuestaHandler();
            accesoDatos.eliminarEncuesta(encuestaID);
            return RedirectToAction("Portada", "comunidad");
        }

        private void enviarCorreoThread(int encuestaID, string dominio) 
        {
            List<string> emails = new MiembroHandler().obtenerTodosLosCorreos();

            foreach (string email in emails)
            {
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("introduccion.biologia.ucr@gmail.com", "IntroBiolo*0123");
                smtp.EnableSsl = true;

               
                MailMessage mensaje = new MailMessage(correoMaestro, email);
                mensaje.Subject = "[Encuestas]" + encuestaID.ToString();
                mensaje.Body = "Enhorabuena! Te ha compartido una encuesta de la comunidad de práctica, la cual "
                   + "puedes acceder desde el enlace: " + dominio + "/Encuesta/contestarEncuesta?encuestaID=" + encuestaID;
                try
                {
                    smtp.Send(mensaje);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }
        }

        [HttpGet]
        public ActionResult compartirEncuesta(int encuestaID)
        {
            ItemHandler itemHandler = new ItemHandler();

            if (itemHandler.obtenerItemsSegunEncuesta(encuestaID).Count > 0)
            {
                string dominio = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                Thread hilo = new Thread(() => enviarCorreoThread(encuestaID, dominio));
                hilo.Start();

                TempData["Mensaje"] = "La encuesta se ha compartido correctamente a sus correos electrónicos.";
                return RedirectToAction("listadoDeEncuestas", "Encuesta");
            }

            TempData["Error"] = "Debe agregar un enunciado al menos.";
            return RedirectToAction("agregarItems", "Encuesta", new { encuesta = encuestaID });
        }

        [HttpGet]
        public ActionResult agregarItems(int? encuesta)
        {
            if (encuesta.HasValue)
            {
                EncuestaHandler accesoDatos = new EncuestaHandler();
                EncuestaModel modelo = accesoDatos.obtenerEncuesta((int)encuesta);
                if (modelo != null)
                {
                    ViewBag.Error = "";
                    ViewBag.Message = "";
                    if (TempData["Error"] != null)
                    {
                        ViewBag.Error = TempData["Error"].ToString();
                        TempData.Remove("Error");
                    }
                    if (TempData["Success"] != null)
                    {
                        ViewBag.Message = TempData["Success"].ToString();
                        TempData.Remove("Success");
                    }
                    return View(modelo);
                }
                else
                {
                    return RedirectToAction("Portada", "Comunidad");
                }
            }
            else
            {
                return RedirectToAction("Portada", "Comunidad");
            }
        }

        [HttpPost]
        public ActionResult agregarItems(int encuesta, string enunciado)
        {
            if (enunciado.Length > 1)
            {
                EncuestaHandler handler = new EncuestaHandler();
                int indiceItemNuevo = handler.obtenerNumeroItems(encuesta);
                int numItem = handler.insertarItemEncuesta(indiceItemNuevo + 1, encuesta, enunciado);
                if (numItem >= 0)
                {
                    TempData["Success"] = "Se agrego el enunciado correctamente.";
                    return RedirectToAction("agregarOpciones", "Encuesta", new { encuesta, numItem });
                }
            }
            else
            {
                TempData["Error"] = "No se puede agregar ese enunciado.";

            }
            return RedirectToAction("agregarItems", "Encuesta", new { encuesta });

        }

        [HttpGet]
        public ActionResult agregarOpciones(int? encuesta, int? numItem)
        {
            if (encuesta.HasValue && numItem.HasValue)
            {
                ItemHandler accesoDatos = new ItemHandler();
                ItemModel modelo = accesoDatos.obtenerItemEncuesta((int)encuesta, (int)numItem);
                if (modelo != null)
                {
                    ViewBag.Error = "";
                    ViewBag.Message = "";
                    if (TempData["Error"] != null)
                    {
                        ViewBag.Error = TempData["Error"].ToString();
                        TempData.Remove("Error");
                    }
                    if (TempData["Success"] != null)
                    {
                        ViewBag.Message = TempData["Success"].ToString();
                        TempData.Remove("Success");
                    }
                    ViewBag.EncuestaID = encuesta;
                    return View(modelo);
                }
                else
                {
                    return RedirectToAction("agregarItems", "Encuesta", new { encuesta = (int)encuesta });
                }
            }
            else
            {
                return RedirectToAction("Portada", "Comunidad");
            }
        }

        [HttpPost]
        public ActionResult agregarOpciones(int encuesta, int numItem, string opcion)
        {
            ItemHandler handler = new ItemHandler();
            ItemModel modelo = handler.obtenerItemEncuesta(encuesta, numItem);

            if (opcion.Length > 1)
            {
                bool exito = handler.crearOpcion(encuesta, numItem, opcion);
                if (exito)
                {
                    TempData["Success"] = "Se agrego la opción correctamente.";
                    return RedirectToAction("agregarOpciones", "Encuesta", new { encuesta, numItem });
                }
            }
            else
            {
                TempData["Error"] = "No se puede agregar esa opción.";
                return RedirectToAction("agregarOpciones", "Encuesta", new { encuesta, numItem });
            }

            return View("agregarOpciones", modelo);
        }

        [HttpPost]
        public ActionResult eliminarItemDeEncuesta(int encuestaID, int itemID)
        {
            ItemHandler accesoDatos = new ItemHandler();
            accesoDatos.eliminarItem(encuestaID, itemID);
            return RedirectToAction("agregarItems", "Encuesta", new { encuesta = (int)encuestaID });
        }

        [HttpPost]
        public ActionResult eliminarOpcionDeItem(int encuestaID, int itemID, string opcion)
        {
            ItemHandler accesoDatos = new ItemHandler();
            bool exito = accesoDatos.eliminarOpcion(encuestaID, itemID, opcion);
            return RedirectToAction("agregarOpciones", "Encuesta", new { encuesta = encuestaID, numItem = itemID });
        }

        [HttpPost]
        public ActionResult editarItemDeEncuesta(int encuestaID, int itemID)
        {
            return RedirectToAction("agregarOpciones", "Encuesta", new { encuesta = encuestaID, numItem = itemID });
        }

        [HttpPost]
        public ActionResult obtenerCategorias(List<string> categorias)
        {
            ViewBag.ExitoAlObtenerTopicos = false;
            try
            {
                if (ModelState.IsValid)
                {
                    EncuestaHandler accesoDatos = new EncuestaHandler();
                    ViewBag.Topicos = accesoDatos.obtenerTopicos(categorias);
                    ModelState.Clear();
                }
            }
            catch
            {
                ViewBag.Message = "Lo sentimos, hubo un problema al obtener tópicos.";
            }

            return Json(ViewBag.Topicos);
        }

        [HttpGet]
        public ActionResult obtenerRespuestas(int? encuestaID)
        {
            if (encuestaID.HasValue) {
                EncuestaHandler accesoDatos = new EncuestaHandler();
                EncuestaModel modelo = accesoDatos.obtenerEncuesta((int)encuestaID);
                ViewBag.encuesta = modelo;
                ViewBag.cantRespuestas = accesoDatos.actualizarRespuestas(modelo);
                return View(modelo);
            }
            return RedirectToAction("Portada", "Comunidad");
        }

        [HttpGet]
        public FileResult descargarRespuestas(int encuestaID)
        {
            EncuestaHandler accesoDatos = new EncuestaHandler();
            DataTable datos = accesoDatos.obtenerRepuestas(encuestaID);
            MemoryStream datosParseados = construirArchivoExcel(datos);
            return File(datosParseados.ToArray(), "application/vnd.ms-excel", "respuestas.xls");
        }

        public MemoryStream construirArchivoExcel(DataTable tabla) {

            List<int> enunciados = new List<int>();
            XLWorkbook libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add("Resultados");

            // Preparar la hoja de estilos, en comenzando en (1,1)
            hoja.Cell(1, 1).Value = "Encuestado";

            // Colocar las preguntas en la hoja
            int columna = 2;
            foreach (DataRow fila in tabla.Rows)
            {
                var enunciado = Convert.ToInt32(fila["NumItem"]);
                if (!enunciados.Contains(enunciado)) 
                {
                    // En caso de que dos preguntas se llamen iguales, si su ID es distinto, son preguntas distintas
                    hoja.Cell(1, columna++).Value = Convert.ToString(fila["Enunciado"]);
                    enunciados.Add(enunciado);
                }
            }

            // Colocar los intentos como filas
            // Cada fila coloca las respuestas
            int numeroPreguntas = columna - 2;
            int filaLocal  = 2;
            int filaRemota = 0;

            for (;filaRemota < tabla.Rows.Count;) 
            {
                for (int respuesta = 0; respuesta < numeroPreguntas; respuesta++) {
                    hoja.Cell(filaLocal, 2 + respuesta).Value = Convert.ToString(tabla.Rows[filaRemota++]["Respuesta"]);
                }

                hoja.Cell(filaLocal++, 1).Value = Convert.ToString(tabla.Rows[filaRemota-1]["Encuestado"]);
            }

            MemoryStream dataStream = new MemoryStream();
            hoja.Rows().AdjustToContents();
            hoja.Columns().AdjustToContents();
            libro.SaveAs(dataStream);

            return dataStream;
        }

        
    }
}