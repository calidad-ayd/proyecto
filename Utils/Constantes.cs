using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace ComunidadPractica
{
    public class Constantes
    {
        public static List<string> obtenerListadoPaises()
        {
            List<string> paises = new List<string>();

            CultureInfo[] informacionRegional = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            foreach (CultureInfo iteradorRegion in informacionRegional)
            {
                RegionInfo datosRegion = new RegionInfo(iteradorRegion.LCID);
                if (datosRegion != null)
                {
                    if (!paises.Contains(datosRegion.EnglishName))
                    {
                        paises.Add(datosRegion.EnglishName);
                    }
                }

            }
            paises.Sort();
            paises.Insert(0, " - Sin Selección - ");

            return paises;
        }

        public static List<string> obtenerListadoGeneros()
        {
            List<string> generos = new List<string>();
            generos.Add(" - Sin Selección - ");
            generos.Add("Masculino");
            generos.Add("Femenino");
            generos.Sort();
            return generos;
        }

        public static List<string> obtenerListadoIdiomas()
        {
            List<string> idiomas = new List<string>();
            idiomas = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath("~/App_Data/Compartido/lenguajes.txt")).ToList();
            return idiomas;
        }

        public static List<string> obtenerListadoHabilidades()
        {
            List<string> habilidades = new List<string>();
            habilidades = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath("~/App_Data/Compartido/habilidades.txt")).ToList();
            return habilidades;
        }

        public static class ErroresSQL {
            public const int SQL_DUPLICATE_PK = 2627;
            public const int SQL_NOTFOUND_FK = 547;
        }

        public static class Estado
        {
            public const int ERROR_NOT_DEFINED = 3;
            public const int SUCCESS = 0;
        }

    }
}