using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System;

namespace ComunidadPractica.Models
{
    public class ItemModel
    {
        [Required(ErrorMessage = "Es necesario que ingrese el enunciado")]
        [Display(Name = "Enunciado")]
        public string enunciado { get; set; }

        public int numItem { get; set; }

        public List<string> opciones { get; set; }

        public int cantRespuestas { get; set; }

        public List<Tuple<string, int>> respuestas { get; set; } = new List<Tuple<string, int>>();
    }

}