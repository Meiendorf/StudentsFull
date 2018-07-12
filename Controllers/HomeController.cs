using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using MessageContracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Students.Data;
using Students.Models;

namespace Students.Controllers
{
    //Контроллер основной страницы
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private ApplicationDbContext _db;
        private RoleManager<IdentityRole> roleManager;
        private UserManager<User> userManager;
        private IConfiguration configuration;
        private IHostingEnvironment env;
        private IBus _bus;
        private string rmqHost;

        public HomeController(ApplicationDbContext db, RoleManager<IdentityRole> roles, IBus bus,
            UserManager<User> users, IConfiguration _configuration, IHostingEnvironment environment)
        {
            roleManager = roles;
            userManager = users;
            _db = db;
            configuration = _configuration;
            env = environment;
            _bus = bus;
            rmqHost = configuration["RabbitMq:host"];
        }
        
        //Обработчик главной страницы, где отображаются списки всех моделей
        public async Task<IActionResult> Index()
        {
            /*var testEndPoint = await _bus.GetSendEndpoint(new Uri(rmqHost + "/testQueue"));
           
            await testEndPoint.Send<StudentContract>(new StudentContract
            {
                GroupId = 1,
                Name = "John Bry"
            });*/

            ViewBag.IsAdmin = false;
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.IsAdmin = User.IsInRole("admin") || User.IsInRole("professor");
            }
            
            ViewBag.Students = _db.Students.Include(s => s.Group).Where(s => s.Group.Active).ToList();
            ViewBag.Groups= _db.Groups.ToList();
            ViewBag.Pairs = _db.Pairs.Include(p => p.Group).Include(p => p.Professor).Where(p => p.Group.Active).ToList();
            ViewBag.Professors = _db.Professors.Include(p => p.Pairs).ToList();

            return View();
        }

        //Обработчик ошибок
        [Route("error/{id?}")]
        [HttpGet]
        public IActionResult Error(string id = "403")
        {
            var error = new ErrorViewModel();
            error.RequestId = id;
            return View(error);
        }

        //Обработчик страницы контактов
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public async Task<string> CallApi()
        {
            var res = await EmailHelper.SendMailToAdmin("User deleted record from db",
                "Students notification", configuration);
            return res.ToString();
        }
  
    }

    
}
