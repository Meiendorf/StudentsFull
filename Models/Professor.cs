using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Students.Models
{
    //Модель профессора
    public class Professor
    {
        public int Id { get; set; }
        [Display(Name = "Имя")]
        [Required]
        public string Name { get; set; }
        [JsonIgnore]
        public List<Pair> Pairs { get; set; }
    }
}
