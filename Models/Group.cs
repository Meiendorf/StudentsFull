using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Students.Models
{
    //Модель группы
    public class Group
    {
        public int Id { get; set; }
        //Отображаемое имя
        [Display(Name="Название группы")]
        [Required]
        public string Name { get; set; }
        [Display(Name="Активна")]
        public bool Active { get; set; } = true;
        //Ссылка на студентов, принадлежащих этой группе
        [JsonIgnore]
        public ICollection<Student> Students { get; set; } 
    }
}