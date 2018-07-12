using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Students.Models;
using Students.Data;
using Microsoft.AspNetCore.Authorization;

namespace Students.Controllers
{
    //Контроллер для управления ролями, доступен только администратору
    // [Authorize(Roles = "admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RoleController : Controller
    {
        RoleManager<IdentityRole> _roleManager;
        UserManager<User> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        //Список ролей
        public IActionResult Index() => View(_roleManager.Roles.ToList());
        //Cоздание роли
        public IActionResult Create() => View();
        //Создание роли
        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(name));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(name);
        }
        //Удаление роли
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await _roleManager.DeleteAsync(role);
            }
            return RedirectToAction("Index");
        }
        //Список пользователей
        public IActionResult UserList() => View(_userManager.Users.ToList());
        //Редактирование ролей пользователя
        public async Task<IActionResult> Edit(string userId)
        {
            //Получаем пользователя
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                //Получем список ролей пользователя
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                ChangeRoleViewModel model = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };
                return View(model);
            }

            return NotFound();
        }

        //Редактирование ролей пользователя
        [HttpPost]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            //Получаем пользователя
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                var addedRoles = roles.Except(userRoles);
                var removedRoles = userRoles.Except(roles);

                await _userManager.AddToRolesAsync(user, addedRoles);

                await _userManager.RemoveFromRolesAsync(user, removedRoles);

                return RedirectToAction("UserList");
            }

            return NotFound();
        }
    }
}