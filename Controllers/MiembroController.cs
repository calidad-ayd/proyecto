#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ComunidadPractica.Handlers;
using System.Web.Mvc;
using ComunidadPractica.Models;
using Microsoft.Ajax.Utilities;
using ComunidadPractica.Utils.MotorRecomendacion;

namespace ComunidadPractica.Controllers
{
    public class MiembroController : Controller
    {
        [HttpGet]
        public ActionResult listadoDeMiembros(int pagina = 0)
        {
            MiembroHandler accesoDatos = new MiembroHandler();
            List<MiembroModel> miembrosPorPagina = accesoDatos.obtenerMiembrosPorPagina(pagina, 10);

            int cantidadMiembros = accesoDatos.obtenerCantidadTuplas("Miembro");
            int cantidadPaginas = (int)Math.Ceiling((double)cantidadMiembros / 10);

            ViewBag.Pagina = pagina;
            ViewBag.CantidadPaginas = cantidadPaginas;
            ViewBag.MiembrosPagina = miembrosPorPagina;
            return View();
        }

        [HttpPost]
        public ActionResult actualizarMiembroInformacionBasica(Mensaje modelo)
        {
            UsuarioHandler handler = new UsuarioHandler();
            MiembroModel miembro = modelo.miembroModel;
            UsuarioModel usuario = new UsuarioModel {
                nombre = miembro.nombre,
                correo = miembro.correo,
                contacto = miembro.contacto,
                pais = miembro.pais,
                sexo = miembro.sexo
            };
            handler.actualizarUsuario(usuario);
            TempData["mensaje"] = "Se ha actualizado los datos correctamente";
            return RedirectToAction("verMiembro", "Miembro", new { correo = Tokenizador.TokenizarHilera(miembro.correo)} ); 
        }

        [HttpPost]
        public ActionResult actualizarMiembroInformacionAdicional(Mensaje modelo)
        {
            MiembroHandler handler = new MiembroHandler();
            handler.actualizarInformacionMiembro(modelo.miembroModel);
            return RedirectToAction("verMiembro", "Miembro", new { correo = Tokenizador.TokenizarHilera(modelo.miembroModel.correo) });
        }

        [HttpGet]
        public ActionResult verMiembro(string correo = "")
        {
            correo = Tokenizador.DetokenizarHilera(correo);
            ActionResult vista;
            try
            {
                MiembroHandler accesoDatos = new MiembroHandler();
                MiembroModel modeloUsuario = accesoDatos.obtenerMiembro(correo);

                if (modeloUsuario == null)
                {
                    vista = RedirectToAction("e404", "Errores");
                }
                else
                {
                    List<string> listaPaises = Constantes.obtenerListadoPaises();
                    List<SelectListItem> valoresPaises = new List<SelectListItem>();

                    foreach (string pais in listaPaises)
                    {
                        valoresPaises.Add(
                            new SelectListItem { Text = pais, Value = pais, Selected = modeloUsuario.pais.ToLower().Equals(pais.ToLower()) ? true : false }
                        );
                    }

                    List<SelectListItem> valoresGenero = new List<SelectListItem>() {
                        new SelectListItem { Text = "Masculino", Value = "Masculino" , Selected = modeloUsuario.sexo.ToLower().Equals("masculino") ? true : false },
                        new SelectListItem { Text = "Femenino", Value = "Femenino"  , Selected = modeloUsuario.sexo.ToLower().Equals("femenino") ? true : false   }
                    };

                    List<SelectListItem> valoresEmpleado = new List<SelectListItem>() {
                        new SelectListItem { Text = "Empleado", Value = "Empleado" , Selected = modeloUsuario.sexo.ToLower().Equals("Empleado") ? true : false },
                        new SelectListItem { Text = "Desempleado", Value = "Desempleado"  , Selected = modeloUsuario.sexo.ToLower().Equals("Desempleado") ? true : false   }
                    };

                    ViewBag.Generos = valoresGenero;
                    ViewBag.Paises = valoresPaises;
                    ViewBag.CondicionesLaborales = valoresEmpleado;

                    int cantPerfiles = 12;
                    FiltroTotal total = new FiltroTotal();
                    AlgoritmoFrecuenciado algoritmoFrecuenciadoTotal = new AlgoritmoFrecuenciado(total);
                    List<string> correosSimilares = algoritmoFrecuenciadoTotal.obtenerPerfiles(correo, cantPerfiles);
                    List<MiembroModel> miembrosSimilares = new List<MiembroModel>();
                    foreach (var actual in correosSimilares)
                    {
                        miembrosSimilares.Add(accesoDatos.obtenerMiembro(actual));
                    }
                    ViewBag.perfilesSimilares = miembrosSimilares;
                    ViewBag.cantPerfiles = cantPerfiles;

                    FiltroHabilidades habilidades = new FiltroHabilidades();
                    AlgoritmoFrecuenciado algoritmoFrecuenciadoHabilidades = new AlgoritmoFrecuenciado(habilidades);
                    correosSimilares = algoritmoFrecuenciadoHabilidades.obtenerPerfiles(correo, cantPerfiles);
                    List<MiembroModel> similaresHabilidades = new List<MiembroModel>();
                    foreach (var actual in correosSimilares)
                    {
                        similaresHabilidades.Add(accesoDatos.obtenerMiembro(actual));
                    }
                    ViewBag.perfilesHabilidades = similaresHabilidades;

                    Mensaje modelo = new Mensaje();
					modelo.miembroModel = modeloUsuario;

                    if (TempData["mensaje"] != null) {
                        modelo.exito = (string)TempData["mensaje"];
                        TempData.Remove("mensaje");
                    }

                    vista = View(modelo);
                }
            }
            catch
            {
                vista = RedirectToAction("Portada", "Comunidad");
            }

            return vista;
        }

        [HttpGet]
        public ActionResult agregarMiembro()
        {
            if (SesionController.existeSesion(System.Web.HttpContext.Current))
            {
                return RedirectToAction("Portada", "Comunidad");
            }
            return View(new Mensaje());
        }

        [HttpPost]
        public ActionResult agregarMiembro(MiembroModel miembro)
        {
            Mensaje respuesta = new Mensaje();

            MiembroHandler accesoDatos = new MiembroHandler();
            int codigoRespuesta = accesoDatos.agregarMiembro(miembro);
            if (codigoRespuesta >= 0)
            {
                respuesta.exito = "Se ha registrado correctamente el usuario.";
                ModelState.Clear();
            }
            else
            {
                respuesta.error = "Lo sentimos, este miembro ya pertenece a la comunidad.";
            }
           
            return View(respuesta);
        }

        [HttpPost]
        public ActionResult agregarMiembroParcial(string correo)
        {

            if (correo.IsNullOrWhiteSpace())
            {
                return PartialView(new MiembroModel());
            }
            else {
                UsuarioHandler accesoDatos = new UsuarioHandler();
                UsuarioModel usuario = accesoDatos.obtenerUsuario(correo);

                MiembroModel miembroModel = new MiembroModel();

                if (usuario != null)
                {
                    var rol = new SesionHandler().consultarRol(correo);
                    miembroModel.nombre = usuario.nombre;
                    miembroModel.correo = usuario.correo;
                    miembroModel.contacto = usuario.contacto;
                    miembroModel.sexo = usuario.sexo;
                    miembroModel.pais = usuario.pais;
                    miembroModel.esParticipanteExterno = (rol == "Externo");

                    return PartialView(miembroModel);
                }
                else {
                    return PartialView(new MiembroModel());
                }
            }
            
        }

    }

}