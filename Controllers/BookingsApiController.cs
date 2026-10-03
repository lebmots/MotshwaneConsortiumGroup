using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.DTOs;
using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings(
    int page = 1,
    int pageSize = 10,
    string? status = null,
    string? search = null)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Bookings
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b =>
                    b.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.Reference.Contains(search) ||
                    b.Location.Contains(search) ||
                    b.Customer.User.FullName.Contains(search));
            }

            var totalRecords = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    Reference = b.Reference,
                    CustomerId = b.CustomerId,
                    CustomerName = b.Customer.User.FullName,
                    UnitId = b.UnitId,
                    UnitName = b.Unit.Name,
                    BookingDate = b.BookingDate,
                    Location = b.Location,
                    Notes = b.Notes,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalRecords,
                totalPages =
                    (int)Math.Ceiling(
                        totalRecords / (double)pageSize),

                data = bookings
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.Id == id)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    Reference = b.Reference,

                    CustomerId = b.CustomerId,
                    CustomerName = b.Customer.User.FullName,

                    UnitId = b.UnitId,
                    UnitName = b.Unit.Name,

                    BookingDate = b.BookingDate,
                    Location = b.Location,
                    Notes = b.Notes,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus
                })
                .FirstOrDefaultAsync();

            if (booking == null)
                return NotFound(new
                {
                    message = "Booking not found."
                });

            return Ok(booking);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(
            CreateBookingDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var customerExists = await _context.Customers
                .AnyAsync(c => c.Id == dto.CustomerId);

            if (!customerExists)
            {
                return BadRequest(new
                {
                    message = "Customer does not exist."
                });
            }

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == dto.UnitId);

            if (unit == null)
            {
                return BadRequest(new
                {
                    message = "Unit does not exist."
                });
            }

            if (!unit.Available)
            {
                return BadRequest(new
                {
                    message = "Unit is not currently available."
                });
            }

            if (dto.BookingDate.Date < DateTime.Today)
            {
                return BadRequest(new
                {
                    message = "Booking date cannot be in the past."
                });
            }

            var alreadyBooked = await _context.Bookings
                .AnyAsync(b =>
                    b.UnitId == dto.UnitId &&
                    b.BookingDate.Date == dto.BookingDate.Date &&
                    b.Status != "Cancelled");

            if (alreadyBooked)
            {
                return Conflict(new
                {
                    message = "This unit is already booked for that date."
                });
            }

            var booking = new Booking
            {
                Reference = "MC-" +
                    Random.Shared.Next(100000, 999999),

                CustomerId = dto.CustomerId,
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
                new
                {
                    message = "Booking created successfully.",
                    bookingId = booking.Id,
                    booking.Reference,
                    booking.CustomerId,
                    booking.UnitId,
                    booking.BookingDate,
                    booking.Location,
                    booking.Notes,
                    booking.Status,
                    booking.PaymentStatus
                });
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBooking(int id,UpdateBookingDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound(new
                {
                    message = "Booking not found."
                });

            booking.BookingDate = dto.BookingDate;
            booking.Location = dto.Location;
            booking.Notes = dto.Notes;
            booking.Status = dto.Status;
            booking.PaymentStatus = dto.PaymentStatus;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking updated successfully.",
                bookingId = booking.Id,
                booking.Reference,
                booking.CustomerId,
                booking.UnitId,
                booking.BookingDate,
                booking.Location,
                booking.Notes,
                booking.Status,
                booking.PaymentStatus
            });
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound(new
                {
                    message = "Booking not found."
                });
            }

            _context.Bookings.Remove(booking);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking deleted successfully."
            });
        }
    }
}