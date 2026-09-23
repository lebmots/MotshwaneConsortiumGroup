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
}
