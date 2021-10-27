using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ComunidadPractica.Utils.MotorRecomendacion
{
    public class ProgramaPruebas
    {
        public static void Main()
        {
            FiltroHabilidades habilidades = new FiltroHabilidades();
            FiltroTotal total = new FiltroTotal();

            AlgoritmoSimple simple = new AlgoritmoSimple(habilidades);
            AlgoritmoFrecuenciado frecuenciado = new AlgoritmoFrecuenciado(habilidades);
            string correoBuscado = "diego.murilloporras@ucr.ac.cr";
            int cantPerfiles = 5;

            //Pruebas con el filtro de habilidades
            List<string> simpleHabilidades = simple.obtenerPerfiles(correoBuscado, cantPerfiles);
            List<string> frecHabilidades = frecuenciado.obtenerPerfiles(correoBuscado, cantPerfiles);

            AlgoritmoSimple algoritmoSimpleTotal = new AlgoritmoSimple(total);
            AlgoritmoFrecuenciado algoritmoFrecuenciadoTotal = new AlgoritmoFrecuenciado(total);
            List<string> simpleTotal = algoritmoSimpleTotal.obtenerPerfiles(correoBuscado, cantPerfiles);
            List<string> frecTotal = algoritmoFrecuenciadoTotal.obtenerPerfiles(correoBuscado, cantPerfiles);

            TextWriter sh = new StreamWriter("C:/Users/Lucía Sanahuja/Desktop/habilidadesSimple.txt");
            TextWriter fh = new StreamWriter("C:/Users/Lucía Sanahuja/Desktop/habilidadesFrecuenciado.txt");
            TextWriter st = new StreamWriter("C:/Users/Lucía Sanahuja/Desktop/totalSimple.txt");
            TextWriter ft = new StreamWriter("C:/Users/Lucía Sanahuja/Desktop/totalFrecuenciado.txt");

            for (int s= 0; s < cantPerfiles; ++s)
            {
                sh.WriteLine(simpleHabilidades[s]);
                fh.WriteLine(frecHabilidades[s]);
                st.WriteLine(simpleTotal[s]);
                ft.WriteLine(frecTotal[s]);
            }

            sh.Close();
            fh.Close();
            st.Close();
            ft.Close();

        }
    }
}