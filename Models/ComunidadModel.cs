using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ComunidadPractica.Models
{
    public class ComunidadModel
    {
        [Key]
        public string nombre { get; set; }

        [Required(ErrorMessage = "Es necesario que ingrese la descripción.")]
        [Display(Name = "Ingrese la descripción de la comunidad.")]
        public string descripcion { get; set; }

        [Required(ErrorMessage = "Es necesario que ingrese la visión.")]
        [Display(Name = "Ingrese la visión de la comunidad.")]
        public string vision { get; set; }

        [Required(ErrorMessage = "Es necesario que ingrese la misión.")]
        [Display(Name = "Ingrese la misión de la comunidad.")]
        public string mision { get; set; }

        public string correo{ get; set; }

}
}