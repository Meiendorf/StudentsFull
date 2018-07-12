using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Students.Data;
using Students.Models;
using Students.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Students.Controllers
{
    //Контроллер для страниц регистрации и авторизации
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        public AccountsController(UserManager<User> userManager, IConfiguration configuration,
            SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, IHostingEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _env = env;
        }
        
        //Обработчик GET запроса для страницы регистрации
        [Route("/register")]
        [HttpGet]
        public IActionResult Register()
        {
            //Если пользователь уже вошел, редиректим на главную страницу
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //Обработчик POST запроса для страницы регистрации
        [Route("/register")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            //Если пользователь уже вошел, редиректим на главную страницу
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            //Если введенные данные не прошли верификацию, возвращаем страницу с ошибками
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email };
                //Пытаемся создать нового пользователя
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("student"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("student"));
                        await _roleManager.CreateAsync(new IdentityRole("admin"));
                        await _roleManager.CreateAsync(new IdentityRole("professor"));
                    }
                    await _userManager.AddToRoleAsync(user, "student");
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                //Если не получилось, скорее всего, пользователь с такими данными уже есть
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        //Обработчик GET запроса для страницы авторизации
        [Route("/login")]
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.MPassToken = GenerateMpassToken();
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [Route("/token")]
        [HttpPost]
        public async Task<IActionResult> GetToken(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Пытаемя войти
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                //Проверяем правильность введенных данных
                if (result.Succeeded)
                {

                    var user = _userManager.Users.Where(u => u.Email == model.Email).First();
                    var token = await GenerateJwtTokenAsync(model.Email, user);
                    return Ok(token);
                }
                else
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }
        //Обработчик POST запроса для страницы авторизации
        [Route("/login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            
            if (ModelState.IsValid)
            {
                //Пытаемя войти
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                //Проверяем правильность введенных данных
                if (result.Succeeded)
                {
                    //Авторизация прошла успешно, если указана ссылка для возвращения, редиректим
                    //на неё, иначе на главную
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            ViewBag.MPassToken = GenerateMpassToken();
            return View(model);
        }

        //Обработчик выхода из учетной записи
        [Route("/logoff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            //Удаляем аутентификационные куки для текущего пользователя
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //Обработчик личного кабинета пользователя
        [Route("/account")]
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            if (_signInManager.IsSignedIn(User))
            {
                User user = await _userManager.GetUserAsync(User);
                ViewBag.User = user;
                return View();
            }
            return Unauthorized();
        }
        
        public async Task<string> Test()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var token = await GenerateJwtTokenAsync(user.Email, user);
            return token.ToString();
        }

        private async Task<object> GenerateJwtTokenAsync(string email, User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var roles = await _userManager.GetRolesAsync(user);

            claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Route("/mpasscallback")]
        public async Task<IActionResult> LogInByMPassAsync(string response)
        {
            var headerString = "";
            foreach(var header in Request.Headers.Values)
            {
                headerString += header + "\n";
            }
            if(response == "" || response == null)
            {
                return Ok(headerString);
            }
            string email, id;
            try
            {
                var token = new JwtSecurityToken(response);
                id = token.Payload["NameID"].ToString();
                email = token.Payload["EmailAddress"].ToString();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
            var user = _userManager.Users.Where(u => u.Email == email).FirstOrDefault();
            if (user != null)
            {
                await _signInManager.SignInAsync(user, true);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                user = new User { Email = email, UserName = email };
                //Пытаемся создать нового пользователя
                var result = await _userManager.CreateAsync(user, id);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("student"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("student"));
                        await _roleManager.CreateAsync(new IdentityRole("admin"));
                        await _roleManager.CreateAsync(new IdentityRole("professor"));
                    }
                    await _userManager.AddToRoleAsync(user, "student");
                    await _signInManager.SignInAsync(user, true);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
        }
        [Route("/mpass")]
        public string GenerateMpassToken()
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim("returnUrl", _configuration["mpassReturnUrl"]),
                    new Claim("clientIssuer", _configuration["mpassIssuer"])
                };

                X509Certificate2 cert = new X509Certificate2(Path.Combine(_env.ContentRootPath, _configuration["JwtSertificate"]), _configuration["JwtPass"]);
                SecurityKey key = new X509SecurityKey(cert);

                var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
                var token = new JwtSecurityToken(signingCredentials: creds, claims: claims);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch
            {
                return "Can't connect to MPass";
            }
        }

    }
}
