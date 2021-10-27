using System.Collections.Generic;
using ComunidadPractica.Models;

namespace ComunidadPractica.Utils.MotorRecomendacion
{
    public class FiltroTotal : Filtro
    {
        protected override void InitCaracteristicas()
        {
            this.caracteristicas = Constantes.obtenerListadoHabilidades();
            this.caracteristicas.AddRange(Constantes.obtenerListadoIdiomas());
            this.caracteristicas.AddRange(Constantes.obtenerListadoPaises());
        }

        protected override List<string> InitCaracteristicasMiembro(MiembroModel actual)
        {
            List<string> caractMiembro = actual.habilidades;
            caractMiembro.AddRange(actual.idiomas);
            caractMiembro.Add(actual.pais);
            return caractMiembro;
        }
    }
}