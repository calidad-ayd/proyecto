using System.Collections.Generic;
using ComunidadPractica.Models;

namespace ComunidadPractica.Utils.MotorRecomendacion
{
    public abstract class Filtro:MotorRecomendacion
    {
        public Filtro() 
        {
            InitCorreos();
            InitCaracteristicas();
            InitPerfiles();
        }

        protected abstract void InitCaracteristicas();

        protected abstract List<string> InitCaracteristicasMiembro(MiembroModel actual);

        protected void InitPerfiles()
        {
            this.perfiles = new List<List<byte>>();
            List<MiembroModel> miembros = buscarMiembros();
            foreach (MiembroModel actual in miembros)
            {
                List<byte> perfilActual = new List<byte>();
                List<string> caractMiembro = InitCaracteristicasMiembro(actual);
                
                for (int caracteristica = 0; caracteristica < this.caracteristicas.Count; ++caracteristica)
                {
                    if (caractMiembro.Contains(this.caracteristicas[caracteristica]))
                    {
                        perfilActual.Add(1);
                    }
                    else
                    {
                        perfilActual.Add(0);
                    }
                }
                this.perfiles.Add(perfilActual);
            }
        }
    }
}