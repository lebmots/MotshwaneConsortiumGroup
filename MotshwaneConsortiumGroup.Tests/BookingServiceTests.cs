using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services;
using MotshwaneConsortiumGroup.Services.InMemory;
using Xunit;

namespace MotshwaneConsortiumGroup.Tests;

public class BookingServiceTests
{
    // Each test gets its own DemoDataService, since it holds seed data in normal (non-static)
    // lists. That keeps tests independent of each other and of run order.
    private static InMemoryBookingService CreateService(out DemoDataService data)
    {
        data = new DemoDataService();
        return new InMemoryBookingService(data);
    }

    private static NewBookingRequest ValidRequest(DemoDataService data, DateTime? start = null, DateTime? end = null) => new()
    {
        CustomerName = "Test Customer",
        ServiceItemId = data.Services.First().Id,
        Location = "Idutywa",
        StartDate = start ?? DateTime.Today.AddDays(5),
        EndDate = end ?? DateTime.Today.AddDays(6),
    };

    [Fact]
    public async Task CreateAsync_WithValidRequest_SavesBookingAsPending()
    {
        var service = CreateService(out var data);

        var result = await service.CreateAsync(ValidRequest(data));

        Assert.True(result.Success);
        Assert.Equal(BookingStatus.Pending, result.Value!.Status);
        Assert.Contains(data.Bookings, b => b.Id == result.Value.Id);
    }

    [Fact]
    public async Task CreateAsync_GeneratesAUniqueReference()
    {
        var service = CreateService(out var data);

        var first = await service.CreateAsync(ValidRequest(data, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2)));
        var second = await service.CreateAsync(ValidRequest(data, DateTime.Today.AddDays(10), DateTime.Today.AddDays(11)));

        Assert.NotEqual(first.Value!.Reference, second.Value!.Reference);
    }

    [Fact]
    public async Task CreateAsync_WithMissingCustomerName_Fails()
    {
        var service = CreateService(out var data);
        var request = ValidRequest(data);
        request.CustomerName = "";

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Contains("name", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WithMissingLocation_Fails()
    {
        var service = CreateService(out var data);
        var request = ValidRequest(data);
        request.Location = "";

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Contains("location", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WithPastStartDate_Fails()
    {
        var service = CreateService(out var data);
        var request = ValidRequest(data, DateTime.Today.AddDays(-1), DateTime.Today);

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Contains("past", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WithEndDateNotAfterStartDate_Fails()
    {
        var service = CreateService(out var data);
        var sameDay = DateTime.Today.AddDays(3);
        var request = ValidRequest(data, sameDay, sameDay);

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Contains("end date", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownServiceItem_Fails()
    {
        var service = CreateService(out var data);
        var request = ValidRequest(data);
        request.ServiceItemId = -999;

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_WhenDatesOverlapAnExistingBooking_Fails()
    {
        var service = CreateService(out var data);
        var start = DateTime.Today.AddDays(5);
        var end = DateTime.Today.AddDays(8);
        var first = await service.CreateAsync(ValidRequest(data, start, end));
        Assert.True(first.Success);

        // Overlaps the middle of the first booking's range.
        var overlapping = ValidRequest(data, DateTime.Today.AddDays(6), DateTime.Today.AddDays(7));
        var second = await service.CreateAsync(overlapping);

        Assert.False(second.Success);
        Assert.Contains("not available", second.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WhenDatesDoNotOverlap_Succeeds()
    {
        var service = CreateService(out var data);
        var first = await service.CreateAsync(ValidRequest(data, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2)));
        Assert.True(first.Success);

        // Starts exactly when the first booking ends — back-to-back, not overlapping.
        var backToBack = ValidRequest(data, DateTime.Today.AddDays(2), DateTime.Today.AddDays(3));
        var second = await service.CreateAsync(backToBack);

        Assert.True(second.Success);
    }

    [Fact]
    public async Task CreateAsync_ForADifferentServiceItem_IgnoresOtherItemsBookings()
    {
        var service = CreateService(out var data);
        var start = DateTime.Today.AddDays(5);
        var end = DateTime.Today.AddDays(8);
        var firstItemId = data.Services.First().Id;
        var secondItemId = data.Services.Skip(1).First().Id;

        var first = await service.CreateAsync(new NewBookingRequest
        {
            CustomerName = "Customer A", ServiceItemId = firstItemId,
            Location = "Idutywa", StartDate = start, EndDate = end,
        });
        Assert.True(first.Success);

        // Same dates, but a different service item — should not be blocked by the first booking.
        var second = await service.CreateAsync(new NewBookingRequest
        {
            CustomerName = "Customer B", ServiceItemId = secondItemId,
            Location = "Idutywa", StartDate = start, EndDate = end,
        });

        Assert.True(second.Success);
    }

    [Fact]
    public async Task IsAvailableAsync_ExcludingABookingsOwnId_IgnoresItself()
    {
        var service = CreateService(out var data);
        var created = await service.CreateAsync(ValidRequest(data, DateTime.Today.AddDays(5), DateTime.Today.AddDays(8)));

        // Re-checking the same date range while excluding the booking's own id should read as available
        // (this is what re-validating an existing booking, e.g. on edit, would need).
        var available = await service.IsAvailableAsync(
            created.Value!.ServiceItemId, DateTime.Today.AddDays(5), DateTime.Today.AddDays(8), created.Value.Id);

        Assert.True(available);
    }
}
