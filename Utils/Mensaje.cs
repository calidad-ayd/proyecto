using ComunidadPractica.Models;

namespace ComunidadPractica {
    public class Mensaje
    {
        public string error { get; set; }

        public string exito { get; set; }

        public MiembroModel miembroModel { get; set; }
        public UsuarioModel usuarioModel { get; set; }

        public Mensaje(){
            miembroModel = new MiembroModel();
            usuarioModel = new UsuarioModel();
        }
    }
}