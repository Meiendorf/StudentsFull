using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Students.Models
{
    //Модель студента
    public class Student
    {
        public int Id { get; set; }
        [Display(Name = "Имя")]
        [Required]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Группа")]
        public int GroupId { get; set; }

        [ForeignKey("GroupId")]
        public Group Group { get; set; }
    }
}
