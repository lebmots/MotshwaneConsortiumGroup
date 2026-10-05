using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.EfCore;

public class EfStaffJobService : IStaffJobService
{
    private readonly ApplicationDbContext _db;
    private readonly IStaffService _staff;

    public EfStaffJobService(ApplicationDbContext db, IStaffService staff)
    {
        _db = db;
        _staff = staff;
    }

    private IQueryable<StaffJob> WithIncludes() =>
        _db.StaffJobs.Include(j => j.Booking).ThenInclude(b => b.Unit)
                      .Include(j => j.Booking).ThenInclude(b => b.Customer).ThenInclude(c => c.User);

    public async Task<IReadOnlyList<StaffJob>> GetAllAsync() =>
        await WithIncludes().AsNoTracking().ToListAsync();

    public async Task<StaffJob?> GetByIdAsync(int id) =>
        await WithIncludes().AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);

    public async Task<OperationResult<StaffJob>> AssignAsync(int bookingId, int staffId)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null)
            return OperationResult<StaffJob>.Fail("Booking does not exist.");

        if (booking.Status != BookingStatus.Confirmed)
            return OperationResult<StaffJob>.Fail("Only confirmed bookings can be assigned to staff.");

        if (await _db.StaffJobs.AnyAsync(j => j.BookingId == bookingId))
            return OperationResult<StaffJob>.Fail("This booking already has staff assigned.");

        var staff = (await _staff.GetAllAsync()).FirstOrDefault(s => s.Id == staffId);
        if (staff is null)
            return OperationResult<StaffJob>.Fail("Selected staff member does not exist.");

        bool alreadyBusy = await _db.StaffJobs
            .Where(j => j.StaffUserId == staffId && j.Status != JobStatus.Completed)
            .Join(_db.Bookings, j => j.BookingId, b => b.Id, (j, b) => b.BookingDate.Date)
            .AnyAsync(date => date == booking.BookingDate.Date);
        if (alreadyBusy)
            return OperationResult<StaffJob>.Fail($"{staff.Name} already has a job on {booking.BookingDate:d}.");

        var job = new StaffJob
        {
            BookingId = booking.Id,
            StaffUserId = staff.Id,
            Status = JobStatus.Assigned,
        };

        _db.StaffJobs.Add(job);
        await _db.SaveChangesAsync();

        return OperationResult<StaffJob>.Ok((await GetByIdAsync(job.Id))!);
    }

    public async Task<OperationResult<StaffJob>> UpdateStatusAsync(int id, string status)
    {
        var job = await _db.StaffJobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job is null)
            return OperationResult<StaffJob>.Fail("Job does not exist.");

        if (!JobStatus.CanTransition(job.Status, status))
            return OperationResult<StaffJob>.Fail($"Cannot move a job from {job.Status} to {status}.");

        job.Status = status;
        await _db.SaveChangesAsync();

        return OperationResult<StaffJob>.Ok((await GetByIdAsync(job.Id))!);
    }
}
