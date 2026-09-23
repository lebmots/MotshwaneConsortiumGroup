using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Services.Interfaces;

/// <summary>Booking operations used by the Admin and Customer controllers.</summary>
public interface IBookingService
{
    Task<IReadOnlyList<Booking>> GetAllAsync();

    Task<Booking?> GetByIdAsync(int id);

    /// <summary>Validates the request, checks availability, and saves a Pending booking.</summary>
    Task<OperationResult<Booking>> CreateAsync(NewBookingRequest request);

    /// <summary>True if the service item has no other active (Pending/Confirmed) booking
    /// overlapping the given date range. excludeBookingId lets a booking ignore itself when re-checked.</summary>
    Task<bool> IsAvailableAsync(int serviceItemId, DateTime start, DateTime end, int? excludeBookingId = null);
}
