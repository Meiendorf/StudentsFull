using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Students.Data;
using Students.Models;

namespace Students.Controllers
{

    //Контроллер для управления группами, большая часть его методов доступна
    //только администраторам и профессорам
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "admin, professor")]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Обработчик главной страницы, доступен анонимным пользователям
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.IsAdmin = false;
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.IsAdmin = User.IsInRole("admin") || User.IsInRole("professor");
            }
            return View(await _context.Groups.ToListAsync());
        }

        //Страница просмотра группы, доступна всем авторизированным пользователям
        [Authorize(Roles = "admin, student, professor")]
        public async Task<IActionResult> Details(int? id)
        {
            //Если id нет, возвращаем 404
            if (id == null)
            {
                return NotFound();
            }
            //Пытаемся найти группу
            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            //Если группа не найдена, возвращаем 404
            if (@group == null)
            {
                return NotFound();
            }
            return View(@group);
        }

        //Обработчик GET запроса для страницы создания группы
        public IActionResult Create()
        {
            return View();
        }

        //Обработчик POST запроса для страницы создания группы
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Active")] Group @group)
        {
            if (_context.Groups.FirstOrDefault(g => g.Name == @group.Name) != null)
            {
                ModelState.AddModelError("Name", "Group with such name already exists");
            }
            //Проверяем валидность данных
            if (ModelState.IsValid)
            {
                _context.Add(@group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }

        //Обработчик GET запроса для страницы редактирования группы
        public async Task<IActionResult> Edit(int? id)
        {
            //Если id нет
            if (id == null)
            {
                //Возвращаем 404
                return NotFound();
            }
            //Ищем в базе группу
            var @group = await _context.Groups.FindAsync(id);
            //Если её нет, возвращаем 404
            if (@group == null)
            {
                return NotFound();
            }
            return View(@group);
        }

        //Обработчик POST запроса для страницы редактирования группы
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Active")] Group @group)
        {
            if (id != @group.Id)
            {
                return NotFound();
            }
            if (_context.Groups.FirstOrDefault(g => g.Name == @group.Name && g.Id != @group.Id) != null)
            {
                ModelState.AddModelError("Name", "Group with such name already exists");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }
        //Обработчик GET запроса страницы удаления группы
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        //Обработчик POST запроса страницы удаления группы
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @group = await _context.Groups.FindAsync(id);
            _context.Groups.Remove(@group);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Метод для проверки сущестования группы с указанным id
        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.Id == id);
        }
    }
}
