using Students.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Students.Models
{
    //Класс заполнения базы тестовыми данными
    public static class SampleData
    {
        public static void Initialize(ApplicationDbContext db)
        {
            if (!db.Students.Any())
            {
                db.Groups.AddRange(
                    new Group
                    {
                        Name = "1A"
                    },
                    new Group
                    {
                        Name = "2A"
                    }
                );
                db.Professors.AddRange(
                    new Professor
                    {
                        Name = "Kate Page",
                        //Pairs = 
                    },
                    new Professor
                    {
                        Name = "Larry Paul",
                        //Pairs = db.Pairs.Where(x => x.Group.Name == "2A").ToList()
                    }
                );
                db.SaveChanges();
                db.Students.AddRange(
                    new Student
                    {
                        Name = "Jerry Laurence",
                        Group = db.Groups.First(x => x.Name == "1A")
                    },
                    new Student
                    {
                        Name = "Tom Rossiu",
                        Group = db.Groups.First(x => x.Name == "1A")
                    },
                    new Student
                    {
                        Name = "Nick Swift",
                        Group = db.Groups.First(x => x.Name == "2A")
                    }
                );
                db.Pairs.AddRange(
                    new Pair
                    {
                        Date = DateTime.Now,
                        Group = db.Groups.First(x => x.Name == "1A"),
                        ProfessorId = 1
                    },
                    new Pair
                    {
                        Date = DateTime.Now.AddHours(2),
                        Group = db.Groups.First(x => x.Name == "1A"),
                         ProfessorId = 1
                    },
                    new Pair
                    {
                        Date = DateTime.Now,
                        Group = db.Groups.First(x => x.Name == "2A"),
                        ProfessorId = 2
                    },
                    new Pair
                    {
                        Date = DateTime.Now.AddHours(3),
                        Group = db.Groups.First(x => x.Name == "2A"),
                        ProfessorId = 2
                    }
                );
                db.SaveChanges();
            }
        }
    }
}
