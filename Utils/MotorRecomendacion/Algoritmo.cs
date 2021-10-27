using System.Collections.Generic;

namespace ComunidadPractica.Utils.MotorRecomendacion
{
    public abstract class Algoritmo:MotorRecomendacion
    {
        public Algoritmo() { }

        protected abstract float similitud(List<byte> perfilA, List<byte> perfilB);

        protected List<List<float>> calcularSimilitudes(List<List<byte>> valores)
        {
            List<List<float>> similitudes = new List<List<float>>();
            for (int actual = valores.Count-1; actual >= 0; --actual)
            {
                List<float> similitudesActual = new List<float>();
                for (int comparado = 0; comparado < actual; ++comparado)
                {
                    similitudesActual.Add(similitud(valores[actual], valores[comparado]));
                }
                similitudes.Insert(0,similitudesActual);
            }
            return similitudes;
        }
    }
}