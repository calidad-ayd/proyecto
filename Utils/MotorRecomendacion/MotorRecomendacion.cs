using System.Collections.Generic;
using System.Linq;
using ComunidadPractica.Models;
using ComunidadPractica.Handlers;
using System;

namespace ComunidadPractica.Utils.MotorRecomendacion
{
    public abstract class MotorRecomendacion
    {
        public List<string> caracteristicas { set; get; }

        public List<List<byte>> perfiles { set; get; }

        public List<string> correos { set; get; }

        protected List<List<float>> similitudes { set; get; }

        protected void InitCorreos()
        {
            MiembroHandler accesoDatos = new MiembroHandler();
            this.correos = accesoDatos.obtenerTodosLosCorreos();
        }

        protected List<MiembroModel> buscarMiembros() {
            MiembroHandler accesoDatos = new MiembroHandler();
            List<MiembroModel> miembros = new List<MiembroModel>();
            foreach (var correo in this.correos)
            {
                MiembroModel miembro = accesoDatos.obtenerMiembro(correo);
                miembros.Add(miembro);
            }
            return miembros;
        }

        private int obtenerIndiceCorreo(string correo)
        {
            int indice = -1;
            for (int miembro = 0; miembro < correos.Count && miembro != indice; ++miembro)
            {
                if (correo == correos[miembro])
                {
                    indice = miembro;
                }
            }
            return indice;
        }

        private List<string> ordenarPerfiles(List<float> valoresSimilitud, int actual)
        {
            Dictionary<string, float> perfilConSimilitud = new Dictionary<string, float>();
            for(int indiceCorreo = 0; indiceCorreo < valoresSimilitud.Count; ++indiceCorreo)
            {
                int correo;
                if(indiceCorreo < actual)
                {
                    correo = indiceCorreo;
                }
                else
                {
                    correo = indiceCorreo + 1;
                }
                perfilConSimilitud.Add(this.correos[correo], valoresSimilitud[indiceCorreo]);
            }
            perfilConSimilitud = perfilConSimilitud.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            List<string>  correosOrdenados = perfilConSimilitud.Keys.ToList();
            return correosOrdenados;
        }

        public List<string> obtenerPerfiles(string correo, int cantidadPerfiles) {
            int indiceCorreo = this.obtenerIndiceCorreo(correo);
            // intersección de fila y columna
            List<float> valoresSimilitud = this.similitudes[indiceCorreo];
            for (int otroPerfil = indiceCorreo + 1; otroPerfil < this.perfiles.Count; ++otroPerfil)
            {
                valoresSimilitud.Add(this.similitudes[otroPerfil][indiceCorreo]);
            }
            List<string> similares = ordenarPerfiles(valoresSimilitud,indiceCorreo).Take(cantidadPerfiles).ToList();
            return similares;
        }
    }
}