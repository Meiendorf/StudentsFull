using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Students.Models
{
    //Модель пары
    public class Pair
    {
        public int Id { get; set; }
        [Display(Name = "Дата")]
        [Required]
        public DateTime Date { get; set; }
        [Display(Name = "Группа")]
        [Required]
        public int GroupId { get; set; }
        [Display(Name = "Профессор")]
        [Required]
        public int ProfessorId { get; set; }

        public Group Group { get; set; }
        public Professor Professor { get; set; }
    }
}
