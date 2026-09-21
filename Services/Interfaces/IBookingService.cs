using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Services.Interfaces;

/// <summary>Booking operations used by the Admin and Customer controllers.</summary>
public interface IBookingService
{
    Task<IReadOnlyList<Booking>> GetAllAsync();
}
