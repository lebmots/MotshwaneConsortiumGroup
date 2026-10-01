using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.DTOs;
using MotshwaneConsortiumGroup.Models;


namespace MotshwaneConsortiumGroup.Controllers
{
    [ApiController]
    [Route("api/units")]
    public class UnitsApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UnitsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnits()
        {
            var units = await _context.Units
                .AsNoTracking()
                .ToListAsync();

            return Ok(units);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUnit(int id)
        {
            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
                return NotFound();

            return Ok(unit);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUnit(CreateUnitDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var unit = new Unit
            {
                Name = dto.Name,
                Category = dto.Category,
                Description = dto.Description,
                PriceFrom = dto.PriceFrom,
                Available = dto.Available
            };

            _context.Units.Add(unit);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetUnit),
                new { id = unit.Id },
                unit);
        }
    }
}
