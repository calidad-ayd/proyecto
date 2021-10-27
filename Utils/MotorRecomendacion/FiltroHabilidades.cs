using System.Collections.Generic;
using ComunidadPractica.Models;

namespace ComunidadPractica.Utils.MotorRecomendacion
{
    public class FiltroHabilidades : Filtro
    {
        protected override void InitCaracteristicas()
        {
            this.caracteristicas = Constantes.obtenerListadoHabilidades();
        }

        protected override List<string> InitCaracteristicasMiembro(MiembroModel actual)
        {
            List<string> caractMiembro = actual.habilidades;
            return caractMiembro;
        }
    }
}