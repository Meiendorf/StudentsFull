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
    [Route("api/pairs")]
    [ApiController]
    public class PairsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PairsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IEnumerable<Pair> GetPairs()
        {
            return _context.Pairs.Include(p => p.Group).Include(p => p.Professor);
        }

        [Authorize(Roles = "admin, professor, student")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPair([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pair = await _context.Pairs
                .Include(p => p.Group)
                .Include(p => p.Professor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pair == null)
            {
                return NotFound();
            }

            return Ok(pair);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPair([FromRoute] int id, [FromBody] Pair pair)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != pair.Id)
            {
                return BadRequest();
            }

            _context.Entry(pair).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PairExists(id))
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
        public async Task<IActionResult> PostPair([FromBody] Pair pair)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Pairs.Add(pair);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPair", new { id = pair.Id }, pair);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePair([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pair = await _context.Pairs
                .Include(p => p.Group)
                .Include(p => p.Professor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pair == null)
            {
                return NotFound();
            }

            _context.Pairs.Remove(pair);
            await _context.SaveChangesAsync();

            return Ok(pair);
        }

        private bool PairExists(int id)
        {
            return _context.Pairs.Any(e => e.Id == id);
        }
    }
}