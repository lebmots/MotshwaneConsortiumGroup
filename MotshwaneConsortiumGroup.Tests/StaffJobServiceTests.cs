using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services;
using MotshwaneConsortiumGroup.Services.InMemory;
using Xunit;

namespace MotshwaneConsortiumGroup.Tests;

public class StaffJobServiceTests
{
    private static (InMemoryStaffJobService jobs, DemoDataService data, InMemoryStaffService staff) CreateService()
    {
        var data = new DemoDataService();
        var staff = new InMemoryStaffService();
        return (new InMemoryStaffJobService(data, staff), data, staff);
    }

    private static async Task<Booking> AddConfirmedBooking(DemoDataService data)
    {
        var booking = new Booking
        {
            Id = data.Bookings.Count == 0 ? 1 : data.Bookings.Max(b => b.Id) + 1,
            Reference = "MC-TEST",
            CustomerName = "Test Customer",
            Service = data.Services.First().Name,
            ServiceItemId = data.Services.First().Id,
            BookingDate = DateTime.Today.AddDays(3),
            EndDate = DateTime.Today.AddDays(4),
            Location = "Idutywa",
            Status = BookingStatus.Confirmed,
        };
        data.Bookings.Add(booking);
        return await Task.FromResult(booking);
    }

    [Fact]
    public async Task AssignAsync_ToAConfirmedBooking_Succeeds()
    {
        var (jobs, data, staff) = CreateService();
        var booking = await AddConfirmedBooking(data);
        var staffMember = (await staff.GetAllAsync()).First();

        var result = await jobs.AssignAsync(booking.Id, staffMember.Id);

        Assert.True(result.Success);
        Assert.Equal(JobStatus.Assigned, result.Value!.Status);
        Assert.Equal(booking.Id, result.Value.BookingId);
    }

    [Fact]
    public async Task AssignAsync_ToAPendingBooking_Fails()
    {
        var (jobs, data, staff) = CreateService();
        var booking = await AddConfirmedBooking(data);
        booking.Status = BookingStatus.Pending; // not confirmed yet
        var staffMember = (await staff.GetAllAsync()).First();

        var result = await jobs.AssignAsync(booking.Id, staffMember.Id);

        Assert.False(result.Success);
        Assert.Contains("confirmed", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignAsync_ToABookingThatAlreadyHasAJob_Fails()
    {
        var (jobs, data, staff) = CreateService();
        var booking = await AddConfirmedBooking(data);
        var staffMember = (await staff.GetAllAsync()).First();
        var first = await jobs.AssignAsync(booking.Id, staffMember.Id);
        Assert.True(first.Success);

        var second = await jobs.AssignAsync(booking.Id, staffMember.Id);

        Assert.False(second.Success);
        Assert.Contains("already", second.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignAsync_WhenStaffMemberAlreadyBusyThatDay_Fails()
    {
        var (jobs, data, staff) = CreateService();
        var day = DateTime.Today.AddDays(3);
        var bookingA = await AddConfirmedBooking(data);
        bookingA.BookingDate = day;
        var bookingB = await AddConfirmedBooking(data);
        bookingB.BookingDate = day;
        var staffMember = (await staff.GetAllAsync()).First();

        var first = await jobs.AssignAsync(bookingA.Id, staffMember.Id);
        Assert.True(first.Success);

        var second = await jobs.AssignAsync(bookingB.Id, staffMember.Id);

        Assert.False(second.Success);
        Assert.Contains(staffMember.Name, second.Error);
    }

    [Fact]
    public async Task AssignAsync_ToAnUnknownStaffMember_Fails()
    {
        var (jobs, data, _) = CreateService();
        var booking = await AddConfirmedBooking(data);

        var result = await jobs.AssignAsync(booking.Id, staffId: -999);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateStatusAsync_OneStepForward_Succeeds()
    {
        var (jobs, data, staff) = CreateService();
        var booking = await AddConfirmedBooking(data);
        var staffMember = (await staff.GetAllAsync()).First();
        var job = (await jobs.AssignAsync(booking.Id, staffMember.Id)).Value!;

        var result = await jobs.UpdateStatusAsync(job.Id, JobStatus.InProgress);

        Assert.True(result.Success);
        Assert.Equal(JobStatus.InProgress, result.Value!.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_SkippingAStep_FailsAndLeavesStatusUnchanged()
    {
        var (jobs, data, staff) = CreateService();
        var booking = await AddConfirmedBooking(data);
        var staffMember = (await staff.GetAllAsync()).First();
        var job = (await jobs.AssignAsync(booking.Id, staffMember.Id)).Value!;

        var result = await jobs.UpdateStatusAsync(job.Id, JobStatus.Completed);

        Assert.False(result.Success);
        var reloaded = await jobs.GetByIdAsync(job.Id);
        Assert.Equal(JobStatus.Assigned, reloaded!.Status);
    }
}
