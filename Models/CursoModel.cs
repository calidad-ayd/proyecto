using System.Collections.Generic;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ComunidadPractica.Models
{
    public class CursoModel
    {
        [Key]
        [Required(ErrorMessage = "Por favor agregar un nombre al curso.")]
        [Display(Name = "Nombre del curso")]
        public string nombre { get; set; }

        public int version { get; set; }

        [Required(ErrorMessage = "Por favor agregar una descripcion del curso.")]
        [Display(Name = "Descripción del curso")]
        public string descripcion { get; set; }

        [Required(ErrorMessage = "Por favor agregar contenidos del curso.")]
        [Display(Name = "Contenidos del curso")]
        public string contenidos { get; set; }

        [Required(ErrorMessage = "Es necesario que el curso tenga al menos una categoría.")]
        [Display(Name = "Categorías")]
        public List<string> categorias { get; set; }

        [Required(ErrorMessage = "Es necesario que el curso tenga al menos un tópico.")]
        [Display(Name = "Tópicos")]
        public List<string> topicos { get; set; }

        [Display(Name = "Estado")]
        public byte estado { get; set; }

        [RegularExpression("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,6}$",
         ErrorMessage = "Por favor ingresar un correo válido.")]
        [Display(Name = "Educador")]
        public string educador { get; set; }

        [RegularExpression("^[+-]?([0-9]+([.][0-9]*)?|[.][0-9]+)$",
            ErrorMessage = "Por favor ingrese un costo de curso válido.")]
        [Display(Name = "Costo del curso (USD)")]
        [Required(ErrorMessage = "Por favor ingresar el costo del curso.")]
        public float costo { get; set; }


        [Display(Name = "Educador")]
        public string nombreEducador { get; set; }

        public List<Seccion> secciones { get; set; } = new List<Seccion>();
    }

    public class Seccion
    {
        [Display(Name = "Nombre de la sección")]
        public string nombre { get; set; }

        [Display(Name = "Descripción de la sección" )]
        public string descripcion { get; set; }
        
        public List<Material> materiales { get; set; } = new List<Material>();

    }
    public class Material
    {
        [Display(Name = "Nombre del material")]
        public string nombre { get; set; }

        [Display(Name ="Descripción del material")]
        public string descripcion { get; set; }
        
        [Display(Name = "Seleccione un archivo")]
        public HttpPostedFileBase file { get; set; }

        public string path { get; set; }

        [Display(Name = "Ingrese un link del material.")]
        public string link { get; set; }
    }

    public class ParticipaCurso
    {
        [Display(Name = "Nombre del curso")]
        public string nombreCurso { get; set; }

        [Display(Name = "Correo del estudiante")]
        public string correoEstudiante { get; set; }

        [Display(Name = "Porcentaje completado")]
        public int porcentaje { get; set; }

        [Display(Name = "Versión del curso")]
        public int version { get; set; }

    }
}
