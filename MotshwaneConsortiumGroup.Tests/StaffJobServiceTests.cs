using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.EfCore;
using MotshwaneConsortiumGroup.Services.Interfaces;
using Xunit;

namespace MotshwaneConsortiumGroup.Tests;

public class StaffJobServiceTests
{
    private static async Task<Booking> AddConfirmedBooking(Data.ApplicationDbContext db)
    {
        var booking = new Booking
        {
            Reference = $"MC-TEST{Guid.NewGuid().ToString("N")[..4]}",
            CustomerId = TestDbFactory.FirstCustomerId(db),
            UnitId = TestDbFactory.FirstUnitId(db),
            BookingDate = DateTime.Today.AddDays(3),
            EndDate = DateTime.Today.AddDays(4),
            Location = "Idutywa",
            Status = BookingStatus.Confirmed,
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return booking;
    }

    [Fact]
    public async Task AssignAsync_ToAConfirmedBooking_Succeeds()
    {
        using var db = TestDbFactory.Create();
        var staffService = new EfStaffService(db);
        var jobs = new EfStaffJobService(db, staffService);
        var booking = await AddConfirmedBooking(db);
        var staffId = TestDbFactory.FirstStaffId(db);

        var result = await jobs.AssignAsync(booking.Id, staffId);

        Assert.True(result.Success);
        Assert.Equal(JobStatus.Assigned, result.Value!.Status);
        Assert.Equal(booking.Id, result.Value.BookingId);
    }

    [Fact]
    public async Task AssignAsync_ToAPendingBooking_Fails()
    {
        using var db = TestDbFactory.Create();
        var staffService = new EfStaffService(db);
        var jobs = new EfStaffJobService(db, staffService);
        var booking = await AddConfirmedBooking(db);
        booking.Status = BookingStatus.Pending;
        await db.SaveChangesAsync();

        var result = await jobs.AssignAsync(booking.Id, TestDbFactory.FirstStaffId(db));

        Assert.False(result.Success);
        Assert.Contains("confirmed", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignAsync_ToABookingThatAlreadyHasAJob_Fails()
    {
        using var db = TestDbFactory.Create();
        var staffService = new EfStaffService(db);
        var jobs = new EfStaffJobService(db, staffService);
        var booking = await AddConfirmedBooking(db);
        var staffId = TestDbFactory.FirstStaffId(db);
        var first = await jobs.AssignAsync(booking.Id, staffId);
        Assert.True(first.Success);

        var second = await jobs.AssignAsync(booking.Id, staffId);

        Assert.False(second.Success);
        Assert.Contains("already", second.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignAsync_WhenStaffMemberAlreadyBusyThatDay_Fails()
    {
        using var db = TestDbFactory.Create();
        var staffService = new EfStaffService(db);
        var jobs = new EfStaffJobService(db, staffService);
        var day = DateTime.Today.AddDays(3);

        var bookingA = await AddConfirmedBooking(db);
        bookingA.BookingDate = day;
        var bookingB = await AddConfirmedBooking(db);
        bookingB.BookingDate = day;
        await db.SaveChangesAsync();

        var staffId = TestDbFactory.FirstStaffId(db);
        var first = await jobs.AssignAsync(bookingA.Id, staffId);
        Assert.True(first.Success);

        var second = await jobs.AssignAsync(bookingB.Id, staffId);

        Assert.False(second.Success);
    }

    [Fact]
    public async Task AssignAsync_ToAnUnknownStaffMember_Fails()
    {
        using var db = TestDbFactory.Create();
        var staffService = new EfStaffService(db);
        var jobs = new EfStaffJobService(db, staffService);
        var booking = await AddConfirmedBooking(db);

        var result = await jobs.AssignAsync(booking.Id, staffId: -999);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateStatusAsync_OneStepForward_Succeeds()
    {
        using var db = TestDbFactory.Create();
        var staffService = new EfStaffService(db);
        var jobs = new EfStaffJobService(db, staffService);
        var booking = await AddConfirmedBooking(db);
        var job = (await jobs.AssignAsync(booking.Id, TestDbFactory.FirstStaffId(db))).Value!;

        var result = await jobs.UpdateStatusAsync(job.Id, JobStatus.InProgress);

        Assert.True(result.Success);
        Assert.Equal(JobStatus.InProgress, result.Value!.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_SkippingAStep_FailsAndLeavesStatusUnchanged()
    {
        using var db = TestDbFactory.Create();
        var staffService = new EfStaffService(db);
        var jobs = new EfStaffJobService(db, staffService);
        var booking = await AddConfirmedBooking(db);
        var job = (await jobs.AssignAsync(booking.Id, TestDbFactory.FirstStaffId(db))).Value!;

        var result = await jobs.UpdateStatusAsync(job.Id, JobStatus.Completed);

        Assert.False(result.Success);
        var reloaded = await jobs.GetByIdAsync(job.Id);
        Assert.Equal(JobStatus.Assigned, reloaded!.Status);
    }
}
