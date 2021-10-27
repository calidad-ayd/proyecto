using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.IO;
using ComunidadPractica.Handlers;
using ComunidadPractica.Models;


namespace ComunidadPractica.Controllers
{
    public class CursoController : Controller
    {
        public static class Constants
        {
            public const byte CURSO_POR_APROBAR = 0;
            public const byte CURSO_APROBADO = 1;
            public const byte CURSO_REPROBADO = 2;
            public const byte CURSO_CREADO = 3;
        }

        [HttpGet]
        public ActionResult inscribirmeEnCurso(string nombreCurso)
        {
            CursoHandler cursoHandler = new CursoHandler();
            if (nombreCurso != null) {
                nombreCurso = Tokenizador.DetokenizarHilera(nombreCurso);
                int version = CursoHandler.obtenerUltimaVersion(nombreCurso);
                if (nombreCurso.Length > 0 && cursoHandler.obtenerCurso(nombreCurso, version) != null)
                {
                    if (SesionController.existeSesion(System.Web.HttpContext.Current)){
                        string correo = SesionController.obtenerModeloSesionUsuario(System.Web.HttpContext.Current).correo;
                        inscribirCorreoEnCurso(correo, nombreCurso);
                        nombreCurso =  Tokenizador.TokenizarHilera(nombreCurso);
                        return RedirectToAction("verCurso", "Curso", new { nombreCurso, version });
                    }
                    ViewBag.Curso = nombreCurso;
                    return View();
                }
            }
            
            return RedirectToAction("Portada", "Comunidad");
        }

        [HttpPost]
        public ActionResult inscribirCorreoEnCurso(string correo, string nombreCurso)
        {
            nombreCurso = HttpUtility.HtmlDecode(nombreCurso);

            MiembroHandler miembroHandler = new MiembroHandler();
            UsuarioHandler usuarioHandler = new UsuarioHandler();
            

            if (usuarioHandler.obtenerUsuario(correo) != null || miembroHandler.obtenerMiembro(correo) != null) 
            {
                CursoHandler cursoHandler = new CursoHandler();
                int version = CursoHandler.obtenerUltimaVersion(nombreCurso);
                Mensaje respuesta = new Mensaje();
                if (cursoHandler.inscribirUsuarioEnCurso(correo, nombreCurso, version) > 0)
                {
                    respuesta.exito = "El usuario se ha inscrito correctamente.";
                }
                else
                {
                    respuesta.error = "El usuario ya estaba inscrito previamente en este curso. Inicie sesión con sus datos.";
                }
                return PartialView("../Shared/mensaje", respuesta);
            }
            else
            {
                UsuarioEnCurso modelo = new UsuarioEnCurso();
                modelo.curso.nombre = nombreCurso;
                modelo.usuario.correo = correo;

                return PartialView("formularioInscribirmeParcial", modelo);
            }

        }

        [HttpPost]
        public ActionResult inscribirParticipanteExterno(UsuarioEnCurso modelo) 
        {
            UsuarioHandler usuarioHandler = new UsuarioHandler();
            CursoHandler cursoHandler = new CursoHandler();

            UsuarioModel usuario = modelo.usuario;
            usuarioHandler.insertarUsuario(usuario);
            int version = CursoHandler.obtenerUltimaVersion(modelo.curso.nombre);
            cursoHandler.inscribirUsuarioEnCurso(usuario.correo, modelo.curso.nombre, version);

            var mensaje = new Mensaje();
            mensaje.exito = "Prueba a iniciar sesión con el correo y la contraseña para ver el progreso de tus cursos.";
            TempData["Mensaje"] = mensaje;

            return RedirectToAction("listadoDeCursos", "Curso");
        }
        
        [HttpGet]
        public ActionResult proponerUnCurso()
        {
            if (SesionController.existeSesion(System.Web.HttpContext.Current))
            {
                CursoHandler accesoDatos = new CursoHandler();
                ViewBag.Categorias = accesoDatos.obtenerCategorias();
                ViewBag.Topicos = accesoDatos.obtenerTopicos(ViewBag.Categorias);
                return View();
            }

            return RedirectToAction("e404", "Errores");
        }

        [HttpPost]
        public ActionResult proponerUnCurso(CursoModel curso)
        {
            ViewBag.ExitoAlProponer = false;
            try
            {
                if (ModelState.IsValid)
                {
                    CursoHandler accesoDatos = new CursoHandler();
                    ViewBag.ExitoAlProponer = accesoDatos.proponerCurso(curso);
                    ViewBag.Categorias = accesoDatos.obtenerCategorias();
                    ViewBag.Topicos = accesoDatos.obtenerTopicos(ViewBag.Categorias);
                    if (ViewBag.ExitoAlProponer == 0)
                    {
                        ViewBag.Message = "La propuesta del curso '" + curso.nombre + "' fue enviada satisfactoriamente.";
                        ModelState.Clear();
                    }
                    else
                    {
                        if (ViewBag.ExitoAlProponer == Constantes.ErroresSQL.SQL_DUPLICATE_PK)
                        {
                            ViewBag.Message = "Ya existe otro curso con este nombre.";
                        }
                        else if (ViewBag.ExitoAlProponer == Constantes.ErroresSQL.SQL_NOTFOUND_FK)
                        {
                            ViewBag.Message = "Este miembro no pertence a la comunidad.";
                        }
                        else
                        {
                            ViewBag.Message = "No se pudo ingresar el curso.";
                        }
                    }
                }
            }
            catch
            {

                ViewBag.Message = "Lo sentimos, hubo un problema al enviar la propuesta del curso.";
            }

            return View();
        }

        [HttpPost]
        public ActionResult proponerCurso(List<string> categorias)
        {

            ViewBag.ExitoAlObtenerTopicos = false;
            try
            {
                if (ModelState.IsValid)
                {
                    CursoHandler accesoDatos = new CursoHandler();
                    ViewBag.Categorias = accesoDatos.obtenerCategorias();
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
        public ActionResult verCurso(string nombreCurso, int version = 0)
        {
            if(nombreCurso.Length > 0 && version > 0)
            {
                nombreCurso = Tokenizador.DetokenizarHilera(nombreCurso);
                var usuario = SesionController.obtenerModeloSesionUsuario(System.Web.HttpContext.Current);
                CursoHandler accesoDatos = new CursoHandler();

                if (usuario != null && CursoHandler.perteneceCurso(usuario.correo, nombreCurso)) {
                    ViewBag.progreso = accesoDatos.obtenerPorcentaje(usuario.correo, nombreCurso);
                }
                CursoModel modeloCurso = accesoDatos.obtenerCurso(nombreCurso, version);
                return View(modeloCurso);
            }

            return RedirectToAction("e404", "Errores");
        }

        [HttpGet]
        public FileResult descargarArchivoMaterial(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/force-download", "descarga" + ext);
        }

        [HttpGet]
        public ActionResult editarCurso(string nombreCurso, int version)
        {
            nombreCurso = Tokenizador.DetokenizarHilera(nombreCurso);
            CursoHandler accesoDatos = new CursoHandler();
            CursoModel modeloCurso = accesoDatos.obtenerCurso(nombreCurso, version);

            return View(modeloCurso);
        }

        [HttpPost]
        public ActionResult editarCurso(CursoModel modeloCurso)
        {
            CursoHandler accesoDatos = new CursoHandler();
            modeloCurso.version =  accesoDatos.editarCurso(modeloCurso);
            return RedirectToAction("listadoDeCursos", "Curso");
        }

        [HttpPost]
        public ActionResult editarCursoParcial(CursoModel modeloCurso)
        {
            return PartialView(modeloCurso);
        }

        public ActionResult aprobarCursos(int pagina = 0)
        {
            CursoHandler accesoDatos = new CursoHandler();
            List<CursoModel> cursos = accesoDatos.obtenerCursosPorPagina(pagina, 10, Constants.CURSO_POR_APROBAR);
            int cantidadCursos = accesoDatos.obtenerCantidadTuplas("Curso", "WHERE estado = 0");
            int cantidadPaginas = (int)Math.Ceiling((double)cantidadCursos / 10);

            ViewBag.Cursos = cursos;
            ViewBag.Pagina = pagina;
            ViewBag.CantidadPaginas = cantidadPaginas;
            return View();
        }

        [HttpPost]
        public ActionResult aprobarCursos(int pagina = 0, string nombreCurso = null, int aprobado = Constants.CURSO_POR_APROBAR)
        {
            CursoHandler accesoDatos = new CursoHandler();
            if (nombreCurso != null) {
                accesoDatos.cambiarEstadoCurso(nombreCurso, aprobado);
            }
            return RedirectToAction("aprobarCursos", "Curso", new { pagina });
        }

        [HttpGet]
        public ActionResult crearCurso()
        {
            if (!SesionController.existeSesion(System.Web.HttpContext.Current)) {
                return RedirectToAction("e404", "Errores");
            }

            CursoHandler accesoDatos = new CursoHandler();
            string correo = SesionController.obtenerModeloSesion(System.Web.HttpContext.Current).correo;
            ViewBag.nombresCursos = accesoDatos.obtenerNombreCursosAprobados(correo);
            return View();
        }

        [HttpPost]
        public ActionResult crearCurso(CursoModel curso)
        {
            CursoHandler accesoDatos = new CursoHandler();
            try
            {
                accesoDatos.crearCurso(curso);
            }
            catch
            {
                ViewBag.Message = "Error: no se logró crear curso.";
                ViewBag.creado = false;
            }
            ViewBag.Message = "Curso creado satisfactoriamente.";
            ViewBag.creado = true;
            accesoDatos.cambiarEstadoCurso(curso.nombre, Constants.CURSO_CREADO);
            return crearCurso();
        }

        [HttpGet]
        public ActionResult listadoDeCursos(int pagina = 0, int encuestasPorPagina = 10, int aprobado = Constants.CURSO_CREADO)
        {
            CursoHandler accesoDatos = new CursoHandler();
            List<string> categorias = accesoDatos.obtenerCategorias();
            int cantidadCursos = accesoDatos.obtenerCantidadTuplas("Curso", "WHERE estado = " + aprobado.ToString());
            int cantidadPaginas = (int)Math.Ceiling((double)cantidadCursos / 10);
            List<string> topicos = accesoDatos.obtenerTopicos(categorias);

            ViewBag.Categorias = categorias;
            ViewBag.topicos = topicos;
            ViewBag.estado = aprobado;

            if (TempData["Mensaje"] != null) {
                var mensaje = (Mensaje)TempData["Mensaje"];
                TempData.Remove("Mensaje");
                return View(mensaje);
            }
            return View(new Mensaje());
        }

        [HttpPost]
        public ActionResult listadoDeCursosParcial(string titulo, string categoria, string topico, int estado)
        {
            int version;
            List<int> indices = new List<int>();
            CursoHandler accesoDatos = new CursoHandler();
            List<CursoModel> lista = accesoDatos.obtenerCursosPorFiltros(titulo, categoria, topico, estado);

            bool haySesion = SesionController.existeSesion(System.Web.HttpContext.Current);

            UsuarioModel usuario = new MiembroModel();
            if (haySesion)
            {
               usuario = SesionController.obtenerModeloSesionUsuario(System.Web.HttpContext.Current);
            }

            for(int indice = 0; indice < lista.Count; indice++)
            {
                if(haySesion)
                {
                    if (CursoHandler.perteneceCurso(usuario.correo, lista[indice].nombre))
                    {
                        version = accesoDatos.obtenerVersion(usuario.correo, lista[indice].nombre);
                        if(lista[indice].version != version)
                        {
                            indices.Add(indice);
                        }
                    }
                    else if(usuario.correo != lista[indice].educador)
                    {
                        version = CursoHandler.obtenerUltimaVersion(lista[indice].nombre);
                        if(lista[indice].version != version)
                        {
                            indices.Add(indice);
                        }
                    }
                }
                else
                {
                    version = CursoHandler.obtenerUltimaVersion(lista[indice].nombre);
                    if (lista[indice].version != version)
                    {
                        indices.Add(indice);
                    }
                }
            }

            for(int indice = indices.Count-1; indice >= 0; indice--)
            {
                lista.RemoveAt(indices[indice]);
            }
            
            return PartialView(lista);
        }

        [HttpPost]
        public void actualizarPorcentaje(string nombreCurso, string correoEstudiante, string nombreMaterial, string nombreSeccion)
        {
            CursoHandler accesoDatos = new CursoHandler();
            bool modifica = accesoDatos.insertarCompletaMaterial(nombreCurso, correoEstudiante, nombreMaterial, nombreSeccion);
        }

        [HttpGet]
        public ActionResult materialCompletado(string nombreCurso, int version = 1)
        {

            if (SesionController.existeSesion(System.Web.HttpContext.Current))
            {
                nombreCurso = Tokenizador.DetokenizarHilera(nombreCurso);
                CursoHandler accesoDatos = new CursoHandler();
                List<ParticipaCurso> estudiantes = accesoDatos.obtenerEstudiantesCurso(nombreCurso, version);
                ViewBag.nombre = nombreCurso;
                ViewBag.estudiantes = estudiantes;
                return View();
            }

            return RedirectToAction("e404", "Errores");
                
        }

        public static bool tengoCursosAprobados(string correo)
        {
            CursoHandler accesoDatos = new CursoHandler();
            bool tengoCursos = accesoDatos.tengoCurso(correo, Constants.CURSO_APROBADO);
            return tengoCursos;
        }
    }
}
