using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ComunidadPractica.Models
{
    public class UsuarioModel
    {
        [Key]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,6}$",
         ErrorMessage = "Debe ingresar una dirección de correo válida.")]
        [Display(Name = "Correo electrónico")]
        public string correo { get; set; }

        [Required(ErrorMessage = "Es necesario que agregue el nombre.")]
        [Display(Name = "Nombre")]
        public string nombre { get; set; }

        [Required(ErrorMessage = "Es necesario que ingrese una contraseña de 8 caracteres")]
        [Display(Name = "Contraseña")]
        public string contrasenna { get; set; }

        [Required(ErrorMessage = "Es necesario que seleccione el sexo.")]
        [Display(Name = "Sexo")]
        public string sexo { get; set; }

        [RegularExpression(@"^[0-9]{5,15}$", ErrorMessage = "Debe ingresar un número de teléfono válido.")]
        [Required(ErrorMessage = "Es necesario que indique un contacto.")]
        [Display(Name = "Teléfono")]
        public string contacto { get; set; }

        [Display(Name = "País")]
        public string pais { get; set; }

    }

    public class UsuarioEnCurso {

        public UsuarioModel usuario { get; set; }

        public CursoModel curso { get; set; }

        public UsuarioEnCurso()
        {
            usuario = new UsuarioModel();
            curso = new CursoModel();
        }
    }

}


