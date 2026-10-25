using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.DTOs;
using MotshwaneConsortiumGroup.Models;


namespace MotshwaneConsortiumGroup.Controllers
{
    [ApiController]
    [Route("api/units")]
    public class UnitsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UnitsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnits(int page = 1, int pageSize = 10, string? category = null, bool? available = null)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Units
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(u =>
                    u.Category == category);
            }

            if (available.HasValue)
            {
                query = query.Where(u =>
                    u.Available == available.Value);
            }

            var totalRecords = await query.CountAsync();

            var units = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalRecords,
                totalPages =
                    (int)Math.Ceiling(
                        totalRecords / (double)pageSize),

                data = units
            });
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
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUnit(int id, CreateUnitDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
            {
                return NotFound(new
                {
                    message = "Unit not found."
                });
            }

            unit.Name = dto.Name;
            unit.Category = dto.Category;
            unit.Description = dto.Description;
            unit.PriceFrom = dto.PriceFrom;
            unit.Available = dto.Available;

            await _context.SaveChangesAsync();

            return Ok(unit);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var unit = await _context.Units
                .Include(u => u.Bookings)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
            {
                return NotFound(new
                {
                    message = "Unit not found."
                });
            }

            if (unit.Bookings.Any())
            {
                return BadRequest(new
                {
                    message = "Cannot delete a unit that has bookings."
                });
            }

            _context.Units.Remove(unit);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Unit deleted successfully."
            });
        }
    }
}
