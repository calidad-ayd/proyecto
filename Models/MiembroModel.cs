using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComunidadPractica.Models
{
    public class MiembroModel : UsuarioModel
    {
        [Display(Name = "Habilidades")]
        public List<string> habilidades { get; set; }

        [Required(ErrorMessage = "Es necesario que ingrese al menos un idioma")]
        [Display(Name = "Idiomas")]
        public List<string> idiomas { get; set; }

        [Display(Name = "Condición Laboral")]
        public string condicionLaboral { get; set; }

        public bool esParticipanteExterno { get; set; }

        public MiembroModel(){
            esParticipanteExterno = false;
        }
    }
}