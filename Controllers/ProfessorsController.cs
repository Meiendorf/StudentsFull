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
    //Контроллер для управления профессорами, большая часть его методов доступна
    //только администраторам и профессорам
    [Authorize(Roles = "admin, professor")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ProfessorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfessorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Страница списка профессоров, доступна всем
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.IsAdmin = false;
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.IsAdmin = User.IsInRole("admin") || User.IsInRole("professor");
            }
            return View(await _context.Professors.ToListAsync());
        }

        //Страница просмотра профессора, доступна только авторизированным пользователям
        [Authorize(Roles = "admin, student, professor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professor = await _context.Professors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (professor == null)
            {
                return NotFound();
            }

            return View(professor);
        }
        
        //Обработчик GET запроса страницы создания профессора
        public IActionResult Create()
        {
            return View();
        }

        //Обработчик POST запроса страницы создания профессора
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Professor professor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(professor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(professor);
        }

        //Обработчик GET запроса страницы редактирования профессора
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professor = await _context.Professors.FindAsync(id);
            if (professor == null)
            {
                return NotFound();
            }
            return View(professor);
        }

        //Обработчик POST запроса страницы редактирования профессора
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Professor professor)
        {
            if (id != professor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(professor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfessorExists(professor.Id))
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
            return View(professor);
        }

        //Обработчик GET запроса страницы удаления профессора
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professor = await _context.Professors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (professor == null)
            {
                return NotFound();
            }

            return View(professor);
        }

        //Обработчик POST запроса страницы редактирования профессора
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var professor = await _context.Professors.FindAsync(id);
            _context.Professors.Remove(professor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Метод проверки существования профессора с указанным id
        private bool ProfessorExists(int id)
        {
            return _context.Professors.Any(e => e.Id == id);
        }
    }
}
