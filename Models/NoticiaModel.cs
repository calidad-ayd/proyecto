using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ComunidadPractica.Models
{
    public class NoticiaModel
    {
        [Required(ErrorMessage = "Es necesario que agregue la fecha de publicación.")]
        [Display(Name = "Fecha")]
        public string fecha { get; set; }

        [Required(ErrorMessage = "Es necesario que agregue el autor.")]
        [Display(Name = "Autor")]
        public string autor { get; set; }

        [Required(ErrorMessage = "Es necesario que agregue el estado de visibilidad.")]
        [Display(Name = "Estado")]
        public bool aceptada { get; set; }

        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,6}$",
        ErrorMessage = "Debe ingresar una dirección de correo válida.")]
        [Display(Name = "Correo")]
        public string correo { get; set; }

        public string tipoImagen { get; set; }

        [Required(ErrorMessage = "Es necesario que agregue el título.")]
        [Display(Name = "Título")]
        public string titulo { get; set; }

        [Required(ErrorMessage = "Es necesario que agregue contenido.")]
        [Display(Name = "Contenido")]
        public string cuerpo { get; set; }

        
    }
}