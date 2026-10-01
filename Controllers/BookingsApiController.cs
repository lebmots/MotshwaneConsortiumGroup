using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.DTOs;
using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                    .ThenInclude(c => c.User)
                .Include(b => b.Unit)
                .AsNoTracking()
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                    .ThenInclude(c => c.User)
                .Include(b => b.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(
            CreateBookingDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var customerExists =
                await _context.Customers
                    .AnyAsync(c => c.Id == dto.CustomerID);

            if (!customerExists)
                return BadRequest("Customer does not exist.");

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == dto.UnitId);

            if (unit == null)
                return BadRequest("Unit does not exist.");

            if (!unit.Available)
                return BadRequest("Unit is not currently available.");

            var booking = new Booking
            {
                Reference = "MC-" +
                    Random.Shared.Next(100000, 999999),

                CustomerId = dto.CustomerID,
                UnitId = dto.UnitId,
                BookingDate = dto.BookingDate,
                Location = dto.Location,
                Notes = dto.Notes,
                Status = "Pending",
                PaymentStatus = "Awaiting Proof"
            };

            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetBooking),
                new { id = booking.Id },
                booking);
        }
    }
}
