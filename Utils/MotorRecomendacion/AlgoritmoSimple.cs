using System;
using System.Collections.Generic;
using System.Linq;

namespace ComunidadPractica.Utils.MotorRecomendacion
{
    public class AlgoritmoSimple : Algoritmo
    {
        public AlgoritmoSimple(Filtro filtro)
        {
            this.correos = filtro.correos;
            this.perfiles = filtro.perfiles;
            this.caracteristicas = filtro.caracteristicas;
            this.similitudes = this.calcularSimilitudes(perfiles);
            //Borra datos innecesarios
            //this.perfiles.Clear();
        }

        protected override float similitud(List<byte> A, List<byte> B)
        {
            int matches = 0;
            foreach (var match in A.Zip(B, (first, second) => first == second ? 1:0))
            {
                matches += match;
            }
            return  Convert.ToSingle(matches / (Math.Sqrt(A.Count) * Math.Sqrt(B.Count)));
        }
    }
}