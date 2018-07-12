using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    [Route("api/professors")]
    [ApiController]
    public class ProfessorsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfessorsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IEnumerable<Professor> GetProfessors()
        {
            return _context.Professors;
        }

        [Authorize(Roles = "admin, professor, student")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfessor([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var professor = await _context.Professors.FindAsync(id);

            if (professor == null)
            {
                return NotFound();
            }

            return Ok(professor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfessor([FromRoute] int id, [FromBody] Professor professor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != professor.Id)
            {
                return BadRequest();
            }

            _context.Entry(professor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfessorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> PostProfessor([FromBody] Professor professor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Professors.Add(professor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProfessor", new { id = professor.Id }, professor);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessor([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var professor = await _context.Professors.FindAsync(id);
            if (professor == null)
            {
                return NotFound();
            }

            _context.Professors.Remove(professor);
            await _context.SaveChangesAsync();

            return Ok(professor);
        }

        private bool ProfessorExists(int id)
        {
            return _context.Professors.Any(e => e.Id == id);
        }
    }
}