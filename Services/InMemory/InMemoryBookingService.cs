using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.InMemory;

/// <summary>Temporary implementation backed by DemoDataService. Replaced once the database is ready.</summary>
public class InMemoryBookingService : IBookingService
{
    private readonly DemoDataService _data;
    public InMemoryBookingService(DemoDataService data) => _data = data;

    public Task<IReadOnlyList<Booking>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Booking>>(_data.Bookings);

    public Task<Booking?> GetByIdAsync(int id) =>
        Task.FromResult(_data.Bookings.FirstOrDefault(b => b.Id == id));

    public async Task<OperationResult<Booking>> CreateAsync(NewBookingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return OperationResult<Booking>.Fail("Customer name is required.");

        if (string.IsNullOrWhiteSpace(request.Location))
            return OperationResult<Booking>.Fail("Location is required.");

        if (request.StartDate.Date < DateTime.Today)
            return OperationResult<Booking>.Fail("Start date cannot be in the past.");

        if (request.EndDate.Date <= request.StartDate.Date)
            return OperationResult<Booking>.Fail("End date must be after the start date.");

        var service = _data.Services.FirstOrDefault(s => s.Id == request.ServiceItemId);
        if (service is null)
            return OperationResult<Booking>.Fail("Selected service does not exist.");

        if (!await IsAvailableAsync(request.ServiceItemId, request.StartDate, request.EndDate))
            return OperationResult<Booking>.Fail($"{service.Name} is not available for the selected dates.");

        var booking = new Booking
        {
            Id = _data.Bookings.Count == 0 ? 1 : _data.Bookings.Max(b => b.Id) + 1,
            Reference = $"MC-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            CustomerName = request.CustomerName,
            Service = service.Name,
            ServiceItemId = service.Id,
            BookingDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            Status = BookingStatus.Pending,
            PaymentStatus = "Awaiting Proof",
        };

        _data.Bookings.Add(booking);
        return OperationResult<Booking>.Ok(booking);
    }

    public Task<bool> IsAvailableAsync(int serviceItemId, DateTime start, DateTime end, int? excludeBookingId = null)
    {
        // Old seed bookings have EndDate == default; treat them as a single-day booking (EndDate = BookingDate)
        // so they still correctly block overlapping dates instead of matching every range.
        bool overlaps = _data.Bookings.Any(b =>
            b.Id != excludeBookingId &&
            b.ServiceItemId == serviceItemId &&
            b.Status != BookingStatus.Cancelled &&
            start.Date < (b.EndDate == default ? b.BookingDate : b.EndDate).Date.AddDays(1) &&
            end.Date > b.BookingDate.Date);

        return Task.FromResult(!overlaps);
    }
}
