using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Services.Interfaces;

/// <summary>Jobs assigned to staff (deliveries and collections).</summary>
public interface IStaffJobService
{
    Task<IReadOnlyList<StaffJob>> GetAllAsync();

    Task<StaffJob?> GetByIdAsync(int id);

    /// <summary>Assigns a staff member to a Confirmed booking, creating their job.
    /// Fails if the booking isn't Confirmed, or the staff member already has a job that day.</summary>
    Task<OperationResult<StaffJob>> AssignAsync(int bookingId, int staffId);

    /// <summary>Moves a job to the next status. Only the single next step in the
    /// Assigned -> In Progress -> Delivered -> Completed order is allowed.</summary>
    Task<OperationResult<StaffJob>> UpdateStatusAsync(int id, string status);
}
