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
    //Контроллер для управления парами, большая часть его методов доступна
    //только администраторам и профессорам
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "admin, professor")]
    public class PairsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PairsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Обработчик главной страницы со списком пар, доступна всем
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.IsAdmin = false;
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.IsAdmin = User.IsInRole("admin") || User.IsInRole("professor");
            }
            var applicationDbContext = _context.Pairs.Include(p => p.Group).Include(p => p.Professor).Where(p => p.Group.Active);
            return View(await applicationDbContext.ToListAsync());
        }

        //Обработчик страницы просмотра пар, доступна всем авторизированным пользователям
        [Authorize(Roles = "admin, student, professor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pair = await _context.Pairs
                .Include(p => p.Group)
                .Include(p => p.Professor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pair == null || !pair.Group.Active)
            {
                return NotFound();
            }

            return View(pair);
        }

        //Обработчик GET запроса страницы создания пары
        public async Task<IActionResult> Create()
        {
            ViewBag.Groups = await _context.Groups.ToListAsync();
            ViewBag.Professors = await _context.Professors.ToListAsync();
            return View();
        }

        //Обработчик POST запроса страницы создания пары
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,GroupId,ProfessorId")] Pair pair)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pair);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Groups = await _context.Groups.ToListAsync();
            ViewBag.Professors = await _context.Professors.ToListAsync();

            return View(pair);
        }

        //Обработчик GET запроса страницы редактирования пары
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pair = await _context.Pairs.FindAsync(id);
            if (pair == null)
            {
                return NotFound();
            }

            ViewBag.Groups = await _context.Groups.ToListAsync();
            ViewBag.Professors = await _context.Professors.ToListAsync();

            return View(pair);
        }

        //Обработчик POST запроса страницы редактирования пары
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,GroupId,ProfessorId")] Pair pair)
        {
            if (id != pair.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pair);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PairExists(pair.Id))
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
            ViewBag.Groups = new SelectList(_context.Groups, "Id", "Name");
            ViewBag.Professors = new SelectList(_context.Professors, "Id", "Name");
            return View(pair);
        }

        //Обработчик GET запроса страницы удаления пары
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pair = await _context.Pairs
                .Include(p => p.Group)
                .Include(p => p.Professor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pair == null)
            {
                return NotFound();
            }

            return View(pair);
        }

        //Обработчик POST запроса страницы удаления пары
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pair = await _context.Pairs.FindAsync(id);
            _context.Pairs.Remove(pair);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Метод проверки существования пары с указанным id
        private bool PairExists(int id)
        {
            return _context.Pairs.Any(e => e.Id == id);
        }
    }
}
