using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MessageContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Students.Data;
using Students.Models;

namespace Students.Controllers
{
    //Контроллер для управления студентами, большая часть его методов доступна
    //только администраторам и профессорам
    [Authorize(Roles = "admin, professor")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class StudentController : Controller
    {
        private ApplicationDbContext _db;
        private IConfiguration configuration;
        private IBus _bus;
        private string rmqHost;

        public StudentController(ApplicationDbContext db, IConfiguration _configuration, IBus bus)
        {
            _bus = bus;
            configuration = _configuration;
            _db = db;
            rmqHost = configuration["RabbitMq:host"];
        }

        //Главная страница со списком студентов, доступна для анонимных пользователей
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.IsAdmin = false;
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.IsAdmin = User.IsInRole("admin") || User.IsInRole("professor");
            }
            return View(await _db.Students.Include(s => s.Group).Where(s => s.Group.Active).ToListAsync());
        }

        //Страница просмотра студента, доступна всем авторизированным пользователям
        [Authorize(Roles = "admin, student, professor")]
        [Route("[controller]/{id:int}", Name = "CreateStudent")]
        public IActionResult Details(int id)
        {
            var student = _db.Students
                .Include(s => s.Group)
                .FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return new NotFoundResult();
            }
            return View(student);
        }

        //Обработчик GET запроса для страницы создания студента
        [HttpGet]
        public IActionResult Create()
        {
            //Загружаем из базы группы
            ViewBag.Groups = _db.Groups.ToList();
            return View();
        }
        //Обработчик POST запроса для страницы создания студента
        [HttpPost]
        public async Task<IActionResult> Create(Student student)
        {
            //Проверяем валидность данных
            if (ModelState.IsValid)
            {
                //Добавляем студента
                var emailEndPoint = await _bus.GetSendEndpoint(new Uri(rmqHost + "/emailQueue"));

                await emailEndPoint.Send<EmailContract>(new EmailContract
                {
                    Email = configuration["adminMail"],
                    Subject = "Student added",
                    Content = $"Student {student.Name} was added to database!"
                });

                _db.Students.Add(student);
                _db.SaveChanges();
                return RedirectToAction("Index", "Student");
            }
            if (_db.Groups.FirstOrDefault(g => g.Id == student.GroupId) == null)
            {
                return BadRequest("{ \"error\": \"Invalide group id\"}");
            }
            return View(student);
        }

        //Обработчик GET запроса страницы редактирования студента, принимает его id
        [HttpGet]
        [Route("[controller]/[action]/{id:int}", Name = "EditStudent")]
        public IActionResult Edit(int? id)
        {
            if(id == null)
            {
                //Возвращаем 404
                return NotFound();
            }
            var student = _db.Students
                .AsNoTracking()
                .Include(s => s.Group)
                .FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                //Возвращаем 404
                return NotFound();
            }
            ViewBag.Groups = _db.Groups.ToList();
            return View(student);
        }

        //Обработчик POST запроса страницы редактирования студента,
        [HttpPost]
        [Route("[controller]/[action]/{id?}")]
        public IActionResult Edit(Student student)
        {
            //Проверяем валидность данных
            if (!ModelState.IsValid)
            {
                return View(student);
            }
            //Если студента с таким id нет
            if(_db.Students.AsNoTracking().FirstOrDefault(s => s.Id == student.Id) == null)
            {
                //Возвращаем 404
                return NotFound();
            }
            if(_db.Groups.FirstOrDefault(g => g.Id == student.GroupId) == null)
            {
                return BadRequest("{ \"error\": \"Invalide group id\"}");
            }
            //Если все прошло успешно, обновляем данные пользователя
            _db.Update(student);
            _db.SaveChanges();
            return RedirectToAction("Index", "Student");
        }

        //Обработчик GET запроса страницы удаления студента, принимает его id
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if(id == null)
            {
                //Возвращаем 404
                return NotFound();
            }
            var student = _db.Students
                .AsNoTracking()
                .Include(s => s.Group)
                .FirstOrDefault(s => s.Id == id);
            if(student == null)
            {
                //Возвращаем 404
                return NotFound();
            }
            return View(student);
        }
        //Обработчик POST запроса страницы удаления студента
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = _db.Students
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == id);
            _db.Students.Remove(student);
            await EmailHelper.SendMailToAdmin(String.Format("Пользователь \"{0}\" удалил студента \"{1}\"",
                User.Identity.Name, student.Name),"Notification", configuration);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Student");
        }

    }

}