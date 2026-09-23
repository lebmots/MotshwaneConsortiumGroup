using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Services.Interfaces;

/// <summary>Jobs assigned to staff (deliveries and collections).</summary>
public interface IStaffJobService
{
    Task<IReadOnlyList<StaffJob>> GetAllAsync();

    Task<StaffJob?> GetByIdAsync(int id);

    /// <returns>true if the job exists and was updated.</returns>
    Task<bool> UpdateStatusAsync(int id, string status);
}
