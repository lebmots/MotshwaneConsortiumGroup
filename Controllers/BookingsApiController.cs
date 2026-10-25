using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.DTOs;
using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IBookingService _bookings;

    public BookingsApiController(ApplicationDbContext context, IBookingService bookings)
    {
        _context = context;
        _bookings = bookings;
    }

    private static BookingResponseDto ToDto(Booking b) => new()
    {
        Id = b.Id,
        Reference = b.Reference,
        CustomerId = b.CustomerId,
        CustomerName = b.CustomerName,
        UnitId = b.UnitId,
        UnitName = b.Service,
        BookingDate = b.BookingDate,
        EndDate = b.EndDate,
        Location = b.Location,
        Notes = b.Notes,
        Status = b.Status,
        PaymentStatus = b.PaymentStatus,
    };

    [HttpGet]
    public async Task<IActionResult> GetBookings(int page = 1, int pageSize = 10, string? status = null, string? search = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Bookings.Include(b => b.Customer).ThenInclude(c => c.User).Include(b => b.Unit).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Reference.Contains(search) || b.Location.Contains(search) || b.Customer.User.FullName.Contains(search));

        var totalRecords = await query.CountAsync();

        var bookings = await query.OrderByDescending(b => b.BookingDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalRecords,
            totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
            data = bookings.Select(ToDto),
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBooking(int id)
    {
        var booking = await _bookings.GetByIdAsync(id);
        if (booking is null)
            return NotFound(new { message = "Booking not found." });
        return Ok(ToDto(booking));
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking(CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // Routed through IBookingService (not raw EF here) so the API enforces exactly the
        // same validation and date-range availability rules as the MVC booking form —
        // one source of truth for the business rules, not two that could drift apart.
        var result = await _bookings.CreateAsync(new NewBookingRequest
        {
            CustomerId = dto.CustomerId,
            ServiceItemId = dto.UnitId,
            StartDate = dto.BookingDate,
            EndDate = dto.EndDate,
            Location = dto.Location,
            Notes = dto.Notes,
        });

        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetBooking), new { id = result.Value!.Id },
            new { message = "Booking created successfully.", bookingId = result.Value.Id, booking = ToDto(result.Value) });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBooking(int id, UpdateBookingDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null)
            return NotFound(new { message = "Booking not found." });

        booking.BookingDate = dto.BookingDate;
        booking.EndDate = dto.EndDate;
        booking.Location = dto.Location;
        booking.Notes = dto.Notes;
        booking.Status = dto.Status;
        booking.PaymentStatus = dto.PaymentStatus;

        await _context.SaveChangesAsync();

        var reloaded = await _bookings.GetByIdAsync(id);
        return Ok(new { message = "Booking updated successfully.", booking = ToDto(reloaded!) });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null)
            return NotFound(new { message = "Booking not found." });

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Booking deleted successfully." });
    }
}
