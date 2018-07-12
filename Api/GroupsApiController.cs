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
    [Route("api/groups")]
    [ApiController]
    public class GroupsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroupsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IEnumerable<Group> GetGroups()
        {
            return _context.Groups;
        }

        [Authorize(Roles = "admin, professor, student")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var @group = await _context.Groups.FindAsync(id);

            if (@group == null)
            {
                return NotFound();
            }

            return Ok(@group);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroup([FromRoute] int id, [FromBody] Group @group)
        {
            if (_context.Groups.FirstOrDefault(g => g.Name == @group.Name && g.Id != @group.Id) != null)
            {
                return BadRequest("{\"error\":\"Group with such name already exists\"}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != @group.Id)
            {
                return BadRequest();
            }

            _context.Entry(@group).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(id))
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
        public async Task<IActionResult> PostGroup([FromBody] Group @group)
        {
            if (_context.Groups.FirstOrDefault(g => g.Name == @group.Name) != null)
            {
                return BadRequest("{\"error\":\"Group with such name already exists\"}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Groups.Add(@group);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGroup", new { id = @group.Id }, @group);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var @group = await _context.Groups.FindAsync(id);
            if (@group == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(@group);
            await _context.SaveChangesAsync();

            return Ok(@group);
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.Id == id);
        }
    }
}