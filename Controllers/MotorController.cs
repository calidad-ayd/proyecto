using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ComunidadPractica.Utils.MotorRecomendacion;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ComunidadPractica.Controllers
{
    public class MotorController : Controller
    {
        [HttpGet]
        public ActionResult TestMotor()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TestMotor(string nulo)
        {
            ProgramaPruebas.Main();
            return View();
        }
    }
}