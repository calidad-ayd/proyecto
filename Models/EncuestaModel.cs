using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComunidadPractica.Models
{
    public class EncuestaModel
    {
        public EncuestaModel() {
            items = new List<ItemModel>();
            categorias = new List<string>();
            topicos = new List<string>();
        }

        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Por favor agregue un título.")]
        [Display(Name = "Título de encuesta")]
        public string titulo { get; set; }

        [Required(ErrorMessage = "Por favor agregue una fecha límite de cierre.")]
        [Display(Name = "Fecha de cierre de la encuesta")]
        public string fechaCierre { get; set; }

        [Required(ErrorMessage = "Es necesario que el curso tenga al menos una categoría.")]
        [Display(Name = "Categorías")]
        public List<string> categorias { get; set; }

        [Required(ErrorMessage = "Es necesario que el curso tenga al menos un tópico.")]
        [Display(Name = "Tópicos")]
        public List<string> topicos { get; set; }

        [Required(ErrorMessage = "Una encuesta necesita de al menos 1 item.")]
        public List<ItemModel> items { get; set; }

        public string correoAutor { get; set; }

}
}