using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Students.Data;
using Students.Models;

namespace Students.Api
{
    [Authorize(Roles = "admin, professor", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/students")]
    [ApiController]
    public class StudentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [AllowAnonymous]
        [HttpGet]
        public IEnumerable<Student> GetStudents()
        {
            return _context.Students.Include(s => s.Group).Where(s => s.Group.Active);
        }

        [Authorize(Roles = "admin, professor, student")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudent([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var student = await _context.Students
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return Ok(student);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent([FromRoute] int id, [FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("{ \"error\": \"Invalide data\"}");
            }

            if (id != student.Id)
            {
                return BadRequest("{ \"error\": \"Id doesn't match\"}");
            }
            if (_context.Groups.FirstOrDefault(g => g.Id == student.GroupId) == null)
            {
                return BadRequest("Invalide group id");
            }
            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound("{ \"error\": \"Student with such id doesn't exist\"}");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> PostStudent([FromBody] Student student)
        {
            if (_context.Groups.FirstOrDefault(g => g.Id == student.GroupId) == null)
            {
                return BadRequest("{ \"error\": \"Invalide group id\"}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("{ \"error\": \"Invalide data\"}");
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudent", new { id = student.Id }, student);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var student = await _context.Students.Include(s => s.Group).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return Ok(student);
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}