using System;
using System.Collections.Generic;
using System.Linq;

namespace ComunidadPractica.Utils.MotorRecomendacion
{
    public class AlgoritmoFrecuenciado: Algoritmo
    {
        private float[] frecuencias;

        public AlgoritmoFrecuenciado(Filtro filtro)
        {
            this.correos = filtro.correos;
            this.perfiles = filtro.perfiles;
            this.caracteristicas = filtro.caracteristicas;
            this.calcularFrecuencias();
            this.similitudes = this.calcularSimilitudes(perfiles);
            //Borra datos innecesarios
            //this.perfiles.Clear();
        }

        protected override float similitud(List<byte> perfilA, List<byte> perfilB)
        {

            float matches = 0;
            int posicionFrecuencia = 0;

            foreach (var match in perfilA.Zip(perfilB, (first, second) => first == second ? first == 1 ? 1 : 2 : 0))
            {
                // match es 1 si hace match de 1s y 2 si hace match de 0s
                if(match == 1)
                {
                    matches += 1 - this.frecuencias[posicionFrecuencia];
                }
                else
                {
                    if(match == 2)
                    {
                        matches += this.frecuencias[posicionFrecuencia];
                    }
                }

                ++posicionFrecuencia;
            }
            return Convert.ToSingle(matches / (Math.Sqrt(calcularSumatoriaFrecuencias(perfilA)) * Math.Sqrt(calcularSumatoriaFrecuencias(perfilB))));
        }

        private void calcularFrecuencias()
        {
            this.frecuencias = new float[this.caracteristicas.Count];

            for (int caracteristica = 0; caracteristica < this.caracteristicas.Count; ++caracteristica)
            {
                //Cuenta en perfiles la cantidad de 1s para esa característica
                float cuenta = 0;

                for (int perfil = 0; perfil < this.perfiles.Count; ++perfil)
                {
                    cuenta += Convert.ToSingle(this.perfiles[perfil][caracteristica]);
                }
                float numero = cuenta/Convert.ToSingle(this.perfiles.Count);
                this.frecuencias[caracteristica] = numero;
            }
        }

        private float calcularSumatoriaFrecuencias(List<byte> perfil)
        {
            float sumaFrecuencias = 0;
            int posicionFrecuencia = 0;

            foreach(byte atributo in perfil)
            {
                if(atributo == 1)
                {
                    sumaFrecuencias += 1 - this.frecuencias[posicionFrecuencia];
                }
                else
                {
                    sumaFrecuencias += this.frecuencias[posicionFrecuencia];
                }
            }

            return sumaFrecuencias;
        }
    }
}