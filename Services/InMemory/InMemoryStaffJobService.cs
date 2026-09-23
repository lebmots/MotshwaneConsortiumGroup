using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.InMemory;

public class InMemoryStaffJobService : IStaffJobService
{
    private readonly DemoDataService _data;
    private readonly IStaffService _staffService;

    public InMemoryStaffJobService(DemoDataService data, IStaffService staffService)
    {
        _data = data;
        _staffService = staffService;
    }

    public Task<IReadOnlyList<StaffJob>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<StaffJob>>(_data.Jobs);

    public Task<StaffJob?> GetByIdAsync(int id) =>
        Task.FromResult(_data.Jobs.FirstOrDefault(j => j.Id == id));

    public async Task<OperationResult<StaffJob>> AssignAsync(int bookingId, int staffId)
    {
        var booking = _data.Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking is null)
            return OperationResult<StaffJob>.Fail("Booking does not exist.");

        if (booking.Status != BookingStatus.Confirmed)
            return OperationResult<StaffJob>.Fail("Only confirmed bookings can be assigned to staff.");

        if (_data.Jobs.Any(j => j.BookingId == bookingId))
            return OperationResult<StaffJob>.Fail("This booking already has staff assigned.");

        var staff = (await _staffService.GetAllAsync()).FirstOrDefault(s => s.Id == staffId);
        if (staff is null)
            return OperationResult<StaffJob>.Fail("Selected staff member does not exist.");

        bool alreadyBusy = _data.Jobs.Any(j =>
            j.StaffId == staffId &&
            j.Status != JobStatus.Completed &&
            j.Date.Date == booking.BookingDate.Date);
        if (alreadyBusy)
            return OperationResult<StaffJob>.Fail($"{staff.Name} already has a job on {booking.BookingDate:d}.");

        var job = new StaffJob
        {
            Id = _data.Jobs.Count == 0 ? 1 : _data.Jobs.Max(j => j.Id) + 1,
            Reference = booking.Reference,
            Service = booking.Service,
            Location = booking.Location,
            Date = booking.BookingDate,
            Status = JobStatus.Assigned,
            BookingId = booking.Id,
            StaffId = staff.Id,
        };

        _data.Jobs.Add(job);
        return OperationResult<StaffJob>.Ok(job);
    }

    public Task<OperationResult<StaffJob>> UpdateStatusAsync(int id, string status)
    {
        var job = _data.Jobs.FirstOrDefault(j => j.Id == id);
        if (job is null)
            return Task.FromResult(OperationResult<StaffJob>.Fail("Job does not exist."));

        if (!JobStatus.CanTransition(job.Status, status))
            return Task.FromResult(OperationResult<StaffJob>.Fail(
                $"Cannot move a job from {job.Status} to {status}."));

        job.Status = status;
        return Task.FromResult(OperationResult<StaffJob>.Ok(job));
    }
}
