using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.EfCore;

public class EfBookingService : IBookingService
{
    private readonly ApplicationDbContext _db;
    public EfBookingService(ApplicationDbContext db) => _db = db;

    private IQueryable<Booking> WithIncludes() =>
        _db.Bookings.Include(b => b.Customer).ThenInclude(c => c.User).Include(b => b.Unit);

    public async Task<IReadOnlyList<Booking>> GetAllAsync() =>
        await WithIncludes().AsNoTracking().OrderByDescending(b => b.BookingDate).ToListAsync();

    public async Task<Booking?> GetByIdAsync(int id) =>
        await WithIncludes().AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

    public async Task<OperationResult<Booking>> CreateAsync(NewBookingRequest request)
    {
        if (request.CustomerId <= 0)
            return OperationResult<Booking>.Fail("A valid customer account is required.");

        if (string.IsNullOrWhiteSpace(request.Location))
            return OperationResult<Booking>.Fail("Location is required.");

        if (request.StartDate.Date < DateTime.Today)
            return OperationResult<Booking>.Fail("Start date cannot be in the past.");

        if (request.EndDate.Date <= request.StartDate.Date)
            return OperationResult<Booking>.Fail("End date must be after the start date.");

        var customerExists = await _db.Customers.AnyAsync(c => c.Id == request.CustomerId);
        if (!customerExists)
            return OperationResult<Booking>.Fail("Customer account could not be found.");

        var unit = await _db.Units.FirstOrDefaultAsync(u => u.Id == request.ServiceItemId);
        if (unit is null)
            return OperationResult<Booking>.Fail("Selected service does not exist.");

        if (!unit.Available)
            return OperationResult<Booking>.Fail($"{unit.Name} is not currently available.");

        if (!await IsAvailableAsync(unit.Id, request.StartDate, request.EndDate))
            return OperationResult<Booking>.Fail($"{unit.Name} is not available for the selected dates.");

        var booking = new Booking
        {
            Reference = $"MC-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            CustomerId = request.CustomerId,
            UnitId = unit.Id,
            BookingDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            Notes = request.Notes,
            Status = BookingStatus.Pending,
            PaymentStatus = PaymentStatus.AwaitingProof,
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return OperationResult<Booking>.Ok((await GetByIdAsync(booking.Id))!);
    }

    public async Task<bool> IsAvailableAsync(int serviceItemId, DateTime start, DateTime end, int? excludeBookingId = null)
    {
        var overlapping = await _db.Bookings
            .Where(b => b.Id != (excludeBookingId ?? -1))
            .Where(b => b.UnitId == serviceItemId)
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Where(b => start.Date < b.EndDate.Date && end.Date > b.BookingDate.Date)
            .AnyAsync();

        return !overlapping;
    }
}
